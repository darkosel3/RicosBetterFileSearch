using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RicosBetterFileSearch.Modules.Search.Application.UseCases;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.WPF.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly SearchUseCases _searchUseCases;

    public ObservableCollection<FileEntry> SearchResults { get; } = new();
    public ObservableCollection<SearchHistory> History { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _extensionFilter = string.Empty;

    public SearchViewModel(SearchUseCases searchUseCases)
    {
        _searchUseCases = searchUseCases;
        _ = LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        IsBusy = true;
        var ext = string.IsNullOrWhiteSpace(ExtensionFilter) ? null : ExtensionFilter;
        var results = await _searchUseCases.SearchFilesAsync(SearchQuery, ext);

        SearchResults.Clear();
        foreach (var r in results) SearchResults.Add(r);

        await LoadHistoryAsync();
        IsBusy = false;
        StatusMessage = $"{SearchResults.Count} rezultata za '{SearchQuery}'";
    }

    [RelayCommand]
    private async Task ClearHistory()
    {
        await _searchUseCases.ClearHistoryAsync();
        History.Clear();
        StatusMessage = "Istorija obrisana.";
    }

    private async Task LoadHistoryAsync()
    {
        var history = await _searchUseCases.GetSearchHistoryAsync();
        History.Clear();
        foreach (var h in history) History.Add(h);
    }
}
