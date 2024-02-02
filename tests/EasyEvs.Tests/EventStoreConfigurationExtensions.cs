namespace EasyEvs.Tests;

using Contracts;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

internal static class EventStoreConfigurationExtensions
{
    internal static IServiceCollection ConfigureEventStoreDb(this IServiceCollection services)
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
            .AddSingleton(conf);

        return services;
    }

    internal static EventStoreSettings GetEventStoreSettings(this IServiceProvider sp) =>
        sp.GetRequiredService<IConfiguration>().GetSection("EasyEvs").Get<EventStoreSettings>()!;
}
