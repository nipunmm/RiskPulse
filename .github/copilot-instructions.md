# Copilot instructions for RiskPulse

## Project overview

This repository contains a single ASP.NET Core MVC application, `RiskPulse/`, built on .NET 10 and backed by PostgreSQL via EF Core + Npgsql. The app manages login/authz, administration (users/roles/units), SAQ/KRI template design, a schedule wizard (activating a schedule auto-creates the assessment from DB-driven workflow steps), submissions, and a dashboard.

The important architectural shape is:
- `RiskPulse/Program.cs` configures DbContext, cookie auth, authorization policies, and dependency injection.
- `Controllers/` is flat and thin; each controller has a matching view folder and usually follows the `GET Grid` / `POST Save` / `POST Delete` pattern.
- `Services/` is organized by workflow (`Login`, `Administration`, `Templates`, `Assessment`, `Dashboard`, `Schedule`). Business rules live here, not in controllers.
- `Data/Entries/` holds the EF Core entity mirror of the `riskpulse.*` tables; `AppDbContext` configures schema, enum conversions, and relationship rules.
- `Models/Dto/`, `Models/ViewModel/`, and `Models/Enum/` are split by responsibility.
- `PermissionCatalog` and `PermissionPageMapper` are the single source of truth for permission strings and page routing.

## Commands

Use these commands from the repo root unless otherwise noted:
- Build: `dotnet build` (from repo root works via the solution; `cd RiskPulse && dotnet build` is also valid)
- Run: `cd RiskPulse && dotnet run`
- Local app URL: http://localhost:5021; HTTPS launch profile adds https://localhost:7110
- Required database: PostgreSQL running on `localhost:5432` with a `sit` database and `riskpulse` schema available; the app expects the schema to be created/seeded before login works.
- Dev login stub: username `nipunmm` with any password; `AdAuthenticationService` is a stub that accepts any login.
- EF setup: `Migrations/` is git-ignored (`**/Migrations/`) and is not present on disk; there is no committed migration baseline or seed artifact. The `riskpulse` schema + seed rows (permissions/roles/unit/workflow steps/test user) live only in the dev DB — a fresh DB must be provisioned/seeded from the live DB independently of the repo.

There are no dedicated test projects, lint tasks, or formatting commands configured in this repo. `dotnet build` is the only automated verification step currently defined.

## Auth and authorization conventions

- Cookie auth is configured in `Program.cs` with a 20-minute sliding expiration, `LoginPath=/Login/Index`, and `AccessDeniedPath=/Login/AccessDenied`.
- Each permission maps to a policy such as `Permission:{PermissionCatalog.Users}`; do not invent a second permission source.
- Claims include user name, user Id, role, default page, unit, and one `Permission` claim per role permission.
- `DefaultPage` drives post-login redirects and return-home behavior on access-denied/error pages.
- New services must be registered with `AddScoped<...>()` in `Program.cs`, and each new permission requires a matching `AddPolicy` line.

## Data model and EF conventions

- The default schema is `riskpulse` via `AppDbContext.HasDefaultSchema("riskpulse")` and the connection string search path.
- Enum-backed properties are stored as `varchar(32)` using EF Core `HasConversion<string>()` + `HasMaxLength(32)` in `OnModelCreating`.
- `Data/Entries/*` entities are a flat mirror of DB tables; keep them aligned with the actual schema.
- Duplicate checks should use `DbSetExtensions.EnsureUniqueAsync(...)` and throw `InvalidOperationException` with a user-facing message.
- FK and relationship behavior are explicit in `AppDbContext`; keep them consistent when adding new entities.
- `Schedule.StartDate`/`EndDate`/`StartMonth` are `timestamptz`; ensure `DateTime.SpecifyKind(value, DateTimeKind.Utc)` before persisting if working with schedule data.

## Backend feature implementation pattern

When adding a CRUD feature, follow the repository pattern already used by the administration and template modules:
- Add the entity in `Data/Entries/` and register it in `AppDbContext`.
- Create a `*SaveDto` with data annotations such as `[Required]` and `[Range(1, int.MaxValue)]` for foreign keys; `Id == 0` means new.
- Implement the service in the relevant `Services/` folder and project `*GridRowViewModel` with `AsNoTracking()` for grid reads.
- Validate in the service and throw `InvalidOperationException` for user-facing failures.
- Controllers stay thin: use `[Authorize(Policy = $"Permission:{PermissionCatalog.X}")]`, `Json(ApiResponse.Ok(...))` for grid calls, and `ControllerHelpers.TrySave(...)` / `ControllerHelpers.TryDelete(...)` for mutations.
- Business logic does not belong in controllers.

## Frontend and UI conventions

- Shared UI helpers live in `wwwroot/js/modules/riskpulse.js`; views should call these helpers instead of re-implementing patterns.
- Use `RiskPulse.postJson()`, `RiskPulse.getJson()`, `RiskPulse.validationError()`, `RiskPulse.showModal()`, `RiskPulse.hideModal()`, and `RiskPulse.initSelect2()` instead of raw jQuery AJAX or Bootstrap modal APIs.
- `select` elements use the repo’s `rp-input` styling and are enhanced through Select2 via the shared module; do not directly call `$el.select2(...)` from views.
- DataTables configuration goes in view script sections; use the `columns.data` names exactly matching the JSON keys from the corresponding `*GridRowViewModel`.
- Follow the existing `edit-{entity}-btn` / `delete-{entity}-btn` selectors and delegated click wiring.
- Keep `PermissionCatalog` values, `Program.cs` policies, the DB seed, and sidebar links in sync when changing permission names.

## Repo-specific habits to preserve

- Keep controllers file-scoped in the `RiskPulse.Controllers` namespace and keep them thin.
- Use `// --- Section name ---` banners in controllers/services where needed; do not add extraneous comments to DTOs or view models.
- Keep `Views/` flat and aligned with controller names.
- Preserve the existing folder organization: `Services/Login`, `Services/Administration`, `Services/Templates`, `Services/Schedule`, `Services/Assessment`, `Services/Dashboard`, `Services/Utilities` (CodeGeneratorService).
- Follow lowercase conventional Git prefixes when committing (`feat/...`, `fix/...`, `refactor/...`) only if asked to commit.
- The AI documentation under `RiskPulse/AI/` is an active source of repo intent; keep it in sync when changing repo patterns or architecture.
