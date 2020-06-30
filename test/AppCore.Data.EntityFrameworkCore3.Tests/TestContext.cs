using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContext : DbContext
    {
        public DbSet<DbEntityWithSimpleId> SimpleEntities => Set<DbEntityWithSimpleId>();

        public DbSet<DbEntityWithComplexId> ComplexEntities => Set<DbEntityWithComplexId>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("DbContextRepositoryTests");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DbEntityWithComplexId>()
                        .HasKey(e => new {e.Id, e.Version});
        }
    }
}