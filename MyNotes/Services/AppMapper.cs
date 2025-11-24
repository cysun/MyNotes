using MyNotes.Models;
using Riok.Mapperly.Abstractions;
using File = MyNotes.Models.File;

namespace MyNotes.Services;

[Mapper]
public partial class AppMapper
{
    // Notes

    [MapperIgnoreTarget(nameof(Note.Id))]
    public partial Note Map(NoteInputModel src);

    public partial NoteInputModel Map(Note src);

    // File and FileHistory

    [MapperIgnoreTarget(nameof(File.Id))]
    [MapperIgnoreTarget(nameof(File.ParentId))]
    [MapperIgnoreTarget(nameof(File.IsFolder))]
    public partial void Map(FileInputModel src, File dest);

    public partial FileInputModel Map(File src);

    [MapProperty(nameof(File.Id), nameof(FileHistory.FileId))]
    public partial FileHistory MapToFileHistory(File src);
}

