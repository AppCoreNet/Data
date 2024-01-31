// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppCoreNet.Data;

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
}
