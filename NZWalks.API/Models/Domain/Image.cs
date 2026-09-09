namespace NZWalks.API.Models.Domain
{
    public class Image : BaseEntity
    {
        // Inherits Id from BaseEntity

        public string? FileDescription { get; set; }

        public required string FileName { get; set; }

        public required string FileExtension { get; set; }

        public long FileSizeInBytes { get; set; }

        public string FilePath { get; set; } = string.Empty;
    }
}
