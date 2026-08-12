# AGENTS.md

ASP.NET Core MVC (net10.0) risk-management app, "Risk Pulse". Single web project under `RiskPulse/`; no solution file, no tests, no CI. EF Core 10 + Npgsql 10 + PostgreSQL.

## Commands
- Build from the project dir (no `.sln` at root): `dotnet build` with `workdir` = `RiskPulse/`.
- There is no test/lint/format step; the build is the only automated verification.
- `dotnet ef` migrations: the working tree contains migration `20260812202536_UserPermissionControl` (untracked; covers AccessControl + SAQ + KRI schema). The older `20260812184751_saq` was deleted. Applying it via `dotnet ef database update` requires a tooling run against the live DB; the DB is otherwise provisioned manually via `Database/Seed.sql`.

## Database & EF
- PostgreSQL, schema `riskpulse` — set via `HasDefaultSchema("riskpulse")` in `Data/AppDbContext.cs` AND `Search Path=riskpulse` in the connection string.
- Connection string is hard-coded and COMMITTED in `appsettings.json` (`Password=123456`) — known security debt (tracked in `PROJECT-ANALYSIS.md` §8); do not add further secrets.
- `Database/Seed.sql`: only lines 1–33 are live (10 permissions, 2 roles, 1 unit, 1 test user). Lines 35+ are a commented-out legacy SQL Server `dbo.tbl*` schema that is NOT PostgreSQL-compatible — never execute it.
- `Models/DbModel/AccessControl/*` map 1:1 to `riskpulse.*` tables. `Unit.UnitType` is an enum persisted as `varchar(32)`.

## Architecture & data flow (match this pattern exactly)
Views → AJAX JSON → thin controller → service → `AppDbContext` → PostgreSQL:
- **JSON contract**: every AJAX endpoint returns `ApiResponse<T>` (`Models/AppModel/ApiResponse.cs`): `{ success, message, data, errors }`. HTTP status stays 200; the `success` flag carries the outcome. Success = `Json(ApiResponse.Ok(...))`; failure = `Json(ApiResponse.Fail<object>(...))`. Do not return raw anonymous JSON from controllers.
- **Binding/validation**: POST bodies bind ViewModel save models (`UserSaveModel`, `RoleSaveModel`, `LoginRequest`) via `[FromBody]`; server-side DataAnnotations are the source of truth, controllers null-guard and return the first `ModelState` error; client JS mirrors validation for UX only.
- **Business rules live in services** and throw `InvalidOperationException` with user-facing messages; controllers catch and surface `ex.Message` via `ApiResponse.Fail`. Never put business rules in controllers.
- **Grids**: `GET {Users,Roles}/Grid` return named DTOs `UserGridRow`/`RoleGridRow`; the camelCase JSON keys must match the DataTables `columns` `data:` names. Client-side DataTables processing (no server-side paging).
- **UI**: shell pages use `Views/Shared/_Layout.cshtml`. Standalone pages (`Login/Index`, `Login/AccessDenied`, `Error/Index`) set `Layout = null` and render full HTML with only `site.css` + FontAwesome. Initial dropdown data is injected server-side via `@Html.Raw(Json.Serialize(...))` (not AJAX). Page JS lives in `@section Scripts` inline IIFEs; page scripts keep only validation rules, DataTables column configs, and event wiring.
- **Shared JS module**: `wwwroot/js/modules/riskpulse.js` (referenced from `_Layout`) is the single home for cross-page helpers under the `RiskPulse.*` namespace: `toastSuccess`/`toastError`/`toastGenericError`, `postJson`/`getJson` (auto generic-error toast, routes `ApiResponse.success`), `serializeForm` (checkbox booleans + numeric coercion), `populateSelect`, `showModal`/`hideModal`, `confirmDelete` (Swal), and `initGrid` (DataTables defaults `pageLength:10`/`lengthMenu:[10,25,50]`). Never re-implement these inline in a view; `$.ajax` should appear in views only via the module.
- **Styling**: use the design-system classes in `wwwroot/css/site.css` (`btn-stasis-primary`, `btn-stasis-secondary`, `status-pill rag-*`), not Bootstrap button classes. Selects use `input-stasis` only (never `input-stasis form-select` — the design system supplies its own arrow).
- **Modals**: programmatically open/close via `RiskPulse.showModal(id)`/`RiskPulse.hideModal(formEl)` — the vendored bundle is Bootstrap 5, which has no jQuery `.modal()` API. Never use `$('#x').modal('show')` or raw `bootstrap.Modal.getOrCreateInstance` outside the module.

## Permissions (single source of truth)
- `PermissionCatalog` strings drive the policies in `Program.cs` (`Permission:<name>`), `[Authorize]`, sidebar `User.HasClaim("Permission", ...)`, and `PermissionPageMapper`. They must match the DB `Permissions.PermissionDesc` seed rows and the `Permission` claims built in `LoginOrchestratorService` exactly. Two values contain spaces: `"Assessment Control"` and `"Form Builder"`. Renaming a permission means updating all four places plus the seed.
- Auth cookie claims: `Name`, `NameIdentifier` (int user Id), `Role`, `DefaultPage` (permission desc), `Unit`, plus one `Permission` claim per role permission. `DefaultPage` drives the post-login redirect and the "Return to Home" buttons on the AccessDenied/Error pages.
- `AdAuthenticationService` is a stub that always validates `true` — dev only, not real auth.
- `PermissionPageMapper.GetRouteForPermission(desc)` → `(controller, action)`, defaulting to Dashboard.

## Code conventions
- `Models/ViewModel/*` and `Models/DbModel/*` use block namespaces (`namespace X { }`); controllers/services use file-scoped namespaces. Follow the folder you are editing.
- Git: lowercase conventional prefixes for commits/branches (`feat/...`, `fix/...`). Commit only when asked.

## Docs to keep current
- `RiskPulse/AI/Specs/PROJECT-ANALYSIS.md` is the living analysis doc. §8 lists open "Pattern Mismatches"; when you resolve one, remove it, renumber the rest, and refresh the cross-references in §6/§7.2/§9/§10/§11. Per-feature design specs live in `RiskPulse/AI/Specs/<feature>/DESIGN.md`.
