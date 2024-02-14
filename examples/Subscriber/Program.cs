namespace Subscriber;

using System;
using System.Threading.Tasks;
using EasyEvs;
using EasyEvs.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Pipelines;

static class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services
            .ConfigureEventStoreDbWithLogging()
            .AddEasyEvs(
                sp => sp.GetEventStoreSettings(),
                c => c.AssembliesToScanForHandlers = [typeof(Handler).Assembly]
            )
            .WithPipeline<UserMetricsPipeline>()
            .AddEasyEvsAggregates();

        var provider = services.BuildServiceProvider();
        var eventStore = provider.GetRequiredService<IEventStore>();
        await eventStore.SubscribeToStream("$ce-user");
        Console.ReadKey();
    }
}
