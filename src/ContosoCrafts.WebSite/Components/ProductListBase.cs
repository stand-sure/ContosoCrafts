using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ContosoCrafts.WebSite.Events;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace ContosoCrafts.WebSite.Components
{
    public class ProductListBase : ComponentBase
    {
        [Inject]
        protected JsonFileProductService ProductService { get; set; }

        [Inject]
        private IEventAggregator EventAggregator { get; set; }

        [Inject]
        private IHttpClientFactory ClientFactory { get; set; }

        protected Product selectedProduct;
        protected string selectedProductId;
        protected const string STORE_NAME = "statestore";

        protected void SelectProduct(string productId)
        {
            selectedProductId = productId;
            selectedProduct = ProductService.GetProducts().First(x => x.Id == productId);
        }

        protected void SubmitRating(int rating)
        {
            ProductService.AddRating(selectedProductId, rating);
            SelectProduct(selectedProductId);
            StateHasChanged();
        }

        protected async Task AddToCart(string productId)
        {
            // get state
            var client = ClientFactory.CreateClient("dapr");
            var resp = await client.GetAsync($"v1.0/state/{STORE_NAME}/cart");

            if (!resp.IsSuccessStatusCode) return;

            Dictionary<string, int> state = null;
            if (resp.StatusCode == HttpStatusCode.NoContent)
            {
                state = new Dictionary<string, int> { [productId] = 1 };
            }
            else if (resp.StatusCode == HttpStatusCode.OK)
            {
                var responseBody = await resp.Content.ReadAsStringAsync();
                state = JsonSerializer.Deserialize<Dictionary<string, int>>(responseBody);
                if (state.ContainsKey(productId))
                {
                    state[productId] = state[productId] + 1;
                }
                else
                {
                    state[productId] = 1;
                }
            }

            // persist state in dapr
            var payload = JsonSerializer.Serialize(new[] {
                new { key = "cart", value = state }
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await client.PostAsync($"v1.0/state/{STORE_NAME}", content);
            await EventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = state.Keys.Count });
        }
    }
}
