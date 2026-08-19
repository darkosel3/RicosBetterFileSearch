using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RicosBetterFileSearch.WPF.ViewModels;

namespace RicosBetterFileSearch.WPF.Views;

public partial class SearchView : UserControl
{
    public SearchView() => InitializeComponent();

    private async void OnIsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true && DataContext is SearchViewModel vm)
            await vm.RefreshTagsAsync();
    }
}
