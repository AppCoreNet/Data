using System;
using AppCoreNet.Data.MongoDB;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Extensions.DependencyInjection;

public static class MongoDataProviderBuilderExtensions
{
    public static MongoDataProviderBuilder AddMongoDB(this IDataProvidersBuilder builder, string name, Action<MongoDataProviderOptions>? configure)
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddOptions();

        // TODO: builder.Services.AddLogging();
        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        builder.AddProvider<MongoDataProvider>(
            name,
            ServiceLifetime.Singleton,
            sp =>
            {
                var services = MongoDataProviderServices.Create(name, sp);
                return new MongoDataProvider(name, services);
            });

        return new MongoDataProviderBuilder(name, builder.Services);
    }
}