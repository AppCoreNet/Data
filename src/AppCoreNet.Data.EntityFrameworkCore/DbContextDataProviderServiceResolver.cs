// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppCoreNet.Data.EntityFrameworkCore;

internal sealed class DbContextDataProviderServiceResolver<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<DbContextDataProviderOptions> _options;

    public DbContextDataProviderServiceResolver(
        IServiceProvider serviceProvider,
        IOptionsMonitor<DbContextDataProviderOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public DbContextDataProviderServices<TDbContext> Get(string name)
    {
        DbContextDataProviderOptions o = _options.Get(name);

        IEntityMapper entityMapper = o.EntityMapperType != null
            ? (IEntityMapper)ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, o.EntityMapperType)
            : _serviceProvider.GetRequiredService<IEntityMapper>();

        ITokenGenerator tokenGenerator = o.TokenGeneratorType != null
            ? (ITokenGenerator)ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, o.TokenGeneratorType)
            : _serviceProvider.GetRequiredService<ITokenGenerator>();

        var dbContext = _serviceProvider.GetRequiredService<TDbContext>();
        var queryHandlerFactory = new DbContextQueryHandlerFactory<TDbContext>(_serviceProvider, o.QueryHandlerTypes);
        var transactionManager = new DbContextTransactionManager(
            dbContext,
            _serviceProvider.GetRequiredService<ILogger<DbContextTransactionManager>>());

        return new DbContextDataProviderServices<TDbContext>(
            dbContext,
            entityMapper,
            tokenGenerator,
            queryHandlerFactory,
            transactionManager
        );
    }
}