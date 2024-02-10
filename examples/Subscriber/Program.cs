namespace Subscriber;

using System;
using System.Threading.Tasks;
using Actions;
using EasyEvs;
using EasyEvs.Contracts;
using Microsoft.Extensions.DependencyInjection;

static class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services
            .ConfigureEventStoreDbWithLogging()
            .AddEasyEvs(sp => sp.GetEventStoreSettings())
            .WithPipeline<UserMetricsPipeline>()
            .AddEasyEvsAggregates();

        var provider = services.BuildServiceProvider();
        var eventStore = provider.GetRequiredService<IEventStore>();
        //This assumes
        await eventStore.SubscribeToStream("$ce-user");
        Console.ReadKey();
    }
}
