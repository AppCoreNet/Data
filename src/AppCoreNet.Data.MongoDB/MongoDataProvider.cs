// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Diagnostics;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Represents a MongoDB data provider.
/// </summary>
public sealed class MongoDataProvider : IDataProvider
{
    private readonly MongoDataProviderServices _services;
    private readonly string _name;

    /// <inheritdoc />
    public string Name => _name;

    /// <summary>
    /// Gets the <see cref="IMongoClient"/> used by the data provider.
    /// </summary>
    public IMongoClient Client => _services.Client;

    /// <summary>
    /// Gets the <see cref="IMongoDatabase"/> used by the data provider.
    /// </summary>
    public IMongoDatabase Database => _services.Database;

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/> of the data provider.
    /// </summary>
    public IEntityMapper EntityMapper => _services.EntityMapper;

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/> of the data provider.
    /// </summary>
    public ITokenGenerator TokenGenerator => _services.TokenGenerator;

    /// <summary>
    /// Gets the <see cref="MongoTransactionManager"/> of the data provider.
    /// </summary>
    public MongoTransactionManager TransactionManager => _services.TransactionManager;

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    /// <summary>
    /// Gets the <see cref="MongoQueryHandlerFactory"/> of the data provider.
    /// </summary>
    public MongoQueryHandlerFactory QueryHandlerFactory => _services.QueryHandlerFactory;

    internal DataProviderLogger<MongoDataProvider> Logger => _services.Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDataProvider"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="MongoDataProviderServices"/>.</param>
    public MongoDataProvider(string name, MongoDataProviderServices services)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(services);

        _services = services;
        _name = name;
    }

    internal string GetCollectionName<TDocument>()
    {
        return typeof(TDocument).Name;
    }
}