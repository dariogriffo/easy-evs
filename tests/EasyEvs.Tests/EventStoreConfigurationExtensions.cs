namespace EasyEvs.Tests;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class EventStoreConfigurationExtensions
{
    internal static IServiceCollection ConfigureEventStoreDb(this IServiceCollection services)
    {
        Dictionary<string, string> dict =
            new() { { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" } };

        IConfiguration conf = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        services.AddLogging(configure => configure.AddConsole()).AddSingleton(conf);

        return services;
    }
}
