using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using RicosBetterFileSearch.Infrastructure.DependencyInjection;
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Folders.Domain.Events;
using RicosBetterFileSearch.WPF.ViewModels;
using RicosBetterFileSearch.WPF.Views;

namespace RicosBetterFileSearch.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private QuickSearchWindow? _quickSearch;

    // Win32 hotkey
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 9000;
    private const uint MOD_CTRL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_F = 0x46;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RicosBetterFileSearch", "data");

        services.AddInfrastructure(dataDir, useFakeFileSystem: false);

        // ViewModels
        services.AddTransient<FoldersViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<TagsViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<QuickSearchViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();

        RegisterEventHandlers();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Registruj global hotkey: Ctrl+Shift+F
        var helper = new System.Windows.Interop.WindowInteropHelper(mainWindow);
        RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL | MOD_SHIFT, VK_F);

        System.Windows.Interop.ComponentDispatcher.ThreadPreprocessMessage += (ref System.Windows.Interop.MSG msg, ref bool handled) =>
        {
            if (msg.message == 0x0312 && msg.wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleQuickSearch();
                handled = true;
            }
        };
    }

    private void ToggleQuickSearch()
    {
        if (_quickSearch == null || !_quickSearch.IsLoaded)
        {
            _quickSearch = new QuickSearchWindow();
        }

        if (_quickSearch.IsVisible)
        {
            _quickSearch.Hide();
        }
        else
        {
            _quickSearch.Show();
            _quickSearch.Activate();
        }
    }

    private void RegisterEventHandlers()
    {
        var eventBus = Services.GetRequiredService<IEventBus>();

        eventBus.Subscribe<FolderScannedEvent>(e =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"[EVENT] FolderScanned: {e.FolderPath} - {e.FilesFound} files found");
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var mainWindow = Services.GetRequiredService<MainWindow>();
        var helper = new System.Windows.Interop.WindowInteropHelper(mainWindow);
        UnregisterHotKey(helper.Handle, HOTKEY_ID);
        base.OnExit(e);
    }
}
