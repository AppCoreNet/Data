// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides options for the <see cref="MongoDataProvider"/>.
/// </summary>
public sealed class MongoDataProviderOptions
{
    /// <summary>
    /// Gets or sets the <see cref="MongoClientSettings"/> used by the data provider.
    /// </summary>
    public MongoClientSettings ClientSettings { get; set; } = new ();

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string Database { get; set; } = "db";

    /// <summary>
    /// Gets or sets the <see cref="MongoDatabaseSettings"/> used by the data provider.
    /// </summary>
    public MongoDatabaseSettings? DatabaseSettings { get; set; }

    internal Type? EntityMapperType { get; set; }

    internal Type? TokenGeneratorType { get; set; }

    internal ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}