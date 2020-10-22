using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ContosoCrafts.WebSite.Components;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            services.AddHttpClient();
            services.AddControllers();
            services.AddMemoryCache();

            services.AddScoped<IEventAggregator, EventAggregator.Blazor.EventAggregator>();
            services.AddTransient<IProductService, JsonFileProductService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();

            app.UseStaticFiles();

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
            });
        }
    }
}
