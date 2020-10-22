using System.Collections.Generic;

namespace ContosoCrafts.WebSite.Models
{
    public class ShoppingCartUpdated
    {
        public int ItemCount { get; set; }
    }

    public class CheckoutStarted { }
    
    public class CheckoutSubmitted
    {
        public IEnumerable<CartItem> Items { get; set; }
    }
}