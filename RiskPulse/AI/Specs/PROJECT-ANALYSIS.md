# RiskPulse — Project Architecture & Data-Flow Analysis

> Generated from codebase analysis. Last reviewed: August 2026.

---

## 1. Overview

**RiskPulse** (branded as **RiskIntel MIS**) is an enterprise risk management application built with **ASP.NET Core MVC**. The domain targets KRI (Key Risk Indicators), SAQ (Self-Assessment Questionnaires), branch-level risk submissions, and RAG (Red/Amber/Green) status tracking for high-stakes financial environments.

**Current Phase:** Access-control scaffolding complete — authentication (cookie + claims), user/role/permission management (working CRUD + DataTables grids). All domain pages (Dashboard, Submissions, Assessment Control, Form Builder) remain **stubs**. PostgreSQL persistence is live via EF Core; a separate legacy SQL Server schema draft exists in `Database/Seed.sql`.

---

## 2. Technology Stack

| Layer | Technology | Version / Details |
|---|---|---|
| Runtime | .NET | `net10.0` (target framework, `Microsoft.NET.Sdk.Web`) |
| Language | C# | Implicit usings, nullable enabled |
| Framework | ASP.NET Core MVC | Minimal hosting model (`Program.cs`) |
| ORM | Entity Framework Core | `Microsoft.EntityFrameworkCore` **10.0.10** |
| DB Provider | Npgsql (PostgreSQL) | `Npgsql.EntityFrameworkCore.PostgreSQL` **10.0.3** |
| Database | PostgreSQL | Schema `riskpulse`, DB `sit` (localhost:5432) |
| Auth | Cookie Authentication | `Microsoft.AspNetCore.Authentication.Cookies` + claim-based policies |
| View Engine | Razor `.cshtml` | Server-side rendered, sections for Styles/Scripts |
| Grid | DataTables | `jquery.dataTables.min.js` + `dataTables.bootstrap5.min.js` (client-side processing, AJAX JSON source) |
| JS | jQuery | AJAX, DOM, form `serializeArray` |
| UI Feedback | SweetAlert2 | `Swal.mixin` toast pattern on every page |
| Dropdowns | Select2 | Bootstrap-5 theme, `dropdownParent` bound to modal |
| CSS | Bootstrap 5 + "Stasis Enterprise" | Custom design tokens in `wwwroot/css/site.css` |
| Icons | Font Awesome 6 | `all.min.css` |
| Fonts | Inter, JetBrains Mono | Self-hosted `.woff2` |
| Client Validation | jQuery Validate (vendored) + custom JS rules | Manual `validateXxx()` functions, not unobtrusive tags |
| Scaffolding | EF Core Migrations | `Migrations/`, applied via `dotnet ef` |

**NuGet packages:** `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Tools`, `Npgsql.EntityFrameworkCore.PostgreSQL`. Nothing else.

---

## 3. Current Project Structure

```
RiskPulse.slnx                                   # XML solution (single project)
RiskPulse/
├── Program.cs                                   # Bootstrap: DbContext, DI, auth policies, pipeline
├── RiskPulse.csproj                             # net10.0, 3 EF/Npgsql package refs
├── appsettings.json                             # ConnString: Server=localhost;Port=5432;DB=sit;schema=riskpulse
├── appsettings.Development.json                 # Logging overrides
├── Properties/launchSettings.json
│
├── Controllers/
│   ├── LoginController.cs           # GET/POST Index, POST Logout, AccessDenied (AllowAnonymous)
│   ├── DashboardController.cs       # Stub, [Authorize(Policy="Permission:Dashboard")]
│   ├── SubmissionsController.cs     # Stub, [Authorize(Policy="Permission:Submissions")]
│   ├── AssessmentControlController.cs # Stub, [Authorize(Policy="Permission:Assessment Control")]
│   ├── FormBuilderController.cs     # Stub, [Authorize(Policy="Permission:Form Builder")]
│   ├── UsersController.cs           # Index (View), Grid (JSON), Save (JSON [FromBody])
│   ├── RolesController.cs           # Index (View), Grid (JSON), Save (JSON [FromBody])
│   └── ErrorController.cs           # GET /Error/Index
│
├── Data/
│   └── AppDbContext.cs              # DbContext: schema, DbSets, UnitType enum→string
│
├── Models/
│   ├── AppModel/
│   │   └── LoginResult.cs           # Service result DTO (Success/Message/Principal/Redirect)
│   ├── DbModel/AccessControl/       # EF entities (mirror DB tables)
│   │   ├── User.cs  Role.cs  Permission.cs  RolePermission.cs  Unit.cs  UnitType.cs (enum)
│   └── ViewModel/                   # View + form models
│       ├── UsersIndexViewModel.cs  RolesIndexViewModel.cs  RoleSaveModel.cs  ErrorViewModel.cs
│
├── Services/
│   ├── LoginService/
│   │   ├── AdAuthenticationService.cs     # STUB — ValidateCredentialsAsync always returns true
│   │   ├── DbAuthorizationService.cs       # Loads user+role+permissions+unit, builds nothing itself
│   │   └── LoginOrchestratorService.cs     # Orchestrates: AD check → DB lookup → claims principal
│   └── AccessControlService/
│       ├── UsersService.cs                 # CRUD for users + defaults (direct AppDbContext)
│       ├── RolesService.cs                 # CRUD for roles + permission mapping (direct AppDbContext)
│       └── PermissionPageMapper.cs         # Static: PermissionDesc → (Controller, Action)
│
├── Database/
│   └── Seed.sql                       # Manual permission/role/unit/user inserts + legacy dbo schema draft
│
├── Migrations/                        # 20260811135557_UserPermissionControl (+ Designer, Snapshot)
│
├── Views/
│   ├── _ViewImports.cshtml  _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml          # Sidebar (permission-gated links) + header + Toast mixin
│   ├── Login/Index.cshtml             # Standalone page (Layout=null), AJAX login
│   ├── Login/AccessDenied.cshtml      # Standalone (Layout=null)
│   ├── Users/Index.cshtml             # DataTables grid + Add/Edit modals (Select2 + SweetAlert)
│   ├── Roles/Index.cshtml             # DataTables grid + Add/Edit modals (permission checkboxes)
│   ├── Error/Index.cshtml             # Standalone (Layout=null), RequestId
│   └── Dashboard|Submissions|AssessmentControl|FormBuilder/Index.cshtml  # Stubs
│
├── Infrastructure/                    # EMPTY (contains empty Middleware/ folder)
├── Validation/                        # EMPTY
│
├── AI/
│   ├── Skills/ui-ux-pro-max.md        # AI UI generation prompt
│   └── Specs/{DESIGN,PROJECT-ANALYSIS}.md + {login,layout,branch,kri,error-page}/  # Design refs
│
└── wwwroot/
    ├── css/site.css                   # Stasis Enterprise design system
    ├── js/site.js                     # Sidebar/collapse/flyout/keyboard logic
    └── lib/                           # bootstrap, datatables, font-awesome, jquery, jquery-validation(-unobtrusive), select2, sweetalert2
```

---

## 4. Architecture Patterns

### 4.1 Pattern Set Currently Used

| # | Pattern | Where |
|---|---|---|
| 1 | **Classic MVC** (server-side Razor) | All controllers/views |
| 2 | **Service layer** (concrete classes via DI, Scoped) | `Services/`, registered in `Program.cs:16-20` |
| 3 | **EF Core + DbContext** directly inside services (no repository) | `UsersService`, `RolesService`, `DbAuthorizationService` |
| 4 | **Cookie auth + claim-based authorization** | `Program.cs:21-39`, `[Authorize(Policy=...)]` |
| 5 | **AJAX JSON endpoints** from controllers (not a Web API) | `Grid`/`Save`/`Login` actions |
| 6 | **DataTables grid fed by JSON** | `Views/{Users,Roles}/Index.cshtml` |
| 7 | **ViewModel pattern** for page rendering | `UsersIndexViewModel`, `RolesIndexViewModel` |
| 8 | **DTO/result model** for service → controller | `LoginResult` (`Models/AppModel`) |
| 9 | **Orchestrator service** composing lower services | `LoginOrchestratorService` |

### 4.2 Request Pipeline (in order)

```
UseExceptionHandler (/Error/Index)        dev only
  → UseHsts                               dev only
  → UseHttpsRedirection
  → UseRouting
  → UseAuthentication                     reads auth cookie → ClaimsPrincipal
  → UseAuthorization                      evaluates [Authorize(Policy=...)] claim requirements
  → UseStatusCodePages
  → MapStaticAssets                       wwwroot
  → MapControllerRoute   {controller=Login}/{action=Index}/{id?}
```

Default entry route is **Login/Index**. The sidebar (`_Layout.cshtml`) gates each link with `User.HasClaim("Permission", "<X>")`.

### 4.3 Authorization Model

- **6 permissions** are declared once in code as constants in `Services/AccessControlService/PermissionCatalog.cs` (`PermissionCatalog.Dashboard | Submissions | AssessmentControl | FormBuilder | Users | Roles`) and referenced by:
  1. `Program.cs:31-39` — `AddPolicy($"Permission:{PermissionCatalog.X}")` … `RequireClaim("Permission", PermissionCatalog.X)`
  2. Controllers — `[Authorize(Policy = $"Permission:{PermissionCatalog.X}")]`
  3. `Views/Shared/_Layout.cshtml` — `User.HasClaim("Permission", PermissionCatalog.X)`
  4. `Services/AccessControlService/PermissionPageMapper.cs` — dict keys keyed by `PermissionCatalog.X`
- The **data source** remains the DB `riskpulse.Permissions.PermissionDesc` (`Database/Seed.sql`) — constant values must match those rows exactly.
- At login, `DbAuthorizationService` loads User → Role → RolePermissions → Permissions, and `LoginOrchestratorService` writes each `PermissionDesc` as a `Claim("Permission", ...)` plus `Name`, `NameIdentifier`, `Role`, `DefaultPage`, `Unit` claims into the auth cookie.

---

## 5. Data-Flow Patterns (Frontend → Backend → Database)

There are **five** distinct request/response flows in use today.

### 5.0 Common request/response conventions

- **All AJAX** endpoints return `application/json`.
- **All responses** are `HTTP 200 OK` regardless of outcome — success is signalled by a `success: true/false` flag in the JSON body.
- **Client validation** happens first (SweetAlert toasts), then a `POST` to the server, then another `Toast.fire()` on the returned message.

### 5.1 Flow A — Server-rendered page load (MVC + ViewModel)

Used by the two admin "Index" pages and the login page.

```
Browser ──GET /Users/Index────────────────────────────► UsersController.Index
         ◄──HTML (full page)──────────────────────────── UsersController
                                                          │ UsersService.GetAllAsync()          │
                                                          │ UsersService.GetAllUnitsAsync()     │ EF Core
                                                          │ UsersService.GetAllRolesAsync()     │ (Include+AsNoTracking)
                                                          ▼                                   ▼
                                                        UsersIndexViewModel ──► PostgreSQL (riskpulse)
```

- **View model used:** `UsersIndexViewModel` (also `RolesIndexViewModel`).
- **Rendering:** Razor view + `@Html.Raw(Json.Serialize(...))` to embed *initial dropdown data* (units/roles/permissions) directly into an inline `<script>` block — this is **not** an AJAX call; it is server-side JSON serialization injected into the page at render time.

```js
// Emitted by the Razor view (Views/Users/Index.cshtml:5-7)
var units = [{"unitId":1,"unitCode":"001","unitDesc":"Head Office"}, ...];
var roles = [{"roleId":1,"roleDesc":"IT Admin"}, ...];
var currentUserId = 1;
```

### 5.2 Flow B — AJAX JSON Grid (DataTables)

Used by `Users/Grid` and `Roles/Grid`.

```
Browser (DataTables.ajax) ──GET /Users/Grid──────────► UsersController.Grid
        dataSrc:'data'                               │ GetAllAsync()
        ◄── { success:true, message:null,            │   .Select(u => new UserGridRow { ... })
              data:[ {id,username,unitId,            │
                       roleId,isActive}, ...] }       │
                                                      └──► EF Core → PostgreSQL
```

- **Grid library:** DataTables (client-side processing) — the whole row set is serialized and shipped to the browser in one response; paging/searching/sorting happen in the browser, not in SQL.
- **Payload shape:** named grid DTOs — `UserGridRow` / `RoleGridRow` in `Models/ViewModel` — projected in the controllers (`UsersController.Grid`, `RolesController.Grid`); camelCase JSON via the MVC web serializer defaults matches the DataTables `columns` config.
- **Role grid JSON** additionally includes nested arrays `permissionIds` / `permissionDescs` for the edit modal.

### 5.3 Flow C — AJAX JSON Form Submit (Create / Update)

Used by `Users/Save` and `Roles/Save`.

```
Browser (jQuery serializeArray) ──POST /Users/Save──────────► UsersController.Save
  { contentType:'application/json',                        │ null ?? ModelState check
    data: JSON.stringify(payload) }                        │ business rule (can't edit self)
    ◄── { success:true, message:"User saved..",            ▼
          data:{ id:12 } }                                  UsersService.CreateUserAsync/UpdateUserAsync
                                                             ├─ duplicate checks
                                                             ├─ default unit/role resolution
                                                             └─ _db.SaveChangesAsync()  → PostgreSQL
```

- **Model binding:** `[FromBody]` deserializes the JSON body.
  - `Roles/Save` binds a dedicated **`RoleSaveModel`** (a ViewModel).
  - `Users/Save` binds a dedicated **`UserSaveModel`** (a ViewModel).
- **Return:** `ApiResponse<T>` envelope — success `{ success, message, data:{ id } }`, failure `{ success:false, message }`.

### 5.4 Flow D — AJAX Login (JSON)

```
Browser (loginForm) ──POST /Login/Login (JSON body)──► LoginController.Login
                        contentType:'application/json' │ null ?? ModelState check (LoginRequest)
                        ◄── { success:true,            │ LoginOrchestratorService.AuthenticateAsync
                               data:{ redirectUrl } }  │  1 AdAuthenticationService  (STUB: always true)
                                                       │  2 DbAuthorizationService    (user/role/perm graph)
                                                       │  3 build ClaimsPrincipal
                                                       ▲  PermissionPageMapper       (desc → (controller,action))
Post-success:                         HttpContext.SignInAsync(principal) → auth cookie
  window.location.href = response.data.redirectUrl ─┘
```

- **Payload style:** `application/json` via `JSON.stringify`, bound to `LoginRequest` with `[FromBody]` — consistent with Flows B/C.
- **Outcome:** On success the server sets the cookie via `SignInAsync` and returns `{ success, data:{ redirectUrl } }`; the browser navigates to `response.data.redirectUrl`.

### 5.5 Flow summary table

| Flow | Method/Route | Request body | Server binder | Response JSON | Data source |
|---|---|---|---|---|---|
| A — Page load | `GET /{Users,Roles}/Index` | — | — | HTML + embedded `Json.Serialize` | ViewModel from service |
| B — Grid | `GET /{Users,Roles}/Grid` | — | — | `{success:true, data:[…]}` (`UserGridRow` / `RoleGridRow`) | service → EF Core |
| C — Save | `POST /{Users,Roles}/Save` | JSON | `[FromBody]` (`UserSaveModel` / `RoleSaveModel`) | `{success,message,data:{id}}` | service → `SaveChanges` |
| D — Login | `POST /Login/Login` | JSON | `[FromBody]` (`LoginRequest`) | `{success, data:{redirectUrl}}` | orchestrator → cookie |

All AJAX responses use the shared **`ApiResponse<T>`** envelope (`Models/AppModel/ApiResponse.cs`): `Success`, `Message`, `Data`, `Errors`. HTTP status stays `200`; outcome signalled by `success`. Grid keeps `data` as the array so DataTables `dataSrc:'data'` is unchanged; `id` / `redirectUrl` moved into `data`.

---

## 6. Layer Responsibility Map ("use grid/use JSON/use model where")

| Concern | Layer / File | Notes |
|---|---|---|
| **Grid** (data table UI) | DataTables in `Views/Users/Index.cshtml` + `Views/Roles/Index.cshtml` | client-side processing, AJAX `dataSrc:'data'` |
| **Grid data source** | `UsersController.Grid` / `RolesController.Grid` → `UserGridRow`/`RoleGridRow` via `ApiResponse<T>` | camelCase (web JSON defaults) matches DataTables `columns`; not server-side processing |
| **JSON serialization (server→JS init)** | Razor `@Html.Raw(Json.Serialize(...))` | `Views/Users/Index.cshtml:5-7`, `Views/Roles/Index.cshtml:5` |
| **JSON produce/consume** | Controllers `Json(...)` + jQuery `$.ajax` | all AJAX in views' `@section Scripts` |
| **Model — EF entities** | `Models/DbModel/AccessControl/*` | mapped 1:1 to `riskpulse.*` tables |
| **Model — view models** | `Models/ViewModel/*` | page + form/models (Users/Roles/Login/Error) |
| **Model — service result/DTO** | `Models/AppModel/LoginResult`, `ApiResponse<T>` |
| **Business logic** | `Services/*Service` | no repository layer; each service uses `AppDbContext` directly |
| **Data access** | `Data/AppDbContext` via services | `Include`/`AsNoTracking`/`SaveChanges` in services |
| **DB schema/DDL** | EF Migrations (`Migrations/`) | `HasDefaultSchema("riskpulse")`, migration `20260811135557_UserPermissionControl` |
| **Seed data** | `Database/Seed.sql` (manual) | NOT an EF `HasData` seed — see mismatch #6 |
| **Client validation** | `validateUserPayload`/`validateRolePayload`/`validateLoginForm` in views | hand-rolled, not DataAnnotations-driven |
| **Auth policies** | `PermissionCatalog` (single source) → `Program.cs` + `[Authorize]` + sidebar `HasClaim` + `PermissionPageMapper` | constant values must match `LoginOrchestratorService` claims + DB `Permissions` rows |
| **Auth cookie claims** | `LoginOrchestratorService` | `Name`, `NameIdentifier`, `Role`, `DefaultPage`, `Unit`, `Permission*` |

---

## 7. Database

### 7.1 Schema (`riskpulse`)

```
Permissions (PermissionId PK, PermissionDesc)
Units       (UnitId PK, UnitCode, UnitType varchar(32), UnitDesc)
Roles       (RoleId PK, RoleDesc, DefaultPermissionId FK→Permissions)
RolePermissions (RolePermissionId PK, RoleId FK→Roles, PermissionId FK→Permissions)   [many-to-many join]
Users       (Id PK, Username, IsActive, UnitId FK→Units, RoleId FK→Roles)
```

- **Enum→string conversion:** `Unit.UnitType` stored as `character varying(32)` (`AppDbContext.cs:18-23`).
- Cascade deletes on `Users→Roles/Units` and `RolePermissions→Roles/Permissions`.
- **Implemented via:** EF Core migration; applied with `dotnet ef database update`.

### 7.2 `Database/Seed.sql` — legacy content & risk

Seed.sql inserts the 6 permissions, 2 roles, 1 unit, and 1 test user. **However** lines 35–324 contain an entire **legacy SQL Server schema** (`dbo.tblAssessmentModuleType`, `tblAssessmentHeader`, `tblSAQ*`, `tblKRI*`, `tblRiskRegister*` — `IDENTITY(1,1)`, `NVARCHAR`, `GETDATE()`) inside a "Do not run this manually" comment. This is not PostgreSQL-compatible and is a copy of a different-era design. It should be extracted to a separate reference document (see mismatch #6).

---

## 8. Pattern Mismatches & Inconsistencies

> Current **open** deviations only, with evidence and fix. Already-resolved items (save-model binding + server-side DataAnnotations, `ApiResponse<T>` envelope, login JSON standardization, `PermissionCatalog` single source, named grid DTOs `UserGridRow`/`RoleGridRow`, self-edit rule moved into `UsersService`) are reflected in §4–§6 and §10 and are not repeated here.

### 8.1 Architecture & layering

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 1 | **No repository / unit-of-work; no interface abstractions** — every service talks to `AppDbContext` directly and exposes concrete methods returning entities. The data layer is untestable and services can't be swapped or faked. | `Services/AccessControlService/UsersService.cs:17-24`, `Services/AccessControlService/RolesService.cs:16-25`, `Services/LoginService/DbAuthorizationService.cs:18-29` | Introduce `IUsersService`, `IRolesService`, `ILoginOrchestratorService`, `IAdAuthenticationService`, `IDbAuthorizationService` (optionally `IRepository<T>`/`IUnitOfWork`) and have services return DTOs, not entities. |
| 2 | **Concrete-class-only DI** — every service is registered as `AddScoped<Concrete>()`, so nothing can be mocked or swapped. | `Program.cs:16-20` | Register `AddScoped<IXxx, Xxx>()` against the interfaces from #1. |
| 3 | **DataTables runs client-side processing** — the full row set ships to the browser in one response; paging/search/sort run in JS, not SQL. Latent scale problem once Submissions hold real data. | `Views/Users/Index.cshtml:211-229`, `Views/Roles/Index.cshtml:226-243` | For large tables use `serverSide:true` and handle `start`/`length`/`search` at the Grid endpoints. |
| 4 | **Global/inline JS, no modules, no bundling** — page logic lives in `@section Scripts` with hand-rolled `$.ajax` calls; only vendored libs are static. | Users/Roles/Login view script sections | Extract shared AJAX/grid/toast helpers into `wwwroot/js/modules/*.js` (folder already scaffolded). |
| 5 | **`AdAuthenticationService` is a stub that always returns `true`** — any username/password is "valid" as long as the user exists in DB. | `Services/LoginService/AdAuthenticationService.cs:7-9` | Implement a real directory/identity-provider lookup (or explicitly dev-gate the stub). |

### 8.2 Data & persistence

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 6 | **Seed data is manual SQL that also embeds a legacy SQL Server schema** — `Database/Seed.sql` mixes live PostgreSQL inserts (permissions, roles, unit, test user) with a commented-out, non-PostgreSQL `dbo.tbl*` design (`IDENTITY(1,1)`, `NVARCHAR`, `GETDATE()`). | `Database/Seed.sql` (lines 1–33 live; 35–324 legacy `dbo.tbl*`) | Move seed into EF Core (`modelBuilder.HasData` / seed extension) and extract the legacy schema to a reference doc, out of executable SQL. |
| 7 | **No migrate/seed bootstrap at startup** — the app assumes `dotnet ef database update` was run externally; a fresh DB will fail at first query. | `Program.cs:11-12` (no `Db.Database.Migrate()`) | Add dev-only `Migrate()` (+ data seed) bootstrap, or document the required EF command in a README. |
| 8 | **No logging (`ILogger`) anywhere** — service/DB exceptions bubble with no trace and the catch blocks can't be audited. | all `Services/*` and `Controllers/*` | Inject `ILogger<T>` and log at service and catch boundaries. |

### 8.3 Security

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 9 | **No CSRF protection on cookie-auth state-changing endpoints** — `Users/Save`, `Roles/Save`, and `Login/Login` are JSON POSTs authenticated by cookie, but the app has no antiforgery tokens (`[ValidateAntiForgeryToken]` / `@Html.AntiForgeryToken()` are absent everywhere). | `Controllers/UsersController.cs:50-51`, `Controllers/RolesController.cs:47-48`, `Controllers/LoginController.cs:35-37` | Emit antiforgery tokens in the views and add `[ValidateAntiForgeryToken]` on the POST actions; for `[FromBody]` JSON use `AddAntiforgery` + a header token. |
| 10 | **Database credentials committed to source** — the PostgreSQL connection string (`Server`, `Port`, user, `Password=123456`) is hard-coded in `appsettings.json` and tracked by git. | `appsettings.json:8-10` | Move credentials to user-secrets / environment variables; keep no secret (or a harmless dev value) in the repo. |

### 8.4 Code hygiene & minor

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 11 | **Dead fallback code now unreachable** — because `UserSaveModel` makes `UnitId`/`RoleId` required (`[Range(1,…)]`) and the controller validates, the `Id == 0 → default` fallbacks and `GetDefaultUnitIdAsync`/`GetDefaultRoleIdAsync` can never run via the UI. | `Services/AccessControlService/UsersService.cs:59-67, 87-105` | Remove the dead fallbacks (or keep them only if a non-validated path is intended). |
| 12 | **Unused validation scaffolding** — `Views/Shared/_ValidationScriptsPartial.cshtml` (jQuery Validate/unobtrusive) is never referenced (validation is hand-rolled JS), and `Infrastructure/`, `Validation/`, `Infrastructure/Middleware/` are empty. | `Views/Shared/_ValidationScriptsPartial.cshtml`; `Infrastructure/`, `Validation/` | Delete the unreferenced partial or wire it up; remove/populate the empty folders. |
| 13 | **Brand naming inconsistency** — the shell uses "RiskIntel MIS" while the sidebar and standalone pages use "Risk Pulse"/"RiskPulse". | `Views/Shared/_Layout.cshtml:6,154` vs `Views/Login/*`, `Views/Error/*` | Choose one product name and apply it consistently. |

---

## 9. Recommended / Target Architecture

A minimal, incremental target that fixes every mismatch above **without** a rewrite:

```
Views (.cshtml + DataTables/Select2/SweetAlert)
   │  GET page (ViewModels + Json.Serialize init data)
   ▼  AJAX JSON (grid + save + login) — all through ApiResponse<T>
Controllers — thin: bind SaveModels, call services, return ApiResponse<T>
   │
Services (interface + concrete, Scoped DI)
   ├── Auth:      IAdAuthenticationService  → AdAuthenticationService (real impl)
   │              IDbAuthorizationService   → DB lookup
   │              ILoginOrchestratorService → composes above
   ├── Access:    IUsersService / IRolesService  (+ PermissionPageMapper from single source)
   └── Data access via IRepository<T> (or keep DbContext here behind service facades)
   │
Data — AppDbContext + EF Migrations + EF seed (HasData)
   │
PostgreSQL (riskpulse schema)
```

Key decisions for the target:
1. **DTOs for everything crossing layers** — save models (`UserSaveModel`, `RoleSaveModel`), named grid rows (`UserGridRow`, `RoleGridRow`), `ApiResponse<T>`.
2. **Uniform JSON contract** — `{ success, message, data, errors }` via `ApiResponse<T>` (already live).
3. **Server-side validation is the source of truth** — DataAnnotations on save models + custom validators; client JS mirrors it for UX only (already live).
4. **Single source for permissions/policies** — `PermissionCatalog` shared by `Program.cs`, sidebar, and `PermissionPageMapper` (already live).
5. **EF seed via `HasData`**; remove legacy SQL Server DDL from `Seed.sql` (mismatch #6).
6. **Real AD provider** behind `IAdAuthenticationService` (or explicitly dev-gated) (mismatch #5).
7. **Logging** (`ILogger`) at service boundaries (mismatch #8).
8. **Server-side DataTables** when row counts grow (mismatch #3).
9. **Anti-forgery on all state-changing POSTs** (mismatch #9) and **secrets out of source** (mismatch #10).

---

## 10. Maturity Assessment

| Aspect | Status |
|---|---|
| Core framework / layout / login | ✅ Complete |
| Cookie auth + claims + permission policies | ✅ Complete |
| User / Role / Permission CRUD (models, services, views) | ✅ Complete — incl. self-edit rule in `UsersService` (controller left thin) |
| PostgreSQL + EF Core migrations | ✅ Live |
| DataTables AJAX grids + JSON save flows | ✅ Working — grids return named DTOs (`UserGridRow`/`RoleGridRow`) |
| Design system (CSS) + AI specs | ✅ Complete |
| Domain pages (Dashboard, Submissions, Assessment Control, Form Builder) | ⬜ Stubs |
| Repository / unit-of-work / interface services | ❌ Not started |
| DTOs & uniform API envelope | ✅ Complete — `ApiResponse<T>` + `LoginRequest`/`UserSaveModel`/`RoleSaveModel`/`UserGridRow`/`RoleGridRow` |
| Server-side validation (DataAnnotations on save models) | ✅ Added on `UserSaveModel`, `RoleSaveModel`, `LoginRequest` |
| Permission single source (`PermissionCatalog`) | ✅ Resolved — policies, `[Authorize]`, sidebar, `PermissionPageMapper` all reference the catalog |
| Business rule placement | ✅ Service-side — self-edit, duplicate, and not-found rules all throw `InvalidOperationException` from `UsersService` |
| CSRF protection | ❌ Not started (§8 #9) |
| Secrets management | ❌ Credentials committed (§8 #10) |
| Real AD / identity provider | ❌ Stub (always true) (§8 #5) |
| Server-side grid processing | ❌ Not started (client-side only) (§8 #3) |
| Logging | ❌ Not started (§8 #8) |
| Tests / CI / Docker | ❌ Not started |

---

## 11. Next Logical Steps

1. ✅ **Fix validation & binding consistency** — `UserSaveModel`/`RoleSaveModel`, `[Required]`/`[RegularExpression]`, null-guard both `Save` actions (resolved).
2. ✅ **Introduce `ApiResponse<T>`** and adopt in `Grid`/`Save`/`Login`; login standardized to JSON `[FromBody]` (resolved).
3. ✅ **Single permission source** (`PermissionCatalog`) for policies/layout/mapper (resolved).
4. ✅ **Named grid DTOs** — `UserGridRow`, `RoleGridRow` replace anonymous grid projections (resolved).
5. ✅ **Move self-edit rule into `UsersService`** — `UpdateUserAsync(model, actingUserId)`; controller left thin (resolved).
6. **Add CSRF protection** — antiforgery tokens + `[ValidateAntiForgeryToken]` on `Save`/`Login` POSTs (§8 #9).
7. **Move DB credentials out of source** — user-secrets / environment variables (§8 #10).
8. **Extract interfaces + register DI** — `IUsersService`, `IRolesService`, `IAdAuthenticationService`, `IDbAuthorizationService` (§8 #1–#2).
9. **Implement real AD** or dev-gate explicitly (§8 #5).
10. **EF seed + clean `Seed.sql`** of legacy SQL Server DDL (§8 #6).
11. **Add logging** at service boundaries (§8 #8).
12. **Hygiene** — remove dead fallbacks, unused validation partial, empty folders; unify product name (§8 #11–#13).
13. Then build domain: keep this file current for Dashboard/Submissions with the standardized flow (server-side grid for submissions volume, §8 #3).