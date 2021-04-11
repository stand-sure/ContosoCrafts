using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ContosoCrafts.Web.Shared.Events;
using ContosoCrafts.Web.Shared.Models;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace ContosoCrafts.Web.Client.Shared
{
    public class ProductListBase : ComponentBase
    {

        [Inject]
        private IEventAggregator EventAggregator { get; set; }

        [Inject]
        private IHttpClientFactory ClientFactory { get; set; }

        protected IEnumerable<Product> products = null;
        protected Product selectedProduct;
        protected string selectedProductId;

        protected override async Task OnInitializedAsync()
        {
            if (products == null)
            {
                var client = ClientFactory.CreateClient("localapi");
                products = await client.GetFromJsonAsync<IEnumerable<Product>>("/api/products");
            }
        }
        protected async Task SelectProduct(string productId)
        {
            selectedProductId = productId;
            var client = ClientFactory.CreateClient("localapi");
            selectedProduct = (await client.GetFromJsonAsync<Product>("/api/products"));
        }

        protected async Task SubmitRating(int rating)
        {
            var client = ClientFactory.CreateClient("localapi");
            await client.PutAsJsonAsync($"/api/products/{selectedProduct}", new { rating = rating });
            await SelectProduct(selectedProductId);
            StateHasChanged();
        }

        protected async Task AddToCart(string productId, string title)
        {
            // get state
            var client = ClientFactory.CreateClient("localapi");
            var resp = await client.GetAsync($"api/state/cart");

            if (!resp.IsSuccessStatusCode) return;

            Dictionary<string, CartItem> state = null;
            if (resp.StatusCode == HttpStatusCode.NoContent)
            {
                // Empty cart
                state = new Dictionary<string, CartItem> { [productId] = new CartItem { Title = title, Quantity = 1 } };
            }
            else if (resp.StatusCode == HttpStatusCode.OK)
            {
                var responseBody = await resp.Content.ReadAsStringAsync();
                state = JsonSerializer.Deserialize<Dictionary<string, CartItem>>(responseBody);
                if (state.ContainsKey(productId))
                {
                    // Product already in cart
                    CartItem selectedItem = state[productId];
                    selectedItem.Quantity++;
                    state[productId] = selectedItem;
                }
                else
                {
                    // Add product to car
                    state[productId] = new CartItem { Title = title, Quantity = 1 };
                }
            }

            // persist state in dapr           
            await client.PostAsJsonAsync($"/api/state/cart", state);
            await EventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = state.Keys.Count });
        }
    }

}