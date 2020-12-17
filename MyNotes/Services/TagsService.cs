using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNotes.Models;

namespace MyNotes.Services
{
    public class TagsService
    {
        private readonly AppDbContext _db;

        public TagsService(AppDbContext db)
        {
            _db = db;
        }

        public List<Tag> GetRecentTags()
        {
            return _db.Tags.Where(r => DateTime.Now.AddDays(-21) < r.LastUsed)
                .OrderByDescending(r => r.LastUsed)
                .ToList();
        }

        public List<Tag> GetTags()
        {
            return _db.Tags.OrderBy(r => r.Label).ToList();
        }

        public Tag GetTag(int id)
        {
            return _db.Tags.Find(id);
        }

        public Tag GetTag(string label)
        {
            return _db.Tags.Where(t => t.Label.ToLower() == label.ToLower()).SingleOrDefault();
        }

        public void AddTag(Tag tag) => _db.Tags.Add(tag);

        public void DeleteTag(Tag tag) => _db.Tags.Remove(tag);

        public void SaveChanges() => _db.SaveChanges();
    }
}
