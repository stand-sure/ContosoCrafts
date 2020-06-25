using System.Threading.Tasks;
using ContosoCrafts.ProductsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoCrafts.ProductsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        private readonly IProductService _productService;

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var result = await _productService.GetProducts();
            return Ok(result);
        }

        [HttpPatch]
        public ActionResult Patch([FromBody] RatingRequest request)
        {
            _productService.AddRating(request.ProductId, request.Rating);

            return Ok();
        }

        public class RatingRequest
        {
            public string ProductId { get; set; }
            public int Rating { get; set; }
        }
    }
}
