using BusinessObjects;
using Repositories;
using Repositories.Interfaces;
using Services.Interfaces;
using Repositories.Implementations;
using Repositories.Interfaces;
namespace Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService()
        {
            _productRepository = new ProductRepository();
        }

        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public Product? GetProductById(int id)
        {
            return _productRepository.GetProductById(id);
        }

        public List<Product> SearchProducts(string keyword)
        {
            return _productRepository.SearchProducts(keyword);
        }

        public List<Product> FilterProducts(string? color, string? storageCapacity, bool? status)
        {
            return _productRepository.FilterProducts(color, storageCapacity, status);
        }

        public void CreateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new Exception("Tên sản phẩm không được để trống.");

            if (product.Price < 0)
                throw new Exception("Giá sản phẩm không hợp lệ.");

            if (product.StockQuantity < 0)
                throw new Exception("Số lượng tồn kho không hợp lệ.");

            _productRepository.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.ProductId <= 0)
                throw new Exception("ID sản phẩm không hợp lệ.");

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new Exception("Tên sản phẩm không được để trống.");

            if (product.Price < 0)
                throw new Exception("Giá sản phẩm không hợp lệ.");

            if (product.StockQuantity < 0)
                throw new Exception("Số lượng tồn kho không hợp lệ.");

            _productRepository.UpdateProduct(product);
        }

        public void DeleteProduct(int id)
        {
            if (id <= 0)
                throw new Exception("ID sản phẩm không hợp lệ.");

            _productRepository.DeleteProduct(id);
        }
    }
}