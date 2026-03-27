using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class ProductDAO
    {
        public List<Product> GetAllProducts()
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Products
                .AsNoTracking()
                .OrderBy(p => p.ProductName)
                .ToList();
        }

        public Product? GetProductById(int id)
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Products
                .AsNoTracking()
                .FirstOrDefault(p => p.ProductId == id);
        }

        public List<Product> SearchProducts(string keyword)
        {
            using var context = new IPhoneInventoryDbContext();
            var query = context.Products.AsNoTracking().AsQueryable();

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
            using var context = new IPhoneInventoryDbContext();
            var query = context.Products.AsNoTracking().AsQueryable();

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

        public List<string> GetAllColors()
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Products
                .AsNoTracking()
                .Where(p => !string.IsNullOrWhiteSpace(p.Color))
                .Select(p => p.Color!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public List<string> GetAllStorageCapacities()
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Products
                .AsNoTracking()
                .Where(p => !string.IsNullOrWhiteSpace(p.StorageCapacity))
                .Select(p => p.StorageCapacity!)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public void AddProduct(Product product)
        {
            using var context = new IPhoneInventoryDbContext();

            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            context.Products.Add(product);
            context.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            using var context = new IPhoneInventoryDbContext();

            var existingProduct = context.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
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

            context.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            using var context = new IPhoneInventoryDbContext();

            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }

            product.Status = false;
            product.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }

        public void UpdateStockQuantity(int productId, int newQuantity)
        {
            if (newQuantity < 0)
            {
                throw new Exception("Số lượng tồn kho không được âm.");
            }

            using var context = new IPhoneInventoryDbContext();

            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }

            product.StockQuantity = newQuantity;
            product.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }
    }
}