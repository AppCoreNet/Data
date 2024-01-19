// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using AppCoreNet.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextDataProviderBuilderTest
{
    [Fact]
    public void RegistersDefaultDataProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IEntityMapper>());
        services.AddDataProviders(b => b.AddDbContext<TestContext>());

        IServiceProvider sp = services.BuildServiceProvider();

        var dataProvider = sp.GetService<IDataProvider>();

        dataProvider.Should()
                    .NotBeNull();

        dataProvider.Should()
                    .BeOfType<DbContextDataProvider<TestContext>>();
    }

    [Fact]
    public void DoesNotRegisterNonDefaultDataProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IEntityMapper>());
        services.AddDataProviders(b => b.AddDbContext<TestContext>("my-provider"));

        IServiceProvider sp = services.BuildServiceProvider();

        var dataProvider = sp.GetService<IDataProvider>();

        dataProvider.Should()
                    .BeNull();
    }

    [Fact]
    public void RegistersMultipleProvidersOfSameType()
    {
        const string providerName1 = "my-provider";
        const string providerName2 = "my-provider-2";

        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IEntityMapper>());
        services.AddDataProviders(b =>
        {
            b.AddDbContext<TestContext>(providerName1);
            b.AddDbContext<TestContext>(providerName2);
        });

        IServiceProvider sp = services.BuildServiceProvider();

        sp.GetService<TestContext>()
          .Should()
          .NotBeNull();

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
        const string providerName = "my-provider";

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(
            () =>
            {
                services.AddDataProviders(
                    b =>
                    {
                        b.AddDbContext<TestContext>(providerName);
                        b.AddDbContext<TestContext2>(providerName);
                    });
            });
    }

    [Fact]
    public void RegisterDuplicateProviderWithSameTypeRegistersProviderOnlyOnce()
    {
        const string providerName = "my-provider";

        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IEntityMapper>());
        services.AddDataProviders(
            b =>
            {
                b.AddDbContext<TestContext>(providerName);
                b.AddDbContext<TestContext>(providerName);
            });

        IServiceProvider sp = services.BuildServiceProvider();

        IEnumerable<DbContextDataProvider<TestContext>> providers =
            sp.GetServices<DbContextDataProvider<TestContext>>();

        providers.Should()
                 .ContainSingle();
    }

    [Fact]
    public void RegistersRepositoryWithDataProvider()
    {
        const string providerName1 = "my-provider";
        const string providerName2 = "my-provider-2";

        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IEntityMapper>());
        services.AddDataProviders(b =>
        {
            b.AddDbContext<TestContext>(providerName1)
             .AddRepository<TestContextSimpleIdRepository>();

            b.AddDbContext<TestContext>(providerName2)
             .AddRepository<TestContextComplexIdRepository>();
        });

        IServiceProvider sp = services.BuildServiceProvider();

        var repository1 = sp.GetService<TestContextSimpleIdRepository>();
        repository1.Should()
                  .NotBeNull();

        repository1!.Provider.Name.Should()
                   .Be(providerName1);

        var repository2 = sp.GetService<TestContextComplexIdRepository>();
        repository2.Should()
                   .NotBeNull();

        repository2!.Provider.Name.Should()
                    .Be(providerName2);

    }
}