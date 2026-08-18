# Rico's Better File Search

Desktop aplikacija za indeksiranje i brzu pretragu fajlova na lokalnom disku. Razvijena kao modularan monolit sa primenom Clean Architecture i Hexagonal Architecture principa.

## Funkcionalnosti

- Indeksiranje foldera sa rekurzivnim skeniranjem fajl sistema
- Brza pretraga indeksiranih fajlova po imenu i ekstenziji
- Tagovanje fajlova sa kolor oznakama
- Istorija pretrage
- Statistika po ekstenzijama, folderima i veličini
- Rico Quick Search — globalni search launcher (Ctrl+Shift+F)

## Arhitektura

Projekat koristi modularni monolit sa Clean Architecture slojevima i Hexagonal (Ports & Adapters) pristupom.

### Moduli

```
SharedKernel          BaseEntity, IRepository<T>, IEventBus, IDomainEvent
Modules.Folders       Upravljanje indeksiranim folderima
Modules.Indexing      Skeniranje fajl sistema, indeksiranje
Modules.Tags          Tagovanje fajlova, upravljanje tagovima
Modules.Search        Pretraga fajlova, istorija pretrage
Modules.Statistics    Agregacija statistike po razlicitim kriterijumima
Infrastructure        JSON i SQLite repozitorijumi, FS adapteri, DI registracija
WPF                   UI shell, ViewModels, Views
```

### Slojevi (po modulu)

```
Domain          Entiteti, Domain eventi
Application     Use case-ovi, Portovi (interfejsi)
Infrastructure  Adapteri (konkretne implementacije portova)
```

### Portovi i adapteri

| Port (interfejs) | Adapter |
|---|---|
| `IRepository<T>` | `JsonRepository<T>`, `SqliteRepository<T>` |
| `IFileSystemService` | `RealFileSystemService`, `FakeFileSystemService` |
| `IEventBus` | `InMemoryEventBus` |

Zamena implementacije se vrsi iskljucivo kroz DI konfiguraciju bez izmene poslovne logike.

### Zavisnosti modula

```
Indexing → Folders
Search → Indexing, Folders
Statistics → Indexing, Folders
Infrastructure → SharedKernel, svi moduli
WPF → Infrastructure, svi moduli
```

## Tehnologije

- .NET 10, WPF, XAML
- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Microsoft.Data.Sqlite
- xUnit
- GitHub Actions (CI)

## Pokretanje

```bash
dotnet restore
dotnet build
dotnet run --project src/UI/RicosBetterFileSearch.WPF
```

## Testovi

```bash
dotnet test
```

## Persistencija

Podrazumevana persistencija je JSON. Za SQLite, u `App.xaml.cs` promeniti:

```csharp
services.AddInfrastructure(dataDir, persistence: PersistenceProvider.Sqlite);
```

## CI/CD

GitHub Actions automatski pokrece build i testove na svakom push-u na master branch.
