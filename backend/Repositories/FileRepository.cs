using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;

using System.IO;
using FileModel = backend.Models.File;

namespace backend.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly ApplicationDbContext _context;

        public FileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FileModel>> GetAllFilesAsync()
        {
            return await _context.Files.ToListAsync();
        }

        public async Task<FileModel> GetFileByIdAsync(int id)
        {
            return await _context.Files.FindAsync(id);
        }

        public async Task AddFileAsync(FileModel file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFileAsync(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file != null)
            {
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
            }
        }
    }
}
