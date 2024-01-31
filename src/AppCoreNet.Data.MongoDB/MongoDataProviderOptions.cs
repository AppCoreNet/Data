using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

public class MongoDataProviderOptions
{
    public MongoClientSettings ClientSettings { get; set; } = new ();

    public string Database { get; set; } = "db";

    public MongoDatabaseSettings? DatabaseSettings { get; set; }

    internal Type? EntityMapperType { get; set; }

    internal Type? TokenGeneratorType { get; set; }

    internal ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}