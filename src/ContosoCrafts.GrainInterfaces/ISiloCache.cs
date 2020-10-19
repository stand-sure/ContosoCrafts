using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.Models;
using Orleans;

namespace ContosoCrafts.GrainInterfaces
{
    public interface ISiloCache : IGrainWithStringKey
    {
        Task UpdateCartCache(List<CartItem> entries);
        Task UpdateSiloCache(Dictionary<int,SiloEntry> entries);
        
        Task ClearCartCacheEntry();
        Task<List<CartItem>> GetCartCache();

        Task ClearSiloCacheEntry();
        Task<Dictionary<int,SiloEntry>> GetSiloCache();

    }
}