using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
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

        public IActionResult View(int id)
        {
            var folder = _filesService.GetFolder(id);
            if (folder == null) return NotFound();

            ViewBag.Ancestors = _filesService.GetAncestors(folder);
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
    }
}
