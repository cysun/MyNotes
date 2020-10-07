using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNotes.Models;

namespace MyNotes.Services
{
    public class TagRecordsService
    {
        private readonly AppDbContext _db;

        public TagRecordsService(AppDbContext db)
        {
            _db = db;
        }

        public List<TagRecord> GetRecentTagRecords()
        {
            return _db.TagRecords.Where(r => DateTime.Now.AddDays(-21) < r.Updated)
                .OrderByDescending(r => r.Updated)
                .ToList();
        }

        public List<TagRecord> GetTagRecords()
        {
            return _db.TagRecords.OrderBy(r => r.Label).ToList();
        }

        public void AddTagRecord(TagRecord tagRecord) => _db.TagRecords.Add(tagRecord);

        public void SaveChanges() => _db.SaveChanges();
    }
}
