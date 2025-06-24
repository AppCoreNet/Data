// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppCoreNet.Data.EntityFrameworkCore.Queries;
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextRepositoryTests : RepositoryTests
{
    private const string DatabaseName = "test";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        Mapper = EntityMapper.Instance;

        services.AddDataProvider(
            p =>
            {
                p.AddEntityFrameworkCore<TestContext>(
                    ProviderName,
                    o =>
                    {
                        o.UseInMemoryDatabase(DatabaseName);
                    })
                 .AddRepository<ITestEntityRepository, DbContextTestEntityRepository>()
                 .AddQueryHandler<TestEntityByIdQueryHandler>()
                 .AddQueryHandler<TestEntity2ByIdQueryHandler>()
                 .AddRepository<ITestEntity2Repository, DbContextTestEntity2Repository>();
            });
    }

    private async Task<TDao?> FindDataEntity<TDao, TEntity>(IDataProvider provider, Expression<Func<TDao, bool>> expression)
        where TDao : class
        where TEntity : IEntity
    {
        var dbContextDataProvider = (DbContextDataProvider<TestContext>)provider;

        TDao? dao =
            await dbContextDataProvider.DbContext.Set<TDao>()
                                       .AsNoTracking()
                                       .Where(expression)
                                       .FirstOrDefaultAsync();

        return dao;
    }

    private async Task<DAO.TestEntity?> FindDataEntity(IDataProvider provider, Guid id)
    {
        return await FindDataEntity<DAO.TestEntity, Entities.TestEntity>(
            provider,
            e => e.Id == id);
    }

    private async Task<DAO.TestEntity2?> FindDataEntity(IDataProvider provider, Entities.ComplexId id)
    {
        return await FindDataEntity<DAO.TestEntity2, Entities.TestEntity2>(
            provider,
            e => e.Id == id.Id && e.Version == id.Version);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, Entities.TestEntity entity)
    {
        DAO.TestEntity? dao = await FindDataEntity(provider, entity.Id);

        dao.Should()
           .NotBeNull();

        dao.Should()
           .BeEquivalentTo(entity);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, Entities.TestEntity2 entity)
    {
        DAO.TestEntity2? dao = await FindDataEntity(provider, entity.Id);

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
        DAO.TestEntity? dao = await FindDataEntity(provider, id);

        dao.Should()
           .BeNull();
    }

    private async Task CreateDataEntity<TDao>(IDataProvider provider, TDao dataEntity)
        where TDao : class
    {
        var dbContextDataProvider = (DbContextDataProvider<TestContext>)provider;
        TestContext dbContext = dbContextDataProvider.DbContext;

        dbContext.Set<TDao>()
                 .Add(dataEntity);

        await dbContext.SaveChangesAsync();

        EntityEntry[] entries = dbContext.ChangeTracker.Entries()
                                         .ToArray();

        foreach (EntityEntry entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    protected override async Task<object> CreateDataEntity(IDataProvider provider, Entities.TestEntity entity)
    {
        var dataEntity = Mapper.Map<DAO.TestEntity>(entity);
        await CreateDataEntity(provider, dataEntity);
        return dataEntity;
    }

    protected override async Task<object> CreateDataEntity(IDataProvider provider, Entities.TestEntity2 entity)
    {
        var dataEntity = Mapper.Map<DAO.TestEntity2>(entity);
        await CreateDataEntity(provider, dataEntity);
        return dataEntity;
    }
}