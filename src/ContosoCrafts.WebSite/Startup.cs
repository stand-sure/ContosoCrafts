using ContosoCrafts.WebSite.Services;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.Http.Discovery;

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
            services.AddHttpClient("discovery",
                c =>
                {
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                    c.DefaultRequestHeaders.Add("User-Agent", "contoso-app");
                })
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>()
                .AddTypedClient<IProductService, SteeltoeProductService>();
            services.AddStackExchangeRedisCache(options =>
            {                
                options.ConfigurationOptions.Password = "S0m3P@$$w0rd";
                options.ConfigurationOptions.EndPoints.Add("redis_service:6379");   

                // This allows partitioning a single backend cache for use with multiple apps/services.
                options.InstanceName = "ContosoRedis";             
            });
            services.AddHealthChecks();
            services.AddControllers();
            services.AddScoped<IEventAggregator, EventAggregator.Blazor.EventAggregator>();
            // services.AddSingleton<IProductService, JsonFileProductService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler("/Error");

            app.UseHsts();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
            });
        }
    }
}
