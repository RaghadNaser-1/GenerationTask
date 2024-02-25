using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace GenerationTask.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        

        public string GeneratedResult { get; set; }
        [BindProperty]
        public string DocumentType { get;  set; }
        [BindProperty]
        public string Name { get;  set; }
        [BindProperty]
        public string ExtraDetails { get;  set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var openai = new OpenAIAPI("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");

            string prompt = $"Hi ChatGPT, Please help me to write {DocumentType} for {Name}, keep in mind {ExtraDetails}";

            CompletionRequest completionRequest = new CompletionRequest();
            completionRequest.Prompt = prompt;
            completionRequest.Model = "gpt-3.5-turbo-instruct"; // or other models as needed
            completionRequest.MaxTokens = 500;

            var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

            GeneratedResult = "";
            foreach (var completion in completions.Completions)
            {
                GeneratedResult = completion.Text;
            }
            TempData["GeneratedResult"] = GeneratedResult;

            return Page();
        }

        public async Task<IActionResult> OnPostDownloadPdfAsync()
        {
            if (TempData["GeneratedResult"] is string generatedResult)
            {
                // Generate PDF from the retrieved generated result
                byte[] pdfBytes = GeneratePdfFromText(generatedResult);
                // Use the local variable, not the property

                // Set the file content type
                string contentType = "application/pdf";

                // Return the PDF as a file download
                return File(pdfBytes, contentType, "generated_document.pdf");
            }
            else
            {
                // Handle the case where there is no generated result
                // This could redirect to another page with an error message or simply return to the current page with a message.
                TempData["ErrorMessage"] = "No content was generated to create a PDF.";
                return RedirectToPage(); // Or however you prefer to handle this scenario.
            }
        }


        private byte[] GeneratePdfFromText(string text)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Arial", 15);

                // Define padding and calculate content area
                double padding = 10;
                double contentWidth = page.Width - (2 * padding); // Adjust width for padding
                double x = padding;
                double y = padding;

                // Split the text into words
                var words = text.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var line = string.Empty;

                foreach (var word in words)
                {
                    var testLine = line + (line == string.Empty ? "" : " ") + word;
                    var testLineWidth = gfx.MeasureString(testLine, font).Width;

                    if (testLineWidth > contentWidth)
                    {
                        // Draw the current line because the next word would exceed the content width
                        gfx.DrawString(line, font, XBrushes.Black, new XRect(x, y, contentWidth, page.Height - (2 * padding)), XStringFormats.TopLeft);
                        // Prepare the next line
                        line = word;
                        y += font.GetHeight(); // Move to the next line
                                               // Check if we need a new page
                        if (y > page.Height - padding - font.GetHeight())
                        {
                            page = pdf.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            y = padding; // Reset y position
                        }
                    }
                    else
                    {
                        // Word fits; add it to the line
                        line = testLine;
                    }
                }

                // Draw any remaining text
                if (!string.IsNullOrEmpty(line))
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(x, y, contentWidth, page.Height - (2 * padding)), XStringFormats.TopLeft);
                }

                pdf.Save(stream, false);
                return stream.ToArray();
            }
        }




    }
}
