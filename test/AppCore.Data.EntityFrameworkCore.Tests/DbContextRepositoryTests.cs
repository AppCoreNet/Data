// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AppCore.Data.EntityFrameworkCore;

public class DbContextRepositoryTests
{
    private static DbContextDataProvider<DefaultDataProvider, TestContext> CreateProvider()
    {
        return new DbContextDataProvider<DefaultDataProvider, TestContext>(
            new TestContext(),
            Substitute.For<ILoggerFactory>());
    }

    private static TestContextSimpleIdRepository CreateSimpleIdRepository(
        DbContextDataProvider<DefaultDataProvider, TestContext> provider, IDbContextQueryHandlerProvider<TestContext>? queryHandlerProvider = null)
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
                                Value = dbEntity.Value
                            };
                        });

        return new TestContextSimpleIdRepository(
            provider,
            queryHandlerProvider ?? Substitute.For<IDbContextQueryHandlerProvider<TestContext>>(),
            Substitute.For<ITokenGenerator>(),
            entityMapper,
            Substitute.For<ILogger>());
    }

    private static TestContextComplexIdRepository CreateComplexIdRepository(
        DbContextDataProvider<DefaultDataProvider, TestContext> provider)
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
                                Value = dbEntity.Value
                            };
                        });

        return new TestContextComplexIdRepository(
            provider,
            Substitute.For<IDbContextQueryHandlerProvider<TestContext>>(),
            Substitute.For<ITokenGenerator>(),
            entityMapper,
            Substitute.For<ILogger>());
    }

    private static TestContextChangeTokenRepository CreateChangeTokenRepository(
        DbContextDataProvider<DefaultDataProvider, TestContext> provider,
        ITokenGenerator generator)
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
                                ChangeToken = dbEntity.ChangeToken
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
                                ChangeToken = dbEntity.ChangeToken
                            };
                        });

        return new TestContextChangeTokenRepository(
            provider,
            Substitute.For<IDbContextQueryHandlerProvider<TestContext>>(),
            generator,
            entityMapper,
            Substitute.For<ILogger>());
    }

    private static TestContextChangeTokenExRepository CreateChangeTokenExRepository(
        DbContextDataProvider<DefaultDataProvider, TestContext> provider,
        ITokenGenerator generator)
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
                                ChangeToken = dbEntity.ChangeToken
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
                                ChangeToken = dbEntity.ChangeToken
                            };
                        });

        return new TestContextChangeTokenExRepository(
            provider,
            Substitute.For<IDbContextQueryHandlerProvider<TestContext>>(),
            generator,
            entityMapper,
            Substitute.For<ILogger>());
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
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();

        var dbEntity = new DbEntityWithSimpleId
        {
            Id = 1,
            Value = Guid.NewGuid().ToString()
        };

        TestContext testContext = provider.GetContext();
        testContext.SimpleEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        TestContextSimpleIdRepository repository = CreateSimpleIdRepository(provider);
        EntityWithSimpleId? result = await repository.FindAsync(dbEntity.Id, CancellationToken.None);

        result.Should()
              .BeEquivalentTo(dbEntity);
    }

    [Fact]
    public async Task FindAsyncLoadsEntityWithComplexId()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();

        var dbEntity = new DbEntityWithComplexId
        {
            Id = 1,
            Version = 1,
            Value = Guid.NewGuid().ToString()
        };

        TestContext testContext = provider.GetContext();
        testContext.ComplexEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        TestContextComplexIdRepository repository = CreateComplexIdRepository(provider);
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
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenRepository repository = CreateChangeTokenRepository(provider, generator);

        EntityWithChangeToken result = await repository.CreateAsync(
            new EntityWithChangeToken(),
            CancellationToken.None);

        result.ChangeToken.Should()
              .Be(changeToken);

        DbEntityWithChangeToken dbEntity =
            await provider.GetContext()
                          .ChangeTokenEntities.FirstAsync(e => e.Id == result.Id);

        dbEntity.ChangeToken.Should()
                .Be(changeToken);
    }

    [Fact]
    public async Task CreateInitializesChangeTokenEx()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(provider, generator);

        EntityWithChangeTokenEx result = await repository.CreateAsync(
            new EntityWithChangeTokenEx(),
            CancellationToken.None);

        result.ChangeToken.Should()
              .Be(changeToken);

        DbEntityWithChangeToken dbEntity =
            await provider.GetContext()
                          .ChangeTokenEntities.FirstAsync(e => e.Id == result.Id);

        dbEntity.ChangeToken.Should()
                .Be(changeToken);
    }

    [Fact]
    public async Task CreateUsesExplicitChangeToken()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(provider, Substitute.For<ITokenGenerator>());

        EntityWithChangeTokenEx result = await repository.CreateAsync(
            new EntityWithChangeTokenEx { ChangeToken = changeToken },
            CancellationToken.None);

        result.ChangeToken.Should()
              .Be(changeToken);

        DbEntityWithChangeToken dbEntity =
            await provider.GetContext()
                          .ChangeTokenEntities.FirstAsync(e => e.Id == result.Id);

        dbEntity.ChangeToken.Should()
                .Be(changeToken);
    }

    [Fact]
    public async Task UpdateSucceedsIfChangeTokenMatches()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContext testContext = provider.GetContext();
        var dbEntity = new DbEntityWithChangeToken() {ChangeToken = changeToken};
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        TestContextChangeTokenRepository repository = CreateChangeTokenRepository(provider, generator);
        await repository.UpdateAsync(
            new EntityWithChangeToken {Id = dbEntity.Id, ChangeToken = changeToken},
            CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSucceedsIfExpectedChangeTokenMatches()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContext testContext = provider.GetContext();
        var dbEntity = new DbEntityWithChangeToken() {ChangeToken = changeToken};
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(provider, generator);
        await repository.UpdateAsync(
            new EntityWithChangeTokenEx {Id = dbEntity.Id, ChangeToken = "abc", ExpectedChangeToken = changeToken },
            CancellationToken.None);
    }

    [Fact]
    public async Task UpdateThrowsIfChangeTokenDoesNotMatch()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContext testContext = provider.GetContext();
        var dbEntity = new DbEntityWithChangeToken() {ChangeToken = changeToken};
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        TestContextChangeTokenRepository repository = CreateChangeTokenRepository(provider, generator);

        await Assert.ThrowsAsync<EntityConcurrencyException>(
            () => repository.UpdateAsync(
                new EntityWithChangeToken {Id = dbEntity.Id, ChangeToken = "abc"},
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateThrowsIfExpectedChangeTokenDoesNotMatch()
    {
        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        string changeToken = Guid.NewGuid().ToString("N");
        ITokenGenerator generator = CreateTokenGenerator(changeToken);

        TestContext testContext = provider.GetContext();
        var dbEntity = new DbEntityWithChangeToken() {ChangeToken = changeToken};
        testContext.ChangeTokenEntities.Add(dbEntity);
        await testContext.SaveChangesAsync();

        TestContextChangeTokenExRepository repository = CreateChangeTokenExRepository(provider, generator);

        await Assert.ThrowsAsync<EntityConcurrencyException>(
            () => repository.UpdateAsync(
                new EntityWithChangeTokenEx {Id = dbEntity.Id, ExpectedChangeToken = "abc"},
                CancellationToken.None));
    }

    [Fact]
    public async Task QueryInvokesQueryHandler()
    {
        var queryHandler = Substitute.For<IDbContextQueryHandler<EntityWithSimpleId, EntityWithSimpleId, TestContext>>();
        queryHandler.QueryType.Returns(typeof(EntityWithSimpleIdByIdQuery));

        var queryHandlerProvider = Substitute.For<IDbContextQueryHandlerProvider<TestContext>>();
        queryHandlerProvider
            .GetHandler<EntityWithSimpleId, EntityWithSimpleId>(
                Arg.Is(typeof(EntityWithSimpleIdByIdQuery)))
            .Returns(queryHandler);

        var query = new EntityWithSimpleIdByIdQuery();

        DbContextDataProvider<DefaultDataProvider, TestContext> provider = CreateProvider();
        TestContextSimpleIdRepository repository = CreateSimpleIdRepository(provider, queryHandlerProvider);

        await repository.QueryAsync(query, CancellationToken.None);

        await queryHandler
              .Received(1)
              .ExecuteAsync(Arg.Is(query), Arg.Any<CancellationToken>());
    }
}