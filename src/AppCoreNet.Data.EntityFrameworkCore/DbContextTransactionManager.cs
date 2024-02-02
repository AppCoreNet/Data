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
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP003:Dispose previous before re-assigning",
    Justification = "Pre-condition is that no transaction is active.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP006:Implement IDisposable",
    Justification = "Transaction must be disposed by consumer.")]
public sealed class DbContextTransactionManager : ITransactionManager
{
    private readonly DbContext _dbContext;
    private readonly ILogger _logger;
    private DbContextTransaction? _currentTransaction;

    /// <summary>
    /// Gets the currently active <see cref="DbContextTransaction"/>.
    /// </summary>
    public DbContextTransaction? CurrentTransaction => _currentTransaction;

    ITransaction? ITransactionManager.CurrentTransaction => CurrentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextTransactionManager"/> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public DbContextTransactionManager(DbContext dbContext, ILogger logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _logger = logger;
    }

    private void OnTransactionFinished(object? sender, EventArgs args)
    {
        var transaction = (DbContextTransaction)sender!;
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
            await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                            .ConfigureAwait(false);

        try
        {
            var t = new DbContextTransaction(_dbContext, transaction, _logger);
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

        IDbContextTransaction transaction = _dbContext.Database.BeginTransaction();

        try
        {
            var t = new DbContextTransaction(_dbContext, transaction, _logger);
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