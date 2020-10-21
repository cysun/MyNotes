﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MyNotes.Services
{
    public class FilesService
    {
        private readonly AppDbContext _db;

        private readonly IMapper _mapper;

        private readonly string _filesDir;

        public FilesService(AppDbContext db, IMapper mapper, IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            _filesDir = config["FilesDirectory"];
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

            string diskFile = Path.Combine(_filesDir, $"{file.Id}-{file.Version}");
            using (var fileStream = new FileStream(diskFile, FileMode.Create))
            {
                uploadedFile.CopyTo(fileStream);
            }

            return file;
        }

        public string GetDiskFile(int fileId, int version)
        {
            return Path.Combine(_filesDir, $"{fileId}-{version}");
        }

        public void AddFolder(Models.File folder) => _db.Files.Add(folder);

        public List<Models.File> SearchFiles(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Models.File>();

            return _db.Files.FromSqlRaw("SELECT * FROM \"SearchFiles\"({0})", term)
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