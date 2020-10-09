using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
    [Authorize(Policy = "IsOwner")]
    public class TagsController : Controller
    {
        private readonly TagsService _tagRecordsService;

        public TagsController(TagsService tagRecordsService)
        {
            _tagRecordsService = tagRecordsService;
        }

        public IActionResult Index()
        {
            return View(_tagRecordsService.GetTags());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string label)
        {
            _tagRecordsService.AddTag(new Tag
            {
                Label = label,
                LastUsed = DateTime.Now
            });
            _tagRecordsService.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
