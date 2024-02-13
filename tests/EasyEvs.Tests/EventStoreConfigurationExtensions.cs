namespace EasyEvs.Tests;

using System;
using System.Collections.Generic;
using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class EventStoreConfigurationExtensions
{
    internal static IServiceCollection ConfigureEventStoreTestsDbWithLogging(
        this IServiceCollection services
    )
    {
        Dictionary<string, string> dict =
            new()
            {
                { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" },
                { "EasyEvs:SubscriptionGroup", "easy-evs-tests" }
            };

        IConfiguration conf = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        services
            .AddLogging(configure =>
            {
                configure.SetMinimumLevel(LogLevel.Trace);
                configure.AddConsole();
            })
            .AddSingleton<IStreamNames, StreamNames>()
            .AddSingleton(conf);

        return services;
    }

    internal static EventStoreSettings GetEventStoreSettings(this IServiceProvider sp) =>
        sp.GetRequiredService<IConfiguration>().GetSection("EasyEvs").Get<EventStoreSettings>()!;
}
