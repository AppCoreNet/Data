// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.EntityFrameworkCore;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class TestContext : DbContext
{
    public DbSet<DbEntityWithSimpleId> SimpleEntities => Set<DbEntityWithSimpleId>();

    public DbSet<DbEntityWithComplexId> ComplexEntities => Set<DbEntityWithComplexId>();

    public DbSet<DbEntityWithChangeToken> ChangeTokenEntities => Set<DbEntityWithChangeToken>();

    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DAO.TestEntity>()
                    .HasKey(e => e.Id);

        modelBuilder.Entity<DAO.TestEntity>()
                    .Property(e => e.ChangeToken)
                    .IsConcurrencyToken();

        modelBuilder.Entity<DAO.TestEntity2>()
                    .HasKey(e => new { e.Id, e.Version });

        modelBuilder.Entity<DAO.TestEntity2>()
                    .Property(e => e.ChangeToken)
                    .IsConcurrencyToken();
    }
}