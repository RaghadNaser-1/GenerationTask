using GenerationTask.Data;
using GenerationTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace GenerationTask.Pages
{
    public class ModifyFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public ModifyFormModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
        }

        [BindProperty]
        public GeneratedPdf GeneratedPdf { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GeneratedPdf = await _context.GeneratedPdfs.FirstOrDefaultAsync(m => m.Id == id);

            if (GeneratedPdf == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var pdfToUpdate = await _context.GeneratedPdfs.FindAsync(GeneratedPdf.Id);

            if (pdfToUpdate == null)
            {
                return NotFound();
            }

            pdfToUpdate.FileName = GeneratedPdf.FileName;
            pdfToUpdate.Content = GeneratedPdf.Content;

            // Here, you'd generate the new PDF file from the updated content
            // This example assumes you have a method to generate the PDF bytes
            pdfToUpdate.PdfFile = GeneratePdfFromContent(GeneratedPdf.Content);

            _context.Attach(pdfToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.GeneratedPdfs.Any(e => e.Id == pdfToUpdate.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index"); // Adjust redirect as necessary
        }

        private byte[] GeneratePdfFromContent(string text)
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

