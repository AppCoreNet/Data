// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCoreNet.Data;

public class DataProviderResolverTests
{
    [Fact]
    public void ResolvesProviderByName()
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

        // resolve IDataProvider's explicitly
        var dataProviderResolver = sp.GetRequiredService<IDataProviderResolver>();

        IDataProvider provider1 = dataProviderResolver.Resolve(providerName1);
        provider1.Name.Should()
                 .Be(providerName1);

        IDataProvider provider2 = dataProviderResolver.Resolve(providerName2);
        provider2.Name.Should()
                 .Be(providerName2);
    }
}