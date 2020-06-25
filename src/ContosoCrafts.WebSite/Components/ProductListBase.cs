using System.Linq;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Components;

namespace ContosoCrafts.WebSite.Components
{
    public class ProductListBase : ComponentBase
    {
        [Inject]
        protected JsonFileProductService ProductService { get; set; }

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
    }
}
