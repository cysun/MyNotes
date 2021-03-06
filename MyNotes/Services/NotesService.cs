﻿using System;
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

        public List<Note> GetRecentNotes(bool publicOnly = false)
        {
            return _db.Notes.Where(n => DateTime.Now.AddDays(-21) < n.Updated
                && (n.Published < DateTime.Now || !publicOnly))
                .Include(n => n.NoteTags)
                .OrderByDescending(n => n.Updated)
                .ToList();
        }

        public Note GetNote(int id)
        {
            return _db.Notes.Where(n => n.Id == id).Include(n => n.NoteTags).SingleOrDefault();
        }

        public void AddNote(Note note) => _db.Notes.Add(note);

        public void DeleteNote(Note note)
        {
            if (note.Content == null || note.Content.Length < 200)
                _db.Notes.Remove(note);
            else
                note.Deleted = true;
        }

        public List<Note> SearchNotes(string term, bool publicOnly = false)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Note>();

            return _db.Notes.FromSqlRaw("SELECT * FROM \"SearchNotes\"({0})", term)
                .Where(n => n.Published < DateTime.Now || !publicOnly)
                .Include(n => n.NoteTags) // This is very cool
                .ToList();
        }

        public List<Note> SearchNotesByTag(string label)
        {
            return _db.Notes.Where(n => n.NoteTags.Any(t => t.Label == label))
                .Include(n => n.NoteTags)
                .OrderByDescending(n => n.Updated)
                .ToList();
        }

        public List<Note> GetBlogs(int limit = 4)
        {
            return _db.Notes.Where(n => n.IsBlog && n.Published < DateTime.Now)
                .Include(n => n.NoteTags)
                .OrderByDescending(n => n.Published)
                .Take(limit)
                .ToList();
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
