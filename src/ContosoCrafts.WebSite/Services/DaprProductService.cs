using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ContosoCrafts.WebSite.Models;

namespace ContosoCrafts.WebSite.Services
{
    public class DaprProductService : IProductService
    {
        private readonly IHttpClientFactory clientFactory;

        public DaprProductService(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        public Task AddRating(string productId, int rating)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var client = clientFactory.CreateClient("dapr");
            var resp = await client.GetAsync("/v1.0/invoke/productsapi/method/products");

            if (!resp.IsSuccessStatusCode)
            {
                // probably log some stuff here
                return Enumerable.Empty<Product>();
            }
            var contentStream = await resp.Content.ReadAsStreamAsync();
            var products = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(contentStream,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return products;
        }
    }
}