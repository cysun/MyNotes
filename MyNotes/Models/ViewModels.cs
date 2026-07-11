namespace MyNotes.Models;

public enum BatchItemKind
{
    Note,
    File,
    Folder
}

public class BatchItemInput
{
    public string Type { get; set; }
    public int Id { get; set; }

    public BatchItemKind? GetKind() => Type?.Trim().ToLowerInvariant() switch
    {
        "note" => BatchItemKind.Note,
        "file" => BatchItemKind.File,
        "folder" => BatchItemKind.Folder,
        _ => null
    };
}

public class BatchMoveInput
{
    public List<BatchItemInput> Items { get; set; } = [];
    public int? ParentId { get; set; }
}

public class BatchDeleteInput
{
    public List<BatchItemInput> Items { get; set; } = [];
}

public class NoteViewModel
{
    public Note Note { get; init; }
    public bool ShowParent { get; init; } = true;
}

public class FileViewModel
{
    public File File { get; init; }
    public bool ShowParent { get; init; } = true;
}
