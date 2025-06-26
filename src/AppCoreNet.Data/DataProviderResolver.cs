// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCoreNet.Diagnostics;

namespace AppCoreNet.Data;

internal sealed class DataProviderResolver : IDataProviderResolver
{
    private readonly IEnumerable<IDataProvider> _providers;

    public DataProviderResolver(IEnumerable<IDataProvider> providers)
    {
        _providers = providers;
    }

    public IDataProvider Resolve(string name)
    {
        Ensure.Arg.NotNull(name);

        IDataProvider? provider = _providers.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            throw new InvalidOperationException($"Data provider with name '{name}' is not registered.");
        }

        return provider;
    }
}