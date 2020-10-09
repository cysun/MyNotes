using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public List<NoteTag> NoteTags { get; set; }

        public string Content { get; set; }

        public bool IsPublic { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;

        public bool Deleted { get; set; }
    }

    [Table("NoteTags")]
    public class NoteTag
    {
        public int NoteId { get; set; }
        public Note Note { get; set; }

        [Required]
        [MaxLength(30)]
        public string Label { get; set; }
    }
}
