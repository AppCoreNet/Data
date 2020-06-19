// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;

namespace AppCore.Data
{
    internal sealed class DataProvider<TTag> : IDataProvider<TTag>
    {
        // Internal to support testing
        internal IDataProvider Provider { get; }

        public string Name => Provider.Name;

        ITransactionManager IDataProvider.TransactionManager => TransactionManager;

        public ITransactionManager<TTag> TransactionManager => (ITransactionManager<TTag>) Provider.TransactionManager;

        public DataProvider(IEnumerable<IDataProvider> providers)
        {
            Ensure.Arg.NotNull(providers, nameof(providers));

            Provider = FindProvider(providers)
                        ?? throw new InvalidOperationException($"Data provider with name '{typeof(TTag)}' is not registered.");
        }

        private static IDataProvider FindProvider(IEnumerable<IDataProvider> providers)
        {
            string name = typeof(TTag).FullName;
            return providers.FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));
        }

        public IDisposable BeginChangeScope(Action afterSaveCallback = null)
        {
            return Provider.BeginChangeScope(afterSaveCallback);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Provider.SaveChangesAsync(cancellationToken);
        }
    }
}