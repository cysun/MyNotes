using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNotes.Models;
using MyNotes.Services;

namespace MyNotes.Controllers
{
    [Authorize(Policy = "IsOwner")]
    public class FilesController : Controller
    {
        private readonly FilesService _filesService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<FilesController> _logger;

        public FilesController(FilesService filesService, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<FilesController> logger)
        {
            _filesService = filesService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string term)
        {
            List<File> files;

            if (string.IsNullOrWhiteSpace(term))
                files = _filesService.GetFiles();
            else
                files = _filesService.SearchFiles(term);

            return View(files);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            ViewBag.Ancestors = _filesService.GetAncestors(file);
            return View(_mapper.Map<FileInputModel>(file));
        }

        [HttpPost]
        public IActionResult Edit(int id, FileInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            _mapper.Map(input, file);
            file.Updated = DateTime.Now;
            _filesService.SaveChanges();

            if (file.ParentId != null)
                return RedirectToAction("View", "Folders", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            if (file.IsFolder)
            {
                var filesDeleted = _filesService.DeleteFolder(id);
                _logger.LogInformation("{n} files deleted.", filesDeleted);
            }
            else
            {
                var versionsDeleted = _filesService.DeleteFile(id);
                _logger.LogInformation("{n} versions of {file} deleted.", versionsDeleted, id);
            }

            if (file.ParentId != null)
                return RedirectToAction("View", "Folders", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Upload(int? parentId, IFormFile[] uploadedFiles)
        {
            foreach (var uploadedFile in uploadedFiles)
                _filesService.UploadFile(parentId, uploadedFile);

            if (parentId != null)
            {
                var parent = _filesService.GetFile((int)parentId);
                parent.Updated = DateTime.Now;
                _filesService.SaveChanges();
            }

            return Ok();
        }

        [AllowAnonymous]
        public async Task<IActionResult> DownloadAsync(int id)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();
            if (file.IsFolder) return BadRequest();

            if (!file.IsPublic && !(await _authorizationService.AuthorizeAsync(User, "IsOwner")).Succeeded)
                return Forbid();

            if (!User.Identity.IsAuthenticated)
            {
                file.AccessCount++;
                _filesService.SaveChanges();
            }

            return PhysicalFile(_filesService.GetDiskFile(file.Id, file.Version),
                file.ContentType, file.Name);
        }

        [HttpPut("Files/{id}/{field}")]
        public IActionResult SetField(int id, [Required] string field, string value)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            switch (field.ToLower())
            {
                case "ispublic":
                    file.IsPublic = bool.Parse(value);
                    break;
                case "isfavorite":
                    file.IsFavorite = bool.Parse(value);
                    break;
                default:
                    _logger.LogWarning("Unrecognized field: {field}", field);
                    return BadRequest();
            }

            file.Updated = DateTime.Now;
            _filesService.SaveChanges();

            return Ok();
        }
    }
}

namespace MyNotes.Models
{
    public class FileInputModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public bool IsFolder { get; set; }

        [Display(Name = "Favorite")]
        public bool IsFavorite { get; set; }

        [Display(Name = "Public")]
        public bool IsPublic { get; set; }
    }
}
