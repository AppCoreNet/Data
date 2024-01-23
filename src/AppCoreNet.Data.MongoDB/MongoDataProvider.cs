using System;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

public sealed class MongoDataProvider : IDataProvider
{
    /// <inheritdoc />
    public string Name { get; }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/> of the data provider.
    /// </summary>
    public IEntityMapper EntityMapper { get; }

    /// <summary>
    /// Gets the <see cref="MongoTransactionManager"/> of the data provider.
    /// </summary>
    public MongoTransactionManager TransactionManager { get; }

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    /// <summary>
    /// Gets the <see cref="MongoQueryHandlerFactory"/> of the data provider.
    /// </summary>
    public MongoQueryHandlerFactory QueryHandlerFactory { get; }

    public MongoDataProvider(string name, MongoDataProviderServices services)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(services);

        Name = name;
        Client = services.Client;
        Database = services.Database;
        EntityMapper = services.EntityMapper;
        TransactionManager = services.TransactionManager;
        QueryHandlerFactory = services.QueryHandlerFactory;
    }

    public IDisposable BeginChangeScope(Action? afterSaveCallback = null)
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}