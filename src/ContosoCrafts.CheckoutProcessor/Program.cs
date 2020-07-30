using ContosoCrafts.CheckoutProcessor.Services;
using ContosoCrafts.CheckoutProcessor.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using Steeltoe.Discovery.Client;

namespace ContosoCrafts.CheckoutProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddServiceDiscovery()
                .ConfigureServices((hostContext, services) =>
                {
                    services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                    // RabbitMQ services
                    services.AddSingleton<IConnectionFactory, ConnectionFactory>(provider =>
                    {
                        return new ConnectionFactory
                        {
                            VirtualHost = Constants.RABBITMQ_VHOST,
                            HostName = "rabbitmq_service",
                            UserName = "demo",
                            Password = "demo"
                        };
                    });

                    services.AddSingleton<ObjectPool<IModel>>(provider =>
                    {
                        var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
                        var cf = provider.GetRequiredService<IConnectionFactory>();
                        var policy = new RabbitModelPooledObjectPolicy(cf);

                        return poolProvider.Create(policy);
                    });

                    services.AddTransient<RabbitMQBus>();

                    // Worker services
                    services.AddHostedService<BootstrapWorker>();
                    services.AddHostedService<ProcessorWorker>();
                });
    }
}
