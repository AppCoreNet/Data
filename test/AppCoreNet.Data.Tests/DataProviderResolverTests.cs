// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCoreNet.Data;

public class DataProviderResolverTests
{
    private class TestDataProvider : IDataProvider
    {
        public string Name { get; }

        public ITransactionManager? TransactionManager => null;

        public TestDataProvider(string name)
        {
            Name = name;
        }
    }

    private class TestDataProvider2 : IDataProvider
    {
        public string Name { get; }

        public ITransactionManager? TransactionManager => null;

        public TestDataProvider2(string name)
        {
            Name = name;
        }
    }

    [Fact]
    public void RegistersDefaultDataProvider()
    {
        var services = new ServiceCollection();
        services.AddDataProviders(b => b.AddProvider<TestDataProvider>(string.Empty, ServiceLifetime.Scoped));

        using ServiceProvider sp = services.BuildServiceProvider();

        var dataProvider = sp.GetService<IDataProvider>();

        dataProvider.Should()
                    .NotBeNull();

        dataProvider.Should()
                    .BeOfType<TestDataProvider>();

        dataProvider!.Name.Should()
                     .BeEmpty();
    }

    [Fact]
    public void DoesNotRegisterNonDefaultDataProvider()
    {
        var services = new ServiceCollection();
        services.AddDataProviders(b => b.AddProvider<TestDataProvider>("provider-1", ServiceLifetime.Scoped));

        using ServiceProvider sp = services.BuildServiceProvider();

        var dataProvider = sp.GetService<IDataProvider>();

        dataProvider.Should()
                    .BeNull();
    }

    [Fact]
    public void RegistersMultipleProvidersOfSameType()
    {
        const string providerName1 = "provider-1";
        const string providerName2 = "provider-2";

        var services = new ServiceCollection();
        services.AddDataProviders(b =>
        {
            b.AddProvider<TestDataProvider>(providerName1, ServiceLifetime.Scoped);
            b.AddProvider<TestDataProvider>(providerName2, ServiceLifetime.Scoped);
        });

        using ServiceProvider sp = services.BuildServiceProvider();

        var dataProviderResolver = sp.GetRequiredService<IDataProviderResolver>();

        IDataProvider provider1 = dataProviderResolver.Resolve(providerName1);
        provider1.Name.Should()
                 .Be(providerName1);

        IDataProvider provider2 = dataProviderResolver.Resolve(providerName2);
        provider2.Name.Should()
                 .Be(providerName2);
    }

    [Fact]
    public void RegisterDuplicateProviderWithDifferentTypeThrows()
    {
        const string providerName = "provider";

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(
            () =>
            {
                services.AddDataProviders(
                    b =>
                    {
                        b.AddProvider<TestDataProvider>(providerName, ServiceLifetime.Scoped);
                        b.AddProvider<TestDataProvider2>(providerName, ServiceLifetime.Scoped);
                    });
            });
    }

    [Fact]
    public void RegisterDuplicateProviderWithSameTypeRegistersProviderOnlyOnce()
    {
        const string providerName = "provider";

        var services = new ServiceCollection();
        services.AddDataProviders(
            b =>
            {
                b.AddProvider<TestDataProvider>(providerName, ServiceLifetime.Scoped);
                b.AddProvider<TestDataProvider>(providerName, ServiceLifetime.Scoped);
            });

        using ServiceProvider sp = services.BuildServiceProvider();

        IEnumerable<TestDataProvider> providers =
            sp.GetServices<TestDataProvider>();

        providers.Should()
                 .ContainSingle();
    }
}