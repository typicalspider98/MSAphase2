namespace backend.Models
{
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Uploader { get; set; }
        public DateTime UploadedAt { get; set; }
        public long Size { get; set; }
    }
}
