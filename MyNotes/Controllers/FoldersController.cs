using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Services;

namespace MyNotes.Controllers;

[Authorize(Policy = "IsOwner")]
public class FoldersController : Controller
{
    private readonly FilesService _filesService;

    private readonly IAuthorizationService _authorizationService;

    private readonly IMapper _mapper;
    private readonly ILogger<FoldersController> _logger;

    public FoldersController(FilesService filesService, IAuthorizationService authorizationService,
        IMapper mapper, ILogger<FoldersController> logger)
    {
        _filesService = filesService;
        _authorizationService = authorizationService;
        _mapper = mapper;
        _logger = logger;
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewAsync(int id)
    {
        var folder = _filesService.GetFolder(id);
        if (folder == null) return NotFound();

        if (!folder.IsPublic && !(await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded)
            return Forbid();

        if (User.Identity.IsAuthenticated)
        {
            ViewBag.Ancestors = _filesService.GetAncestors(folder);
        }
        else
        {
            folder.AccessCount++;
            _filesService.SaveChanges();
        }

        return View(folder);
    }

    [HttpPost("Folders")]
    public IActionResult Create(string name, int? parentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest();

        var folder = new Models.File
        {
            Name = name,
            ParentId = parentId,
            IsFolder = true
        };
        _filesService.AddFolder(folder);
        _filesService.SaveChanges();

        return Ok();
    }

    public List<Models.File> GetChildFolders(int? parentId)
    {
        return _filesService.GetChildren(parentId, true);
    }
}
