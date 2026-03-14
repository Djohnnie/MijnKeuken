# Copilot Instructions — MijnKeuken

MijnKeuken is a complete custom kitchen configurator/solution built as a .NET 10 Blazor Server web application.

## Tech Stack

- **Framework:** .NET 10, Blazor Server
- **UI Components:** MudBlazor
- **Database:** SQL Server with Entity Framework Core
- **Language:** C#

## Language & Localization

- **Code** (classes, methods, variables, comments): English
- **UI text** (labels, buttons, messages, tooltips): Dutch (nl-NL)
- **Routes and URLs**: English (routes are technical, not user-facing text)
- Use resource files or a localization service for UI strings — do not hardcode Dutch text in Razor components

## Build & Run

```shell
dotnet build MijnKeuken.slnx
dotnet run --project src/MijnKeuken.Web
dotnet watch --project src/MijnKeuken.Web   # hot-reload during development
```

## Tests

```shell
dotnet test                             # full suite
dotnet test --filter "FullyQualifiedName~MyTestClass.MyMethod"  # single test
dotnet test --filter "Category=Unit"    # unit tests only
```

## Architecture Conventions

- Clean Architecture with four layers:
  - `MijnKeuken.Domain` — entities, value objects, domain interfaces
  - `MijnKeuken.Application` — use cases, DTOs, MediatR handlers (references Domain)
  - `MijnKeuken.Infrastructure` — EF Core, external services (references Application)
  - `MijnKeuken.Web` — Blazor Server UI, API controllers, DI composition root (references Application + Infrastructure)
- Use the CQRS pattern with MediatR for commands and queries
- **Never call MediatR handlers directly from Razor components.** The call chain is:
  `Razor component → Service (e.g. IAuthService) → API Controller → MediatR Handler`
- API controllers live in `MijnKeuken.Web/Controllers/` and are hosted in the same process as Blazor
- Service classes in `MijnKeuken.Web/Services/` wrap HttpClient calls to the API and return `Result<T>`
- Keep Razor components thin — business logic belongs in handlers, not in `@code` blocks

## EF Core & Data Access

- Use code-first migrations
- Add migrations with: `dotnet ef migrations add <Name> --project src/MijnKeuken.Infrastructure`
- Apply migrations with: `dotnet ef database update --project src/MijnKeuken.Infrastructure`
- Never call `DbContext` directly from Razor components — go through a repository or MediatR handler
- All entities inherit `BaseEntity`, which provides:
  - `Guid Id` — primary key, **non-clustered**
  - `int SysId` — auto-increment identity column with a **unique clustered index**
- When adding a new entity, call `ConfigureBaseEntity<T>(modelBuilder)` in `AppDbContext.OnModelCreating` before any entity-specific configuration

## MudBlazor Conventions

- Use `MudBlazor` components (`MudButton`, `MudTextField`, etc.) — do not mix in raw HTML form elements
- Follow MudBlazor theming for colors and typography; define custom theme in a shared location
- Use `MudForm` with `MudTextField` validation for all form inputs

## Code Style

- Use file-scoped namespaces
- Use primary constructors where appropriate
- Prefer `record` types for DTOs and value objects
- Use nullable reference types (`<Nullable>enable</Nullable>`)
