using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.WPF.ViewModels;

public partial class QuickSearchViewModel : ObservableObject
{
    private readonly IRepository<FileEntry> _fileRepository;

    public ObservableCollection<QuickSearchResult> Results { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _resultCount;

    [ObservableProperty]
    private QuickSearchResult? _selectedResult;

    private System.Timers.Timer? _debounceTimer;

    public QuickSearchViewModel(IRepository<FileEntry> fileRepository)
    {
        _fileRepository = fileRepository;
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            ResultCount = 0;
            return;
        }

        _debounceTimer = new System.Timers.Timer(250);
        _debounceTimer.Elapsed += async (s, e) =>
        {
            _debounceTimer?.Stop();
            await SearchAsync(value);
        };
        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
    }

    private async Task SearchAsync(string query)
    {
        var allFiles = await _fileRepository.GetAllAsync();

        var results = allFiles
            .Where(f => f.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f.FileName.Length)
            .Take(50)
            .Select(f => new QuickSearchResult(f))
            .ToList();

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Results.Clear();
            foreach (var r in results) Results.Add(r);
            ResultCount = results.Count;
            if (results.Count > 0) SelectedResult = results[0];
        });
    }
}

public class QuickSearchResult
{
    public string FileName { get; }
    public string FilePath { get; }
    public string Extension { get; }
    public long SizeInBytes { get; }
    public string FormattedSize { get; }
    public string IconEmoji { get; }

    public QuickSearchResult(FileEntry entry)
    {
        FileName = entry.FileName;
        FilePath = entry.FilePath;
        Extension = entry.Extension;
        SizeInBytes = entry.SizeInBytes;
        FormattedSize = FormatBytes(entry.SizeInBytes);
        IconEmoji = GetIconForExtension(entry.Extension);
    }

    private static string GetIconForExtension(string ext) => ext.ToLower() switch
    {
        ".pdf" => "📕",
        ".doc" or ".docx" => "📘",
        ".xls" or ".xlsx" => "📗",
        ".ppt" or ".pptx" => "📙",
        ".txt" or ".md" => "📝",
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "🖼️",
        ".mp4" or ".avi" or ".mkv" or ".mov" => "🎬",
        ".mp3" or ".wav" or ".flac" or ".ogg" => "🎵",
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "📦",
        ".exe" or ".msi" => "⚙️",
        ".cs" or ".js" or ".ts" or ".py" or ".java" => "💻",
        ".html" or ".css" => "🌐",
        ".json" or ".xml" or ".yaml" => "📋",
        ".sln" or ".csproj" => "🔧",
        _ => "📄"
    };

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1) { order++; size /= 1024; }
        return $"{size:0.#} {sizes[order]}";
    }
}
