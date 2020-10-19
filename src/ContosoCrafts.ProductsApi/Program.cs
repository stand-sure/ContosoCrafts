using System;
using System.IO;
using ContosoCrafts.Grains;
using ContosoCrafts.Grains.Placement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Placement;
using Serilog;

namespace ContosoCrafts.ProductsApi
{
    public class Program
    {
        private static string HOST_ENVIRONMENT =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

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
                    var orleansConfig = Configuration.GetSection("Orleans");
                    
                    siloBuilder
                        .ConfigureDefaults()
                        .AddStartupTask<SiloRegistrationStartup>()
                        .UseRedisClustering(opt =>
                        {
                            var redisHost = orleansConfig["RedisCluster:Host"];
                            var redisPort = orleansConfig["RedisCluster:Port"];
                            var redisPassword = orleansConfig["RedisCluster:Password"];

                            opt.ConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                            opt.Database = 0;
                        })
                        .Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(2))
                        .Configure<ClusterOptions>(options =>
                        {
                            var clusterId = orleansConfig["ClusterId"];
                            var serviceId = orleansConfig["ServiceId"];

                            options.ClusterId = clusterId;
                            options.ServiceId = serviceId;
                        })
                        .Configure<ClusterMembershipOptions>(options =>
                        {
                            options.DefunctSiloCleanupPeriod = TimeSpan.FromMinutes(2);
                            options.DefunctSiloExpiration = TimeSpan.FromMinutes(1);
                        })
                        .ConfigureEndpoints(siloPort: orleansConfig.GetValue<int>("SiloPort"),
                                            gatewayPort: orleansConfig.GetValue<int>("GatewayPort"),
                                            listenOnAnyHostAddress: true)
                        .AddSimpleMessageStreamProvider("simple-stream", options =>
                        {
                            options.FireAndForgetDelivery = true;
                            options.OptimizeForImmutableData = true;
                        })
                        .AddRedisGrainStorage("contosoCartStore", options =>
                        {
                            var redisHost = orleansConfig["RedisStorage:Host"];
                            var redisPort = orleansConfig["RedisStorage:Port"];
                            var redisPassword = orleansConfig["RedisStorage:Password"];

                            options.DataConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                            options.UseJson = true;

                            options.DatabaseNumber = 1;
                        })
                        .AddRedisGrainStorage("contosoSiloStore", options =>
                        {
                            var redisHost = orleansConfig["RedisStorage:Host"];
                            var redisPort = orleansConfig["RedisStorage:Port"];
                            var redisPassword = orleansConfig["RedisStorage:Password"];

                            options.DataConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                            options.UseJson = true;

                            options.DatabaseNumber = 2;
                        })
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(ProductGrain).Assembly).WithReferences());
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingletonNamedService<PlacementStrategy, CategoryPlacementStrategy>(
                        nameof(CategoryPlacementStrategy));
                    services.AddSingletonKeyedService<Type, IPlacementDirector, CategoryPlacementStrategyDirector>(
                        typeof(CategoryPlacementStrategy));
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}