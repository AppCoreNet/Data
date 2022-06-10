// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCore.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection;

internal sealed class DbContextDataBuilder<TDbContext> : IDbContextDataBuilder<TDbContext>
    where TDbContext : DbContext
{
    private static readonly Type _dbContextType = typeof(TDbContext);

    public IServiceCollection Services { get; }

    public DbContextDataBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IDbContextDataBuilder<TDbContext> AddRepository<TId, TEntity, TDbEntity>()
        where TEntity : IEntity<TId>
        where TDbEntity : class
    {
        Services.TryAddScoped<IRepository<TId, TEntity>, DbContextRepository<TId, TEntity, TDbContext, TDbEntity>>();
        return this;
    }

    public IDbContextDataBuilder<TDbContext> AddRepository<TRepository>()
        where TRepository : class
    {
        Type repositoryType = typeof(TRepository);

        Type genericRepositoryBaseType = typeof(DbContextRepository<,,,>);
        Type? genericRepositoryType = repositoryType.FindClosedTypeOf(genericRepositoryBaseType);
        if (genericRepositoryType == null)
        {
            throw new ArgumentException($"The repository must inherit from {genericRepositoryBaseType.GetDisplayName()}.");
        }

        Type repositoryIdType = genericRepositoryType.GenericTypeArguments[0];
        Type repositoryEntityType = genericRepositoryType.GenericTypeArguments[1];
        Type repositoryDbContextType = genericRepositoryType.GenericTypeArguments[2];

        if (repositoryDbContextType != _dbContextType)
        {
            throw new ArgumentException(
                $"The repository {repositoryType.GetDisplayName()} is not compatible with {_dbContextType.GetDisplayName()}");
        }

        Type repositoryServiceType = typeof(IRepository<,>).MakeGenericType(repositoryIdType, repositoryEntityType);
        Services.TryAddScoped(repositoryServiceType, repositoryType);
        return this;
    }

    public IDbContextDataBuilder<TDbContext> AddQueryHandler<TQueryHandler>()
        where TQueryHandler : class
    {
        Type queryHandlerType = typeof(TQueryHandler);

        Type queryHandlerBaseType = typeof(IDbContextQueryHandler<,,>);
        Type? genericRepositoryType = queryHandlerType.FindClosedTypeOf(queryHandlerBaseType);
        if (genericRepositoryType == null)
        {
            throw new ArgumentException($"The query handler must inherit from {queryHandlerBaseType.GetDisplayName()}.");
        }

        Type entityType = genericRepositoryType.GenericTypeArguments[0];
        Type resultType = genericRepositoryType.GenericTypeArguments[1];
        Type queryDbContextType = genericRepositoryType.GenericTypeArguments[2];

        if (queryDbContextType != _dbContextType)
        {
            throw new ArgumentException(
                $"The query handler {queryHandlerType.GetDisplayName()} is not compatible with {_dbContextType.GetDisplayName()}");
        }

        Type queryHandlerServiceType = queryHandlerBaseType.MakeGenericType(entityType, resultType, queryDbContextType);
        Services.TryAddEnumerable(ServiceDescriptor.Transient(queryHandlerServiceType, queryHandlerType));
        return this;
    }
}