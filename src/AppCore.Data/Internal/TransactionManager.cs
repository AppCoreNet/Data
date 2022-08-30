// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;

namespace AppCore.Data;

internal sealed class TransactionManager<TTag> : ITransactionManager<TTag>
{
    // Internal to support testing
    internal ITransactionManager Manager { get; }

    public IDataProvider Provider => Manager.Provider;

    public ITransaction? CurrentTransaction => Manager.CurrentTransaction;

    public TransactionManager(IEnumerable<ITransactionManager> managers)
    {
        Ensure.Arg.NotNull(managers, nameof(managers));

        Manager = FindTransactionManager(managers)
                  ?? throw new InvalidOperationException($"Data provider '{typeof(TTag)}' is not registered.");
    }

    private static ITransactionManager? FindTransactionManager(IEnumerable<ITransactionManager> managers)
    {
        string name = typeof(TTag).FullName;
        return managers.FirstOrDefault(
            m => string.Equals(name, m.Provider.Name, StringComparison.OrdinalIgnoreCase));
    }

    public Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        return Manager.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public ITransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        return Manager.BeginTransaction(isolationLevel);
    }
}