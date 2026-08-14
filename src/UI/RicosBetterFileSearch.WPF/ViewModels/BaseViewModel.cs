using CommunityToolkit.Mvvm.ComponentModel;

namespace RicosBetterFileSearch.WPF.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;
}
