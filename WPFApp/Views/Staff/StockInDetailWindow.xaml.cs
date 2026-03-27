using System;
using System.Linq;
using System.Windows;
using Services.Implementations;
using Services.Interfaces;
using WPFApp.ViewModels;

namespace WPFApp.Views.Staff
{
    public partial class StockInDetailWindow : Window
    {
        private readonly IStockInService _stockInService;
        private readonly int _stockInId;

        public StockInDetailWindow(int stockInId)
        {
            InitializeComponent();
            _stockInService = new StockInService();
            _stockInId = stockInId;

            LoadStockInDetail();
        }

        private void LoadStockInDetail()
        {
            try
            {
                var stockIn = _stockInService.GetStockInById(_stockInId);
                if (stockIn == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu nhập.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                txtStockInId.Text = stockIn.StockInId.ToString();
                txtStockInDate.Text = stockIn.StockInDate.ToString("dd/MM/yyyy HH:mm");
                txtCreatedBy.Text = stockIn.CreatedByStaff?.FullName ?? "N/A";
                txtNote.Text = string.IsNullOrWhiteSpace(stockIn.Note) ? "N/A" : stockIn.Note;

                var detailItems = stockIn.StockInDetails
                    .Select(d => new StockInDetailDisplayItem
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.ProductName ?? "N/A",
                        Quantity = d.Quantity,
                        ImportPrice = d.ImportPrice
                    })
                    .ToList();

                dgDetails.ItemsSource = detailItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Detail Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}