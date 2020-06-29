// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AppCore.Data
{
    public class RepositoryExtensionsTests
    {
        [Fact]
        public async Task LoadAsyncCallsFind()
        {
            var repository = Substitute.For<IRepository<int, EntityWithSimpleId>>();
            var entity = new EntityWithSimpleId();

            repository.FindAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                      .Returns(entity);

            int id = new Random(777).Next();
            var ct = new CancellationToken();

            await repository.LoadAsync(id, ct);

            await repository.Received(1)
                            .FindAsync(id, ct);
        }

        [Fact]
        public async Task LoadAsyncThrowsExceptionIfNotFound()
        {
            var repository = Substitute.For<IRepository<int, EntityWithSimpleId>>();

            int id = new Random(777).Next();
            Func<Task> loadAction = () => repository.LoadAsync(id, CancellationToken.None);

            var error = await loadAction.Should()
                                        .ThrowAsync<EntityNotFoundException>();

            error.Where(e => (int) e.Id == id && e.EntityType == typeof(EntityWithSimpleId));
        }
    }
}