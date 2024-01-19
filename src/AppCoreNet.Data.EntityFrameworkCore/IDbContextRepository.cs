// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.EntityFrameworkCore;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Represents a <see cref="DbContext"/> based repository.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public interface IDbContextRepository<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the <see cref="DbContextDataProvider{TDbContext}"/> which owns the repository.
    /// </summary>
    DbContextDataProvider<TDbContext> Provider { get; }
}