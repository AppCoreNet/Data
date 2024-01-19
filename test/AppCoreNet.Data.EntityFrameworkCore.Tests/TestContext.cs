// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.EntityFrameworkCore;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class TestContext : DbContext
{
    public DbSet<DbEntityWithSimpleId> SimpleEntities => Set<DbEntityWithSimpleId>();

    public DbSet<DbEntityWithComplexId> ComplexEntities => Set<DbEntityWithComplexId>();

    public DbSet<DbEntityWithChangeToken> ChangeTokenEntities => Set<DbEntityWithChangeToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("DbContextRepositoryTests");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DbEntityWithComplexId>()
                    .HasKey(e => new {e.Id, e.Version});

        modelBuilder.Entity<DbEntityWithChangeToken>()
                    .Property(p => p.ChangeToken)
                    .IsConcurrencyToken();
    }
}