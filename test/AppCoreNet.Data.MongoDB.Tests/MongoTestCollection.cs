// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Xunit;

namespace AppCoreNet.Data.MongoDB;

[CollectionDefinition(Name)]
public class MongoTestCollection : ICollectionFixture<MongoTestFixture>
{
    public const string Name = "MongoDB";
}