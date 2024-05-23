using ContactsApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace ContactsApp.Data
{
    public class ContactsAppDbContext : DbContext
    {
        public ContactsAppDbContext(DbContextOptions<ContactsAppDbContext> options) : base(options)
        {
        
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(e => e.Contacts)
                .WithOne().HasForeignKey(c => c.userId);
        }

        public DbSet<Contact>? Contacts { get; set; }
        public DbSet<User> Users { get;set; }
    }
}
