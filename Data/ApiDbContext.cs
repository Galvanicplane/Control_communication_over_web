using ControlOverWeb.Models; 
using Microsoft.EntityFrameworkCore; 

namespace ControlOverWeb.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)// baglanti hazilaniyor
        {
        }

        // --------------------------------
        public DbSet<Cihaz> Cihazlar { get; set; }//echo

        public DbSet<KomutKuyrugu> KomutKuyrugu { get; set; }//echo
    }
}