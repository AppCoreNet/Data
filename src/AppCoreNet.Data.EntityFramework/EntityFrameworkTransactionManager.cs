// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Data;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Provides a transaction manager using a <see cref="DbContext"/>.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP003:Dispose previous before re-assigning",
    Justification = "Pre-condition is that no transaction is active.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP006:Implement IDisposable",
    Justification = "Transaction must be disposed by consumer.")]
public sealed class EntityFrameworkTransactionManager<TDbContext> : ITransactionManager
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> _logger;
    private EntityFrameworkTransaction<TDbContext>? _currentTransaction;

    /// <summary>
    /// Gets the currently active <see cref="EntityFrameworkTransaction{TDbContext}"/>.
    /// </summary>
    public EntityFrameworkTransaction<TDbContext>? CurrentTransaction => _currentTransaction;

    ITransaction? ITransactionManager.CurrentTransaction => CurrentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFrameworkTransactionManager{TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public EntityFrameworkTransactionManager(
        TDbContext dbContext,
        DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _logger = logger;
    }

    private void OnTransactionFinished(object? sender, EventArgs args)
    {
        var transaction = (EntityFrameworkTransaction<TDbContext>)sender!;
        transaction.TransactionFinished -= OnTransactionFinished;
        _currentTransaction = null;
    }

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level of the transaction.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The created transaction.</returns>
    public Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(BeginTransaction(isolationLevel));
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level of the transaction.</param>
    /// <returns>The created transaction.</returns>
    public ITransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        DbContextTransaction transaction = _dbContext.Database.BeginTransaction(isolationLevel);

        try
        {
            var t = new EntityFrameworkTransaction<TDbContext>(_dbContext, transaction, _logger);
            t.TransactionFinished += OnTransactionFinished;
            return _currentTransaction = t;
        }
        catch
        {
            transaction.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public ITransaction BeginTransaction()
    {
        return BeginTransaction(IsolationLevel.Unspecified);
    }
}