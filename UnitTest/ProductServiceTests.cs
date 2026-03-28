using BusinessObjects;
using Services.Implementations;
using Xunit;

namespace UnitTest
{
    public class ProductServiceTests
    {
        private readonly FakeProductRepository _fakeProductRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _fakeProductRepository = new FakeProductRepository();
            _productService = new ProductService(_fakeProductRepository);
        }

        private Product CreateValidProduct()
        {
            return new Product
            {
                ProductId = 0,
                StaffId = 1,
                ProductName = "iPhone 15 Pro Max",
                Model = "A3108",
                Color = "Black",
                StorageCapacity = "256GB",
                Price = 30000000,
                StockQuantity = 10,
                UrlImages = "https://example.com/iphone15promax.jpg",
                Status = true
            };
        }

        [Fact]
        public void CreateProduct_EmptyImageUrl_ThrowsException()
        {
            var product = CreateValidProduct();
            product.UrlImages = "";

            var ex = Assert.Throws<Exception>(() => _productService.CreateProduct(product));

            Assert.Equal("Image URL cannot be empty.", ex.Message);
        }

        [Fact]
        public void UpdateProduct_EmptyImageUrl_ThrowsException()
        {
            var product = CreateValidProduct();
            product.ProductId = 1;
            product.UrlImages = " ";

            var ex = Assert.Throws<Exception>(() => _productService.UpdateProduct(product));

            Assert.Equal("Image URL cannot be empty.", ex.Message);
        }

        [Fact]
        public void CreateProduct_DuplicateName_ThrowsException()
        {
            _fakeProductRepository.Products.Add(CreateValidProduct());

            var newProduct = CreateValidProduct();
            newProduct.ProductName = "iPhone 15 Pro Max";

            var ex = Assert.Throws<Exception>(() => _productService.CreateProduct(newProduct));

            Assert.Equal("A product with this name already exists.", ex.Message);
        }

        [Fact]
        public void CreateProduct_ValidProduct_AddsSuccessfully()
        {
            var product = CreateValidProduct();

            _productService.CreateProduct(product);

            Assert.Equal(1, _fakeProductRepository.Products.Count);
            Assert.Equal("iPhone 15 Pro Max", _fakeProductRepository.Products[0].ProductName);
        }
    }
}