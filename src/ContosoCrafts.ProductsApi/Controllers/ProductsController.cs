using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoCrafts.ProductsApi.Models;
using ContosoCrafts.ProductsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.ProductsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        public ProductsController(IProductService productService)
        {
            ProductService = productService;
        }

        public IProductService ProductService { get; }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var result = await ProductService.GetProducts();
            return Ok(result);
        }

        [HttpPatch]
        public ActionResult Patch([FromBody] RatingRequest request)
        {
            ProductService.AddRating(request.ProductId, request.Rating);

            return Ok();
        }

        public class RatingRequest
        {
            public string ProductId { get; set; }
            public int Rating { get; set; }
        }
    }
}
