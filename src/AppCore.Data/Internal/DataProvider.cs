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
        private readonly IDataProvider _provider;

        internal IDataProvider WrappedProvider => _provider;

        public string Name => _provider.Name;

        ITransactionManager IDataProvider.TransactionManager => TransactionManager;

        public ITransactionManager<TTag> TransactionManager => (ITransactionManager<TTag>) _provider.TransactionManager;

        public DataProvider(IEnumerable<IDataProvider> providers)
        {
            Ensure.Arg.NotNull(providers, nameof(providers));

            _provider = FindProvider(providers)
                        ?? throw new InvalidOperationException($"Data provider with name '{typeof(TTag)}' is not registered.");
        }

        private static IDataProvider FindProvider(IEnumerable<IDataProvider> providers)
        {
            string name = typeof(TTag).FullName;
            return providers.FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));
        }

        public IDisposable BeginChangeScope(Action afterSaveCallback = null)
        {
            return _provider.BeginChangeScope(afterSaveCallback);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _provider.SaveChangesAsync(cancellationToken);
        }
    }
}