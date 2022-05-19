using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers;

[Authorize(Policy = "IsOwner")]
public class BlogsController : Controller
{
    private readonly NotesService _notesService;

    private readonly IMapper _mapper;

    public BlogsController(NotesService notesService, IMapper mapper)
    {
        _notesService = notesService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return View(_notesService.GetBlogs());
    }

    [AllowAnonymous]
    public IActionResult View(int id)
    {
        var blog = _notesService.GetNote(id);
        if (blog == null) return NotFound();

        if (!blog.IsBlog || !blog.IsPublic)
            return Forbid();

        if (!User.Identity.IsAuthenticated)
        {
            blog.ViewCount++;
            _notesService.SaveChanges();
        }

        return View(blog);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var blog = _notesService.GetNote(id);
        if (blog == null) return NotFound();

        return View(_mapper.Map<NoteInputModel>(blog));
    }

    [HttpPost]
    public IActionResult Edit(int id, string summary)
    {
        var blog = _notesService.GetNote(id);
        if (blog == null) return NotFound();

        blog.Summary = summary;
        blog.IsBlog = true;
        _notesService.SaveChanges();

        return RedirectToAction("Edit", "Notes", new { id = id });
    }
}
