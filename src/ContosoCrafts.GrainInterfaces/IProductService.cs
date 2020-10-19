using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.Models;
using Orleans;

namespace ContosoCrafts.GrainInterfaces
{
    public interface IProductService: IGrainWithStringKey
    {
        Task<IEnumerable<Product>> GetProducts(int page = 1, int limit = 20);
        Task AddRating(string productId, int rating);
        Task<Product> GetSingle(string id);
    }
}