// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppCoreNet.Data.EntityFramework.DAO;
using AppCoreNet.Data.SpecificationTests; // For RepositoryTests base and domain entities
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCoreNet.Data.EntityFramework;

// Define a simple repository interface for testing purposes
public interface ITestEntityRepository : IRepository<Guid, SpecificationTests.Entities.TestEntity>
{
}

// Define a simple repository implementation for testing purposes
public class EntityFrameworkTestEntityRepository : EntityFrameworkRepository<Guid, SpecificationTests.Entities.TestEntity, TestDbContext, DAO.TestEntity>, ITestEntityRepository
{
    public EntityFrameworkTestEntityRepository(EntityFrameworkDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }

    // Example of how to map domain entity to DB entity if they were different
    // For this basic test, they are directly mapped by TestEntityMapper if names match
    // or if TEntity and TDbEntity are the same.
    // If more complex mapping is needed, this is where it would be customized.
}


public class EntityFrameworkRepositoryTests : RepositoryTests
{
    private const string ProviderName = "ef6-test";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Use the simple TestEntityMapper
        services.AddSingleton<IEntityMapper, TestEntityMapper>();

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
                 .AddEntityMapper<TestEntityMapper>(); // Register the specific mapper for this provider
                 // .AddQueryHandler<...>() // Add query handlers if needed for more complex tests
            });
    }

    // Helper to find data directly from DbContext for assertion
    private async Task<DAO.TestEntity?> FindDataEntity(IDataProvider provider, Guid id)
    {
        var efDataProvider = (EntityFrameworkDataProvider<TestDbContext>)provider;
        return await efDataProvider.DbContext.Set<DAO.TestEntity>()
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(e => e.Id == id);
    }

    protected override async Task AssertExistingDataEntity(IDataProvider provider, SpecificationTests.Entities.TestEntity entity)
    {
        DAO.TestEntity? dao = await FindDataEntity(provider, entity.Id);

        dao.Should().NotBeNull();
        // Assuming TestEntityMapper maps properties correctly or they are the same
        dao!.Id.Should().Be(entity.Id);
        dao.Name.Should().Be(entity.Name);
        // dao.ChangeToken.Should().Be(entity.ChangeToken); // If domain entity had ChangeToken
    }

    protected override async Task AssertNonExistingDataEntity(IDataProvider provider, Guid id)
    {
        DAO.TestEntity? dao = await FindDataEntity(provider, id);
        dao.Should().BeNull();
    }

    // CreateDataEntity and AssertExistingDataEntity for TestEntity2 would be similar if needed

    // This test is adapted from the base class or typical repository tests.
    // The base RepositoryTests class in SpecificationTests should provide the actual [Fact] methods.
    // This class just provides the EF6 specific setup and assertions.

    // Example of a local test if not using the base class structure:
    [Fact]
    public async Task CreateAndFindAsync_Should_PersistAndRetrieveEntity()
    {
        // Arrange
        var newEntity = new SpecificationTests.Entities.TestEntity(Guid.NewGuid(), "Test Name 1");
        IRepository<Guid, SpecificationTests.Entities.TestEntity> repository =
            ServiceProvider.GetRequiredService<IDataProviderResolver>()
                           .Resolve(ProviderName)
                           .GetRepository<IRepository<Guid, SpecificationTests.Entities.TestEntity>>();

        // Act
        await repository.CreateAsync(newEntity, default);

        // Assert
        SpecificationTests.Entities.TestEntity? foundEntity = await repository.FindAsync(newEntity.Id, default);
        foundEntity.Should().NotBeNull();
        foundEntity!.Id.Should().Be(newEntity.Id);
        foundEntity.Name.Should().Be(newEntity.Name);

        // Assert against DB directly
        await AssertExistingDataEntity(repository.Provider, newEntity);
    }
}
