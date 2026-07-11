using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Models;
using MyNotes.Services;
using File = MyNotes.Models.File;

namespace MyNotes.Controllers;

[Authorize(Policy = "IsOwner")]
public class FoldersController : Controller
{
    private readonly FilesService _filesService;
    private readonly NotesService _notesService;

    private readonly IAuthorizationService _authorizationService;
    
    public FoldersController(FilesService filesService, NotesService notesService,
        IAuthorizationService authorizationService)
    {
        _filesService = filesService;
        _notesService = notesService;
        _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewAsync(int id)
    {
        var folder = _filesService.GetFolder(id);
        if (folder == null) return NotFound();

        if (!folder.IsPublic && !(await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded)
            return Forbid();

        if (User.Identity?.IsAuthenticated is true)
        {
            ViewBag.Ancestors = _filesService.GetAncestors(folder);
        }
        else
        {
            folder.AccessCount++;
            _filesService.SaveChanges();
        }

        ViewBag.Notes = _notesService.GetNotesInFolder(id);

        return View(folder);
    }

    [HttpPost("Folders")]
    public IActionResult Create(string name, int? parentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest();

        var folder = new File
        {
            Name = name,
            ParentId = parentId,
            IsFolder = true
        };
        _filesService.AddFolder(folder);
        _filesService.SaveChanges();

        return Ok();
    }

    public List<File> GetChildFolders(int? parentId)
    {
        return _filesService.GetChildren(parentId, true);
    }

    [HttpPost("Folders/BatchMove")]
    public IActionResult BatchMove([FromBody] BatchMoveInput input)
    {
        if (input?.Items == null || input.Items.Count == 0)
            return BadRequest();

        if (!IsValidDestination(input.ParentId))
            return BadRequest();

        var moved = 0;
        var seen = new HashSet<string>();

        foreach (var item in input.Items)
        {
            var kind = item.GetKind();
            if (kind == null)
                return BadRequest();

            if (!seen.Add($"{kind}:{item.Id}"))
                continue;

            switch (kind.Value)
            {
                case BatchItemKind.Note:
                    var note = _notesService.GetNote(item.Id);
                    if (note == null) return NotFound();

                    note.ParentId = input.ParentId;
                    moved++;
                    break;

                case BatchItemKind.File:
                    var file = _filesService.GetFile(item.Id);
                    if (file == null) return NotFound();
                    if (file.IsFolder) return BadRequest();

                    file.ParentId = input.ParentId;
                    moved++;
                    break;

                case BatchItemKind.Folder:
                    var folder = _filesService.GetFile(item.Id);
                    if (folder == null) return NotFound();
                    if (!folder.IsFolder || IsInvalidFolderDestination(folder.Id, input.ParentId))
                        return BadRequest();

                    folder.ParentId = input.ParentId;
                    moved++;
                    break;
            }
        }

        _notesService.SaveChanges();
        _filesService.SaveChanges();

        return Ok(new { moved });
    }

    [HttpPost("Folders/BatchDelete")]
    public async Task<IActionResult> BatchDeleteAsync([FromBody] BatchDeleteInput input)
    {
        if (input?.Items == null || input.Items.Count == 0)
            return BadRequest();

        var seen = new HashSet<string>();
        var notes = new List<Note>();
        var files = new List<File>();
        var folders = new List<File>();

        foreach (var item in input.Items)
        {
            var kind = item.GetKind();
            if (kind == null)
                return BadRequest();

            if (!seen.Add($"{kind}:{item.Id}"))
                continue;

            switch (kind.Value)
            {
                case BatchItemKind.Note:
                    var note = _notesService.GetNote(item.Id);
                    if (note == null) return NotFound();

                    notes.Add(note);
                    break;

                case BatchItemKind.File:
                    var file = _filesService.GetFile(item.Id);
                    if (file == null) return NotFound();
                    if (file.IsFolder) return BadRequest();

                    files.Add(file);
                    break;

                case BatchItemKind.Folder:
                    var folder = _filesService.GetFile(item.Id);
                    if (folder == null) return NotFound();
                    if (!folder.IsFolder) return BadRequest();

                    folders.Add(folder);
                    break;
            }
        }

        var deleted = 0;

        foreach (var note in notes)
        {
            _notesService.DeleteNote(note);
            deleted++;
        }

        foreach (var file in files)
        {
            await _filesService.DeleteFileAsync(file.Id);
            deleted++;
        }

        foreach (var folder in folders) deleted += await _filesService.DeleteFolderAsync(folder.Id);

        _notesService.SaveChanges();

        return Ok(new { deleted });
    }

    private bool IsValidDestination(int? parentId)
    {
        if (parentId == null)
            return true;

        var parent = _filesService.GetFile(parentId.Value);
        return parent?.IsFolder == true;
    }

    private bool IsInvalidFolderDestination(int folderId, int? parentId)
    {
        while (parentId != null)
        {
            if (parentId == folderId)
                return true;

            var parent = _filesService.GetFile(parentId.Value);
            parentId = parent?.ParentId;
        }

        return false;
    }
}
