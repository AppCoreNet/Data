// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System.ComponentModel;
using AppCore.Data;
using AppCore.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection;

/// <summary>
/// Builder object for <see cref="DbContext"/> data providers.
/// </summary>
public interface IDbContextDataBuilder<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds default repository implementation for the specified entity type.
    /// </summary>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <typeparam name="TEntity">Type type of the entity.</typeparam>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <returns></returns>
    IDbContextDataBuilder<TDbContext> AddRepository<TId, TEntity, TDbEntity>()
        where TEntity : IEntity<TId>
        where TDbEntity : class;

    /// <summary>
    /// Adds the specified repository implementation.
    /// </summary>
    /// <typeparam name="TRepository">The type of the <see cref="DbContextRepository{TId,TEntity,TDbContext,TDbEntity}"/>.</typeparam>
    /// <returns></returns>
    IDbContextDataBuilder<TDbContext> AddRepository<TRepository>()
        where TRepository : class;

    /// <summary>
    /// Adds the specified query handler implementation.
    /// </summary>
    /// <typeparam name="TQueryHandler">The type of the <see cref="IDbContextQueryHandler{TEntity,TResult,TDbContext}"/>.</typeparam>
    /// <returns></returns>
    IDbContextDataBuilder<TDbContext> AddQueryHandler<TQueryHandler>()
        where TQueryHandler : class;
}