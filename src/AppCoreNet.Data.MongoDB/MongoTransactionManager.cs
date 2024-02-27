// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
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
    private readonly AsyncLocal<MongoTransaction?> _currentTransaction = new ();
    private readonly DataProviderLogger<MongoDataProvider> _logger;

    /// <summary>
    /// Gets the currently active transaction.
    /// </summary>
    public MongoTransaction? CurrentTransaction
    {
        get => _currentTransaction.Value;
        private set => _currentTransaction.Value = value;
    }

    ITransaction? ITransactionManager.CurrentTransaction => CurrentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoTransactionManager"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IMongoClient"/>.</param>
    /// <param name="logger">The logger.</param>
    public MongoTransactionManager(IMongoClient client, DataProviderLogger<MongoDataProvider> logger)
    {
        Ensure.Arg.NotNull(client);
        Ensure.Arg.NotNull(logger);

        _client = client;
        _logger = logger;
    }

    private void OnTransactionFinished(object? sender, EventArgs args)
    {
        var transaction = (MongoTransaction)sender!;
        transaction.TransactionFinished -= OnTransactionFinished;
        CurrentTransaction = null;
    }

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="options">Specifies the options for the transaction.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The created transaction.</returns>
    public async Task<ITransaction> BeginTransactionAsync(
        TransactionOptions? options,
        CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        IClientSessionHandle session =
            await _client
                  .StartSessionAsync(null, cancellationToken)
                  .ConfigureAwait(false);

        MongoTransaction transaction;
        try
        {
            session.StartTransaction(options);
            transaction = new MongoTransaction(session, _logger);
            transaction.TransactionFinished += OnTransactionFinished;
        }
        catch
        {
            session.Dispose();
            throw;
        }

        return CurrentTransaction = transaction;
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await BeginTransactionAsync(null, cancellationToken);
    }

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="options">Specifies the options for the transaction.</param>
    /// <returns>The created transaction.</returns>
    public ITransaction BeginTransaction(TransactionOptions? options)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        IClientSessionHandle session = _client.StartSession();
        MongoTransaction transaction;
        try
        {
            session.StartTransaction(options);
            transaction = new MongoTransaction(session, _logger);
            transaction.TransactionFinished += OnTransactionFinished;
        }
        catch
        {
            session.Dispose();
            throw;
        }

        return CurrentTransaction = transaction;
    }

    /// <inheritdoc />
    public ITransaction BeginTransaction()
    {
        return BeginTransaction(null);
    }
}