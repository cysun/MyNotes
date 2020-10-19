using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNotes.Models;

namespace MyNotes.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<FileHistory> FileHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasQueryFilter(n => !n.Deleted);
            modelBuilder.Entity<NoteTag>().HasKey(t => new { t.NoteId, t.Label });
            modelBuilder.Entity<Tag>().HasAlternateKey(t => t.Label);
            modelBuilder.Entity<FileHistory>().HasKey(h => new { h.FileId, h.Version });
        }
    }
}
