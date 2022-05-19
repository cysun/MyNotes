using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers;

[Authorize(Policy = "IsOwner")]
public class TagsController : Controller
{
    private readonly TagsService _tagsService;

    public TagsController(TagsService tagsService)
    {
        _tagsService = tagsService;
    }

    public IActionResult Index()
    {
        return View(_tagsService.GetTags());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(string label)
    {
        var tag = _tagsService.GetTag(label);

        if (tag == null)
        {
            _tagsService.AddTag(new Tag
            {
                Label = label,
                LastUsed = DateTime.UtcNow
            });
            _tagsService.SaveChanges();
        }
        else if (tag.Retired)
        {
            tag.Retired = false;
            _tagsService.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var tag = _tagsService.GetTag(id);
        if (tag == null) return NotFound();

        if (tag.NoteCount == 0)
            _tagsService.DeleteTag(tag);
        else
            tag.Retired = true;

        _tagsService.SaveChanges();

        return RedirectToAction("Index");
    }
}
