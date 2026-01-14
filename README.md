## Introduction
WWRR Modular Data Load is a .NET8 Background Service for ingesting and processing CSV-based workloads into a PostgreSQL (AlloyDB) backend in batched chunks. The objectives are:
- Efficient large CSV handling via streaming and chunking.
- Clear separation of concerns: App host, Business services, Data models, Tests.
- Resilient bulk load with per-row fallback logic.
- Testable abstractions for connection and file operations.

## Solution Structure
```
src/
 UPS.WWRR.APP/ -> Application entry point (Program.cs), DI/Hosting configuration
 UPS.WWRR.Business/ -> Business services, helpers, background jobs, interfaces, extensions
 Common/Helper/ -> SQL builder, connection helpers
 Interfaces/ -> Service contracts (ICsvSplitterService, ICopyBatchDataService, etc.)
 Services/ -> Implementations (CsvSplitterService, CopyBatchDataService, MyBatchProcessor)
 Extensions/ -> Common extension utilities
 UPS.WWRR.Data/ -> Data models (TableConfiguration, CopyBatchResult)

tests/
 UPS.WWRR.Test/ -> xUnit tests (CsvSplitterServiceTest, CopyBatchDataServiceTest, helpers)
```

## Key Components
| Component | Description |
|-----------|-------------|
| CsvSplitterService | Streams CSV content into fixed-size chunk strings (supports header) |
| CopyBatchDataService | Performs PostgreSQL COPY bulk insert with fallback row-level retry |
| NpgsqlConnectionHelper | Abstracts connection creation for easier mocking/testing |
| MyBatchProcessor | Background service scaffold for scheduled batch jobs |
| MockDataStore | Test helper utilities (temp CSV creation, async collectors) |

## Getting Started
###1. Prerequisites
- .NET8 SDK
- PostgreSQL / AlloyDB connection string (environment variable `AlloyDBConnection` or `appsettings.json`)
- Optional tools: Visual Studio2022 / VS Code / Docker

###2. Clone
```bash
git clone <repo-url>
cd wwrr-mod-data-load
```

###3. Configuration
`src/UPS.WWRR.APP/appsettings.json` example:
```json
{
 "ConnectionStrings": {
 "AlloyDBConnection": "Host=localhost;Port=5432;Database=demo;Username=user;Password=pass"
 },
 "BatchLoad": {
 "ChunkSize":2000,
 "HasHeader": true,
 "Delimiter": ","
 }
}
```
Environment override: set `AlloyDBConnection` before running.

###4. Restore & Build
```bash
dotnet restore
dotnet build
```

###5. Run
```bash
cd src/UPS.WWRR.APP
dotnet run
```

## Build and Test
### Build
```bash
dotnet build --configuration Release
```
### Run All Tests
```bash
dotnet test --no-build --verbosity normal
```
### Filter by Test Class
```bash
dotnet test --filter FullyQualifiedName~CsvSplitterServiceTest
```
Coverage via `coverlet.collector` (already referenced).

## API References (High-Level Contracts)
| Interface | Method | Purpose |
|-----------|--------|---------|
| ICsvSplitterService | SplitAsync | Async chunk enumeration of CSV file |
| ICopyBatchDataService | CopyAsync | Bulk load CSV chunks with COPY/fallback |
| INpgsqlConnectionHelper | OpenConnectionAsync | Obtain open PostgreSQL connection |

## Contribute
1. Fork repository.
2. Create a feature branch: `git checkout -b feature/my-change`.
3. Commit: `git commit -m "Implement X"`.
4. Push: `git push origin feature/my-change`.
5. Open Pull Request with description and rationale.

### Guidelines
- Enable nullable reference types (`#nullable enable`) in new files.
- Follow Arrange / Act / Assert in tests with `ShouldX_WhenY` naming.
- Avoid direct DB calls in tests; mock connection helpers.
- Keep chunk size / delimiter configurable (BatchLoad section).

### Code Style
- Prefer `IAsyncEnumerable` for streaming large data.
- Use `ILogger<T>` for diagnostics; never swallow exceptions silently.
- Add XML doc comments for public interfaces and complex methods.

## Latest Releases
Versioning not yet formalized. Tag stable milestones as `v0.x.y`.

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references
## Roadmap
- Parallelized chunk COPY with concurrency controls.
- Health/metrics endpoints.
- Integration tests using ephemeral PostgreSQL container.
- Enhanced error classification and retry strategies.

# Build and Test
TODO: Describe and show how to build your code and run the tests. 
## Inspiration / References
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Chakra Core](https://github.com/chakra-core/ChakraCore)

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 
## License
(Define license: e.g. MIT, Apache2.0, or internal proprietary.)

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
---
Questions or enhancement ideas? Open an issue / discussion.