using System;
using System.Collections.Generic;
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

        [HttpPost]
        public IActionResult Upload(int? parentId, IFormFile[] uploadedFiles)
        {
            foreach (var uploadedFile in uploadedFiles)
                _filesService.UploadFile(parentId, uploadedFile);

            return Ok();
        }

        public IActionResult Download(int id)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();
            if (file.IsFolder) return BadRequest();

            return PhysicalFile(_filesService.GetDiskFile(file.Id, file.Version),
                file.ContentType, file.Name);
        }

        [HttpPut("Files/{id}/{field}")]
        public IActionResult SetField(int id, string field, string value)
        {
            var file = _filesService.GetFile(id);
            if (file == null) return NotFound();

            switch (field)
            {
                case "IsPublic":
                    file.IsPublic = bool.Parse(value);
                    break;
                case "IsFavorite":
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
