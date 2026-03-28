using BusinessObjects;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
    public class FakeProductRepository : IProductRepository
    {
        public List<Product> Products { get; set; } = new();

        public List<Product> GetAllProducts()
        {
            return Products.ToList();
        }

        public Product? GetProductById(int id)
        {
            return Products.FirstOrDefault(p => p.ProductId == id);
        }

        public List<Product> SearchProducts(string keyword)
        {
            keyword ??= string.Empty;
            return Products
                .Where(p => (p.ProductName ?? string.Empty)
                .Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Product> FilterProducts(string? color, string? storageCapacity, bool? status)
        {
            IEnumerable<Product> query = Products;

            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(p => string.Equals(p.Color, color, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(storageCapacity))
                query = query.Where(p => string.Equals(p.StorageCapacity, storageCapacity, StringComparison.OrdinalIgnoreCase));

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return query.ToList();
        }

        public void AddProduct(Product product)
        {
            if (product.ProductId == 0)
                product.ProductId = Products.Count == 0 ? 1 : Products.Max(p => p.ProductId) + 1;

            Products.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            var existing = GetProductById(product.ProductId);
            if (existing == null) return;

            existing.ProductName = product.ProductName;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;
            existing.Model = product.Model;
            existing.Color = product.Color;
            existing.StorageCapacity = product.StorageCapacity;
            existing.UrlImages = product.UrlImages;
            existing.Status = product.Status;
            existing.StaffId = product.StaffId;
        }

        public void DeleteProduct(int id)
        {
            var product = GetProductById(id);
            if (product != null)
                Products.Remove(product);
        }

        public void UpdateStockQuantity(int productId, int newQuantity)
        {
            var product = GetProductById(productId);
            if (product != null)
                product.StockQuantity = newQuantity;
        }

        public List<string> GetAllColors()
        {
            return Products
                .Where(p => !string.IsNullOrWhiteSpace(p.Color))
                .Select(p => p.Color!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public List<string> GetAllStorageCapacities()
        {
            return Products
                .Where(p => !string.IsNullOrWhiteSpace(p.StorageCapacity))
                .Select(p => p.StorageCapacity!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}