// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data;

/// <summary>
/// Provides default implementation of <see cref="DataProviderLogger{T}"/>.
/// </summary>
/// <typeparam name="T">The category of the logger.</typeparam>
public partial class DefaultDataProviderLogger<T> : DataProviderLogger<T>
    where T : IDataProvider
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDataProviderLogger{T}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
    public DefaultDataProviderLogger(ILoggerFactory loggerFactory)
    {
        Ensure.Arg.NotNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<T>();
    }

    [LoggerMessage(
        EventId = 0,
        EventName = nameof(EntityCreating),
        Level = LogLevel.Debug,
        Message = "Creating entity {EntityType} with id {EntityId} ...")]
    private static partial void EntityCreating(ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 1,
        EventName = nameof(EntityCreated),
        Level = LogLevel.Information,
        Message = "Created entity {EntityType} with id {EntityId} in {ElapsedTimeMs} ms")]
    private static partial void EntityCreated(ILogger logger, Type entityType, object entityId, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 2,
        EventName = nameof(EntityCreateFailed),
        Level = LogLevel.Error,
        Message = "Error deleting entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    private static partial void EntityCreateFailed(
        ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    [LoggerMessage(
        EventId = 3,
        EventName = nameof(EntityUpdating),
        Level = LogLevel.Debug,
        Message = "Updating entity {EntityType} with id {EntityId} ...")]
    private static partial void EntityUpdating(ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 4,
        EventName = nameof(EntityUpdated),
        Level = LogLevel.Information,
        Message = "Updated entity {EntityType} with id {EntityId} in {ElapsedTimeMs} ms")]
    private static partial void EntityUpdated(ILogger logger, Type entityType, object entityId, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 5,
        EventName = nameof(EntityUpdateFailed),
        Level = LogLevel.Error,
        Message = "Error updating entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    private static partial void EntityUpdateFailed(
        ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    [LoggerMessage(
        EventId = 6,
        EventName = nameof(EntityDeleting),
        Level = LogLevel.Debug,
        Message = "Deleting entity {EntityType} with id {EntityId} ...")]
    private static partial void EntityDeleting(ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 7,
        EventName = nameof(EntityDeleted),
        Level = LogLevel.Information,
        Message = "Deleted entity {EntityType} with id {EntityId} in {ElapsedTimeMs} ms")]
    private static partial void EntityDeleted(ILogger logger, Type entityType, object entityId, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 8,
        EventName = nameof(EntityDeleteFailed),
        Level = LogLevel.Error,
        Message = "Error deleting entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    private static partial void EntityDeleteFailed(
        ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    [LoggerMessage(
        EventId = 9,
        EventName = nameof(QueryExecuting),
        Level = LogLevel.Debug,
        Message = "Executing query {QueryType} ...")]
    private static partial void QueryExecuting(ILogger logger, Type queryType);

    [LoggerMessage(
        EventId = 10,
        EventName = nameof(QueryExecuted),
        Level = LogLevel.Information,
        Message = "Executed query {QueryType} in {ElapsedTimeMs} ms")]
    private static partial void QueryExecuted(ILogger logger, Type queryType, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 11,
        EventName = nameof(QueryExecuteFailed),
        Level = LogLevel.Error,
        Message = "Error executing query {QueryType}: {ErrorMessage}")]
    private static partial void QueryExecuteFailed(
        ILogger logger,
        Exception error,
        Type queryType,
        string errorMessage);

    [LoggerMessage(
        EventId = 12,
        EventName = nameof(EntityLoading),
        Level = LogLevel.Debug,
        Message = "Loading entity {EntityType} with id {EntityId} ...")]
    private static partial void EntityLoading(ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 13,
        EventName = nameof(EntityLoaded),
        Level = LogLevel.Information,
        Message = "Loaded entity {EntityType} with id {EntityId} in {ElapsedTimeMs} ms")]
    private static partial void EntityLoaded(ILogger logger, Type entityType, object entityId, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 14,
        EventName = nameof(EntityLoadFailed),
        Level = LogLevel.Error,
        Message = "Error loading entity {EntityType} with id {EntityId}: {ErrorMessage}")]
    private static partial void EntityLoadFailed(
        ILogger logger,
        Exception error,
        Type entityType,
        object entityId,
        string errorMessage);

    [LoggerMessage(
        EventId = 15,
        EventName = nameof(EntityNotFound),
        Level = LogLevel.Information,
        Message = "Entity {EntityType} with id {EntityId} was not found")]
    private static partial void EntityNotFound(ILogger logger, Type entityType, object entityId);

    [LoggerMessage(
        EventId = 16,
        EventName = nameof(TransactionCreated),
        Level = LogLevel.Debug,
        Message = "Transaction with id {TransactionId} created")]
    private static partial void TransactionCreated(ILogger logger, string transactionId);

    [LoggerMessage(
        EventId = 17,
        EventName = nameof(TransactionCommitting),
        Level = LogLevel.Debug,
        Message = "Committing transaction with id {TransactionId} ...")]
    private static partial void TransactionCommitting(ILogger logger, string transactionId);

    [LoggerMessage(
        EventId = 18,
        EventName = nameof(TransactionCommitted),
        Level = LogLevel.Information,
        Message = "Transaction with id {TransactionId} successfully committed in {ElapsedTimeMs} ms")]
    private static partial void TransactionCommitted(ILogger logger, string transactionId, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 19,
        EventName = nameof(TransactionCommitFailed),
        Level = LogLevel.Error,
        Message = "Error committing transaction with id {TransactionId}: {ErrorMessage}")]
    private static partial void TransactionCommitFailed(
        ILogger logger,
        Exception error,
        string transactionId,
        string errorMessage);

    [LoggerMessage(
        EventId = 20,
        EventName = nameof(TransactionRollingback),
        Level = LogLevel.Debug,
        Message = "Rolling back transaction with id {TransactionId} ...")]
    private static partial void TransactionRollingback(ILogger logger, string transactionId);

    [LoggerMessage(
        EventId = 21,
        EventName = nameof(TransactionRolledback),
        Level = LogLevel.Information,
        Message = "Transaction with id {TransactionId} rolled back in {ElapsedTimeMs} ms")]
    private static partial void TransactionRollback(ILogger logger, string transactionId, long elapsedTimeMs);

    [LoggerMessage(
        EventId = 22,
        EventName = nameof(TransactionRollbackFailed),
        Level = LogLevel.Error,
        Message = "Error rolling back transaction with id {TransactionId}: {ErrorMessage}")]
    private static partial void TransactionRollbackFailed(
        ILogger logger,
        Exception error,
        string transactionId,
        string errorMessage);

    [LoggerMessage(
        EventId = 23,
        EventName = nameof(TransactionDisposed),
        Level = LogLevel.Debug,
        Message = "Transaction with id {TransactionId} disposed")]
    private static partial void TransactionDisposed(ILogger logger, string transactionId);

    /// <inheritdoc />
    public override void EntityCreating(IEntity entity)
    {
        EntityCreating(_logger, entity.GetType(), entity.Id);
    }

    /// <inheritdoc />
    public override void EntityCreated(IEntity entity, long elapsedTimeMs)
    {
        EntityCreated(_logger, entity.GetType(), entity.Id, elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void EntityCreateFailed(Exception error, IEntity entity)
    {
        EntityCreateFailed(_logger, error, entity.GetType(), entity.Id, error.Message);
    }

    /// <inheritdoc />
    public override void EntityUpdating(IEntity entity)
    {
        EntityUpdating(_logger, entity.GetType(), entity.Id);
    }

    /// <inheritdoc />
    public override void EntityUpdated(IEntity entity, long elapsedTimeMs)
    {
        EntityUpdated(_logger, entity.GetType(), entity.Id, elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void EntityUpdateFailed(Exception error, IEntity entity)
    {
        EntityUpdateFailed(_logger, error, entity.GetType(), entity.Id, error.Message);
    }

    /// <inheritdoc />
    public override void EntityLoading(Type entityType, object entityId)
    {
        EntityLoading(_logger, entityType, entityId);
    }

    /// <inheritdoc />
    public override void EntityLoaded(IEntity entity, long elapsedTimeMs)
    {
        EntityLoaded(_logger, entity.GetType(), entity.Id, elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void EntityLoadFailed(Exception error, Type entityType, object entityId)
    {
        EntityLoadFailed(_logger, error, entityType, entityId, error.Message);
    }

    /// <inheritdoc />
    public override void EntityNotFound(Type entityType, object entityId)
    {
        EntityNotFound(_logger, entityType, entityId);
    }

    /// <inheritdoc />
    public override void EntityDeleting(IEntity entity)
    {
        EntityDeleting(_logger, entity.GetType(), entity.Id);
    }

    /// <inheritdoc />
    public override void EntityDeleted(IEntity entity, long elapsedTimeMs)
    {
        EntityDeleted(_logger, entity.GetType(), entity.Id, elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void EntityDeleteFailed(Exception error, IEntity entity)
    {
        EntityDeleteFailed(_logger, error, entity.GetType(), entity.Id, error.Message);
    }

    /// <inheritdoc />
    public override void QueryExecuting<TEntity, TResult>(IQuery<TEntity, TResult> query)
    {
        QueryExecuting(_logger, query.GetType());
    }

    /// <inheritdoc />
    public override void QueryExecuted<TEntity, TResult>(IQuery<TEntity, TResult> query, long elapsedTimeMs)
    {
        QueryExecuted(_logger, query.GetType(), elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void QueryExecuteFailed<TEntity, TResult>(Exception error, IQuery<TEntity, TResult> query)
    {
        QueryExecuteFailed(_logger, error, query.GetType(), error.Message);
    }

    /// <inheritdoc />
    public override void TransactionCreated(ITransaction transaction)
    {
        TransactionCreated(_logger, transaction.Id);
    }

    /// <inheritdoc />
    public override void TransactionCommitting(ITransaction transaction)
    {
        TransactionCommitting(_logger, transaction.Id);
    }

    /// <inheritdoc />
    public override void TransactionCommitted(ITransaction transaction, long elapsedTimeMs)
    {
        TransactionCommitted(_logger, transaction.Id, elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void TransactionCommitFailed(ITransaction transaction, Exception error)
    {
        TransactionCommitFailed(_logger, error, transaction.Id, error.Message);
    }

    /// <inheritdoc />
    public override void TransactionRollingback(ITransaction transaction)
    {
        TransactionRollingback(_logger, transaction.Id);
    }

    /// <inheritdoc />
    public override void TransactionRolledback(ITransaction transaction, long elapsedTimeMs)
    {
        TransactionRollback(_logger, transaction.Id, elapsedTimeMs);
    }

    /// <inheritdoc />
    public override void TransactionRollbackFailed(ITransaction transaction, Exception error)
    {
        TransactionRollbackFailed(_logger, error, transaction.Id, error.Message);
    }

    /// <inheritdoc />
    public override void TransactionDisposed(ITransaction transaction)
    {
        TransactionDisposed(_logger, transaction.Id);
    }
}