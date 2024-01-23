using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

internal class MongoDataProviderOptions
{
    public MongoClientSettings ClientSettings { get; set; } = new ();

    public string Database { get; set; } = "db";

    public MongoDatabaseSettings? DatabaseSettings { get; set; }

    public Type? EntityMapperType { get; set; }

    public Type? TokenGeneratorType { get; set; }

    public ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}