using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace MyNotes.Services;

public class FilesService
{
    private readonly AppDbContext _db;
    private readonly MinioService _minioService;

    private readonly IMapper _mapper;
    private readonly ILogger<FilesService> _logger;

    public FilesService(AppDbContext db, MinioService minioService, IMapper mapper, ILogger<FilesService> logger)
    {
        _db = db;
        _minioService = minioService;
        _mapper = mapper;
        _logger = logger;
    }

    public List<Models.File> GetRecentFiles(bool publicOnly = false)
    {
        return _db.Files.Where(f => DateTime.UtcNow.AddDays(-21) < f.Created && (f.IsPublic || f.IsPublic == publicOnly))
            .OrderByDescending(f => f.Updated)
            .ToList();
    }

    public List<Models.File> GetFiles()
    {
        return _db.Files.Where(f => f.ParentId == null)
            .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
            .ToList();
    }

    public Models.File GetFile(int id)
    {
        return _db.Files.Find(id);
    }

    public Models.File GetFolder(int id)
    {
        var folder = _db.Files.Where(f => f.Id == id && f.IsFolder)
            .Include(f => f.Children)
            .SingleOrDefault();

        if (folder != null)
        {
            folder.Children = folder.Children
                .OrderByDescending(c => c.IsFolder)
                .ThenBy(c => c.Name)
                .ToList();
        }

        return folder;
    }

    public List<Models.File> GetAncestors(Models.File file)
    {
        var ancestors = new List<Models.File>();

        var parentId = file.ParentId;
        while (parentId != null)
        {
            var parent = _db.Files.Find(parentId);
            ancestors.Insert(0, parent);
            parentId = parent.ParentId;
        }

        return ancestors;
    }

    public List<Models.File> GetChildren(int? parentId, bool folderOnly = false)
    {
        return _db.Files.Where(f => f.ParentId == parentId && (f.IsFolder || !folderOnly))
            .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
            .ToList();
    }

    public Models.File GetFile(int? parentId, string name)
    {
        return _db.Files.Where(f => f.ParentId == parentId && f.Name == name).FirstOrDefault();
    }

    public async Task<Models.File> UploadFileAsync(int? parentId, IFormFile uploadedFile)
    {
        string name = Path.GetFileName(uploadedFile.FileName);
        var file = GetFile(parentId, name);
        if (file == null)
        {
            file = new Models.File
            {
                Name = name,
                ContentType = uploadedFile.ContentType,
                Size = uploadedFile.Length,
                ParentId = parentId
            };
            if (parentId != null)
            {
                var parent = GetFile((int)parentId);
                file.IsPublic = parent.IsPublic;
            }
            _db.Files.Add(file);
            _logger.LogDebug($"Uploading new file: {name}");
        }
        else
        {
            _db.FileHistories.Add(_mapper.Map<Models.FileHistory>(file));
            file.ContentType = uploadedFile.ContentType;
            file.Size = uploadedFile.Length;
            file.Updated = file.Created = DateTime.UtcNow;
            file.Version++;
            _logger.LogDebug($"Updating existing file: {name}");
        }
        _db.SaveChanges();
        _logger.LogInformation($"File {name} added to database");

        await _minioService.UploadFileAsync(file, uploadedFile);
        _logger.LogInformation($"File {name} saved to object store");

        return file;
    }

    public async Task<string> GetDownloadUrlAsync(Models.File file, bool inline = false) =>
        await _minioService.GetDownloadUrlAsync(file, inline);

    public void AddFolder(Models.File folder) => _db.Files.Add(folder);

    public List<Models.File> SearchFiles(string term, bool publicOnly = false)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<Models.File>();

        return _db.Files.FromSqlRaw("SELECT * FROM \"SearchFiles\"({0})", term)
            .Where(f => f.IsPublic || !publicOnly)
            .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
            .ToList();
    }

    public async Task<int> DeleteFileAsync(int id)
    {
        var file = _db.Files.Find(id);
        if (file != null)
        {
            for (int i = 1; i < file.Version; ++i)
            {
                await _minioService.DeleteFileAsync(id, i);
                _db.FileHistories.Remove(new Models.FileHistory
                {
                    FileId = id,
                    Version = i
                });
            }
        }
        await _minioService.DeleteFileAsync(id, file.Version);
        _db.Files.Remove(file);
        _db.SaveChanges();

        return file.Version;
    }

    public async Task<int> DeleteFolderAsync(int id)
    {
        var totalRemoved = 0;
        var folder = _db.Files.Find(id);
        if (folder != null)
        {
            var children = _db.Files.Where(f => f.ParentId == id).ToList();
            foreach (var child in children)
            {
                if (child.IsFolder)
                    totalRemoved += await DeleteFolderAsync(child.Id);
                else
                {
                    await DeleteFileAsync(child.Id);
                    totalRemoved++;
                }
            }

            _db.Files.Remove(folder);
            _db.SaveChanges();
            ++totalRemoved;
        }

        return totalRemoved;
    }

    public void SaveChanges() => _db.SaveChanges();
}
