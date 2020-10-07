using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
    public class NotesController : Controller
    {
        private readonly NotesService _notesService;

        private readonly IMapper _mapper;
        private readonly ILogger<NotesController> _logger;

        public NotesController(NotesService notesService,
            IMapper mapper, ILogger<NotesController> logger)
        {
            _notesService = notesService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string term)
        {
            return View(_notesService.SearchNotes(term));
        }

        public IActionResult Create()
        {
            var note = new Note();
            _notesService.AddNote(note);
            _notesService.SaveChanges();

            return RedirectToAction("Edit", new { id = note.Id });
        }

        public IActionResult View(int id)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            if (string.IsNullOrWhiteSpace(note.Subject))
                return RedirectToAction("Edit", new { id });

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

            note.Viewed = note.Updated = DateTime.Now;
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

        [MaxLength(80)]
        public string Subject { get; set; }

        public string Content { get; set; }
    }
}
