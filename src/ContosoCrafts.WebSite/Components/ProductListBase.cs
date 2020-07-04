using System.Linq;
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
        private IEventAggregator _eventAggregator { get; set; }

        protected Product selectedProduct;
        protected string selectedProductId;


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
            // persist state in dapr
            //
            await _eventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = 10 });
        }
    }
}
