// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace AppCoreNet.Data.EntityFramework.DAO;

public class TestDbContext : DbContext
{
    public TestDbContext(string connectionString)
        : base(connectionString)
    {
    }

    public TestDbContext()
        : base(Effort.DbConnectionFactory.CreateTransient(), true)
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DAO.TestDao>().ToTable("TestEntities");
        modelBuilder.Entity<DAO.TestDao>().HasKey(e => e.Id);
        modelBuilder.Entity<DAO.TestDao>().Property(p => p.ChangeToken).IsConcurrencyToken();

        modelBuilder.Entity<DAO.TestDao2>().ToTable("TestEntities2");
        modelBuilder.Entity<DAO.TestDao2>().HasKey(e => new { e.Id, e.Version });
        modelBuilder.Entity<DAO.TestDao2>().Property(p => p.ChangeToken).IsConcurrencyToken();
    }
}
