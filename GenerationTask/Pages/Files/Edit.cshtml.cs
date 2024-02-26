using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GenerationTask.Data;
using GenerationTask.Models;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace GenerationTask.Pages.Files
{
    public class EditModel : PageModel
    {
        private readonly GenerationTask.Data.ApplicationDbContext _context;

        public EditModel(GenerationTask.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GeneratedPdf GeneratedPdf { get; set; } = default!;
        [BindProperty]
        public IFormFile PdfFileUpload { get; set; } // Property to handle the file upload

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generatedpdf =  await _context.GeneratedPdfs.FirstOrDefaultAsync(m => m.Id == id);
            if (generatedpdf == null)
            {
                return NotFound();
            }
            GeneratedPdf = generatedpdf;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
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

            if (PdfFileUpload != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await PdfFileUpload.CopyToAsync(memoryStream);

                    // Update the PdfFile property with the new byte array
                    pdfToUpdate.PdfFile = memoryStream.ToArray();
                }
            }

            // Update other properties as needed
            pdfToUpdate.FileName = GeneratedPdf.FileName;
            pdfToUpdate.Content = GeneratedPdf.Content;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeneratedPdfExists(GeneratedPdf.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }


        private bool GeneratedPdfExists(int id)
        {
            return _context.GeneratedPdfs.Any(e => e.Id == id);
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
