using backend.Models;
using backend.Repositories;
using backend.Hubs; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileModel = backend.Models.File;
using Microsoft.AspNetCore.Http;
using System.Linq;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;
        private readonly IHubContext<FileHub> _hubContext;

        public FilesController(IFileRepository fileRepository, IHubContext<FileHub> hubContext)
        {
            _fileRepository = fileRepository;
            _hubContext = hubContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            // Generate a unique file name to ensure security
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            //var filePath = Path.Combine(uploadsFolder, Guid.NewGuid() + Path.GetExtension(file.FileName));
            //var filePath = Path.Combine("uploads", Guid.NewGuid() + Path.GetExtension(file.FileName));
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileEntity = new FileModel
                {
                    FileName = file.FileName,
                    FilePath = filePath,
                    UploadedAt = DateTime.UtcNow,
                    //UploadedAt = DateTime.Now,
                    Size = file.Length,
                    Uploader = User.Identity.Name?? "Anonymous" // Handling unauthenticated users
                };

                await _fileRepository.AddFileAsync(fileEntity);

                // Notify clients using SignalR
                var hubContext = HttpContext.RequestServices.GetService<IHubContext<FileHub>>();
                if (hubContext == null)
                {
                    // Handling the case where hubContext is null
                    return StatusCode(500, "Internal server error: Unable to get hub context.");
                }
                await _hubContext.Clients.All.SendAsync("FileUploaded", fileEntity.FileName);

                return Ok(new { fileId = fileEntity.Id });
            }
            catch (Exception ex)
            {
                // output err log
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return StatusCode(500, "Internal server error during file upload.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();
            // check
            if (!System.IO.File.Exists(file.FilePath))
                return NotFound("File not found on server.");
            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(file.FilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                //return File(memory, "application/octet-stream", file.FileName);
                return File(memory, "application/octet-stream", file?.FileName ?? "unknown");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return StatusCode(500, "Internal server error during file download.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();
            try
            {
                System.IO.File.Delete(file.FilePath);
                await _fileRepository.DeleteFileAsync(id);
                Console.WriteLine($"File with ID {id} deleted by {User.Identity.Name}.");

                // Notify clients using SignalR
                await _hubContext.Clients.All.SendAsync("FileDeleted", file.FileName);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return StatusCode(500, "Internal server error during file deletion.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileModel>>> GetFiles(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var files = await _fileRepository.GetAllFilesAsync();
                var totalFiles = files.Count();
                Console.WriteLine($"Total files in database: {totalFiles}");
                var pagedFiles = files.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                Console.WriteLine($"Requested page {pageNumber} with size {pageSize}, returning {pagedFiles.Count} files");
                return Ok(pagedFiles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving files: {ex.Message}");
                return StatusCode(500, "Internal server error during file retrieval.");
            }
        }
    }
}
