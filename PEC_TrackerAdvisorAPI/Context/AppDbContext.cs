using Microsoft.EntityFrameworkCore;
using PEC_TrackerAdvisorAPI.Models;

namespace PEC_TrackerAdvisorAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Device>().ToTable("devices");
        }
    }
}
