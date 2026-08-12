namespace RicosBetterFileSearch.Modules.Statistics.Application.DTOs;

public class StatisticsResult
{
    public int TotalFiles { get; set; }
    public int TotalFolders { get; set; }
    public long TotalSizeBytes { get; set; }
    public Dictionary<string, int> FilesByExtension { get; set; } = new();
    public Dictionary<string, int> FilesByFolder { get; set; } = new();
    public string LargestFileName { get; set; } = string.Empty;
    public long LargestFileSize { get; set; }
}
