using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.DTOs;

namespace Trainingsmanager.Database
{
    public class Context : DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscription>()
        .HasKey(s => s.Id);

            // Remove User relationship entirely — only store UserName (no FK)
            modelBuilder.Entity<Subscription>()
                .Property(s => s.UserName)
                .IsRequired();

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Session)
                .WithMany(sess => sess.Subscriptions)
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint based on UserName + SessionId
            modelBuilder.Entity<Subscription>()
                .HasIndex(s => new { s.UserName, s.SessionId })
                .IsUnique();

            modelBuilder.Entity<Session>()
                .HasOne(s => s.CreatedBy)
                .WithMany(u => u.CreatedSessions)
                .HasForeignKey(s => s.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
