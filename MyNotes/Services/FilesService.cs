using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MyNotes.Services
{
    public class FilesSettings
    {
        public string Directory { get; set; }

        // Attachment Types are files (e.g. doc) whose Content-Disposition header should be
        // set to attachment even for View File operation. These files cannot be displayed
        // directly in browser so browsers will try to save them. Providing a file name to
        // PhysicalFile() will ensure that the file is saved with the right name instead of
        // having id as its name.
        public HashSet<string> AttachmentTypes { get; set; }

        // Text Types are files (e.g. java) that should be displayed directly in browser.
        // Browsers may not display them because of their content types, so we'll overwrite
        // their content types with "text/plain".
        public HashSet<string> TextTypes { get; set; }
    }

    public class FilesService
    {
        private readonly AppDbContext _db;

        private readonly IMapper _mapper;

        private readonly FilesSettings _settings;

        public FilesService(AppDbContext db, IMapper mapper, IOptions<FilesSettings> settings)
        {
            _db = db;
            _mapper = mapper;
            _settings = settings.Value;
        }

        public bool IsAttachmentType(string fileName)
        {
            return _settings.AttachmentTypes.Contains(Path.GetExtension(fileName).ToLower());
        }

        public bool IsTextType(string fileName)
        {
            return _settings.TextTypes.Contains(Path.GetExtension(fileName).ToLower());
        }

        public List<Models.File> GetRecentFiles(bool publicOnly = false)
        {
            return _db.Files.Where(f => DateTime.Now.AddDays(-21) < f.Created && (f.IsPublic || f.IsPublic == publicOnly))
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

        public Models.File UploadFile(int? parentId, IFormFile uploadedFile)
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
            }
            else
            {
                _db.FileHistories.Add(_mapper.Map<Models.FileHistory>(file));
                file.ContentType = uploadedFile.ContentType;
                file.Size = uploadedFile.Length;
                file.Updated = file.Created = DateTime.Now;
                file.Version++;
            }
            _db.SaveChanges();

            string diskFile = Path.Combine(_settings.Directory, $"{file.Id}-{file.Version}");
            using (var fileStream = new FileStream(diskFile, FileMode.Create))
            {
                uploadedFile.CopyTo(fileStream);
            }

            return file;
        }

        public string GetDiskFile(int fileId, int version)
        {
            return Path.Combine(_settings.Directory, $"{fileId}-{version}");
        }

        public void AddFolder(Models.File folder) => _db.Files.Add(folder);

        public List<Models.File> SearchFiles(string term, bool publicOnly = false)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Models.File>();

            return _db.Files.FromSqlRaw("SELECT * FROM \"SearchFiles\"({0})", term)
                .Where(f => f.IsPublic || !publicOnly)
                .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
                .ToList();
        }

        public int DeleteFile(int id)
        {
            var file = _db.Files.Find(id);
            if (file != null)
            {
                for (int i = 1; i < file.Version; ++i)
                {
                    File.Delete(GetDiskFile(id, i));
                    _db.FileHistories.Remove(new Models.FileHistory
                    {
                        FileId = id,
                        Version = i
                    });
                }
            }
            File.Delete(GetDiskFile(id, file.Version));
            _db.Files.Remove(file);
            _db.SaveChanges();

            return file.Version;
        }

        public int DeleteFolder(int id)
        {
            var totalRemoved = 0;
            var folder = _db.Files.Find(id);
            if (folder != null)
            {
                var children = _db.Files.Where(f => f.ParentId == id).ToList();
                foreach (var child in children)
                {
                    if (child.IsFolder)
                        totalRemoved += DeleteFolder(child.Id);
                    else
                    {
                        DeleteFile(child.Id);
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
}
