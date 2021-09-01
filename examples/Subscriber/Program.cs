namespace Subscriber
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyEvs.Contracts;
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
            var configuration = new EasyEvsDependencyInjectionConfiguration()
            {
                Assemblies = new []{typeof(Program).Assembly},
                StreamResolver = typeof(StreamResolver)
            };
            services
                .AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddConsole())
                .AddSingleton((IConfiguration)conf)
                .AddEasyEvs(configuration);

            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            //This assumes 
            await eventStore.SubscribeToStream("$ce-user");
            Console.ReadKey();
        }
    }
}
