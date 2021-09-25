// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    internal static class LoggerExtensions
    {
        // DbContextDataProvider

        private static readonly Action<ILogger, Type, Exception> _savingChanges =
            LoggerMessage.Define<Type>(LogLevel.Trace, LogEventIds.SavingChanges,
                "Saving changes for context {dbContextType} ...");

        private static readonly Action<ILogger, int, Type, Exception> _savedChanges =
            LoggerMessage.Define<int,Type>(LogLevel.Debug, LogEventIds.SavedChanges,
                                           "Saved {entityCount} changes for context {dbContextType}.");

        private static readonly Action<ILogger, Type, Exception> _saveChangesDeferred =
            LoggerMessage.Define<Type>(LogLevel.Trace, LogEventIds.SaveChangesDeferred,
                                       "Deferred saving changes for context {dbContextType}.");

        private static readonly Action<ILogger, Guid, Type, Exception> _transactionInit =
            LoggerMessage.Define<Guid, Type>(LogLevel.Trace, LogEventIds.TransactionInit,
                                             "Initialized transaction {transactionId} for context {dbContextType}.");

        private static readonly Action<ILogger, Guid, Type, Exception> _transactionCommit =
            LoggerMessage.Define<Guid, Type>(LogLevel.Debug, LogEventIds.TransactionCommit,
                                             "Committed transaction {transactionId} for context {dbContextType}.");

        private static readonly Action<ILogger, Guid, Type, Exception> _transactionRollback =
            LoggerMessage.Define<Guid, Type>(LogLevel.Debug, LogEventIds.TransactionRollback,
                                             "Rolled back transaction {transactionId} for context {dbContextType}.");

        internal static void SaveChangesDeferred(this ILogger logger, Type dbContextType)
        {
            _saveChangesDeferred(logger, dbContextType, null);
        }

        internal static void SavingChanges(this ILogger logger, Type dbContextType)
        {
            _savingChanges(logger, dbContextType, null);
        }

        internal static void SavedChanges(this ILogger logger, Type dbContextType, int entityCount)
        {
            _savedChanges(logger, entityCount, dbContextType, null);
        }

        internal static void TransactionInit(this ILogger logger, Type dbContextType, Guid transactionId)
        {
            _transactionInit(logger, transactionId, dbContextType, null);
        }

        internal static void TransactionCommit(this ILogger logger, Type dbContextType, Guid transactionId)
        {
            _transactionCommit(logger, transactionId, dbContextType, null);
        }

        internal static void TransactionRollback(this ILogger logger, Type dbContextType, Guid transactionId)
        {
            _transactionRollback(logger, transactionId, dbContextType, null);
        }

        // DbContextRepository

        private static readonly Action<ILogger, Type, object, Exception> _entitySaving =
            LoggerMessage.Define<Type, object>(
                LogLevel.Debug,
                LogEventIds.EntitySaving,
                "Saving entity {entityType} with id {entityId} ...");

        private static readonly Action<ILogger, Type, object, string, Exception> _concurrentEntitySaving =
            LoggerMessage.Define<Type, object, string>(
                LogLevel.Debug,
                LogEventIds.EntitySaving,
                "Saving entity {entityType} with id {entityId} and token {entityConcurrencyToken} ...");

        private static readonly Action<ILogger, Type, object, Exception> _entitySaved =
            LoggerMessage.Define<Type, object>(
                LogLevel.Information,
                LogEventIds.EntitySaved,
                "Entity {entityType} with id {entityId} saved.");

        private static readonly Action<ILogger, Type, object, string, Exception> _concurrentEntitySaved =
            LoggerMessage.Define<Type, object, string>(
                LogLevel.Information,
                LogEventIds.EntitySaved,
                "Entity {entityType} with id {entityId} and token {entityConcurrencyToken} saved.");

        private static readonly Action<ILogger, Type, object, Exception> _entityDeleting =
            LoggerMessage.Define<Type, object>(
                LogLevel.Debug,
                LogEventIds.EntityDeleting,
                "Deleting entity {entityType} with id {entityId} ...");

        private static readonly Action<ILogger, Type, object, string, Exception> _concurrentEntityDeleting =
            LoggerMessage.Define<Type, object, string>(
                LogLevel.Debug,
                LogEventIds.EntityDeleting,
                "Deleting entity {entityType} with id {entityId} and token {entityConcurrencyToken} ...");

        private static readonly Action<ILogger, Type, object, Exception> _entityDeleted =
            LoggerMessage.Define<Type, object>(
                LogLevel.Information,
                LogEventIds.EntityDeleted,
                "Entity {entityType} with id {entityId} deleted.");

        public static void EntitySaving<TId>(this ILogger logger, IEntity<TId> entity)
        {
            if (entity is IHasChangeToken concurrentEntity)
            {
                _concurrentEntitySaving(
                    logger,
                    entity.GetType(),
                    entity.Id,
                    concurrentEntity.ChangeToken,
                    null);
            }
            else
            {
                _entitySaving(logger, entity.GetType(), entity.Id, null);
            }
        }

        public static void EntitySaved<TId>(this ILogger logger, IEntity<TId> entity)
        {
            if (entity is IHasChangeToken concurrentEntity)
            {
                _concurrentEntitySaved(
                    logger,
                    entity.GetType(),
                    entity.Id,
                    concurrentEntity.ChangeToken,
                    null);
            }
            else
            {
                _entitySaved(logger, entity.GetType(), entity.Id, null);
            }
        }

        public static void EntityDeleting<TId>(this ILogger logger, IEntity<TId> entity)
        {
            if (entity is IHasChangeToken concurrentEntity)
            {
                _concurrentEntityDeleting(
                    logger,
                    entity.GetType(),
                    entity.Id,
                    concurrentEntity.ChangeToken,
                    null);
            }
            else
            {
                _entityDeleting(logger, entity.GetType(), entity.Id, null);
            }
        }

        public static void EntityDeleted<TId>(this ILogger logger, IEntity<TId> entity)
        {
            _entityDeleted(logger, entity.GetType(), entity.Id, null);
        }
    }
}