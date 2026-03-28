using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Services.Implementations
{
    public class ProductService : IProductService
    {
        private static readonly HashSet<string> AllowedStorageCapacities = new(StringComparer.OrdinalIgnoreCase)
        {
            "64GB", "128GB", "256GB", "512GB", "1TB"
        };

        private const decimal MinPrice = 1000000m;
        private const decimal MaxPrice = 100000000m;
        private const int MaxStockQuantity = 10000;
        private const int MinProductNameLength = 3;
        private const int MaxProductNameLength = 120;
        private const int MinModelLength = 1;
        private const int MaxModelLength = 60;
        private const int MaxColorLength = 30;

        private readonly IProductRepository _productRepository;

        public ProductService()
        {
            _productRepository = new ProductRepository();
        }

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }
        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public Product? GetProductById(int id)
        {
            if (id <= 0)
                throw new Exception("Invalid product ID.");

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

            if (string.IsNullOrWhiteSpace(product.UrlImages))
                throw new Exception("Image URL cannot be empty.");

            if (ProductNameExists(product.ProductName, excludeProductId: null))
                throw new Exception("A product with this name already exists.");

            _productRepository.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.ProductId <= 0)
                throw new Exception("Invalid product ID.");

            if (string.IsNullOrWhiteSpace(product.UrlImages))
                throw new Exception("Image URL cannot be empty.");

            if (ProductNameExists(product.ProductName, excludeProductId: product.ProductId))
                throw new Exception("A product with this name already exists.");

            _productRepository.UpdateProduct(product);
        }

        public void DeleteProduct(int id)
        {
            if (id <= 0)
                throw new Exception("Invalid product ID.");

            _productRepository.DeleteProduct(id);
        }

        public void UpdateStockQuantity(int productId, int newQuantity)
        {
            if (productId <= 0)
                throw new Exception("Invalid product ID.");

            if (newQuantity < 0)
                throw new Exception("Stock quantity cannot be negative.");

            _productRepository.UpdateStockQuantity(productId, newQuantity);
        }

        private void ValidateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // New products must be tied to a valid staff. Updates may fix legacy rows (StaffId was 0).
            if (product.ProductId == 0 && product.StaffId <= 0)
                throw new Exception("Invalid staff for product create/update.");
            if (product.ProductId > 0 && product.StaffId <= 0)
                throw new Exception("Invalid staff ID for product update.");

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new Exception("Product name cannot be empty.");

            product.ProductName = product.ProductName.Trim();
            if (product.ProductName.Length < MinProductNameLength || product.ProductName.Length > MaxProductNameLength)
                throw new Exception($"Product name must be between {MinProductNameLength} and {MaxProductNameLength} characters.");

            if (string.IsNullOrWhiteSpace(product.Model))
                throw new Exception("Model cannot be empty.");

            product.Model = product.Model.Trim();
            if (product.Model.Length < MinModelLength || product.Model.Length > MaxModelLength)
                throw new Exception($"Model must be between {MinModelLength} and {MaxModelLength} characters.");

            if (!Regex.IsMatch(product.Model, @"^[\p{L}\d\s\-]+$"))
                throw new Exception("Model cannot contain special characters.");

            if (string.IsNullOrWhiteSpace(product.Color))
                throw new Exception("Color cannot be empty.");

            product.Color = product.Color.Trim();
            if (product.Color.Length > MaxColorLength)
                throw new Exception($"Color cannot exceed {MaxColorLength} characters.");

            if (!Regex.IsMatch(product.Color, @"^[\p{L}\d\s\-]+$"))
                throw new Exception("Color may only contain letters, digits, spaces, or hyphens.");

            if (string.IsNullOrWhiteSpace(product.StorageCapacity))
                throw new Exception("Storage capacity cannot be empty.");

            product.StorageCapacity = product.StorageCapacity.Trim().ToUpperInvariant();
            if (!AllowedStorageCapacities.Contains(product.StorageCapacity))
                throw new Exception("Invalid storage capacity. Allowed values: 64GB, 128GB, 256GB, 512GB, 1TB.");

            if (product.Price < MinPrice || product.Price > MaxPrice)
                throw new Exception($"Price must be between {MinPrice:N0} and {MaxPrice:N0} VND.");

            if (decimal.Truncate(product.Price) != product.Price)
                throw new Exception("Price must be a whole number.");

            string priceInvariant = product.Price.ToString(CultureInfo.InvariantCulture);
            if (!Regex.IsMatch(priceInvariant, @"^\d+$"))
                throw new Exception("Price may only contain digits (0-9), with no commas, dots, or other characters.");

            if (product.StockQuantity < 0)
                throw new Exception("Invalid stock quantity.");

            if (product.StockQuantity > MaxStockQuantity)
                throw new Exception($"Stock quantity cannot exceed {MaxStockQuantity}.");
        }

        private bool ProductNameExists(string productName, int? excludeProductId)
        {
            string name = productName.Trim();
            return _productRepository.GetAllProducts().Any(p =>
                string.Equals((p.ProductName ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase)
                && (excludeProductId == null || p.ProductId != excludeProductId.Value));
        }
    }
}
