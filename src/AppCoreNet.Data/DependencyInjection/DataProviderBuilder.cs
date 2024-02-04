// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

internal sealed class DataProviderBuilder : IDataProviderBuilder
{
    public IServiceCollection Services { get; }

    public DataProviderBuilder(IServiceCollection services)
    {
        Services = services;
    }
}