// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides options for the <see cref="MongoDataProvider"/>.
/// </summary>
public sealed class MongoDataProviderOptions
{
    private static readonly Func<IServiceProvider, IEntityMapper> _defaultEntityMapperFactory =
        static sp => sp.GetRequiredService<IEntityMapper>();

    private static readonly Func<IServiceProvider, ITokenGenerator> _defaultTokenGeneratorFactory =
        static sp => sp.GetRequiredService<ITokenGenerator>();

    /// <summary>
    /// Gets or sets the <see cref="MongoClientSettings"/> used by the data provider.
    /// </summary>
    public MongoClientSettings ClientSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string DatabaseName { get; set; } = "db";

    /// <summary>
    /// Gets or sets the <see cref="MongoDatabaseSettings"/> used by the data provider.
    /// </summary>
    public MongoDatabaseSettings? DatabaseSettings { get; set; }

    internal Func<IServiceProvider, IEntityMapper> EntityMapperFactory { get; set; } = _defaultEntityMapperFactory;

    internal Func<IServiceProvider, ITokenGenerator> TokenGeneratorFactory { get; set; } = _defaultTokenGeneratorFactory;

    internal ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}