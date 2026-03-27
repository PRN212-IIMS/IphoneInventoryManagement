using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;

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
            if (id <= 0)
                throw new Exception("ID sản phẩm không hợp lệ.");

            return _productRepository.GetProductById(id);
        }

        public List<Product> SearchProducts(string keyword)
        {
            return _productRepository.SearchProducts(keyword ?? string.Empty);
        }

        public List<Product> FilterProducts(string? color, string? storageCapacity, bool? status)
        {
            return _productRepository.FilterProducts(color, storageCapacity, status);
        }

        public List<string> GetAllColors()
        {
            return _productRepository.GetAllColors();
        }

        public List<string> GetAllStorageCapacities()
        {
            return _productRepository.GetAllStorageCapacities();
        }

        public void CreateProduct(Product product)
        {
            ValidateProduct(product);
            _productRepository.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.ProductId <= 0)
                throw new Exception("ID sản phẩm không hợp lệ.");

            ValidateProduct(product);
            _productRepository.UpdateProduct(product);
        }

        public void DeleteProduct(int id)
        {
            if (id <= 0)
                throw new Exception("ID sản phẩm không hợp lệ.");

            _productRepository.DeleteProduct(id);
        }

        public void UpdateStockQuantity(int productId, int newQuantity)
        {
            if (productId <= 0)
                throw new Exception("ID sản phẩm không hợp lệ.");

            if (newQuantity < 0)
                throw new Exception("Số lượng tồn kho không được âm.");

            _productRepository.UpdateStockQuantity(productId, newQuantity);
        }

        private void ValidateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new Exception("Tên sản phẩm không được để trống.");

            if (string.IsNullOrWhiteSpace(product.Model))
                throw new Exception("Model không được để trống.");

            if (string.IsNullOrWhiteSpace(product.Color))
                throw new Exception("Màu sắc không được để trống.");

            if (string.IsNullOrWhiteSpace(product.StorageCapacity))
                throw new Exception("Dung lượng không được để trống.");

            if (product.Price <= 0)
                throw new Exception("Giá sản phẩm phải lớn hơn 0.");

            if (product.StockQuantity < 0)
                throw new Exception("Số lượng tồn kho không hợp lệ.");
        }
    }
}