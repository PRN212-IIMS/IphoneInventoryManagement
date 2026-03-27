using System.Collections.ObjectModel;
using System.Linq;

namespace WPFApp
{
    public static class CartStore
    {
        public static ObservableCollection<CartItemViewModel> Items { get; } = new();

        public static void AddItem(CartItemViewModel item)
        {
            var existing = Items.FirstOrDefault(x => x.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public static void RemoveItem(CartItemViewModel item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
            }
        }

        public static decimal GetTotalAmount()
        {
            return Items.Sum(x => x.Total);
        }

        public static void Clear()
        {
            Items.Clear();
        }
    }
}