// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using AppCore.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    internal static class LoggerExtensions
    {
        // DbContextDataProvider

        private static readonly LoggerEventDelegate<Type> _savingChanges =
            LoggerEvent.Define<Type>(LogLevel.Trace, LogEventIds.SavingChanges,
                "Saving changes for context {dbContextType} ...");

        private static readonly LoggerEventDelegate<int, Type> _savedChanges =
            LoggerEvent.Define<int,Type>(LogLevel.Debug, LogEventIds.SavedChanges,
                "Saved {entityCount} changes for context {dbContextType}.");

        private static readonly LoggerEventDelegate<Type> _saveChangesDeferred =
            LoggerEvent.Define<Type>(LogLevel.Trace, LogEventIds.SaveChangesDeferred,
                "Deferred saving changes for context {dbContextType}.");

        private static readonly LoggerEventDelegate<Guid, Type> _transactionInit =
            LoggerEvent.Define<Guid, Type>(LogLevel.Trace, LogEventIds.TransactionInit,
                "Initialized transaction {transactionId} for context {dbContextType}.");

        private static readonly LoggerEventDelegate<Guid, Type> _transactionCommit =
            LoggerEvent.Define<Guid, Type>(LogLevel.Debug, LogEventIds.TransactionCommit,
                "Committed transaction {transactionId} for context {dbContextType}.");

        private static readonly LoggerEventDelegate<Guid, Type> _transactionRollback =
            LoggerEvent.Define<Guid, Type>(LogLevel.Debug, LogEventIds.TransactionRollback,
                "Rolled back transaction {transactionId} for context {dbContextType}.");

        internal static void SaveChangesDeferred(this ILogger logger, Type dbContextType)
        {
            _saveChangesDeferred(logger, dbContextType);
        }

        internal static void SavingChanges(this ILogger logger, Type dbContextType)
        {
            _savingChanges(logger, dbContextType);
        }

        internal static void SavedChanges(this ILogger logger, Type dbContextType, int entityCount)
        {
            _savedChanges(logger, entityCount, dbContextType);
        }

        internal static void TransactionInit(this ILogger logger, Type dbContextType, Guid transactionId)
        {
            _transactionInit(logger, transactionId, dbContextType);
        }

        internal static void TransactionCommit(this ILogger logger, Type dbContextType, Guid transactionId)
        {
            _transactionCommit(logger, transactionId, dbContextType);
        }

        internal static void TransactionRollback(this ILogger logger, Type dbContextType, Guid transactionId)
        {
            _transactionRollback(logger, transactionId, dbContextType);
        }

        // DbContextRepository

        private static readonly LoggerEventDelegate<Type, object> _entitySaving =
            LoggerEvent.Define<Type, object>(
                LogLevel.Debug,
                LogEventIds.EntitySaving,
                "Saving entity {entityType} with id {entityId} ...");

        private static readonly LoggerEventDelegate<Type, object, string> _concurrentEntitySaving =
            LoggerEvent.Define<Type, object, string>(
                LogLevel.Debug,
                LogEventIds.EntitySaving,
                "Saving entity {entityType} with id {entityId} and token {entityConcurrencyToken} ...");

        private static readonly LoggerEventDelegate<Type, object> _entitySaved =
            LoggerEvent.Define<Type, object>(
                LogLevel.Info,
                LogEventIds.EntitySaved,
                "Entity {entityType} with id {entityId} saved.");

        private static readonly LoggerEventDelegate<Type, object, string> _concurrentEntitySaved =
            LoggerEvent.Define<Type, object, string>(
                LogLevel.Info,
                LogEventIds.EntitySaved,
                "Entity {entityType} with id {entityId} and token {entityConcurrencyToken} saved.");

        private static readonly LoggerEventDelegate<Type, object> _entityDeleting =
            LoggerEvent.Define<Type, object>(
                LogLevel.Debug,
                LogEventIds.EntityDeleting,
                "Deleting entity {entityType} with id {entityId} ...");

        private static readonly LoggerEventDelegate<Type, object, string> _concurrentEntityDeleting =
            LoggerEvent.Define<Type, object, string>(
                LogLevel.Debug,
                LogEventIds.EntityDeleting,
                "Deleting entity {entityType} with id {entityId} and token {entityConcurrencyToken} ...");

        private static readonly LoggerEventDelegate<Type, object> _entityDeleted =
            LoggerEvent.Define<Type, object>(
                LogLevel.Info,
                LogEventIds.EntityDeleted,
                "Entity {entityType} with id {entityId} deleted.");

        public static void EntitySaving<TId>(this ILogger logger, IEntity<TId> entity)
            where TId : IEquatable<TId>
        {
            if (entity is IHasConcurrencyToken concurrentEntity)
            {
                _concurrentEntitySaving(
                    logger,
                    entity.GetType(),
                    entity.Id,
                    concurrentEntity.ConcurrencyToken);
            }
            else
            {
                _entitySaving(logger, entity.GetType(), entity.Id);
            }
        }

        public static void EntitySaved<TId>(this ILogger logger, IEntity<TId> entity)
            where TId : IEquatable<TId>
        {
            if (entity is IHasConcurrencyToken concurrentEntity)
            {
                _concurrentEntitySaved(
                    logger,
                    entity.GetType(),
                    entity.Id,
                    concurrentEntity.ConcurrencyToken);
            }
            else
            {
                _entitySaved(logger, entity.GetType(), entity.Id);
            }
        }

        public static void EntityDeleting<TId>(this ILogger logger, IEntity<TId> entity)
            where TId : IEquatable<TId>
        {
            if (entity is IHasConcurrencyToken concurrentEntity)
            {
                _concurrentEntityDeleting(
                    logger,
                    entity.GetType(),
                    entity.Id,
                    concurrentEntity.ConcurrencyToken);
            }
            else
            {
                _entityDeleting(logger, entity.GetType(), entity.Id);
            }
        }

        public static void EntityDeleted<TId>(this ILogger logger, IEntity<TId> entity)
            where TId : IEquatable<TId>
        {
            _entityDeleted(logger, entity.GetType(), entity.Id);
        }
    }
}
