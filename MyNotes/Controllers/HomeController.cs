using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
    public class HomeController : Controller
    {
        private readonly NotesService _notesService;
        private readonly TagRecordsService _tagRecorsService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(NotesService notesService, TagRecordsService tagRecordsService,
            ILogger<HomeController> logger)
        {
            _notesService = notesService;
            _tagRecorsService = tagRecordsService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Notes = _notesService.GetRecentNotes();
            ViewBag.TagRecords = _tagRecorsService.GetRecentTagRecords();
            return View();
        }
    }
}
