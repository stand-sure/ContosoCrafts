using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.Models;
using Orleans;

namespace ContosoCrafts.GrainInterfaces
{
    public interface ICheckoutService : IGrainWithStringKey
    {
        Task ProcessCheckout(IEnumerable<CartItem> items);
    }
}