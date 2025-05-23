// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data.Entity; // Added for DbContext

namespace AppCoreNet.Data.EntityFramework; // Adjusted namespace

/// <summary>
/// Represents a <see cref="System.Data.Entity.DbContext"/> based repository.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
public interface IEntityFrameworkRepository<out TDbContext> // Renamed and made covariant
    where TDbContext : System.Data.Entity.DbContext // Adjusted constraint
{
    /// <summary>
    /// Gets the <see cref="EntityFrameworkDataProvider{TDbContext}"/> which owns the repository.
    /// </summary>
    EntityFrameworkDataProvider<TDbContext> Provider { get; } // Type updated
}