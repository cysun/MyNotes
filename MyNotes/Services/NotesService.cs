using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNotes.Models;

namespace MyNotes.Services
{
    public class NotesService
    {
        private readonly AppDbContext _db;

        public NotesService(AppDbContext db)
        {
            _db = db;
        }

        public List<Note> GetRecentNotes()
        {
            return _db.Notes.Where(n => DateTime.Now.AddDays(-21) < n.Updated)
                .Include(n => n.Tags)
                .OrderByDescending(n => n.Updated)
                .ToList();
        }

        public Note GetNote(int id)
        {
            return _db.Notes.Where(n => n.Id == id).Include(n => n.Tags).SingleOrDefault();
        }

        public void AddNote(Note note) => _db.Notes.Add(note);

        public void DeleteNote(Note note)
        {
            if (DateTime.Now.AddMinutes(-10) < note.Created)
                _db.Notes.Remove(note);
            else
                note.Deleted = true;
        }

        public List<Note> SearchNotes(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Note>();

            return _db.Notes.FromSqlRaw("SELECT * FROM \"SearchEvents\"({0})", term).ToList();
        }

        public List<Note> SearchNotesByTag(string label)
        {
            return _db.Notes.Where(n => n.Tags.Any(t => t.Label == label))
                .OrderByDescending(n => n.Updated)
                .ToList();
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
