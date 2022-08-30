// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Represents a provider for loading and storing entities using a <see cref="DbContext"/>.
/// </summary>
public interface IDbContextDataProvider : IDataProvider
{
    /// <summary>
    /// Gets the <see cref="DbContext"/> of the data provider.
    /// </summary>
    /// <returns>The <see cref="DbContext"/>.</returns>
    DbContext GetContext();
}

/// <inheritdoc />
public interface IDbContextDataProvider<out TDbContext> : IDbContextDataProvider
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the <see cref="DbContext"/> of the data provider.
    /// </summary>
    /// <returns>The <see cref="DbContext"/>.</returns>
    new TDbContext GetContext();
}