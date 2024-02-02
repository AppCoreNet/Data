// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AppCoreNet.Data;

internal sealed class DataProviderResolver : IDataProviderResolver
{
    private readonly IEnumerable<ProviderRegistration> _registrations;
    private readonly IServiceProvider _serviceProvider;

    public DataProviderResolver(IEnumerable<ProviderRegistration> registrations, IServiceProvider serviceProvider)
    {
        _registrations = registrations;
        _serviceProvider = serviceProvider;
    }

    public IDataProvider Resolve(string name)
    {
        Ensure.Arg.NotNull(name);

        ProviderRegistration? registration = _registrations.FirstOrDefault(r => r.Name == name);
        if (registration == null)
        {
            throw new InvalidOperationException($"Data provider with name '{name}' is not registered.");
        }

#if !NET8_0_OR_GREATER
        return _serviceProvider.GetServices(registration.ProviderType)
                               .OfType<IDataProvider>()
                               .First(p => p.Name == name);
#else
        return (IDataProvider)_serviceProvider.GetRequiredKeyedService(registration.ProviderType, name);
#endif
    }
}