using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;

namespace ContosoCrafts.Web.Server.Services
{
    public class DaprStateService : IStateService
    {
        private readonly IHttpClientFactory clientFactory;
        public DaprStateService(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }
    }
}