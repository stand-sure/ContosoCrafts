using System.Threading;
using System.Threading.Tasks;
using ContosoCrafts.GrainInterfaces;
using ContosoCrafts.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace ContosoCrafts.ProductsApi
{
    public class SiloRegistrationStartup : IStartupTask
    {
        private readonly Silo _silo;
        private readonly IGrainFactory _grainFactory;
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<SiloRegistrationStartup> _logger;

        public SiloRegistrationStartup(Silo silo, IGrainFactory grainFactory, IConfiguration configuration,
            IHostApplicationLifetime lifetime, ILogger<SiloRegistrationStartup> logger)
        {
            _silo = silo;
            _grainFactory = grainFactory;
            _configuration = configuration;
            _lifetime = lifetime;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var siloName = _configuration["Orleans:SiloName"];
            var cache = _grainFactory.GetGrain<ISiloCache>(Constants.CACHE_GRAIN_KEY);
            var siloCache = await cache.GetSiloCache();

            var hashCode = _silo.SiloAddress.GetHashCode();

            if (siloCache.ContainsKey(hashCode))
            {
                siloCache.Remove(hashCode);
            }

            // Todo: add clean up by SiloName 

            var siloEntry = new SiloEntry(hashCode, siloName,
                _silo.SiloAddress.IsClient, new[] {"products"});
            
            siloCache.Add(hashCode, siloEntry);

            await cache.UpdateSiloCache(siloCache);

            _lifetime.ApplicationStopping.Register(obj =>
            {
                if (!siloCache.ContainsKey(hashCode)) return;
                if (!siloCache.Remove(hashCode)) return;
                
                _logger.LogInformation($"{hashCode} entry removed");
                cache.UpdateSiloCache(siloCache).GetAwaiter().GetResult();
            }, false);
        }
    }
}