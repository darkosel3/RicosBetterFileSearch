using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using RicosBetterFileSearch.WPF.ViewModels;

namespace RicosBetterFileSearch.WPF.Views;

public partial class QuickSearchWindow : Window
{
    public QuickSearchWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<QuickSearchViewModel>();
        SearchBox.Focus();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Hide();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && DataContext is QuickSearchViewModel vm && vm.SelectedResult != null)
        {
            OpenFile(vm.SelectedResult.FilePath);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            if (ResultsList.SelectedIndex < ResultsList.Items.Count - 1)
                ResultsList.SelectedIndex++;
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (ResultsList.SelectedIndex > 0)
                ResultsList.SelectedIndex--;
            e.Handled = true;
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Placeholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) 
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is QuickSearchViewModel vm && vm.SelectedResult != null)
        {
            OpenFile(vm.SelectedResult.FilePath);
        }
    }

    private void OpenFile(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch { }
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        Hide();
    }
}
