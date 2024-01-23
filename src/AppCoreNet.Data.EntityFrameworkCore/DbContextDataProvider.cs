// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Provides a base class for <see cref="DbContext"/> based data provider.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class DbContextDataProvider<TDbContext> : IDataProvider
    where TDbContext : DbContext
{
    private readonly ILogger<DbContextDataProvider<TDbContext>> _logger;
    private readonly Stack<IDisposable> _pendingChanges = new ();
    private readonly List<Action> _afterSaveCallbacks = new ();

    private sealed class PendingChanges : IDisposable
    {
        private readonly Action<IDisposable> _disposeCallback;

        public PendingChanges(Action<IDisposable> disposeCallback)
        {
            _disposeCallback = disposeCallback;
        }

        public void Dispose()
        {
            _disposeCallback(this);
        }
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="DbContext"/> of the data provider.
    /// </summary>
    /// <value>The <see cref="DbContext"/>.</value>
    public TDbContext DbContext { get; }

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/> of the data provider.
    /// </summary>
    public IEntityMapper EntityMapper { get; }

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/> of the data provider.
    /// </summary>
    public ITokenGenerator TokenGenerator { get; }

    /// <summary>
    /// Gets the <see cref="DbContextQueryHandlerFactory{TDbContext}"/> of the data provider.
    /// </summary>
    public DbContextQueryHandlerFactory<TDbContext> QueryHandlerFactory { get; }

    /// <summary>
    /// Gets the <see cref="DbContextTransactionManager"/>.
    /// </summary>
    public DbContextTransactionManager TransactionManager { get; }

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextDataProvider{TDbContext}"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="DbContextDataProviderServices{TDbContext}"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public DbContextDataProvider(
        string name,
        DbContextDataProviderServices<TDbContext> services,
        ILogger<DbContextDataProvider<TDbContext>> logger)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(services);
        Ensure.Arg.NotNull(logger);

        Name = name;
        DbContext = services.DbContext;
        EntityMapper = services.EntityMapper;
        TokenGenerator = services.TokenGenerator;
        QueryHandlerFactory = services.QueryHandlerFactory;
        TransactionManager = services.TransactionManager;

        _logger = logger;
    }

    /// <inheritdoc />
    public IDisposable BeginChangeScope(Action? afterSaveCallback = null)
    {
        if (afterSaveCallback != null)
            _afterSaveCallbacks.Add(afterSaveCallback);

        IDisposable scope;
        _pendingChanges.Push(scope = new PendingChanges(EndChangeScope));
        return scope;
    }

    private void EndChangeScope(IDisposable scope)
    {
        if (!ReferenceEquals(_pendingChanges.Peek(), scope))
        {
            throw new InvalidOperationException("Data provider change scope must be disposed in reverse-order.");
        }

        _pendingChanges.Pop();
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // if more than one change scope is active, save is a no-op
        if (_pendingChanges.Count > 1)
        {
            _logger.SaveChangesDeferred(DbContext.GetType());
            return;
        }

        _logger.SavingChanges(DbContext.GetType());

        int entityCount;
        try
        {
            bool acceptChanges = DbContext.Database.CurrentTransaction == null;
            entityCount = await DbContext.SaveChangesAsync(acceptChanges, cancellationToken)
                                         .ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException error)
        {
            throw new EntityConcurrencyException(error);
        }
        catch (DbUpdateException error)
        {
            throw new EntityUpdateException(error);
        }

        foreach (Action callback in _afterSaveCallbacks)
        {
            callback();
        }

        _afterSaveCallbacks.Clear();
        _logger.SavedChanges(DbContext.GetType(), entityCount);
    }
}
