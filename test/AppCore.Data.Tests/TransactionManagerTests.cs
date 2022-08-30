// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AppCore.Data;

public class TransactionManagerTests
{
    private static ITransactionManager CreateManager()
    {
        var manager = Substitute.For<ITransactionManager>();
        manager.Provider.Name.Returns(typeof(DefaultDataProvider).FullName);
        return manager;
    }

    [Fact]
    public void ThrowsForUnknownManager()
    {
        Assert.Throws<InvalidOperationException>(
            () => new TransactionManager<DefaultDataProvider>(Enumerable.Empty<ITransactionManager>()));
    }

    [Fact]
    public void ResolvesManagerByTypeName()
    {
        ITransactionManager manager = CreateManager();

        var managerWithTag = new TransactionManager<DefaultDataProvider>(new[] {manager});
        managerWithTag.Manager.Should()
                      .BeSameAs(manager);
    }
}