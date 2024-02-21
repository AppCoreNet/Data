// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

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
public sealed class DbContextTransactionManager<TDbContext> : ITransactionManager
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DataProviderLogger<DbContextDataProvider<TDbContext>> _logger;
    private DbContextTransaction<TDbContext>? _currentTransaction;

    /// <summary>
    /// Gets the currently active <see cref="DbContextTransaction{TDbContext}"/>.
    /// </summary>
    public DbContextTransaction<TDbContext>? CurrentTransaction => _currentTransaction;

    ITransaction? ITransactionManager.CurrentTransaction => CurrentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextTransactionManager{TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public DbContextTransactionManager(
        TDbContext dbContext,
        DataProviderLogger<DbContextDataProvider<TDbContext>> logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _logger = logger;
    }

    private void OnTransactionFinished(object? sender, EventArgs args)
    {
        var transaction = (DbContextTransaction<TDbContext>)sender!;
        transaction.TransactionFinished -= OnTransactionFinished;
        _currentTransaction = null;
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken)
                            .ConfigureAwait(false);

        try
        {
            var t = new DbContextTransaction<TDbContext>(_dbContext, transaction, _logger);
            t.TransactionFinished += OnTransactionFinished;
            return _currentTransaction = t;
        }
        catch
        {
            await transaction.DisposeAsync()
                             .ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public ITransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        if (CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        IDbContextTransaction transaction = _dbContext.Database.BeginTransaction(isolationLevel);

        try
        {
            var t = new DbContextTransaction<TDbContext>(_dbContext, transaction, _logger);
            t.TransactionFinished += OnTransactionFinished;
            return _currentTransaction = t;
        }
        catch
        {
            transaction.Dispose();
            throw;
        }
    }
}