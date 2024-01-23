using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCoreNet.Data.MongoDB;

public class MongoEntity : IEntity<Guid>
{
    public Guid Id { get; }

    object IEntity.Id => Id;
}

[Collection(MongoCollection.Name)]
public class MongoDataProviderTests
{
    private readonly MongoFixture _mongo;

    public MongoDataProviderTests(MongoFixture mongo)
    {
        _mongo = mongo;
    }

    [Fact]
    public async Task ClientCanConnectAsync()
    {

    }
}