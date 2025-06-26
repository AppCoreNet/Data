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
    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="IMongoDatabase"/> used by the data provider.
    /// </summary>
    public IMongoDatabase Database { get; }

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/> of the data provider.
    /// </summary>
    public IEntityMapper EntityMapper { get; }

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/> of the data provider.
    /// </summary>
    public ITokenGenerator TokenGenerator { get; }

    /// <summary>
    /// Gets the <see cref="MongoTransactionManager"/> of the data provider.
    /// </summary>
    public MongoTransactionManager TransactionManager { get; }

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    /// <summary>
    /// Gets the <see cref="MongoQueryHandlerFactory"/> of the data provider.
    /// </summary>
    public MongoQueryHandlerFactory QueryHandlerFactory { get; }

    internal DataProviderLogger<MongoDataProvider> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDataProvider"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="database">The <see cref="IMongoDatabase"/>.</param>
    /// <param name="entityMapper">The <see cref="IEntityMapper"/>.</param>
    /// <param name="tokenGenerator">The <see cref="ITokenGenerator"/>.</param>
    /// <param name="queryHandlerFactory">The <see cref="MongoQueryHandlerFactory"/>.</param>
    /// <param name="transactionManager">The <see cref="MongoTransactionManager"/>.</param>
    /// <param name="logger">The <see cref="DataProviderLogger{T}"/>.</param>
    public MongoDataProvider(
        string name,
        IMongoDatabase database,
        IEntityMapper entityMapper,
        ITokenGenerator tokenGenerator,
        MongoQueryHandlerFactory queryHandlerFactory,
        MongoTransactionManager transactionManager,
        DataProviderLogger<MongoDataProvider> logger)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(database);
        Ensure.Arg.NotNull(entityMapper);
        Ensure.Arg.NotNull(tokenGenerator);
        Ensure.Arg.NotNull(queryHandlerFactory);
        Ensure.Arg.NotNull(transactionManager);
        Ensure.Arg.NotNull(logger);

        Name = name;
        Database = database;
        EntityMapper = entityMapper;
        TokenGenerator = tokenGenerator;
        QueryHandlerFactory = queryHandlerFactory;
        TransactionManager = transactionManager;
        Logger = logger;
    }

    internal string GetCollectionName<TDocument>()
    {
        return typeof(TDocument).Name;
    }
}