using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Models;

namespace MPB_PORTAL_WEB_APP.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CountyActivity> Activities { get; set; }  // Activities DbSet
        public DbSet<Report> Reports { get; set; }             // Reports DbSet
        public DbSet<Cases> Cases { get; set; }                // Cases DbSet
        public DbSet<Pages> Pages { get; set; }                // Added Pages DbSet
    }
}
