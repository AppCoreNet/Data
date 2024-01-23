using Xunit;

namespace AppCoreNet.Data.MongoDB;

[CollectionDefinition(Name)]
public class MongoCollection : ICollectionFixture<MongoFixture>
{
    public const string Name = "MongoDB";
}