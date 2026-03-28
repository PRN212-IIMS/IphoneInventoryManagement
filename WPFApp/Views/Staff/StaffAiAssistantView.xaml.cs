using System;
using System.Windows;
using System.Windows.Controls;
using Services.Implementations;
using Services.Interfaces;

namespace WPFApp.Views.Staff;

public partial class StaffAiAssistantView : UserControl
{
    private const string ReadyMessage = "AI Assistant san sang. Ban co the hoi ve ton kho, don hang, phieu nhap hoac loc san pham.";

    private readonly IAiStaffAssistantService _assistantService;

    public StaffAiAssistantView()
    {
        InitializeComponent();
        _assistantService = new AiStaffAssistantService();
        txtAnswer.Text = ReadyMessage;
    }

    private async void btnAsk_Click(object sender, RoutedEventArgs e)
    {
        var question = txtQuestion.Text.Trim();
        if (string.IsNullOrWhiteSpace(question))
        {
            MessageBox.Show("Vui long nhap cau hoi truoc khi gui cho AI Assistant.", "AI Assistant", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        btnAsk.IsEnabled = false;
        txtAnswer.Text = "Dang kiem tra du lieu trong database va tao cau tra loi...";

        try
        {
            var answer = await _assistantService.AskAsync(question);
            txtAnswer.Text = answer;
        }
        catch (Exception ex)
        {
            txtAnswer.Text = ex.Message;
            MessageBox.Show(ex.Message, "AI Assistant", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            btnAsk.IsEnabled = true;
        }
    }

    private void btnClear_Click(object sender, RoutedEventArgs e)
    {
        txtQuestion.Clear();
        txtAnswer.Text = ReadyMessage;
    }
}
