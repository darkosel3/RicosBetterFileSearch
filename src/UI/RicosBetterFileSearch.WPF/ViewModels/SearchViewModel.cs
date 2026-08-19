using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RicosBetterFileSearch.Modules.Search.Application.UseCases;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Tags.Application.UseCases;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;

namespace RicosBetterFileSearch.WPF.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly SearchUseCases _searchUseCases;
    private readonly TagUseCases _tagUseCases;

    public ObservableCollection<FileEntry> SearchResults { get; } = new();
    public ObservableCollection<SearchHistory> History { get; } = new();
    public ObservableCollection<FileTag> AvailableTags { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _extensionFilter = string.Empty;

    [ObservableProperty]
    private FileTag? _selectedTag;

    public SearchViewModel(SearchUseCases searchUseCases, TagUseCases tagUseCases)
    {
        _searchUseCases = searchUseCases;
        _tagUseCases = tagUseCases;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        IsBusy = true;
        var query = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery;
        var ext = string.IsNullOrWhiteSpace(ExtensionFilter) ? null : ExtensionFilter;
        var tagId = SelectedTag?.Id;

        var results = await _searchUseCases.SearchFilesAsync(query, ext, tagId);

        SearchResults.Clear();
        foreach (var r in results) SearchResults.Add(r);

        await LoadHistoryAsync();
        IsBusy = false;
        StatusMessage = $"{SearchResults.Count} rezultata";
    }

    [RelayCommand]
    private async Task ClearHistory()
    {
        await _searchUseCases.ClearHistoryAsync();
        History.Clear();
        StatusMessage = "Istorija obrisana.";
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        SearchQuery = string.Empty;
        ExtensionFilter = string.Empty;
        SelectedTag = null;
        SearchResults.Clear();
        StatusMessage = string.Empty;
    }

    public async Task RefreshTagsAsync() { await LoadTagsAsync(); }

    private async Task LoadDataAsync()
    {
        await LoadHistoryAsync();
        await LoadTagsAsync();
    }

    private async Task LoadHistoryAsync()
    {
        var history = await _searchUseCases.GetSearchHistoryAsync();
        History.Clear();
        foreach (var h in history) History.Add(h);
    }

    private async Task LoadTagsAsync()
    {
        var tags = await _tagUseCases.GetAllTagsAsync();
        AvailableTags.Clear();
        foreach (var t in tags) AvailableTags.Add(t);
    }
}

