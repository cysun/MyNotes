using System.ComponentModel.DataAnnotations;

namespace MyNotes.Models;

public class File
{
    public int Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Name { get; set; }

    public int Version { get; set; } = 1;

    [MaxLength(255)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public bool IsFolder { get; set; }

    public int? ParentId { get; set; }
    public File Parent { get; set; }

    public List<File> Children { get; set; }

    public int AccessCount { get; set; }

    public bool IsPinned { get; set; }
    public bool IsPublic { get; set; }

    public string GetFormattedSize()
    {
        if (IsFolder) return "";

        if (Size < 1024)
            return $"{Size} B";
        else if (Size < 1048576)
            return (Size / 1024.0).ToString("0.#") + " KB";
        else
            return (Size / 1024.0 / 1024.0).ToString("0.#") + " MB";
    }
}

public class FileHistory
{
    public int FileId { get; set; }
    public File File { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Name { get; set; }

    public int Version { get; set; }

    [MaxLength(255)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
