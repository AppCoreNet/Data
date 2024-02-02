// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides logger methods for <see cref="MongoDataProvider"/>.
/// </summary>
internal static partial class MongoDataProviderLogger
{
    [LoggerMessage(
        EventId = 0,
        EventName = nameof(EntityCreating),
        Level = LogLevel.Debug,
        Message = "Creating entity {EntityType} with id {EntityId} ...")]
    public static partial void EntityCreating(this ILogger logger, Type entityType, object entityId);

    public static void EntityCreating(this ILogger logger, IEntity entity)
    {
        EntityCreating(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 1,
        EventName = nameof(EntityCreated),
        Level = LogLevel.Information,
        Message = "Created entity {EntityType} with id {EntityId}")]
    public static partial void EntityCreated(this ILogger logger, Type entityType, object entityId);

    public static void EntityCreated(this ILogger logger, IEntity entity)
    {
        EntityCreated(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 2,
        EventName = nameof(EntityCreateFailed),
        Level = LogLevel.Error,
        Message = "Error deleting entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    public static partial void EntityCreateFailed(
        this ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    public static void EntityCreateFailed(this ILogger logger, Exception error, IEntity entity)
    {
        EntityCreateFailed(logger, error, entity.GetType(), entity.Id, error.Message);
    }

    [LoggerMessage(
        EventId = 3,
        EventName = nameof(EntityUpdating),
        Level = LogLevel.Debug,
        Message = "Updating entity {EntityType} with id {EntityId} ...")]
    public static partial void EntityUpdating(this ILogger logger, Type entityType, object entityId);

    public static void EntityUpdating(this ILogger logger, IEntity entity)
    {
        EntityUpdating(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 4,
        EventName = nameof(EntityUpdated),
        Level = LogLevel.Information,
        Message = "Updated entity {EntityType} with id {EntityId}")]
    public static partial void EntityUpdated(this ILogger logger, Type entityType, object entityId);

    public static void EntityUpdated(this ILogger logger, IEntity entity)
    {
        EntityUpdated(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 5,
        EventName = nameof(EntityUpdateFailed),
        Level = LogLevel.Error,
        Message = "Error updating entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    public static partial void EntityUpdateFailed(
        this ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    public static void EntityUpdateFailed(this ILogger logger, Exception error, IEntity entity)
    {
        EntityUpdateFailed(logger, error, entity.GetType(), entity.Id, error.Message);
    }

    [LoggerMessage(
        EventId = 6,
        EventName = nameof(EntityDeleting),
        Level = LogLevel.Debug,
        Message = "Deleting entity {EntityType} with id {EntityId} ...")]
    public static partial void EntityDeleting(this ILogger logger, Type entityType, object entityId);

    public static void EntityDeleting(this ILogger logger, IEntity entity)
    {
        EntityDeleting(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 7,
        EventName = nameof(EntityDeleted),
        Level = LogLevel.Information,
        Message = "Deleted entity {EntityType} with id {EntityId}")]
    public static partial void EntityDeleted(this ILogger logger, Type entityType, object entityId);

    public static void EntityDeleted(this ILogger logger, IEntity entity)
    {
        EntityDeleted(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 8,
        EventName = nameof(EntityDeleteFailed),
        Level = LogLevel.Error,
        Message = "Error deleting entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    public static partial void EntityDeleteFailed(
        this ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    public static void EntityDeleteFailed(this ILogger logger, Exception error, IEntity entity)
    {
        EntityDeleteFailed(logger, error, entity.GetType(), entity.Id, error.Message);
    }

    [LoggerMessage(
        EventId = 9,
        EventName = nameof(QueryExecuting),
        Level = LogLevel.Debug,
        Message = "Executing query {QueryType} ...")]
    public static partial void QueryExecuting(this ILogger logger, Type queryType);

    public static void QueryExecuting<TEntity, TResult>(this ILogger logger, IQuery<TEntity, TResult> query)
        where TEntity : class, IEntity
    {
        QueryExecuting(logger, query.GetType());
    }

    [LoggerMessage(
        EventId = 10,
        EventName = nameof(QueryExecuted),
        Level = LogLevel.Information,
        Message = "Executed query {QueryType} in {ElapsedTimeMs}")]
    public static partial void QueryExecuted(this ILogger logger, Type queryType, long elapsedTimeMs);

    public static void QueryExecuted<TEntity, TResult>(this ILogger logger, IQuery<TEntity, TResult> query, long elapsedTimeMs)
        where TEntity : class, IEntity
    {
        QueryExecuted(logger, query.GetType(), elapsedTimeMs);
    }

    [LoggerMessage(
        EventId = 11,
        EventName = nameof(QueryExecuteFailed),
        Level = LogLevel.Error,
        Message = "Error executing query {QueryType}: {ErrorMessage}")]
    public static partial void QueryExecuteFailed(
        this ILogger logger,
        Exception error,
        Type queryType,
        string errorMessage);

    public static void QueryExecuteFailed<TEntity, TResult>(this ILogger logger, Exception error, IQuery<TEntity, TResult> query)
        where TEntity : class, IEntity
    {
        QueryExecuteFailed(logger, error, query.GetType(), error.Message);
    }

    [LoggerMessage(
        EventId = 12,
        EventName = nameof(EntityLoading),
        Level = LogLevel.Debug,
        Message = "Loading entity {EntityType} with id {EntityId} ...")]
    public static partial void EntityLoading(this ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 13,
        EventName = nameof(EntityLoaded),
        Level = LogLevel.Information,
        Message = "Loaded entity {EntityType} with id {EntityId}")]
    public static partial void EntityLoaded(this ILogger logger, Type entityType, object entityId);

    public static void EntityLoaded(this ILogger logger, IEntity entity)
    {
        EntityDeleted(logger, entity.GetType(), entity.Id);
    }

    [LoggerMessage(
        EventId = 14,
        EventName = nameof(EntityLoadFailed),
        Level = LogLevel.Error,
        Message = "Error loading entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    public static partial void EntityLoadFailed(
        this ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    public static void EntityLoadFailed(this ILogger logger, Exception error, Type entityType, object entityId)
    {
        EntityDeleteFailed(logger, error, entityType, entityId, error.Message);
    }

    [LoggerMessage(
        EventId = 15,
        EventName = nameof(EntityNotFound),
        Level = LogLevel.Information,
        Message = "Entity {EntityType} with id {EntityId} was not found")]
    public static partial void EntityNotFound(this ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 16,
        EventName = nameof(TransactionCreated),
        Level = LogLevel.Debug,
        Message = "Transaction with id {TransactionId} created")]
    public static partial void TransactionCreated(
        this ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = 17,
        EventName = nameof(TransactionCommitting),
        Level = LogLevel.Debug,
        Message = "Committing transaction with id {TransactionId} ...")]
    public static partial void TransactionCommitting(
        this ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = 18,
        EventName = nameof(TransactionCommitted),
        Level = LogLevel.Information,
        Message = "Transaction with id {TransactionId} successfully committed")]
    public static partial void TransactionCommitted(
        this ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = 19,
        EventName = nameof(TransactionCommitFailed),
        Level = LogLevel.Error,
        Message = "Error committing transaction with id {TransactionId}: {ErrorMessage}")]
    public static partial void TransactionCommitFailed(
        this ILogger logger,
        Exception error,
        string transactionId,
        string errorMessage);

    public static void TransactionCommitFailed(this ILogger logger, Exception error, string transactionId)
    {
        TransactionCommitFailed(logger, error, transactionId, error.Message);
    }

    [LoggerMessage(
        EventId = 20,
        EventName = nameof(TransactionRollingback),
        Level = LogLevel.Debug,
        Message = "Rolling back transaction with id {TransactionId} ...")]
    public static partial void TransactionRollingback(
        this ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = 21,
        EventName = nameof(TransactionRollback),
        Level = LogLevel.Information,
        Message = "Transaction with id {TransactionId} rolled back")]
    public static partial void TransactionRollback(
        this ILogger logger,
        string transactionId);
}