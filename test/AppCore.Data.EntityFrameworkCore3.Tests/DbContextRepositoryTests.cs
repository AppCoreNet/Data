// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Logging;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AppCore.Data.EntityFrameworkCore
{
    public class DbContextRepositoryTests
    {
        private static DbContextDataProvider<DefaultDataProvider, TestContext> CreateProvider()
        {
            return new DbContextDataProvider<DefaultDataProvider, TestContext>(
                new TestContext(),
                Substitute.For<ILogger<DbContextDataProvider>>());
        }

        private static TestContextSimpleIdRepository CreateSimpleIdRepository(
            DbContextDataProvider<DefaultDataProvider, TestContext> provider)
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
                Substitute.For<ITokenGenerator>(),
                entityMapper,
                Substitute.For<ILogger>());
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
            EntityWithSimpleId result = await repository.FindAsync(dbEntity.Id, CancellationToken.None);

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
            EntityWithComplexId result = await repository.FindAsync(
                new VersionId(dbEntity.Id, dbEntity.Version),
                CancellationToken.None);

            result.Id.Should()
                  .Be(new VersionId(dbEntity.Id, dbEntity.Version));

            result.Should()
                  .BeEquivalentTo(result);
        }
    }
}
