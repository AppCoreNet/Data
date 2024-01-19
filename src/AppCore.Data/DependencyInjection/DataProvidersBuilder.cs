// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCore.Extensions.DependencyInjection;

internal sealed class DataProvidersBuilder : IDataProvidersBuilder
{
    public IServiceCollection Services { get; }

    public DataProvidersBuilder(IServiceCollection services)
    {
        Services = services;
    }
}