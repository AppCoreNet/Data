// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCoreNet.Data;

public class DataProviderRegistrationTests
{
    [Fact]
    public void RegistersMultipleProvidersOfSameType()
    {
        const string providerName1 = "provider-1";
        const string providerName2 = "provider-2";

        var services = new ServiceCollection();
        services.AddDataProvider(b =>
        {
            b.AddProvider<TestDataProvider>(providerName1, ServiceLifetime.Scoped);
            b.AddProvider<TestDataProvider>(providerName2, ServiceLifetime.Scoped);
        });

        using ServiceProvider sp = services.BuildServiceProvider();

        // providers should be registered with implementation type
        TestDataProvider[] dataProviders =
            sp.GetServices<TestDataProvider>()
              .ToArray();

        dataProviders.Should()
                     .HaveCount(2);

        IDataProvider provider1 = dataProviders[0];
        provider1.Name.Should()
                 .Be(providerName1);

        IDataProvider provider2 = dataProviders[1];
        provider2.Name.Should()
                 .Be(providerName2);

        // providers should also be registered using interface
        sp.GetServices<IDataProvider>()
          .Should()
          .BeEquivalentTo(dataProviders);
    }

    [Fact]
    public void RegisterDuplicateProviderWithDifferentTypeThrows()
    {
        const string providerName = "provider";

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(
            () =>
            {
                services.AddDataProvider(
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
        services.AddDataProvider(
            b =>
            {
                b.AddProvider<TestDataProvider>(providerName, ServiceLifetime.Scoped);
                b.AddProvider<TestDataProvider>(providerName, ServiceLifetime.Scoped);
            });

        using ServiceProvider sp = services.BuildServiceProvider();

        TestDataProvider[] providers =
            sp.GetServices<TestDataProvider>()
              .ToArray();

        providers.Should()
                 .ContainSingle();

        IEnumerable<IDataProvider> dataProviders = sp.GetServices<IDataProvider>();

        dataProviders.Should()
                     .BeEquivalentTo(new[] { providers[0] });
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void RegistersProviderAsKeyedServiceType()
    {
        const string providerName1 = "provider-1";
        const string providerName2 = "provider-2";

        var services = new ServiceCollection();
        services.AddDataProvider(b =>
        {
            b.AddProvider<TestDataProvider>(providerName1, ServiceLifetime.Scoped);
            b.AddProvider<TestDataProvider2>(providerName2, ServiceLifetime.Scoped);
        });

        using ServiceProvider sp = services.BuildServiceProvider();

        var provider1 = sp.GetKeyedService<IDataProvider>(providerName1);

        provider1.Should()
                 .BeOfType<TestDataProvider>();

        provider1!.Name.Should()
                 .Be(providerName1);

        var provider2 = sp.GetKeyedService<IDataProvider>(providerName2);

        provider2.Should()
                 .BeOfType<TestDataProvider2>();

        provider2!.Name.Should()
                  .Be(providerName2);
    }

    [Fact]
    public void RegistersProviderAsKeyedImplementation()
    {
        const string providerName1 = "provider-1";
        const string providerName2 = "provider-2";

        var services = new ServiceCollection();
        services.AddDataProvider(b =>
        {
            b.AddProvider<TestDataProvider>(providerName1, ServiceLifetime.Scoped);
            b.AddProvider<TestDataProvider2>(providerName2, ServiceLifetime.Scoped);
        });

        using ServiceProvider sp = services.BuildServiceProvider();

        var provider1 = sp.GetKeyedService<TestDataProvider>(providerName1);
        provider1!.Name.Should()
                  .Be(providerName1);

        var provider2 = sp.GetKeyedService<TestDataProvider2>(providerName2);
        provider2!.Name.Should()
                  .Be(providerName2);
    }
#endif
}