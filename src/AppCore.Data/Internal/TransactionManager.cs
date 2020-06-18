// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;

namespace AppCore.Data
{
    internal sealed class TransactionManager<TTag> : ITransactionManager<TTag>
    {
        private readonly ITransactionManager _manager;
        
        public IDataProvider Provider => _manager.Provider;

        public ITransaction CurrentTransaction => _manager.CurrentTransaction;

        public TransactionManager(IEnumerable<ITransactionManager> managers)
        {
            Ensure.Arg.NotNull(managers, nameof(managers));

            _manager = FindTransactionManager(managers)
                       ?? throw new InvalidOperationException($"Data provider '{typeof(TTag)}' is not registered.");
        }

        private static ITransactionManager FindTransactionManager(IEnumerable<ITransactionManager> managers)
        {
            string name = typeof(TTag).FullName;
            return managers.FirstOrDefault(
                m => string.Equals(name, m.Provider.Name, StringComparison.OrdinalIgnoreCase));
        }

        public Task<ITransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            return _manager.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
    }
}