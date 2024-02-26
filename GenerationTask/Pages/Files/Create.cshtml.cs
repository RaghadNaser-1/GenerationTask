using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GenerationTask.Data;
using GenerationTask.Models;

namespace GenerationTask.Pages.Files
{
    public class CreateModel : PageModel
    {
        private readonly GenerationTask.Data.ApplicationDbContext _context;

        public CreateModel(GenerationTask.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public GeneratedPdf GeneratedPdf { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.GeneratedPdfs.Add(GeneratedPdf);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
