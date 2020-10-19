using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.GrainInterfaces;
using ContosoCrafts.Models;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace ContosoCrafts.Grains
{
    [StatelessWorker(10)]
    public class SiloCacheGrain : Grain, ISiloCache
    {
        private readonly IPersistentState<List<CartItem>> _cartState;
        private readonly IPersistentState<Dictionary<int,SiloEntry>> _siloState;

        public SiloCacheGrain(
            [PersistentState("cart", "contosoCartStore")] IPersistentState<List<CartItem>> cartState,
            [PersistentState("silo", "contosoSiloStore")] IPersistentState<Dictionary<int,SiloEntry>> siloState
        )
        {
            _cartState = cartState;
            _siloState = siloState;
        }

        public async Task UpdateCartCache(List<CartItem> entries)
        {
            _cartState.State = entries;
            await _cartState.WriteStateAsync();
        }
        
        public async Task UpdateSiloCache(Dictionary<int,SiloEntry> entries)
        {
            _siloState.State = entries;
            await _siloState.WriteStateAsync();
        }

        public Task ClearCartCacheEntry() => _cartState.ClearStateAsync();
        
        public Task ClearSiloCacheEntry() => _siloState.ClearStateAsync();

        public async Task<List<CartItem>> GetCartCache()
        {
            await _cartState.ReadStateAsync();
            return _cartState.State;
        }
        
        public async Task<Dictionary<int,SiloEntry>> GetSiloCache()
        {
            await _siloState.ReadStateAsync();
            return _siloState.State;
        }
    }
}