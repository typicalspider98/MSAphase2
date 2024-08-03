using System.ComponentModel.DataAnnotations.Schema;
namespace backend.Models
{
    [Table("Files")]
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Uploader { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public long Size { get; set; }
    }
}
