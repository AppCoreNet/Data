// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using AppCore.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    internal static class LogEventIds
    {
        // DbContextDataProvider
        public static readonly LogEventId SavingChanges = new LogEventId(0, "saving_changes");

        public static readonly LogEventId SavedChanges = new LogEventId(1, "saved_changes");

        public static readonly LogEventId SaveChangesDeferred = new LogEventId(2, "saved_changes_delayer");

        public static readonly LogEventId TransactionInit = new LogEventId(3, "transaction_init");

        public static readonly LogEventId TransactionCommit = new LogEventId(4, "transaction_commit");

        public static readonly LogEventId TransactionRollback = new LogEventId(5, "transaction_rollback");

        // DbContextRepository
        public static readonly LogEventId EntitySaving = new LogEventId(0, "entity_saving");

        public static readonly LogEventId EntitySaved = new LogEventId(1, "entity_saved");

        public static readonly LogEventId EntityDeleting = new LogEventId(2, "entity_deleting");

        public static readonly LogEventId EntityDeleted = new LogEventId(3, "entity_deleted");
    }
}