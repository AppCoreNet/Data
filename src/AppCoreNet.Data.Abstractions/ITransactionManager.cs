// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;

namespace AppCoreNet.Data;

/// <summary>
/// Provides transaction management.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Gets the currently active <see cref="ITransaction"/>. Might be <c>null</c>.
    /// </summary>
    ITransaction? CurrentTransaction { get; }

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The created transaction.</returns>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction in the context of the data provider.
    /// </summary>
    /// <returns>The created transaction.</returns>
    ITransaction BeginTransaction();
}