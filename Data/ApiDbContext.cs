using ControlOverWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlOverWeb.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        public DbSet<Receiver> Receivers { get; set; }
        public DbSet<Sender> Senders { get; set; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<SiteStats> SiteStats { get; set; }
        public DbSet<ConnectionHistory> ConnectionHistory { get; set; }
    }
}