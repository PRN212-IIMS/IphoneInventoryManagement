using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAllProducts();
        Product? GetProductById(int id);
        List<Product> SearchProducts(string keyword);
        List<Product> FilterProducts(string? color, string? storageCapacity, bool? status);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
        void UpdateStockQuantity(int productId, int newQuantity);
        List<string> GetAllColors();
        List<string> GetAllStorageCapacities();
    }
}
