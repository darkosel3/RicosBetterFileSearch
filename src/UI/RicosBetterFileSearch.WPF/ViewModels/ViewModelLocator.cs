using Microsoft.Extensions.DependencyInjection;

namespace RicosBetterFileSearch.WPF.ViewModels;

public static class ViewModelLocator
{
    public static FoldersViewModel Folders => App.Services.GetRequiredService<FoldersViewModel>();
    public static SearchViewModel Search => App.Services.GetRequiredService<SearchViewModel>();
    public static TagsViewModel Tags => App.Services.GetRequiredService<TagsViewModel>();
    public static StatisticsViewModel Statistics => App.Services.GetRequiredService<StatisticsViewModel>();
}
