using Microsoft.EntityFrameworkCore;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Represents a <see cref="DbContext"/> based repository.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public interface IDbContextRepository<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the <see cref="DbContextDataProvider{TDbContext}"/> which owns the repository.
    /// </summary>
    DbContextDataProvider<TDbContext> Provider { get; }
}