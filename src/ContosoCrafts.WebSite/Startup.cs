using System;
using ContosoCrafts.WebSite.Services;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Polly;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Management.Tracing;

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
                .AddTransientHttpErrorPolicy(policy =>
                    policy.WaitAndRetryAsync(1, (retry, ctx) => TimeSpan.FromSeconds(retry * 2),
                        (msg, ts, count, ctx) =>
                        {
                            Log.ForContext<SteeltoeProductService>().Warning("Retrying request attempt: {attempt}", count);
                        }))
                .AddTypedClient<IProductService, SteeltoeProductService>();

            services.AddStackExchangeRedisCache(options =>
            {
                
                var redisConfig = Configuration.GetSection("redis");

                options.ConfigurationOptions = new ConfigurationOptions
                {
                    Password = redisConfig.GetValue<string>("password"),
                    EndPoints = { redisConfig.GetValue<string>("endpoint") }
                };

                // This allows partitioning a single backend cache for use with multiple apps/services.
                options.InstanceName = "ContosoRedis";
            });

            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.AddSingleton<IConnectionFactory, ConnectionFactory>(provider =>
            {
                var rabbitConfig = Configuration.GetSection("rabbitmq");

                return new ConnectionFactory
                {
                    VirtualHost = Constants.RABBITMQ_VHOST,
                    HostName = rabbitConfig.GetValue<string>("HostName"),
                    UserName = rabbitConfig.GetValue<string>("UserName"),
                    Password = rabbitConfig.GetValue<string>("Password"),
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

            services.AddHealthChecks();
            services.AddControllers();
            services.AddScoped<IEventAggregator, EventAggregator.Blazor.EventAggregator>();

            services.AddDistributedTracing(Configuration, builder => builder.UseZipkinWithTraceOptions(services));
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
