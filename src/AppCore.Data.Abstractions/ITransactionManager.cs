// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace AppCore.Data;

/// <summary>
/// Provides transaction management.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// The data provider.
    /// </summary>
    IDataProvider Provider { get; }

    /// <summary>
    /// Gets the currently active <see cref="ITransaction"/>. Might be <c>null</c>.
    /// </summary>
    ITransaction? CurrentTransaction { get; }

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level of the transaction.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The created transaction.</returns>
    Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level of the transaction.</param>
    /// <returns>The created transaction.</returns>
    ITransaction BeginTransaction(IsolationLevel isolationLevel);
}

/// <summary>
/// Provides transaction management for specific data provider.
/// </summary>
/// <typeparam name="TTag">The type used to identify the data provider.</typeparam>
public interface ITransactionManager<TTag> : ITransactionManager
{
}