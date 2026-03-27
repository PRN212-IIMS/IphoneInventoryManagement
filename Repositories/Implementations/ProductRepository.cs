using BusinessObjects;
using DataAccessLayer;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDAO _productDAO;

        public ProductRepository()
        {
            _productDAO = new ProductDAO();
        }

        public List<Product> GetAllProducts()
        {
            return _productDAO.GetAllProducts();
        }

        public Product? GetProductById(int id)
        {
            return _productDAO.GetProductById(id);
        }

        public List<Product> SearchProducts(string keyword)
        {
            return _productDAO.SearchProducts(keyword);
        }

        public List<Product> FilterProducts(string? color, string? storageCapacity, bool? status)
        {
            return _productDAO.FilterProducts(color, storageCapacity, status);
        }

        public void AddProduct(Product product)
        {
            _productDAO.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            _productDAO.UpdateProduct(product);
        }

        public void DeleteProduct(int id)
        {
            _productDAO.DeleteProduct(id);
        }

        public void UpdateStockQuantity(int productId, int newQuantity)
        {
            _productDAO.UpdateStockQuantity(productId, newQuantity);
        }
    }
}