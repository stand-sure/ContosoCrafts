using System;
using System.IO;
using ContosoCrafts.Grains;
using ContosoCrafts.Grains.Placement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Placement;
using Serilog;

namespace ContosoCrafts.CheckoutProcessor
{
    public class Program
    {
        private static string HOST_ENVIRONMENT = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{HOST_ENVIRONMENT}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Environment", HOST_ENVIRONMENT)
                .ReadFrom.Configuration(Configuration, "Serilog")
                .CreateLogger();

            try
            {
                Log.ForContext<Program>().Information("Starting host");
                Log.ForContext<Program>().Information($"Environment {HOST_ENVIRONMENT}");
                CreateHostBuilder(args).Build().Run();
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
                 .UseOrleans(siloBuilder =>
                {
                    siloBuilder
                        .ConfigureDefaults()
                        .AddStartupTask<SiloRegistrationStartup>()
                        .UseRedisClustering(opt =>
                        {
                            var redisHost = Configuration["Orleans:RedisCluster:Host"];
                            var redisPort = Configuration["Orleans:RedisCluster:Port"];
                            var redisPassword = Configuration["Orleans:RedisCluster:Password"];
                            
                            opt.ConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                            opt.Database = 0;
                        })
                        .Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(2))
                        .Configure<ClusterOptions>(options =>
                        {
                            var clusterId = Configuration["Orleans:ClusterId"];
                            var serviceId = Configuration["Orleans:ServiceId"];
                            
                            options.ClusterId = clusterId;
                            options.ServiceId = serviceId;
                        })
                        .Configure<ClusterMembershipOptions>(options =>
                        {
                            options.DefunctSiloCleanupPeriod = TimeSpan.FromMinutes(2);
                            options.DefunctSiloExpiration = TimeSpan.FromMinutes(1);
                        })
                        .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000, listenOnAnyHostAddress: true)
                        .AddSimpleMessageStreamProvider("simple-stream", options =>
                        {
                            options.FireAndForgetDelivery = true;
                            options.OptimizeForImmutableData = true;
                        })
                        .AddRedisGrainStorage("contosoCartStore", options =>
                        {
                            var redisHost = Configuration["Orleans:RedisStorage:Host"];
                            var redisPort = Configuration["Orleans:RedisStorage:Port"];
                            var redisPassword = Configuration["Orleans:RedisStorage:Password"];

                            options.DataConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                            options.UseJson = true;

                            options.DatabaseNumber = 1;
                        })
                        .AddRedisGrainStorage("contosoSiloStore", options =>
                        {
                            var redisHost = Configuration["Orleans:RedisStorage:Host"];
                            var redisPort = Configuration["Orleans:RedisStorage:Port"];
                            var redisPassword = Configuration["Orleans:RedisStorage:Password"];

                            options.DataConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                            options.UseJson = true;

                            options.DatabaseNumber = 2;
                        })
                        .ConfigureApplicationParts(parts => 
                            parts.AddApplicationPart(typeof(CheckoutGrain).Assembly).WithReferences());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingletonNamedService<PlacementStrategy, CategoryPlacementStrategy>(
                        nameof(CategoryPlacementStrategy));
                    services.AddSingletonKeyedService<Type, IPlacementDirector, CategoryPlacementStrategyDirector>(
                        typeof(CategoryPlacementStrategy));
                });
    }
}
