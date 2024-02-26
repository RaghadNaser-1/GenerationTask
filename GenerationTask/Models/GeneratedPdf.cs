namespace GenerationTask.Models
{
    public class GeneratedPdf
    {
        public int Id { get; set; } // Assuming you have an ID property
        public string FileName { get; set; }
        public string Content { get; set; } // Text content
        public byte[] PdfFile { get; set; } // PDF file bytes
        public string FilePath { get; set; } // Relative path to the document

        public DateTime CreatedAt { get; set; }
    }

}
