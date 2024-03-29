﻿// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

/// <summary>
/// Represents a query.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IQuery<TEntity, TResult>
    where TEntity : class, IEntity
{
}