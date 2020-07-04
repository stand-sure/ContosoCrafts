using ContosoCrafts.WebSite.Services;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<IEventAggregator, EventAggregator.Blazor.EventAggregator>();
            services.AddSingleton<JsonFileProductService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler("/Error");
            // See https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

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
