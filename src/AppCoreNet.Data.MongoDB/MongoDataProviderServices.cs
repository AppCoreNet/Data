using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

public sealed class MongoDataProviderServices
{
    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/>.
    /// </summary>
    public IEntityMapper EntityMapper { get; }

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/>.
    /// </summary>
    public ITokenGenerator TokenGenerator { get; }

    /// <summary>
    /// Gets the <see cref="MongoQueryHandlerFactory"/>.
    /// </summary>
    public MongoQueryHandlerFactory QueryHandlerFactory { get; }

    /// <summary>
    /// Gets the <see cref="MongoTransactionManager"/>.
    /// </summary>
    public MongoTransactionManager TransactionManager { get; }

    internal MongoDataProviderServices(
        IMongoClient client,
        IMongoDatabase database,
        IEntityMapper entityMapper,
        ITokenGenerator tokenGenerator,
        MongoQueryHandlerFactory queryHandlerFactory,
        MongoTransactionManager transactionManager)
    {
        Client = client;
        Database = database;
        EntityMapper = entityMapper;
        TokenGenerator = tokenGenerator;
        QueryHandlerFactory = queryHandlerFactory;
        TransactionManager = transactionManager;
    }

    private static T GetOrCreateInstance<T>(IServiceProvider serviceProvider, Type? type)
        where T : notnull
    {
        return type != null
            ? (T)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type)
            : serviceProvider.GetRequiredService<T>();
    }

    internal static MongoDataProviderServices Create(string name, IServiceProvider serviceProvider)
    {
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<MongoDataProviderOptions>>();
        MongoDataProviderOptions options = optionsMonitor.Get(name);

        var entityMapper = GetOrCreateInstance<IEntityMapper>(serviceProvider, options.EntityMapperType);
        var tokenGenerator = GetOrCreateInstance<ITokenGenerator>(serviceProvider, options.TokenGeneratorType);

        var client = new MongoClient(options.ClientSettings);
        IMongoDatabase database = client.GetDatabase(options.Database);

        var queryHandlerFactory = new MongoQueryHandlerFactory(serviceProvider, options.QueryHandlerTypes);
        var transactionManager = new MongoTransactionManager(
            client,
            serviceProvider.GetRequiredService<ILogger<MongoTransactionManager>>());

        return new MongoDataProviderServices(
            client,
            database,
            entityMapper,
            tokenGenerator,
            queryHandlerFactory,
            transactionManager);
    }
}