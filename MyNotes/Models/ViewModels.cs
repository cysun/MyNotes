namespace MyNotes.Models;

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
