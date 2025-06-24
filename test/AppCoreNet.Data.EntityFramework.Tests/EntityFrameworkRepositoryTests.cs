// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppCoreNet.Data.EntityFramework.DAO;
using AppCoreNet.Data.EntityFramework.Queries;
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCoreNet.Data.EntityFramework;

public class EntityFrameworkRepositoryTests : RepositoryTests
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        Mapper = EntityMapper.Instance;

        services.AddDataProvider(
            p =>
            {
                // TDbContext (TestDbContext) is registered with DI here by the AddEntityFramework call
                // if a factory is provided. For EF6, it's common to new it up or have a factory.
                // The AddEntityFramework method will resolve TDbContext if it's registered.
                // For Effort, TestDbContext has a parameterless constructor that uses Effort.
                services.AddScoped<TestDbContext>(); // Register TestDbContext for Effort

                p.AddEntityFramework<TestDbContext>(ProviderName)
                 .AddRepository<ITestEntityRepository, EntityFrameworkTestEntityRepository>()
                 .AddRepository<ITestEntity2Repository, EntityFrameworkTestEntity2Repository>()
                 .AddQueryHandler<TestEntityByIdQueryHandler>()
                 .AddQueryHandler<TestEntity2ByIdQueryHandler>();
            });
    }

    private async Task<TDao?> FindDataEntity<TDao, TEntity>(IDataProvider provider, Expression<Func<TDao, bool>> expression)
        where TDao : class
        where TEntity : IEntity
    {
        var dbContextDataProvider = (EntityFrameworkDataProvider<TestDbContext>)provider;

        TDao? dao =
            await dbContextDataProvider.DbContext.Set<TDao>()
                                       .AsNoTracking()
                                       .Where(expression)
                                       .FirstOrDefaultAsync();

        return dao;
    }

    private async Task<DAO.TestDao?> FindDataEntity(IDataProvider provider, Guid id)
    {
        return await FindDataEntity<DAO.TestDao, Entities.TestEntity>(
            provider,
            e => e.Id == id);
    }

    private async Task<DAO.TestDao2?> FindDataEntity(IDataProvider provider, Entities.ComplexId id)
    {
        return await FindDataEntity<DAO.TestDao2, Entities.TestEntity2>(
            provider,
            e => e.Id == id.Id && e.Version == id.Version);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, Entities.TestEntity entity)
    {
        DAO.TestDao? dao = await FindDataEntity(provider, entity.Id);

        dao.Should()
           .NotBeNull();

        dao.Should()
           .BeEquivalentTo(entity);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, Entities.TestEntity2 entity)
    {
        DAO.TestDao2? dao = await FindDataEntity(provider, entity.Id);

        dao.Should()
           .NotBeNull();

        dao.Should()
           .BeEquivalentTo(
               entity,
               o => o.Excluding(e => e.Id)
                     .ExcludingMissingMembers());

        dao!.Id.Should()
            .Be(entity.Id.Id);

        dao.Version.Should()
           .Be(entity.Id.Version);
    }

    protected override async Task AssertNonExistingDataEntity(IDataProvider provider, Guid id)
    {
        DAO.TestDao? dao = await FindDataEntity(provider, id);

        dao.Should()
           .BeNull();
    }

    private async Task CreateDataEntity<TDao>(IDataProvider provider, TDao dataEntity)
        where TDao : class
    {
        var dbContextDataProvider = (EntityFrameworkDataProvider<TestDbContext>)provider;
        TestDbContext dbContext = dbContextDataProvider.DbContext;

        dbContext.Set<TDao>()
                 .Add(dataEntity);

        await dbContext.SaveChangesAsync();

        DbEntityEntry[] entries = dbContext.ChangeTracker.Entries()
                                         .ToArray();

        foreach (DbEntityEntry entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    protected override async Task<object> CreateDataEntity(IDataProvider provider, Entities.TestEntity entity)
    {
        var dataEntity = Mapper.Map<DAO.TestDao>(entity);
        await CreateDataEntity(provider, dataEntity);
        return dataEntity;
    }

    protected override async Task<object> CreateDataEntity(IDataProvider provider, Entities.TestEntity2 entity)
    {
        var dataEntity = Mapper.Map<DAO.TestDao2>(entity);
        await CreateDataEntity(provider, dataEntity);
        return dataEntity;
    }

    [Fact(Skip = "Not supported by EF6")]
    public override Task CreateAssignsId()
    {
        return base.CreateAssignsId();
    }
}
