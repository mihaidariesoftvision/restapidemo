namespace Library.API.Entities
{
    using Microsoft.EntityFrameworkCore;

    public sealed class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions options)
            : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                modelBuilder.Entity<Author>()
                    .Property(c => c.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<Book>()
                    .Property(c => c.Id)
                    .ValueGeneratedNever();
            }
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}