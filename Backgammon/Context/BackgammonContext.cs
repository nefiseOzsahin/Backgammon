using Backgammon.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backgammon.Context
{
    public class BackgammonContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public BackgammonContext(DbContextOptions<BackgammonContext> options) : base(options)
        {

        }

        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Pair> Pairs { get; set; }

        public DbSet<TournamentUser> TournamentUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
               .Property(u => u.City)
               .IsRequired(false);
            modelBuilder.Entity<AppUser>()
               .Property(u => u.Country)
               .IsRequired(false);
            modelBuilder.Entity<AppUser>()
               .Property(u => u.Club)
               .IsRequired(false);
            modelBuilder.Entity<AppUser>()
              .Property(u => u.Gender)
              .IsRequired(false);
            modelBuilder.Entity<AppUser>()
              .Property(u => u.ImagePath)
              .IsRequired(false);
            modelBuilder.Entity<AppUser>()
              .Property(u => u.Name)
              .IsRequired(false);
            modelBuilder.Entity<AppUser>()
              .Property(u => u.SurName)
              .IsRequired(false);
            modelBuilder.Entity<Tournament>()
              .HasMany(t => t.TournamentUsers)
              .WithOne(tu => tu.Tournament)
              .HasForeignKey(tu => tu.TournamentId);

        }
    }
}
