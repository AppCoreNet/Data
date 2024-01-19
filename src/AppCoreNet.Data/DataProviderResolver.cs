// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Linq;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AppCoreNet.Data;

internal sealed class DataProviderResolver : IDataProviderResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataProviderResolverOptions _options;

    public DataProviderResolver(IServiceProvider serviceProvider, DataProviderResolverOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public IDataProvider Resolve(string name)
    {
        Ensure.Arg.NotNull(name);

        IDataProvider? provider = null;
        if (_options.ProviderMap.TryGetValue(name, out Type type))
        {
            provider = _serviceProvider.GetServices(type)
                                       .OfType<IDataProvider>()
                                       .FirstOrDefault(p => p.Name == name);
        }

        if (provider == null)
            throw new InvalidOperationException($"Data provider with name '{name}' is not registered.");

        return provider;
    }
}