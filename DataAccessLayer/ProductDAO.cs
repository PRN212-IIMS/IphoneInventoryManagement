using BusinessObjects;

namespace DataAccessLayer
{
    public class ProductDAO
    {
        private readonly IPhoneInventoryDbContext _context;

        public ProductDAO()
        {
            _context = new IPhoneInventoryDbContext();
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products
                .OrderBy(p => p.ProductName)
                .ToList();
        }

        public Product? GetProductById(int id)
        {
            return _context.Products.FirstOrDefault(p => p.ProductId == id);
        }

        public List<Product> SearchProducts(string keyword)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(keyword) ||
                    (p.Model != null && p.Model.Contains(keyword)) ||
                    (p.Color != null && p.Color.Contains(keyword)) ||
                    (p.StorageCapacity != null && p.StorageCapacity.Contains(keyword)));
            }

            return query.OrderBy(p => p.ProductName).ToList();
        }

        public List<Product> FilterProducts(string? color, string? storageCapacity, bool? status)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(p => p.Color == color);
            }

            if (!string.IsNullOrWhiteSpace(storageCapacity))
            {
                query = query.Where(p => p.StorageCapacity == storageCapacity);
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            return query.OrderBy(p => p.ProductName).ToList();
        }

        public void AddProduct(Product product)
        {
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            var existingProduct = _context.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (existingProduct == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }

            existingProduct.StaffId = product.StaffId;
            existingProduct.ProductName = product.ProductName;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.Model = product.Model;
            existingProduct.Color = product.Color;
            existingProduct.StorageCapacity = product.StorageCapacity;
            existingProduct.UrlImages = product.UrlImages;
            existingProduct.Status = product.Status;
            existingProduct.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public void UpdateStockQuantity(int productId, int newQuantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }

            product.StockQuantity = newQuantity;
            product.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
        }
    }
}