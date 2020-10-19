using System;
using System.Linq;
using System.Threading.Tasks;
using ContosoCrafts.GrainInterfaces;
using ContosoCrafts.Models;
using Orleans;
using Orleans.Internal;
using Orleans.Placement;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

namespace ContosoCrafts.Grains.Placement
{
    public class CategoryPlacementStrategy : PlacementStrategy
    {
        public string Category { get; }

        public CategoryPlacementStrategy()
        {
            
        }
        public CategoryPlacementStrategy(string category)
        {
            Category = category;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CategoryPlacementStrategyAttribute : PlacementAttribute
    {
        public CategoryPlacementStrategyAttribute(string category) :
            base(new CategoryPlacementStrategy(category))
        {
        }
    }

    public class CategoryPlacementStrategyDirector : IPlacementDirector
    {
        private readonly IGrainFactory _grainFactory;
        private static readonly SafeRandom _random = new SafeRandom();

        public CategoryPlacementStrategyDirector(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        public async Task<SiloAddress> OnAddActivation(PlacementStrategy strategy, PlacementTarget target,
            IPlacementContext context)
        {
            var gps = strategy as CategoryPlacementStrategy;
            var category = gps.Category;

            var cache = _grainFactory.GetGrain<ISiloCache>(Constants.CACHE_GRAIN_KEY);
            var entries = await cache.GetSiloCache();

            var matchedEntries = entries.Where(se => se.Value.Tags.Contains(category))
                .ToDictionary(i => i.Key, i => i.Value);

            var compatibleSilos = context.GetCompatibleSilos(target)
                .OrderBy(s => s).ToArray();

            if (!matchedEntries.Any())
                // try local silo if no categories match
                return context.LocalSilo;

            var result = compatibleSilos.FirstOrDefault(sa => matchedEntries.ContainsKey(sa.GetHashCode()));
            
            // matched entries might contain stale results
            return result ?? context.LocalSilo;
        }
    }
}