using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class NotesController : Controller
    {
        private readonly NotesService _notesService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<NotesController> _logger;

        public NotesController(NotesService notesService, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<NotesController> logger)
        {
            _notesService = notesService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string term)
        {
            return View(_notesService.SearchNotes(term));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new NoteInputModel());
        }

        [HttpPost]
        public IActionResult Create(NoteInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var note = _mapper.Map<Note>(input);
            note.Updated = note.Created = DateTime.Now;
            _notesService.AddNote(note);
            _notesService.SaveChanges();

            return RedirectToAction("Edit", new { id = note.Id });
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            if (note.IsPrivate && !(await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded)
                return Forbid();

            return View(note);
        }

        public IActionResult Edit(int id)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            return View(_mapper.Map<NoteInputModel>(note));
        }

        public IActionResult Delete(int id)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            _notesService.DeleteNote(note);
            _notesService.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPut("Notes/{id}/{field}")]
        public IActionResult SetField(int id, string field, string value)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            if (field == "Subject")
                note.Subject = value;
            else if (field == "Content")
                note.Content = value;
            else
                _logger.LogWarning("Unrecognized field: {field}", field);

            note.Updated = DateTime.Now;
            _notesService.SaveChanges();

            return Ok();
        }
    }
}

namespace MyNotes.Models
{
    public class NoteInputModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        [Display(Name = "Subject", Prompt = "Subject")]
        public string Subject { get; set; }

        public string Content { get; set; }

        [Display(Name = "Private")]
        public bool IsPrivate { get; set; }
    }
}
