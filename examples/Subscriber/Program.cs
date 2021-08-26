namespace Subscriber
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyEvs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static async Task Main(string[] args)
        {
            var dict = new Dictionary<string, string>()
            {
                {"EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false"}
            };

            var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            var services = new ServiceCollection();
            services
                .AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddConsole())
                .AddSingleton((IConfiguration)conf)
                .AddEasyEvs(assemblies: typeof(Program).Assembly);

            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            await eventStore.SubscribeToStream("$ce-user");
            Console.ReadKey();
        }
    }
}
