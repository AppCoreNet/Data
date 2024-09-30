// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppCoreNet.Data.MongoDB.Queries;
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace AppCoreNet.Data.MongoDB;

[Collection(MongoTestCollection.Name)]
[Trait("Category", "Integration")]
public class MongoRepositoryTests : RepositoryTests
{
    private const string DatabaseName = "test";

    private readonly MongoTestFixture _mongoTestFixture;

    public MongoRepositoryTests(MongoTestFixture mongoTestFixture)
    {
        _mongoTestFixture = mongoTestFixture;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        Mapper = EntityMapper.Instance;

        services.AddDataProvider(
            p =>
            {
                p.AddMongoDB(
                    ProviderName,
                    o =>
                    {
                        o.ClientSettings = MongoClientSettings.FromConnectionString(_mongoTestFixture.ConnectionString);
                        o.Database = DatabaseName;
                    })
                 .AddRepository<ITestEntityRepository, MongoTestEntityRepository>()
                 .AddQueryHandler<TestEntityByIdQueryHandler>()
                 .AddQueryHandler<TestEntity2ByIdQueryHandler>()
                 .AddRepository<ITestEntity2Repository, MongoTestEntity2Repository>();
            });
    }

    private async Task<TDao?> FindDataEntity<TDao, TEntity>(IDataProvider provider, Expression<Func<TDao, bool>> expression)
        where TEntity : IEntity
    {
        var mongo = (MongoDataProvider)provider;

        TDao document =
            await mongo.Database.GetCollection<TDao>(mongo.GetCollectionName<TEntity>())
                       .Find(expression)
                       .FirstOrDefaultAsync();

        return document;
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
            e => e.Id.Id == id.Id && e.Id.Version == id.Version);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, Entities.TestEntity entity)
    {
        DAO.TestEntity? document = await FindDataEntity(provider, entity.Id);

        document.Should()
                .NotBeNull();

        document.Should()
                .BeEquivalentTo(entity);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, Entities.TestEntity2 entity)
    {
        DAO.TestEntity2? document = await FindDataEntity(provider, entity.Id);

        document.Should()
                .NotBeNull();

        document.Should()
                .BeEquivalentTo(entity, o => o.ExcludingMissingMembers());
    }

    protected override async Task AssertNonExistingDataEntity(IDataProvider provider, Guid id)
    {
        DAO.TestEntity? document = await FindDataEntity(provider, id);

        document.Should()
                .BeNull();
    }

    protected override async Task<object> CreateDataEntity(IDataProvider provider, Entities.TestEntity entity)
    {
        var mongo = (MongoDataProvider)provider;

        IMongoCollection<DAO.TestEntity> collection =
            mongo.Database.GetCollection<DAO.TestEntity>(mongo.GetCollectionName<Entities.TestEntity>());

        var dataEntity = Mapper.Map<DAO.TestEntity>(entity);
        await collection.InsertOneAsync(dataEntity);
        return dataEntity;
    }

    protected override async Task<object> CreateDataEntity(IDataProvider provider, Entities.TestEntity2 entity)
    {
        var mongo = (MongoDataProvider)provider;

        IMongoCollection<DAO.TestEntity2> collection =
            mongo.Database.GetCollection<DAO.TestEntity2>(mongo.GetCollectionName<Entities.TestEntity2>());

        var dataEntity = Mapper.Map<DAO.TestEntity2>(entity);
        await collection.InsertOneAsync(dataEntity);
        return dataEntity;
    }
}