// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data.Entity;
using AppCoreNet.Data.EntityFramework.DAO; // Using our new DAO entities

namespace AppCoreNet.Data.EntityFramework;

public class TestDbContext : DbContext
{
    public TestDbContext(string connectionString)
        : base(connectionString)
    {
    }

    public TestDbContext()
        : base(Effort.DbConnectionFactory.CreateTransient(), true) // Default constructor for Effort
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<TestEntity2> TestEntities2 { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>().ToTable("TestEntities");
        modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
        modelBuilder.Entity<TestEntity>().Property(p => p.ChangeToken).IsConcurrencyToken();


        modelBuilder.Entity<TestEntity2>().ToTable("TestEntities2");
        modelBuilder.Entity<TestEntity2>().HasKey(e => e.Id);
        modelBuilder.Entity<TestEntity2>().Property(p => p.ChangeToken).IsConcurrencyToken();
        // If Version is intended as a row version / timestamp for EF6:
        // modelBuilder.Entity<TestEntity2>().Property(p => p.Version).IsRowVersion(); 
        // However, the type is int, EF6 rowversion is typically byte[].
        // If it's just a regular property to be used as a concurrency token, it needs to be handled by IHasChangeToken/IHasChangeTokenEx.
        // For now, I will treat `ChangeToken` as the primary concurrency mechanism as per TestEntity.
    }
}
