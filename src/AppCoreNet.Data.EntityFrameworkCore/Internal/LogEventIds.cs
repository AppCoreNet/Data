// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

internal static class LogEventIds
{
    // DbContextDataProvider
    public static readonly EventId SavingChanges = new EventId(0, "saving_changes");

    public static readonly EventId SavedChanges = new EventId(1, "saved_changes");

    public static readonly EventId SaveChangesDeferred = new EventId(2, "saving_changes_delayed");

    public static readonly EventId TransactionInit = new EventId(3, "transaction_init");

    public static readonly EventId TransactionCommit = new EventId(4, "transaction_commit");

    public static readonly EventId TransactionRollback = new EventId(5, "transaction_rollback");

    public static readonly EventId TransactionDisposed = new EventId(6, "transaction_disposed");

    // DbContextRepository
    public static readonly EventId EntitySaving = new EventId(0, "entity_saving");

    public static readonly EventId EntitySaved = new EventId(1, "entity_saved");

    public static readonly EventId EntityDeleting = new EventId(2, "entity_deleting");

    public static readonly EventId EntityDeleted = new EventId(3, "entity_deleted");

    // DbContextQueryHandler
    public static readonly EventId QueryExecuting = new EventId(0, "query_executing");

    public static readonly EventId QueryExecuted = new EventId(1, "query_executed");
}