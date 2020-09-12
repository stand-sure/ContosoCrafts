using System;
using System.IO;
using ContosoCrafts.CheckoutProcessor.Services;
using ContosoCrafts.CheckoutProcessor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using Serilog;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Management.Tracing;

namespace ContosoCrafts.CheckoutProcessor
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration, "Serilog")
                .CreateLogger();

            try
            {
                Log.ForContext<Program>().Information("Starting host");
                var host = CreateHostBuilder(args).Build();
                var hostEnv = host.Services.GetRequiredService<IHostEnvironment>();
                Log.ForContext<Program>().Information($"Host Environment: {hostEnv.EnvironmentName}");
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.Information("Host stopped");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .AddServiceDiscovery(opts => opts.UseEureka())                
                .ConfigureServices((hostContext, services) =>
                {
                    services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                    // RabbitMQ services
                    services.AddSingleton<IConnectionFactory, ConnectionFactory>(provider =>
                    {
                        var rabbitConfig = Configuration.GetSection("rabbitmq");
                        
                        return new ConnectionFactory
                        {
                            VirtualHost = Constants.RABBITMQ_VHOST,
                            HostName = rabbitConfig.GetValue<string>("HostName"),  //"rabbitmq_service",
                            UserName = rabbitConfig.GetValue<string>("UserName"), //"demo",
                            Password = rabbitConfig.GetValue<string>("Password"), //"demo",
                            DispatchConsumersAsync = true
                        };
                    });

                    services.AddSingleton<ObjectPool<IModel>>(provider =>
                    {
                        var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
                        var cf = provider.GetRequiredService<IConnectionFactory>();
                        var policy = new RabbitModelPooledObjectPolicy(cf);

                        return poolProvider.Create(policy);
                    });

                     services.AddDistributedTracing(Configuration,
                         builder => builder.UseZipkinWithTraceOptions(services));

                    // Worker services
                   // services.AddHostedService<BootstrapWorker>();
                   // services.AddHostedService<ProcessorWorker>();
                });
    }
}
