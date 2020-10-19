using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoCrafts.GrainInterfaces;
using ContosoCrafts.Grains.Placement;
using ContosoCrafts.Models;
using Microsoft.Extensions.Logging;
using Orleans;

namespace ContosoCrafts.Grains
{
    [CategoryPlacementStrategy("checkout")]
    public class CheckoutGrain: Grain, ICheckoutService
    {
        private readonly ILogger<CheckoutGrain> _logger;

        public CheckoutGrain(ILogger<CheckoutGrain> logger)
        {
            _logger = logger;
            
        }
        
        public Task ProcessCheckout(IEnumerable<CartItem> items)
        {
            _logger.LogInformation("Message Received on the channel");
            _logger.LogInformation("Received {OrderItemCount} items", items.Count());
            return Task.CompletedTask;
        }
    }
}