// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextRepositoryTestsBak
{
    private static DbContextDataProvider<TestContext> CreateProvider(
        string name = "",
        IEntityMapper? entityMapper = null,
        ITokenGenerator? tokenGenerator = null,
        DbContextQueryHandlerFactory<TestContext>? queryHandlerProvider = null)
    {
        var dbContext = new TestContext(null!);

        queryHandlerProvider ??= new DbContextQueryHandlerFactory<TestContext>(
            Substitute.For<IServiceProvider>(),
            Array.Empty<Type>());

        var transactionManager = new DbContextTransactionManager(
            dbContext,
            Substitute.For<ILogger<DbContextTransactionManager>>());

        var services = new DbContextDataProviderServices<TestContext>(
            dbContext,
            entityMapper ?? Substitute.For<IEntityMapper>(),
            tokenGenerator ?? Substitute.For<ITokenGenerator>(),
            queryHandlerProvider,
            transactionManager,
            Substitute.For<ILogger>());

        return new DbContextDataProvider<TestContext>(name, services);
    }

    private static TestContextSimpleIdRepository CreateSimpleIdRepository(DbContextQueryHandlerFactory<TestContext>? queryHandlerProvider = null)
    {
        var entityMapper = Substitute.For<IEntityMapper>();
        entityMapper.Map<EntityWithSimpleId>(Arg.Any<DbEntityWithSimpleId>())
                    .Returns(
                        ci =>
                        {
                            var dbEntity = ci.ArgAt<DbEntityWithSimpleId>(0);
                            return new EntityWithSimpleId
                            {
                                Id = dbEntity.Id,
                                Value = dbEntity.Value,
                            };
                        });

        DbContextDataProvider<TestContext> provider = CreateProvider(
            entityMapper: entityMapper,
            queryHandlerProvider: queryHandlerProvider);

        return new TestContextSimpleIdRepository(provider, Substitute.For<ILogger<TestContextSimpleIdRepository>>());
    }

    private static TestContextComplexIdRepository CreateComplexIdRepository()
    {
        var entityMapper = Substitute.For<IEntityMapper>();
        entityMapper.Map<EntityWithComplexId>(Arg.Any<DbEntityWithComplexId>())
                    .Returns(
                        ci =>
                        {
                            var dbEntity = ci.ArgAt<DbEntityWithComplexId>(0);
                            return new EntityWithComplexId
                            {
                                Id = new VersionId(dbEntity.Id, dbEntity.Version),
                                Value = dbEntity.Value,
                            };
                        });

        DbContextDataProvider<TestContext> provider = CreateProvider(entityMapper: entityMapper);
        return new TestContextComplexIdRepository(provider, Substitute.For<ILogger<TestContextComplexIdRepository>>());
    }

    private static TestContextChangeTokenRepository CreateChangeTokenRepository(ITokenGenerator? generator = null)
    {
        var entityMapper = Substitute.For<IEntityMapper>();
        entityMapper.Map<EntityWithChangeToken>(Arg.Any<DbEntityWithChangeToken>())
                    .Returns(
                        ci =>
                        {
                            var dbEntity = ci.ArgAt<DbEntityWithChangeToken>(0);
                            return new EntityWithChangeToken
                            {
                                Id = dbEntity.Id,
                                ChangeToken = dbEntity.ChangeToken,
                            };
                        });

        entityMapper.Map<DbEntityWithChangeToken>(Arg.Any<EntityWithChangeToken>())
                    .Returns(
                        ci =>
                        {
                            var dbEntity = ci.ArgAt<EntityWithChangeToken>(0);
                            return new DbEntityWithChangeToken
                            {
                                Id = dbEntity.Id,
                                ChangeToken = dbEntity.ChangeToken,
                            };
                        });

        generator ??= Substitute.For<ITokenGenerator>();

        DbContextDataProvider<TestContext> provider =
            CreateProvider(entityMapper: entityMapper, tokenGenerator: generator);

        return new TestContextChangeTokenRepository(
            provider,
            Substitute.For<ILogger<TestContextChangeTokenRepository>>());
    }

    private static TestContextChangeTokenExRepository CreateChangeTokenExRepository(ITokenGenerator? generator = null)
    {
        var entityMapper = Substitute.For<IEntityMapper>();
        entityMapper.Map<EntityWithChangeTokenEx>(Arg.Any<DbEntityWithChangeToken>())
                    .Returns(
                        ci =>
                        {
                            var dbEntity = ci.ArgAt<DbEntityWithChangeToken>(0);
                            return new EntityWithChangeTokenEx
                            {
                                Id = dbEntity.Id,
                                ChangeToken = dbEntity.ChangeToken,
                            };
                        });

        entityMapper.Map<DbEntityWithChangeToken>(Arg.Any<EntityWithChangeTokenEx>())
                    .Returns(
                        ci =>
                        {
                            var dbEntity = ci.ArgAt<EntityWithChangeTokenEx>(0);
                            return new DbEntityWithChangeToken
                            {
                                Id = dbEntity.Id,
                                ChangeToken = dbEntity.ChangeToken,
                            };
                        });

        generator ??= Substitute.For<ITokenGenerator>();

        DbContextDataProvider<TestContext> provider =
            CreateProvider(entityMapper: entityMapper, tokenGenerator: generator);

        return new TestContextChangeTokenExRepository(
            provider,
            Substitute.For<ILogger<TestContextChangeTokenExRepository>>());
    }

    private static ITokenGenerator CreateTokenGenerator(string changeToken)
    {
        var generator = Substitute.For<ITokenGenerator>();
        generator.Generate()
                 .Returns(changeToken);

        return generator;
    }

    [Fact]
    public async Task FindAsyncLoadsEntityWithSimpleId()
    {
        var dbEntity = new DbEntityWithSimpleId
        {
            Id = 1,
            Value = Guid.NewGuid().ToString(),
        };

        TestContextSimpleIdRepository repository = CreateSimpleIdRepository();

        TestContext testContext = repository.Provider.DbContext;
        testContext.SimpleEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        EntityWithSimpleId? result = await repository.FindAsync(dbEntity.Id, CancellationToken.None);

        result.Should()
              .BeEquivalentTo(dbEntity);
    }

    [Fact]
    public async Task FindAsyncLoadsEntityWithComplexId()
    {
        var dbEntity = new DbEntityWithComplexId
        {
            Id = 1,
            Version = 1,
            Value = Guid.NewGuid().ToString(),
        };

        TestContextComplexIdRepository repository = CreateComplexIdRepository();

        TestContext testContext = repository.Provider.DbContext;
        testContext.ComplexEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        EntityWithComplexId? result = await repository.FindAsync(
            new VersionId(dbEntity.Id, dbEntity.Version),
            CancellationToken.None);

        result.Should()
              .NotBeNull();

        result!.Id.Should()
               .Be(new VersionId(dbEntity.Id, dbEntity.Version));

        result.Should()
              .BeEquivalentTo(result);
    }

    [Fact]
    public async Task CreateInitializesChangeToken()
    {
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenRepository repository = CreateChangeTokenRepository(generator);

        EntityWithChangeToken result = await repository.CreateAsync(
            new EntityWithChangeToken(),
            CancellationToken.None);

        result.ChangeToken.Should()
              .Be(changeToken);

        DbEntityWithChangeToken dbEntity =
            await repository.Provider.DbContext
                            .ChangeTokenEntities.FirstAsync(e => e.Id == result.Id);

        dbEntity.ChangeToken.Should()
                .Be(changeToken);
    }

    [Fact]
    public async Task CreateInitializesChangeTokenEx()
    {
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(generator);

        EntityWithChangeTokenEx result = await repository.CreateAsync(
            new EntityWithChangeTokenEx(),
            CancellationToken.None);

        result.ChangeToken.Should()
              .Be(changeToken);

        DbEntityWithChangeToken dbEntity =
            await repository.Provider.DbContext
                            .ChangeTokenEntities.FirstAsync(e => e.Id == result.Id);

        dbEntity.ChangeToken.Should()
                .Be(changeToken);
    }

    [Fact]
    public async Task CreateUsesExplicitChangeToken()
    {
        string changeToken = Guid.NewGuid().ToString("N");

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository();

        EntityWithChangeTokenEx result = await repository.CreateAsync(
            new EntityWithChangeTokenEx { ChangeToken = changeToken },
            CancellationToken.None);

        result.ChangeToken.Should()
              .Be(changeToken);

        DbEntityWithChangeToken dbEntity =
            await repository.Provider.DbContext
                            .ChangeTokenEntities.FirstAsync(e => e.Id == result.Id);

        dbEntity.ChangeToken.Should()
                .Be(changeToken);
    }

    [Fact]
    public async Task UpdateSucceedsIfChangeTokenMatches()
    {
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenRepository repository = CreateChangeTokenRepository(generator);

        TestContext testContext = repository.Provider.DbContext;
        var dbEntity = new DbEntityWithChangeToken() { ChangeToken = changeToken };
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        await repository.UpdateAsync(
            new EntityWithChangeToken { Id = dbEntity.Id, ChangeToken = changeToken },
            CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSucceedsIfExpectedChangeTokenMatches()
    {
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(generator);

        TestContext testContext = repository.Provider.DbContext;
        var dbEntity = new DbEntityWithChangeToken() { ChangeToken = changeToken };
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        await repository.UpdateAsync(
            new EntityWithChangeTokenEx { Id = dbEntity.Id, ChangeToken = "abc", ExpectedChangeToken = changeToken },
            CancellationToken.None);
    }

    [Fact]
    public async Task UpdateThrowsIfChangeTokenDoesNotMatch()
    {
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenRepository repository = CreateChangeTokenRepository(generator);

        TestContext testContext = repository.Provider.DbContext;
        var dbEntity = new DbEntityWithChangeToken() { ChangeToken = changeToken };
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        await Assert.ThrowsAsync<EntityConcurrencyException>(
            () => repository.UpdateAsync(
                new EntityWithChangeToken { Id = dbEntity.Id, ChangeToken = "abc" },
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateThrowsIfExpectedChangeTokenDoesNotMatch()
    {
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(generator);

        TestContext testContext = repository.Provider.DbContext;
        var dbEntity = new DbEntityWithChangeToken() { ChangeToken = changeToken };
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        await Assert.ThrowsAsync<EntityConcurrencyException>(
            () => repository.UpdateAsync(
                new EntityWithChangeTokenEx { Id = dbEntity.Id, ExpectedChangeToken = "abc" },
                CancellationToken.None));
    }

    [Fact]
    public async Task QueryInvokesQueryHandler()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ILogger<EntityWithSimpleIdByIdQueryHandler>))
                       .Returns(Substitute.For<ILogger<EntityWithSimpleIdByIdQueryHandler>>());

        var queryHandlerProvider = new DbContextQueryHandlerFactory<TestContext>(
            serviceProvider,
            new[] { typeof(EntityWithSimpleIdByIdQueryHandler) });

        var query = new EntityWithSimpleIdByIdQuery();

        TestContextSimpleIdRepository repository = CreateSimpleIdRepository(queryHandlerProvider: queryHandlerProvider);

        await repository.QueryAsync(query, CancellationToken.None);

        EntityWithSimpleIdByIdQueryHandler.ExecutedQueries.Should()
                                          .Contain(query)
                                          .And.ContainSingle();
    }
}