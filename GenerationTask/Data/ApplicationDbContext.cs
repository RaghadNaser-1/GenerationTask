using GenerationTask.Models;
using Microsoft.EntityFrameworkCore;

namespace GenerationTask.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<GeneratedPdf> GeneratedPdfs { get; set; }
    }
}
