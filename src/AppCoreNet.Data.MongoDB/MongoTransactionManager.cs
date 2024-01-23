using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides a transaction manager for MongoDB.
/// </summary>
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP003:Dispose previous before re-assigning",
    Justification = "Pre-condition is that no transaction is active.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP006:Implement IDisposable",
    Justification = "Transaction must be disposed by consumer.")]
public sealed class MongoTransactionManager : ITransactionManager
{
    private readonly IMongoClient _client;

    /// <summary>
    /// Gets the currently active transaction.
    /// </summary>
    public MongoTransaction? CurrentTransaction { get; private set; }

    ITransaction? ITransactionManager.CurrentTransaction => CurrentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoTransactionManager"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IMongoClient"/>.</param>
    /// <param name="logger">The logger.</param>
    public MongoTransactionManager(IMongoClient client, ILogger<MongoTransactionManager> logger)
    {
        Ensure.Arg.NotNull(client);
        Ensure.Arg.NotNull(logger);

        _client = client;
    }

    private ClientSessionOptions CreateSessionOptions(IsolationLevel isolationLevel)
    {
        var options = new ClientSessionOptions();
        return options;
    }

    private void OnTransactionFinished(object sender, EventArgs args)
    {
        var transaction = (MongoTransaction)sender;
        transaction.TransactionFinished -= OnTransactionFinished;
        CurrentTransaction = null;
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        ClientSessionOptions options = CreateSessionOptions(isolationLevel);
        MongoTransaction transaction = await MongoTransaction
                                             .CreateAsync(_client, options, cancellationToken)
                                             .ConfigureAwait(false);

        transaction.TransactionFinished += OnTransactionFinished;
        return CurrentTransaction = transaction;
    }

    /// <inheritdoc />
    public ITransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        ClientSessionOptions options = CreateSessionOptions(isolationLevel);
        var transaction = MongoTransaction.Create(_client, options);
        transaction.TransactionFinished += OnTransactionFinished;
        return CurrentTransaction = transaction;
    }
}