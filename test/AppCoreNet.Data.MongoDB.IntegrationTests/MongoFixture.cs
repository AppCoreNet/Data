using System.Threading.Tasks;
using Testcontainers.MongoDb;
using Xunit;

namespace AppCoreNet.Data.MongoDB;

public class MongoTestFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder().Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}