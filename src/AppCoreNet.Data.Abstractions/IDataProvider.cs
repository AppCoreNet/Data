// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppCore.Data;

/// <summary>
/// Represents a provider for loading and storing entities.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the <see cref="ITransactionManager"/>.
    /// </summary>
    /// <remarks>
    /// Might be <c>null</c> if the provider does not support transactions.
    /// </remarks>
    ITransactionManager? TransactionManager { get; }

    /// <summary>
    /// Begins a scope for changes.
    /// </summary>
    /// <remarks>
    /// Change scopes can be nested, if the depth is greater than, calls to <see cref="SaveChangesAsync"/>
    /// is a no-op.
    /// </remarks>
    /// <param name="afterSaveCallback">A callback which is invoked after changes have been saved.</param>
    /// <returns>The change scope which must be disposed.</returns>
    IDisposable BeginChangeScope(Action? afterSaveCallback = null);

    /// <summary>
    /// Saves all changes made to the data provider.
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
