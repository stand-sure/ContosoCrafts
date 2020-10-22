using System.Threading.Tasks;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoCrafts.WebSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        public ProductsController(JsonFileProductService productService)
        {
            ProductService = productService;
        }

        public JsonFileProductService ProductService { get; }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var products = await ProductService.GetProducts();
            return Ok(products);
        }

        [HttpPatch]
        public async Task<ActionResult> Patch([FromBody] RatingRequest request)
        {
            await ProductService.AddRating(request.ProductId, request.Rating);

            return Ok();
        }
    }
}