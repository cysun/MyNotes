using MyNotes.Models;
using MyNotes.Services;

var failures = new List<string>();

void Assert(bool condition, string message)
{
    if (!condition)
        failures.Add(message);
}

var notesService = new NotesService(null!);
var note = new Note
{
    Subject = "Long note",
    Content = new string('x', 200),
    ParentId = 1001462
};

notesService.DeleteNote(note);

Assert(note.IsDeleted, "Soft-deleted notes should be marked deleted.");
Assert(note.ParentId == null, "Soft-deleted notes should be removed from their folder.");

Assert(new BatchItemInput { Type = "note", Id = 572 }.GetKind() == BatchItemKind.Note,
    "Batch item type 'note' should map to BatchItemKind.Note.");
Assert(new BatchItemInput { Type = "file", Id = 1004514 }.GetKind() == BatchItemKind.File,
    "Batch item type 'file' should map to BatchItemKind.File.");
Assert(new BatchItemInput { Type = "folder", Id = 1002447 }.GetKind() == BatchItemKind.Folder,
    "Batch item type 'folder' should map to BatchItemKind.Folder.");
Assert(new BatchItemInput { Type = "NOT_A_KIND", Id = 1 }.GetKind() == null,
    "Unknown batch item types should not map to a valid kind.");

var folderView = System.IO.File.ReadAllText(Path.Combine("MyNotes", "Views", "Folders", "View.cshtml"));
Assert(folderView.Contains("$(\"#folderItems\").on(\"change\", \".batch-select\""),
    "Batch item checkboxes should use a delegated change handler because DataTables can redraw the checkbox column.");
Assert(folderView.Contains("$(\"#folderItems\").on(\"change\", \"#selectAll\""),
    "The select-all checkbox should use a delegated change handler because DataTables can redraw the table header.");

if (failures.Count == 0)
{
    Console.WriteLine("All MyNotes.Tests checks passed.");
    return 0;
}

foreach (var failure in failures)
    Console.Error.WriteLine(failure);

return 1;
