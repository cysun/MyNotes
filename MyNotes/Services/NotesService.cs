using Microsoft.EntityFrameworkCore;
using MyNotes.Models;

namespace MyNotes.Services;

public class NotesService
{
    private readonly AppDbContext _db;

    public NotesService(AppDbContext db)
    {
        _db = db;
    }

    public List<Note> GetRecentNotes(bool publicOnly = false)
    {
        return _db.Notes.Where(n => DateTime.UtcNow.AddDays(-21) < n.Updated
            && (n.Published < DateTime.UtcNow || !publicOnly))
            .OrderByDescending(n => n.Updated)
            .ToList();
    }

    public List<Note> GetPinnedNotes(bool publicOnly = false)
    {
        return _db.Notes.Where(n => n.IsPinned && (n.Published < DateTime.UtcNow || !publicOnly))
            .OrderBy(n => n.Subject)
            .ToList();
    }

    public Note GetNote(int id)
    {
        return _db.Notes.Where(n => n.Id == id).SingleOrDefault();
    }

    public void AddNote(Note note) => _db.Notes.Add(note);

    public void DeleteNote(Note note)
    {
        if (note.Content == null || note.Content.Length < 200)
            _db.Notes.Remove(note);
        else
            note.IsDeleted = true;
    }

    public List<Note> SearchNotes(string term, bool publicOnly = false)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<Note>();

        return _db.Notes.FromSqlRaw("SELECT * FROM \"SearchNotes\"({0})", term)
            .Where(n => n.Published < DateTime.UtcNow || !publicOnly)
            .ToList();
    }

    public List<Note> GetBlogs(int limit = 20)
    {
        return _db.Notes.Where(n => n.IsBlog && n.Published < DateTime.UtcNow)
            .OrderByDescending(n => n.Published)
            .Take(limit)
            .ToList();
    }

    public void SaveChanges() => _db.SaveChanges();
}
