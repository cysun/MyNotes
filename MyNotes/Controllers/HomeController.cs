using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
    public class HomeController : Controller
    {
        private readonly NotesService _notesService;
        private readonly FilesService _filesService;
        private readonly TagsService _tagsService;

        private readonly IAuthorizationService _authorizationService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(NotesService notesService, FilesService filesService, TagsService tagsService,
            IAuthorizationService authorizationService, ILogger<HomeController> logger)
        {
            _notesService = notesService;
            _filesService = filesService;
            _tagsService = tagsService;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            bool isOwner = (await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded;
            ViewBag.Notes = _notesService.GetRecentNotes(!isOwner);
            ViewBag.Tags = _tagsService.GetRecentTags();
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
}
