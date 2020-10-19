using ContosoCrafts.GrainInterfaces;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;

namespace ContosoCrafts.WebSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddHealthChecks();
            services.AddScoped<IEventAggregator, EventAggregator.Blazor.EventAggregator>();

            services.AddSingleton(sp =>
            {
                var client = new ClientBuilder()
                    .Configure<ClusterOptions>(options =>
                    {
                        var clusterId = Configuration["Orleans:ClusterId"];
                        var serviceId = Configuration["Orleans:ServiceId"];
                            
                        options.ClusterId = clusterId;
                        options.ServiceId = serviceId;
                    })
                    .UseRedisClustering(opt =>
                    {
                        var redisHost = Configuration["Orleans:RedisCluster:Host"];
                        var redisPort = Configuration["Orleans:RedisCluster:Port"];
                        var redisPassword = Configuration["Orleans:RedisCluster:Password"];
                            
                        opt.ConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
                        opt.Database = 0;
                            
                    })
                    .AddSimpleMessageStreamProvider("simple-stream", (SimpleMessageStreamProviderOptions options) =>
                    {
                        options.FireAndForgetDelivery = true;
                        options.OptimizeForImmutableData = true;
                    })
                    .ConfigureApplicationParts(parts => 
                        parts. AddApplicationPart(typeof(IProductService).Assembly))
                    .Build();
                
                client.Connect().GetAwaiter().GetResult();
                return client;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler("/Error");

            app.UseHsts();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
            });
        }
    }
}