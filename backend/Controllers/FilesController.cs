using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileModel = backend.Models.File;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;

        public FilesController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            var filePath = Path.Combine("uploads", Guid.NewGuid() + Path.GetExtension(file.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileEntity = new FileModel
            {
                FileName = file.FileName,
                FilePath = filePath,
                UploadedAt = DateTime.Now,
                Size = file.Length,
                Uploader = User.Identity.Name // 假设已经实现身份验证
            };

            await _fileRepository.AddFileAsync(fileEntity);

            return Ok(new { fileId = fileEntity.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            //return File(memory, "application/octet-stream", file.FileName);
            return File(memory, "application/octet-stream", file?.FileName ?? "unknown");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();

            System.IO.File.Delete(file.FilePath);
            await _fileRepository.DeleteFileAsync(id);

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileModel>>> GetFiles()
        {
            var files = await _fileRepository.GetAllFilesAsync();
            return Ok(files);
        }
    }
}
