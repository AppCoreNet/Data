// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data;

/// <summary>
/// Represents a resolver for <see cref="IDataProvider"/> instances.
/// </summary>
public interface IDataProviderResolver
{
    /// <summary>
    /// Resolves the <see cref="IDataProvider"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the <see cref="IDataProvider"/>.</param>
    /// <returns>The <see cref="IDataProvider"/>.</returns>
    /// <exception cref="ArgumentException">Argument <paramref name="name"/> must not be null.</exception>
    /// <exception cref="InvalidOperationException">A data provider with the specified name is not registered.</exception>
    IDataProvider Resolve(string name);
}