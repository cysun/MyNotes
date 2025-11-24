using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
