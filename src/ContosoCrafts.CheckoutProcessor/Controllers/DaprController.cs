using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.CheckoutProcessor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DaprController : ControllerBase
    {
        [HttpGet("subscribe")]
        public ActionResult Subscribe()
        {
            var payload = new[]
            {
                new {topic= "checkout", route = "checkout" }
            };
            return Ok(payload);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ILogger<CheckoutController> logger;
        public CheckoutController(ILogger<CheckoutController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public ActionResult CheckoutOrder()
        {
            logger.LogInformation("Order received...");
            return Ok();
        }
    }
}
