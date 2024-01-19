// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

namespace AppCore.Data;

/// <summary>
/// Represents a query.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IQuery<TEntity, TResult>
    where TEntity : class, IEntity
{
}