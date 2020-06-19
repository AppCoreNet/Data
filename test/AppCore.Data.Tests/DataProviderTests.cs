// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AppCore.Data
{
    public class DataProviderTests
    {
        private static IDataProvider CreateProvider()
        {
            var provider = Substitute.For<IDataProvider>();
            provider.Name.Returns(typeof(DefaultDataProvider).FullName);
            return provider;
        }

        [Fact]
        public void ThrowsForUnknownProvider()
        {
            Assert.Throws<InvalidOperationException>(
                () => new DataProvider<DefaultDataProvider>(Enumerable.Empty<IDataProvider>()));
        }

        [Fact]
        public void ResolvesProviderByTypeName()
        {
            IDataProvider provider = CreateProvider();

            var providerWithTag = new DataProvider<DefaultDataProvider>(new[] {provider});
            providerWithTag.Provider.Should()
                           .BeSameAs(provider);
        }

        [Fact]
        public void BeginChangeScopeInvokesProvider()
        {
            IDataProvider provider = CreateProvider();
            var disposable = Substitute.For<IDisposable>();
            var action = Substitute.For<Action>();

            provider.BeginChangeScope(Arg.Any<Action>())
                    .Returns(disposable);

            var providerWithTag = new DataProvider<DefaultDataProvider>(new[] {provider});

            IDisposable result = providerWithTag.BeginChangeScope(action);

            provider.Received(1)
                    .BeginChangeScope(action);

            result.Should()
                  .BeSameAs(disposable);
        }

        [Fact]
        public async Task SaveChangesInvokesProvider()
        {
            IDataProvider provider = CreateProvider();
            var providerWithTag = new DataProvider<DefaultDataProvider>(new[] {provider});

            var ct = new CancellationToken();
            await providerWithTag.SaveChangesAsync(ct);

            await provider.Received(1)
                          .SaveChangesAsync(ct);
        }
    }
}
