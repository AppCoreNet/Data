using System;
using System.Threading.Tasks;
using AppCoreNet.Data.Entities;
using AppCoreNet.Data.Queries;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AppCoreNet.Data;

public abstract class RepositoryTests
{
    public const string ProviderName = "";

    private IEntityMapper? _mapper;
    private ITokenGenerator? _tokenGenerator;

    protected IEntityMapper Mapper
    {
        get { return _mapper ??= Substitute.For<IEntityMapper>(); }
        set => _mapper = value;
    }

    protected ITokenGenerator TokenGenerator
    {
        get => _tokenGenerator ??= Substitute.For<ITokenGenerator>();
        set => _tokenGenerator = value;
    }

    protected string GenerateChangeToken()
    {
        return Guid.NewGuid()
                   .ToString("N");
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        TokenGenerator.Generate()
                      .Returns(_ => GenerateChangeToken());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>())
                     .Returns(Substitute.For<ILogger>());

        services.TryAddSingleton(loggerFactory);
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.TryAddSingleton(_ => Mapper);
        services.TryAddSingleton(_ => TokenGenerator);
    }

    protected virtual ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    protected abstract Task AssertExistingDataEntity(IDataProvider provider, TestEntity entity);

    protected abstract Task AssertExistingDataEntity(IDataProvider provider, TestEntity2 entity);

    protected abstract Task AssertNonExistingDataEntity(IDataProvider provider, Guid id);

    protected abstract Task<object> CreateDataEntity(IDataProvider provider, TestEntity entity);

    protected abstract Task<object> CreateDataEntity(IDataProvider provider, TestEntity2 entity);

    [Fact]
    public async Task CreateAssignsId()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var entity = new TestEntity();

        TestEntity createdEntity = await repository.CreateAsync(entity);

        createdEntity.Should()
                     .NotBeSameAs(entity);

        createdEntity.Should()
                     .BeEquivalentTo(
                         entity,
                         o => o
                              .Excluding(e => e.Id)
                              .Excluding(e => e.ChangeToken));

        createdEntity.Id.Should()
                     .NotBe(default(Guid));
    }

    [Fact]
    public async Task CreateUsesProvidedId()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
        };

        TestEntity createdEntity = await repository.CreateAsync(entity);

        createdEntity.Id.Should()
                     .Be(id);
    }

    [Fact]
    public async Task CreateUsesProvidedComplexId()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntity2Repository>();

        var id = new ComplexId
        {
            Id = Guid.NewGuid(),
            Version = 1,
        };

        var entity = new TestEntity2
        {
            Id = id,
        };

        TestEntity2 createdEntity = await repository.CreateAsync(entity);

        createdEntity.Id.Should()
                     .BeEquivalentTo(id);
    }

    [Fact]
    public async Task CreateCreatesDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var entity = new TestEntity
        {
            Name = Guid.NewGuid().ToString("N"),
        };

        TestEntity createdEntity = await repository.CreateAsync(entity);

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        createdEntity.Should()
                     .NotBeSameAs(entity);

        await AssertExistingDataEntity(provider, createdEntity);
    }

    [Fact]
    public async Task CreateCreatesComplexIdDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntity2Repository>();

        var id = new ComplexId
        {
            Id = Guid.NewGuid(),
            Version = 1,
        };

        var entity = new TestEntity2
        {
            Id = id,
            Name = Guid.NewGuid().ToString("N"),
        };

        TestEntity2 createdEntity = await repository.CreateAsync(entity);

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        await AssertExistingDataEntity(provider, createdEntity);
    }

    [Fact]
    public async Task CreateAssignsChangeToken()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntityRepository>();

        string changeToken = GenerateChangeToken();

        TokenGenerator.Generate()
                      .Returns(changeToken);

        var entity = new TestEntity();

        TestEntity createdEntity = await repository.CreateAsync(entity);

        createdEntity.ChangeToken.Should()
                     .Be(changeToken);
    }

    [Fact]
    public async Task CreateUsesExplicitChangeToken()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntity2Repository>();

        string changeToken = GenerateChangeToken();

        var entity = new TestEntity2() { ChangeToken = changeToken };

        TestEntity2 createdEntity = await repository.CreateAsync(entity);

        createdEntity.ChangeToken.Should()
                     .Be(changeToken);

        createdEntity.ExpectedChangeToken.Should()
                     .Be(changeToken);
    }

    [Fact]
    public async Task FindLoadsDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();

        object dataEntity = await CreateDataEntity(
            provider,
            new TestEntity
            {
                Id = id,
                Name = Guid.NewGuid().ToString("N"),
            });

        TestEntity? entity = await repository.FindAsync(id);

        entity.Should()
              .NotBeNull();

        entity.Should()
              .BeEquivalentTo(dataEntity);
    }

    [Fact]
    public async Task FindReturnsNullForUnknownDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        TestEntity? entity = await repository.FindAsync(id);

        entity.Should()
              .BeNull();
    }

    [Fact]
    public async Task LoadLoadsDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();

        object dataEntity = await CreateDataEntity(
            provider,
            new TestEntity
            {
                Id = id,
                Name = Guid.NewGuid().ToString("N"),
            });

        TestEntity entity = await repository.LoadAsync(id);

        entity.Should()
              .NotBeNull();

        entity.Should()
              .BeEquivalentTo(dataEntity);
    }

    [Fact]
    public async Task LoadThrowsForUnknownDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        async Task<TestEntity> Action() => await repository.LoadAsync(id);

        await Assert.ThrowsAsync<EntityNotFoundException>(Action);
    }

    [Fact]
    public async Task DeleteRemovesDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
            Name = Guid.NewGuid().ToString("N"),
        };

        await CreateDataEntity(provider, entity);

        await repository.DeleteAsync(entity);

        await AssertNonExistingDataEntity(provider, id);
    }

    [Fact]
    public async Task DeleteThrowsForUnknownDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
        };

        async Task Action() => await repository.DeleteAsync(entity);

        await Assert.ThrowsAsync<EntityConcurrencyException>(Action);
    }

    [Fact]
    public async Task UpdateModifiesExistingDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
        };

        await CreateDataEntity(provider, entity);

        string newName = Guid.NewGuid().ToString("N");
        entity.Name = newName;

        TestEntity updatedEntity = await repository.UpdateAsync(entity);

        updatedEntity.Should()
                     .BeEquivalentTo(entity, o => o.Excluding(e => e.ChangeToken));

        updatedEntity.Should()
                     .NotBeSameAs(entity);

        await AssertExistingDataEntity(provider, updatedEntity);
    }

    [Fact]
    public async Task UpdateThrowsForNonExistentDataEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();
        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
        };

        async Task Action() => await repository.UpdateAsync(entity);

        await Assert.ThrowsAsync<EntityConcurrencyException>(Action);
    }

    [Fact]
    public async Task UpdateModifiesDataEntityIfChangeTokenMatches()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        string changeToken = GenerateChangeToken();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
            ChangeToken = changeToken,
        };

        await CreateDataEntity(provider, entity);

        TestEntity updatedEntity = await repository.UpdateAsync(entity);

        updatedEntity.Should()
                     .BeEquivalentTo(entity, o => o.Excluding(e => e.ChangeToken));

        updatedEntity.ChangeToken
                     .Should()
                     .NotBe(changeToken);
    }

    [Fact]
    public async Task UpdateThrowsIfChangeTokenDoesNotMatch()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        string changeToken = GenerateChangeToken();
        var entity = new TestEntity
        {
            Id = id,
            ChangeToken = changeToken,
        };

        await CreateDataEntity(provider, entity);

        entity.ChangeToken = GenerateChangeToken();

        async Task Action() => await repository.UpdateAsync(entity);

        await Assert.ThrowsAsync<EntityConcurrencyException>(Action);
    }

    [Fact]
    public async Task UpdateModifiesDataEntityIfExpectedChangeTokenMatches()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntity2Repository>();

        var id = Guid.NewGuid();
        string changeToken = GenerateChangeToken();
        var entity = new TestEntity2
        {
            Id = new ComplexId { Id = id, Version = 0 },
            ChangeToken = changeToken,
        };

        await CreateDataEntity(provider, entity);

        entity.ExpectedChangeToken = changeToken;

        TestEntity2 updatedEntity = await repository.UpdateAsync(entity);

        updatedEntity.Should()
                     .BeEquivalentTo(
                         entity,
                         o => o.Excluding(e => e.ChangeToken)
                               .Excluding(e => e.ExpectedChangeToken));

        updatedEntity.ChangeToken
                     .Should()
                     .NotBe(changeToken);
    }

    [Fact]
    public async Task UpdateThrowsIfExpectedChangeTokenDoesNotMatch()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntity2Repository>();

        var id = Guid.NewGuid();
        string changeToken = GenerateChangeToken();
        var entity = new TestEntity2
        {
            Id = new ComplexId { Id = id, Version = 0 },
            ChangeToken = changeToken,
        };

        await CreateDataEntity(provider, entity);

        entity.ExpectedChangeToken = GenerateChangeToken();

        async Task Action() => await repository.UpdateAsync(entity);

        await Assert.ThrowsAsync<EntityConcurrencyException>(Action);
    }

    [Fact]
    public async Task QueryByIdReturnsEntity()
    {
        await using ServiceProvider sp = CreateServiceProvider();

        IDataProvider provider = sp.GetRequiredService<IDataProviderResolver>()
                                   .Resolve(ProviderName);

        var repository = sp.GetRequiredService<ITestEntityRepository>();

        var id = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = id,
            Name = "name",
        };

        await CreateDataEntity(provider, entity);

        TestEntity? result = await repository.QueryAsync(new TestEntityByIdQuery(id));

        result.Should()
              .NotBeNull();

        result.Should()
              .BeEquivalentTo(entity);
    }
}