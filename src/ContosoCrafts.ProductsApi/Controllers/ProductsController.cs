using System.Threading.Tasks;
using ContosoCrafts.GrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Streams;

namespace ContosoCrafts.ProductsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public ProductsController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }
        
        [HttpGet]
        public async Task<ActionResult> GetList(int page = 1, int limit = 20)
        {
            var grain = _grainFactory.GetGrain<IProductService>("0");
            var result = await grain.GetProducts(page, limit);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetSingle(string id)
        {
            var grain = _grainFactory.GetGrain<IProductService>(nameof(ProductsController));
            var result = await grain.GetSingle(id);
            return Ok(result);
        }

        [HttpPatch]
        public async Task<ActionResult> Patch(RatingRequest request)
        {
            var grain = _grainFactory.GetGrain<IProductService>(nameof(ProductsController));
            await grain.AddRating(request.ProductId, request.Rating);
            return Ok();
        }

        public class RatingRequest
        {
            public string ProductId { get; set; }
            public int Rating { get; set; }
        }
    }
}