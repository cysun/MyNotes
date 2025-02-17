using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            List<Models.File> files;

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
            file.Updated = DateTime.UtcNow;
            _filesService.SaveChanges();

            if (file.ParentId != null)
                return RedirectToAction("View", "Folders", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        public IActionResult Move(int id, int? parentId)
        {
            if (id == parentId) return BadRequest();

            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            var parent = parentId != null ? _filesService.GetFile((int)parentId) : null;
            if (parent != null && !parent.IsFolder)
                return BadRequest();

            _logger.LogInformation("Move {file} from {oldParent} to {newParent} .", file.Id, file.ParentId, parentId);
            file.ParentId = parentId;
            _filesService.SaveChanges();

            if (parentId != null)
                return RedirectToAction("View", "Folders", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            if (file.IsFolder)
            {
                var filesDeleted = await _filesService.DeleteFolderAsync(id);
                _logger.LogInformation("{n} files deleted.", filesDeleted);
            }
            else
            {
                var versionsDeleted = await _filesService.DeleteFileAsync(id);
                _logger.LogInformation("{n} versions of {file} deleted.", versionsDeleted, id);
            }

            if (file.ParentId != null)
                return RedirectToAction("View", "Folders", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        public async Task<IActionResult> AjaxDeleteAsync(int id)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            if (file.IsFolder)
            {
                var filesDeleted = await _filesService.DeleteFolderAsync(id);
                _logger.LogInformation("{n} files deleted.", filesDeleted);
            }
            else
            {
                var versionsDeleted = await _filesService.DeleteFileAsync(id);
                _logger.LogInformation("{n} versions of {file} deleted.", versionsDeleted, id);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadAsync(int? parentId, IFormFile[] uploadedFiles)
        {
            foreach (var uploadedFile in uploadedFiles)
                await _filesService.UploadFileAsync(parentId, uploadedFile);

            if (parentId != null)
            {
                var parent = _filesService.GetFile((int)parentId);
                parent.Updated = DateTime.UtcNow;
                _filesService.SaveChanges();
            }

            return Ok();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id, int? version)
        {
            return await DownloadAsync(id, version, true);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DownloadAsync(int id, int? version = null, bool inline = false)
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

            return Redirect(await _filesService.GetDownloadUrlAsync(file, version, inline));
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
                    file.IsPinned = bool.Parse(value);
                    break;
                default:
                    _logger.LogWarning("Unrecognized field: {field}", field);
                    return BadRequest();
            }

            file.Updated = DateTime.UtcNow;
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

        [Display(Name = "Pinned")]
        public bool IsPinned { get; set; }

        [Display(Name = "Public")]
        public bool IsPublic { get; set; }
    }
}
