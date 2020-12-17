using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyNotes.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Label { get; set; }

        public int NoteCount { get; set; }

        public DateTime LastUsed { get; set; } = DateTime.Now;

        public bool Retired { get; set; }
    }
}
