using Microsoft.EntityFrameworkCore;
using MyNotes.Models;

namespace MyNotes.Services;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Note> Notes { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<FileHistory> FileHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>().HasQueryFilter(n => !n.Deleted);
        modelBuilder.Entity<Note>().Property(n => n.IsBlog).HasDefaultValue(false);
        modelBuilder.Entity<FileHistory>().HasKey(h => new { h.FileId, h.Version });
    }
}
