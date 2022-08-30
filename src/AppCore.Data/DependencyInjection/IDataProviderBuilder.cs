// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCore.Extensions.DependencyInjection;

/// <summary>
/// Builder object for data provider services.
/// </summary>
public interface IDataProviderBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    IServiceCollection Services { get; }
}