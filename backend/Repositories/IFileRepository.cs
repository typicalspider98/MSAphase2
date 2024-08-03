using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using FileModel = backend.Models.File;

namespace backend.Repositories
{
    public interface IFileRepository
    {
        Task<IEnumerable<FileModel>> GetAllFilesAsync();
        Task<FileModel> GetFileByIdAsync(int id);
        Task AddFileAsync(FileModel file);
        Task DeleteFileAsync(int id);
    }
}
