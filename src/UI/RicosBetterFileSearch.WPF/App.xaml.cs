using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RicosBetterFileSearch.Infrastructure.DependencyInjection;
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Folders.Domain.Events;
using RicosBetterFileSearch.WPF.ViewModels;

namespace RicosBetterFileSearch.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Data directory za JSON persistenciju
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RicosBetterFileSearch", "data");

        // Infrastructure registracija (false = koristi pravi FS, true = fake)
        services.AddInfrastructure(dataDir, useFakeFileSystem: false);

        // ViewModels
        services.AddTransient<FoldersViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<TagsViewModel>();
        services.AddTransient<StatisticsViewModel>();

        // MainWindow
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();

        // Registruj event handlere (inter-module komunikacija)
        RegisterEventHandlers();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void RegisterEventHandlers()
    {
        var eventBus = Services.GetRequiredService<IEventBus>();

        // Kad se folder skenira, loguj event (Statistics modul reaguje)
        eventBus.Subscribe<FolderScannedEvent>(e =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"[EVENT] FolderScanned: {e.FolderPath} - {e.FilesFound} files found");
        });
    }
}
