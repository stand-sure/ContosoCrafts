using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoCrafts.GrainInterfaces;
using ContosoCrafts.Models;
using ContosoCrafts.WebSite.Events;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using Orleans;

namespace ContosoCrafts.WebSite.Components
{
    public class ProductListBase : ComponentBase
    {
        [Inject] protected IClusterClient ClusterClient { get; set; }

        [Inject] private IEventAggregator EventAggregator { get; set; }

        protected IEnumerable<Product> products;
        protected Product selectedProduct;
        protected string selectedProductId;
        
        protected override async Task OnInitializedAsync()
        {
            // load products
            var productService = ClusterClient.GetGrain<IProductService>("0");
            products ??= await productService.GetProducts();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // load cart
                var cache = ClusterClient.GetGrain<ISiloCache>(Constants.CACHE_GRAIN_KEY);
                var cartData = await cache.GetCartCache();
                await EventAggregator.PublishAsync(new ShoppingCartUpdated {ItemCount = cartData.Count});
            }
        }

        protected async Task SelectProduct(string productId)
        {
            var productService = ClusterClient.GetGrain<IProductService>("0");
            selectedProductId = productId;
            selectedProduct = await productService.GetSingle(productId);
        }

        protected async Task SubmitRating(int rating)
        {
            var productService = ClusterClient.GetGrain<IProductService>("0");
            await productService.AddRating(selectedProductId, rating);
            await SelectProduct(selectedProductId);
            StateHasChanged();
        }

        protected async Task AddToCart(string productId, string title)
        {
            var cache = ClusterClient.GetGrain<ISiloCache>(Constants.CACHE_GRAIN_KEY);

            // Check for existing cart data
            var cartData = await cache.GetCartCache();
            if (cartData == null || !cartData.Any())
            {
                // Empty cart
                cartData = new List<CartItem> {new CartItem {Title = title, Quantity = 1, ProductId = productId}};
            }
            else
            {
                var oldItem = cartData.SingleOrDefault(ci => ci.ProductId.Equals(productId));
                if (oldItem != null)
                {
                    // Product already in cart
                    oldItem.Quantity++;
                }
                else
                {
                    // Add product to car
                    cartData.Add(new CartItem {Title = title, Quantity = 1, ProductId = productId});
                }
            }

            // Persist new state
            await cache.UpdateCartCache(cartData);
            await EventAggregator.PublishAsync(new ShoppingCartUpdated {ItemCount = cartData.Count});
        }
    }
}