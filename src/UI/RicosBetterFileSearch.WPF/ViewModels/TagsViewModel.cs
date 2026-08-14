using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RicosBetterFileSearch.Modules.Tags.Application.UseCases;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Application.UseCases;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.WPF.ViewModels;

public partial class TagsViewModel : BaseViewModel
{
    private readonly TagUseCases _tagUseCases;
    private readonly IndexingUseCases _indexingUseCases;

    public ObservableCollection<FileTag> Tags { get; } = new();
    public ObservableCollection<FileEntry> Files { get; } = new();
    public ObservableCollection<FileTag> SelectedFileTags { get; } = new();

    [ObservableProperty]
    private string _newTagName = string.Empty;

    [ObservableProperty]
    private string _newTagColor = "#3498db";

    [ObservableProperty]
    private FileTag? _selectedTag;

    [ObservableProperty]
    private FileEntry? _selectedFile;

    public TagsViewModel(TagUseCases tagUseCases, IndexingUseCases indexingUseCases)
    {
        _tagUseCases = tagUseCases;
        _indexingUseCases = indexingUseCases;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task CreateTag()
    {
        if (string.IsNullOrWhiteSpace(NewTagName)) return;
        await _tagUseCases.CreateTagAsync(NewTagName, NewTagColor);
        NewTagName = string.Empty;
        await LoadTagsAsync();
        StatusMessage = "Tag kreiran.";
    }

    [RelayCommand]
    private async Task DeleteTag()
    {
        if (SelectedTag is null) return;
        await _tagUseCases.DeleteTagAsync(SelectedTag.Id);
        await LoadTagsAsync();
        StatusMessage = "Tag obrisan.";
    }

    [RelayCommand]
    private async Task AssignTag()
    {
        if (SelectedFile is null || SelectedTag is null) return;
        await _tagUseCases.AssignTagToFileAsync(SelectedFile.Id, SelectedTag.Id);
        await LoadTagsForFile();
        StatusMessage = $"Tag '{SelectedTag.Name}' dodeljen fajlu '{SelectedFile.FileName}'.";
    }

    [RelayCommand]
    private async Task RemoveTagFromFile()
    {
        if (SelectedFile is null || SelectedTag is null) return;
        await _tagUseCases.RemoveTagFromFileAsync(SelectedFile.Id, SelectedTag.Id);
        await LoadTagsForFile();
        StatusMessage = "Tag uklonjen sa fajla.";
    }

    partial void OnSelectedFileChanged(FileEntry? value)
    {
        if (value is not null)
            _ = LoadTagsForFile();
    }

    private async Task LoadDataAsync()
    {
        await LoadTagsAsync();
        await LoadFilesAsync();
    }

    private async Task LoadTagsAsync()
    {
        var tags = await _tagUseCases.GetAllTagsAsync();
        Tags.Clear();
        foreach (var t in tags) Tags.Add(t);
    }

    private async Task LoadFilesAsync()
    {
        var files = await _indexingUseCases.GetAllFilesAsync();
        Files.Clear();
        foreach (var f in files) Files.Add(f);
    }

    private async Task LoadTagsForFile()
    {
        if (SelectedFile is null) return;
        var tags = await _tagUseCases.GetTagsForFileAsync(SelectedFile.Id);
        SelectedFileTags.Clear();
        foreach (var t in tags) SelectedFileTags.Add(t);
    }
}
