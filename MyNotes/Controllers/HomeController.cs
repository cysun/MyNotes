using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Services;

namespace MyNotes.Controllers;

public class HomeController : Controller
{
    private readonly NotesService _notesService;
    private readonly FilesService _filesService;

    private readonly IAuthorizationService _authorizationService;

    private readonly ILogger<HomeController> _logger;

    public HomeController(NotesService notesService, FilesService filesService,
        IAuthorizationService authorizationService, ILogger<HomeController> logger)
    {
        _notesService = notesService;
        _filesService = filesService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<IActionResult> IndexAsync()
    {
        bool isOwner = (await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded;
        ViewBag.Notes = _notesService.GetRecentNotes(!isOwner);
        return View();
    }

    public async Task<IActionResult> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return RedirectToAction("Index");

        bool isOwner = (await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded;
        ViewBag.Notes = _notesService.SearchNotes(term, !isOwner);
        ViewBag.Files = _filesService.SearchFiles(term, !isOwner);
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
