using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public class IndexModel : PageModel
    {
        private readonly GenerationTask.Data.ApplicationDbContext _context;

        public IndexModel(GenerationTask.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<GeneratedPdf> GeneratedPdf { get;set; } = default!;

        public async Task OnGetAsync()
        {
            GeneratedPdf = await _context.GeneratedPdfs.ToListAsync();
        }

        public async Task<IActionResult> GetPdf(int id)
        {
            var pdfFile = await _context.GeneratedPdfs.FindAsync(id);
            if (pdfFile == null)
            {
                return NotFound();
            }

            return File(pdfFile.PdfFile, "application/pdf", pdfFile.FileName);
        }

    }
}
