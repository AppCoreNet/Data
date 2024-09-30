using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AppCoreNet.Data;

public abstract class TransactionManagerTests
{
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>())
                     .Returns(Substitute.For<ILogger>());

        services.TryAddSingleton(loggerFactory);
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
    }

    protected virtual ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    protected abstract ITransactionManager GetTransactionManager();
}