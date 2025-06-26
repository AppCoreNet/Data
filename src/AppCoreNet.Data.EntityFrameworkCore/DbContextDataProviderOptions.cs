// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace AppCoreNet.Data.EntityFrameworkCore;

internal sealed class DbContextDataProviderOptions
{
    private static readonly Func<IServiceProvider, IEntityMapper> _defaultEntityMapperFactory =
        static sp => sp.GetRequiredService<IEntityMapper>();

    private static readonly Func<IServiceProvider, ITokenGenerator> _defaultTokenGeneratorFactory =
        static sp => sp.GetRequiredService<ITokenGenerator>();

    public Func<IServiceProvider, IEntityMapper> EntityMapperFactory { get; set; } = _defaultEntityMapperFactory;

    public Func<IServiceProvider, ITokenGenerator> TokenGeneratorFactory { get; set; } = _defaultTokenGeneratorFactory;

    public ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}