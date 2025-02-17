using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
    [Authorize(Policy = "IsOwner")]
    public class NotesController : Controller
    {
        private readonly NotesService _notesService;
        private readonly FilesService _filesService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<NotesController> _logger;

        public NotesController(NotesService notesService, FilesService filesService,
            IAuthorizationService authorizationService, IMapper mapper, ILogger<NotesController> logger)
        {
            _notesService = notesService;
            _filesService = filesService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string term)
        {
            ViewBag.PinnedNotes = _notesService.GetPinnedNotes();
            ViewBag.RecentNotes = _notesService.GetRecentNotes();
            return View();
        }

        public IActionResult Search(string term) =>
            string.IsNullOrWhiteSpace(term) ? RedirectToAction("Index") : View(_notesService.SearchNotes(term));

        [HttpGet]
        public IActionResult Create(int? parentId)
        {
            if (parentId != null)
                ViewBag.Parent = _filesService.GetFile(parentId.Value);

            return View(new NoteInputModel
            {
                ParentId = parentId,
            });
        }

        [HttpPost]
        public IActionResult Create(NoteInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var note = _mapper.Map<Note>(input);
            note.Updated = note.Created = DateTime.UtcNow;
            _notesService.AddNote(note);
            _notesService.SaveChanges();

            return RedirectToAction("Edit", new { id = note.Id });
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            if (!note.IsPublic && !(await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded)
                return Forbid();

            if (!User.Identity.IsAuthenticated)
            {
                note.ViewCount++;
                _notesService.SaveChanges();
            }

            return View(note);
        }

        public IActionResult Edit(int id)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            ViewBag.Note = note;

            return View(_mapper.Map<NoteInputModel>(note));
        }

        public IActionResult Move(int id, int? parentId)
        {
            if (id == parentId) return BadRequest();

            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            var parent = parentId != null ? _filesService.GetFile((int)parentId) : null;
            if (parent != null && !parent.IsFolder)
                return BadRequest();

            _logger.LogInformation("Move {note} from {oldParent} to {newParent} .", note.Id, note.ParentId, parentId);
            note.ParentId = parentId;
            _notesService.SaveChanges();

            if (parentId != null)
                return RedirectToAction("View", "Folders", new { id = note.ParentId });
            else
                return RedirectToAction("Index", "Files");
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
        public IActionResult SetField(int id, [Required] string field, string value)
        {
            var note = _notesService.GetNote(id);
            if (note == null) return NotFound();

            switch (field.ToLower())
            {
                case "subject":
                    note.Subject = value;
                    break;
                case "content":
                    note.Content = value;
                    break;
                case "published":
                    if (value == null || value.ToLower() == "null")
                        note.Published = null;
                    else
                        note.Published = DateTime.Parse(value).ToUniversalTime();
                    break;
                case "pinned":
                    note.IsPinned = !note.IsPinned;
                    break;
                default:
                    _logger.LogWarning("Unrecognized field: {field}", field);
                    break;
            }

            note.Updated = DateTime.UtcNow;
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

        public DateTime? Published { get; set; }
        public bool IsPinned { get; set; }

        public string Summary { get; set; }
        public bool IsBlog { get; set; }

        public int? ParentId { get; set; }
    }
}
