// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppCoreNet.Data;

/// <summary>
/// Represents a transaction scope.
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Commit changes done in the transaction.
    /// </summary>
    void Commit();

    /// <summary>
    /// Rolls back changes done in the transaction.
    /// </summary>
    void Rollback();

    /// <summary>
    /// Commit changes done in the transaction.
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back changes done in the transaction.
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}