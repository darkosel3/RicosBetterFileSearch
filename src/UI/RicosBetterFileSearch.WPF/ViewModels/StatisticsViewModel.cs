using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RicosBetterFileSearch.Modules.Statistics.Application.UseCases;
using RicosBetterFileSearch.Modules.Statistics.Application.DTOs;

namespace RicosBetterFileSearch.WPF.ViewModels;

public partial class StatisticsViewModel : BaseViewModel
{
    private readonly StatisticsUseCases _statisticsUseCases;

    [ObservableProperty]
    private int _totalFiles;

    [ObservableProperty]
    private int _totalFolders;

    [ObservableProperty]
    private string _totalSize = "0 B";

    [ObservableProperty]
    private string _largestFile = "-";

    public ObservableCollection<ExtensionStat> ExtensionStats { get; } = new();
    public ObservableCollection<FolderStat> FolderStats { get; } = new();

    public StatisticsViewModel(StatisticsUseCases statisticsUseCases)
    {
        _statisticsUseCases = statisticsUseCases;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsBusy = true;
        var stats = await _statisticsUseCases.GetStatisticsAsync();

        TotalFiles = stats.TotalFiles;
        TotalFolders = stats.TotalFolders;
        TotalSize = FormatBytes(stats.TotalSizeBytes);
        LargestFile = stats.LargestFileSize > 0
            ? $"{stats.LargestFileName} ({FormatBytes(stats.LargestFileSize)})"
            : "-";

        ExtensionStats.Clear();
        foreach (var kvp in stats.FilesByExtension.OrderByDescending(x => x.Value))
            ExtensionStats.Add(new ExtensionStat(kvp.Key, kvp.Value));

        FolderStats.Clear();
        foreach (var kvp in stats.FilesByFolder.OrderByDescending(x => x.Value))
            FolderStats.Add(new FolderStat(kvp.Key, kvp.Value));

        IsBusy = false;
        StatusMessage = "Statistika osvezena.";
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}

public record ExtensionStat(string Extension, int Count);
public record FolderStat(string FolderName, int Count);
