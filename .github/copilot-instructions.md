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
  - `MijnKeuken.Web` — Blazor Server UI, DI composition root (references Application + Infrastructure)
- Use the CQRS pattern with MediatR for commands and queries
- Use an API layer (e.g., ASP.NET Core Web API) to expose handlers to Razor components.
- Wrap API calls in a service layer that Razor components can consume via dependency injection
- Keep Razor components thin — business logic belongs in services or handlers, not in `@code` blocks
- Use Fluxor or a similar state container if client-side state grows beyond simple component parameters

## EF Core & Data Access

- Use code-first migrations
- Add migrations with: `dotnet ef migrations add <Name> --project src/MijnKeuken.Infrastructure`
- Apply migrations with: `dotnet ef database update --project src/MijnKeuken.Infrastructure`
- Never call `DbContext` directly from Razor components — go through a repository or MediatR handler

## MudBlazor Conventions

- Use `MudBlazor` components (`MudButton`, `MudTextField`, etc.) — do not mix in raw HTML form elements
- Follow MudBlazor theming for colors and typography; define custom theme in a shared location
- Use `MudForm` with `MudTextField` validation for all form inputs

## Code Style

- Use file-scoped namespaces
- Use primary constructors where appropriate
- Prefer `record` types for DTOs and value objects
- Use nullable reference types (`<Nullable>enable</Nullable>`)
