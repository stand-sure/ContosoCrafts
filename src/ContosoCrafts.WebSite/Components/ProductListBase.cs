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
        protected IProductService ProductService { get; set; }

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
                products = await ProductService.GetProducts();
        }
        protected async Task SelectProduct(string productId)
        {
            selectedProductId = productId;
            selectedProduct = (await ProductService.GetProducts()).First(x => x.Id == productId);
        }

        protected async Task SubmitRating(int rating)
        {
            await ProductService.AddRating(selectedProductId, rating);
            await SelectProduct(selectedProductId);
            StateHasChanged();
        }

        protected Task AddToCart(string productId, string title)
        {
            //TODO: Check for exisiting cart data

            //TODO: Persist new state
            return Task.CompletedTask;
        }
    }
}