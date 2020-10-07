using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyNotes.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Subject { get; set; }

        public List<Tag> Tags { get; set; }

        public string Content { get; set; }

        public bool IsPrivate { get; set; } = true;

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;

        public bool Deleted { get; set; }
    }

    public class Tag
    {
        public int NoteId { get; set; }
        public Note Note { get; set; }

        [Required]
        [MaxLength(30)]
        public string Label { get; set; }
    }

    public class TagRecord
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string Label { get; set; }

        public int Count { get; set; }

        public DateTime Updated { get; set; } = DateTime.Now;
    }
}
