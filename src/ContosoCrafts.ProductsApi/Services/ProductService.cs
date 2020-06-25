using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.ProductsApi.Models;

namespace ContosoCrafts.ProductsApi.Services
{
    public class ProductService : IProductService
    {
        public Task AddRating(string productId, int rating)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProducts()
        {
            throw new NotImplementedException();
        }
    }

    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProducts();
        Task AddRating(string productId, int rating);
    }
}
