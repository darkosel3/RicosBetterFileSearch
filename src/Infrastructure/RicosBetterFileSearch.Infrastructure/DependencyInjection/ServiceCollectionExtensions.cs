using Microsoft.Extensions.DependencyInjection;
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Infrastructure.Persistence.Json;
using RicosBetterFileSearch.Infrastructure.Persistence.Sqlite;
using RicosBetterFileSearch.Infrastructure.Adapters;
using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Folders.Application.UseCases;
using RicosBetterFileSearch.Modules.Indexing.Application.UseCases;
using RicosBetterFileSearch.Modules.Tags.Application.UseCases;
using RicosBetterFileSearch.Modules.Search.Application.UseCases;
using RicosBetterFileSearch.Modules.Tags.Application.UseCases;
using RicosBetterFileSearch.Modules.Statistics.Application.UseCases;

namespace RicosBetterFileSearch.Infrastructure.DependencyInjection;

public enum PersistenceProvider
{
    Json,
    Sqlite
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string dataDirectory,
        bool useFakeFileSystem = false,
        PersistenceProvider persistence = PersistenceProvider.Json)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        switch (persistence)
        {
            case PersistenceProvider.Sqlite:
                var dbPath = Path.Combine(dataDirectory, "ricosearch.db");
                services.AddSingleton<IRepository<IndexedFolder>>(new SqliteRepository<IndexedFolder>(dbPath));
                services.AddSingleton<IRepository<FileEntry>>(new SqliteRepository<FileEntry>(dbPath));
                services.AddSingleton<IRepository<FileTag>>(new SqliteRepository<FileTag>(dbPath));
                services.AddSingleton<IRepository<FileTagAssignment>>(new SqliteRepository<FileTagAssignment>(dbPath));
                services.AddSingleton<IRepository<SearchHistory>>(new SqliteRepository<SearchHistory>(dbPath));
                break;

            case PersistenceProvider.Json:
            default:
                services.AddSingleton<IRepository<IndexedFolder>>(new JsonRepository<IndexedFolder>(dataDirectory));
                services.AddSingleton<IRepository<FileEntry>>(new JsonRepository<FileEntry>(dataDirectory));
                services.AddSingleton<IRepository<FileTag>>(new JsonRepository<FileTag>(dataDirectory));
                services.AddSingleton<IRepository<FileTagAssignment>>(new JsonRepository<FileTagAssignment>(dataDirectory));
                services.AddSingleton<IRepository<SearchHistory>>(new JsonRepository<SearchHistory>(dataDirectory));
                break;
        }

        if (useFakeFileSystem)
            services.AddSingleton<IFileSystemService, FakeFileSystemService>();
        else
            services.AddSingleton<IFileSystemService, RealFileSystemService>();

        services.AddTransient<FolderUseCases>();
        services.AddTransient<IndexingUseCases>();
        services.AddTransient<TagUseCases>();
        services.AddTransient<SearchUseCases>();
        services.AddTransient<StatisticsUseCases>();

        return services;
    }
}

