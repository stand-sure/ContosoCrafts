using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.CheckoutProcessor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DaprController : ControllerBase
    {
        private readonly ILogger<DaprController> logger;
        public DaprController(ILogger<DaprController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("subscribe")]
        public ActionResult Subscribe()
        {
            var payload = new[]
            {
                new {topic= "checkout", route = "checkout" }
            };
            return Ok(payload);
        }

        [HttpPost("/checkout")]
        public ActionResult CheckoutOrder(CloudEvent cloudEvent)
        {
            logger.LogDebug($"Cloud event {cloudEvent.Id} {cloudEvent.Type} {cloudEvent.DataContentType}");
            logger.LogInformation("Order received...");
            return Ok();
        }
    }
}
