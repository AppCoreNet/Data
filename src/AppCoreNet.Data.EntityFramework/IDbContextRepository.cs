// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data.Entity;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Represents a <see cref="System.Data.Entity.DbContext"/> based repository.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
public interface IDbContextRepository<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the <see cref="DbContextDataProvider{TDbContext}"/> which owns the repository.
    /// </summary>
    DbContextDataProvider<TDbContext> Provider { get; }
}