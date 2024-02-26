using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GenerationTask.Data;
using GenerationTask.Models;

namespace GenerationTask.Pages.Files
{
    public class DetailsModel : PageModel
    {
        private readonly GenerationTask.Data.ApplicationDbContext _context;

        public DetailsModel(GenerationTask.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public GeneratedPdf GeneratedPdf { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generatedpdf = await _context.GeneratedPdfs.FirstOrDefaultAsync(m => m.Id == id);
            if (generatedpdf == null)
            {
                return NotFound();
            }
            else
            {
                GeneratedPdf = generatedpdf;
            }
            return Page();
        }
    }
}
