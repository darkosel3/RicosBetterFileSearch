using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RicosBetterFileSearch.Modules.Folders.Application.UseCases;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Application.UseCases;

namespace RicosBetterFileSearch.WPF.ViewModels;

public partial class FoldersViewModel : BaseViewModel
{
    private readonly FolderUseCases _folderUseCases;
    private readonly IndexingUseCases _indexingUseCases;

    public ObservableCollection<IndexedFolder> Folders { get; } = new();

    [ObservableProperty]
    private string _newFolderPath = string.Empty;

    [ObservableProperty]
    private IndexedFolder? _selectedFolder;

    public FoldersViewModel(FolderUseCases folderUseCases, IndexingUseCases indexingUseCases)
    {
        _folderUseCases = folderUseCases;
        _indexingUseCases = indexingUseCases;
        _ = LoadFoldersAsync();
    }

    [RelayCommand]
    private async Task AddFolder()
    {
        if (string.IsNullOrWhiteSpace(NewFolderPath)) return;

        if (!Directory.Exists(NewFolderPath))
        {
            StatusMessage = "Folder ne postoji!";
            return;
        }

        await _folderUseCases.AddFolderAsync(NewFolderPath);
        NewFolderPath = string.Empty;
        await LoadFoldersAsync();
        StatusMessage = "Folder dodat.";
    }

    [RelayCommand]
    private async Task BrowseFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Izaberi folder za indeksiranje"
        };

        if (dialog.ShowDialog() == true)
        {
            NewFolderPath = dialog.FolderName;
        }
    }

    [RelayCommand]
    private async Task RemoveFolder()
    {
        if (SelectedFolder is null) return;
        await _folderUseCases.RemoveFolderAsync(SelectedFolder.Id);
        await LoadFoldersAsync();
        StatusMessage = "Folder uklonjen.";
    }

    [RelayCommand]
    private async Task ScanFolder()
    {
        if (SelectedFolder is null) return;

        IsBusy = true;
        StatusMessage = $"Skeniram {SelectedFolder.FolderName}...";

        var count = await _indexingUseCases.ScanFolderAsync(SelectedFolder.Id);

        await LoadFoldersAsync();
        IsBusy = false;
        StatusMessage = $"Skenirano: {count} fajlova pronadjeno.";
    }

    [RelayCommand]
    private async Task ScanAllFolders()
    {
        IsBusy = true;
        int total = 0;

        foreach (var folder in Folders.Where(f => f.IsActive))
        {
            StatusMessage = $"Skeniram {folder.FolderName}...";
            total += await _indexingUseCases.ScanFolderAsync(folder.Id);
        }

        await LoadFoldersAsync();
        IsBusy = false;
        StatusMessage = $"Skeniranje zavrseno: {total} fajlova ukupno.";
    }

    private async Task LoadFoldersAsync()
    {
        var folders = await _folderUseCases.GetAllFoldersAsync();
        Folders.Clear();
        foreach (var f in folders) Folders.Add(f);
    }
}
