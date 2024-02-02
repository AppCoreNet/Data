// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data;
using AppCoreNet.Data.AutoMapper;
using AppCoreNet.Diagnostics;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register AutoMapper as an <see cref="IEntityMapper"/>.
/// </summary>
public static class AutoMapperDataProviderBuilderExtensions
{
    /// <summary>
    /// Registers AutoMapper.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="config">The delegate used to configure AutoMapper.</param>
    public static void AddAutoMapper(this IDataProvidersBuilder builder, Action<IMapperConfigurationExpression> config)
    {
        Ensure.Arg.NotNull(builder);
        builder.Services.AddAutoMapper(config);
        builder.Services.TryAddTransient<IEntityMapper, AutoMapperEntityMapper>();
    }
}