namespace Subscriber
{
    using System;
    using System.Threading.Tasks;
    using Actions;
    using Configuration.Extensions.EnvironmentFile;
    using EasyEvs.Contracts;
    using EasyEvs;
    using Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static async Task Main(string[] args)
        {
            var conf = new ConfigurationBuilder().AddEnvironmentFile().Build();
            var services = new ServiceCollection();
            var configuration = new EasyEvsDependencyInjectionConfiguration()
            {
                Assemblies = new []{typeof(Program).Assembly},
                StreamResolver = typeof(StreamResolver)
            };
            services
                .AddScoped<IPipelineHandlesEventAction<UserRegistered>, UserMetricsPipeline>()
                .AddScoped<IPipelineHandlesEventAction<UserUpdated>, UserMetricsPipeline>()
                .AddScoped<IPipelineHandlesEventAction<UserDeleted>, UserMetricsPipeline>()
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
