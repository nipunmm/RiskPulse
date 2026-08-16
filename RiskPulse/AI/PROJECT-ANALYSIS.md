# RiskPulse — Project Architecture & Data-Flow Analysis

> Generated from codebase analysis. Last reviewed: August 2026.

---

## 1. Overview

**RiskPulse** is an enterprise risk management application built with **ASP.NET Core MVC**. The domain targets KRI (Key Risk Indicators), SAQ (Self-Assessment Questionnaires), branch-level risk submissions, and RAG (Red/Amber/Green) status tracking for high-stakes financial environments.

**Current Phase:** Access-control scaffolding complete — authentication (cookie + claims), user/role/permission management (working CRUD + DataTables grids). SAQ Templates and KRI Templates implemented — headers, item designers, threshold-config tabs (merged into KRI Templates), and Locked-immutability rules. Each template header is **linked to a required unit group** (`GroupId FK→Groups`, selected in the add/edit modals and shown in the grid). **Assessment wizard implemented** — step flow (name → SAQ → KRI → schedule → finalize) with per-step AJAX persistence, draft edit/re-save through the stages, and activate rules. **Units page implemented** — two-tab Administration page (Unit CRUD | Unit Group CRUD) under the new `Units` permission with Select2 group→unit assignment. Remaining domain pages (Dashboard, Submissions, Risk Register templates) are **stubs**. PostgreSQL persistence is live via EF Core; a separate legacy SQL Server schema draft exists in `Database/Seed.sql`. Structure is convention-aligned: controllers are **flat** in `Controllers/` (thin, 1:1 with `Views/{Controller}/`), services are **grouped by workflow** (`Login`/`Administration`/`Templates`/`Assessment`), and `Models/` is split by layer distinction into `Models/Dto/` (inter-system data, `*Dto` postfix) and `Models/ViewModel/` (UI-shaped data, `*ViewModel` postfix).

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
| Scaffolding | EF Core Migrations | Schema per migration `20260812202536_UserPermissionControl` (files **not** in working tree); DB provisioned manually via `Database/Seed.sql` |

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
├── Controllers/                  # Flat — thin, 1:1 with Views/{ControllerName}/; routes follow class names
│   ├── LoginController.cs        # GET/POST Index, POST Logout, AccessDenied (AllowAnonymous)
│   ├── UsersController.cs        # Index (View), Grid (JSON), Save (JSON [FromBody])
│   ├── RolesController.cs        # Index (View), Grid (JSON), Save (JSON [FromBody])
│   ├── UnitsController.cs        # 2-tab Units page: UnitGrid/SaveUnit/DeleteUnit + GroupGrid/SaveGroup/DeleteGroup (JSON)
│   ├── SaqTemplatesController.cs # Grid/Save/Delete headers + QuestionsGrid/SaveQuestion/DeleteQuestion (JSON)
│   ├── KriTemplatesController.cs # Templates + config: Grid/Save/Delete headers, KrisGrid/SaveKri/DeleteKri, Colors/Groups/Bands CRUD (JSON)
│   ├── RiskRegisterTemplatesController.cs # Stub, [Authorize(Policy="Permission:Risk Register")]
│   ├── DashboardController.cs    # Stub, [Authorize(Policy="Permission:Dashboard")]
│   ├── SubmissionsController.cs  # Stub, [Authorize(Policy="Permission:Submissions")]
│   ├── AssessmentController.cs   # Wizard: Index (grid), Wizard (step flow), SaveName/SaveSaq/SaveKri/SaveSchedule/Finalize/Delete (JSON)
│   ├── ErrorController.cs        # GET /Error/Index
│
├── Data/
│   ├── AppDbContext.cs               # DbContext: schema, DbSets, enum→string conversions
│   └── Entries/                      # EF entities — flat 1:1 mirror of riskpulse.* tables (no domain grouping)
│       ├── User.cs  Role.cs  Permission.cs  RolePermission.cs  Unit.cs  UnitType.cs (enum)
│       ├── Group.cs  UnitGroup.cs   # Unit grouping (Group 1—N UnitGroup N—1 Unit, unique GroupId+UnitId)
│       ├── SaqHeader.cs  SaqQuestion.cs  SaqQuestionOption.cs  SaqStatus.cs (enum)  QuestionType.cs (enum)
│       ├── KriHeader.cs  Kri.cs  KriThresholdGroup.cs  KriThresholdColor.cs  KriThreshold.cs  KriStatus.cs (enum)
│       └── AssessmentHeader.cs  ScheduleHeader.cs  AssessmentStatus.cs (enum)   # Assessment wizard entities
│
├── Models/
│   ├── Dto/                        # Data that moves between layers/systems
│   │   ├── ApiResponse.cs          # Shared JSON envelope { success, message, data, errors }
│   │   ├── LoginResultDto.cs       # Service result DTO (Success/Message/Principal/Redirect)
│   │   ├── LoginRequestDto.cs  UserSaveDto.cs  RoleSaveDto.cs
│   │   ├── Saq*.cs                 # SaqHeaderSaveDto, SaqQuestionSaveDto, SaqOptionSaveDto, SaqDeleteRequestDto
│   │   └── Kri*.cs                 # KriHeaderSaveDto, KriSaveDto, KriDeleteRequestDto, KriColorSaveDto, KriThresholdGroupSaveDto, KriBandsSaveDto, KriBandSaveDto
│   └── ViewModel/                  # Data shaped specifically for a UI/view — flat (file names carry the feature scope)
│       ├── UsersIndexViewModel.cs  RolesIndexViewModel.cs  UnitsIndexViewModel.cs  ErrorViewModel.cs
│       ├── UserGridRowViewModel.cs  RoleGridRowViewModel.cs  UnitGridRowViewModel.cs  GroupGridRowViewModel.cs
│       ├── Saq*.cs                 # SaqTemplatesIndexViewModel, SaqGridRowViewModel, SaqQuestionGridRowViewModel, SaqOptionGridRowViewModel, SaqStatusOptionViewModel
│       ├── Kri*.cs                 # KriTemplatesIndexViewModel, KriStatusOptionViewModel, KriGroupOptionViewModel, KriGridRowViewModel, KriItemGridRowViewModel, KriGroupGridRowViewModel, KriColorGridRowViewModel, KriBandGridRowViewModel
│       └── Assessment*.cs          # AssessmentGridRowViewModel, AssessmentWizardViewModel, SaqTemplateOptionViewModel, KriTemplateOptionViewModel
│
├── Services/
│   ├── Login/                        # Authentication + authorization
│   │   ├── AdAuthenticationService.cs     # STUB — ValidateCredentialsAsync always returns true
│   │   ├── DbAuthorizationService.cs       # Loads user+role+permissions+unit, builds nothing itself
│   │   ├── LoginOrchestratorService.cs     # Orchestrates: AD check → DB lookup → claims principal
│   │   ├── PermissionCatalog.cs            # Single source for permission constants (policies, layout, mapper)
│   │   └── PermissionPageMapper.cs         # Static: PermissionDesc → (Controller, Action)
│   ├── Administration/               # Users + roles + units CRUD
│   │   ├── UsersService.cs                 # CRUD for users + defaults (direct AppDbContext)
│   │   ├── RolesService.cs                 # CRUD for roles + permission mapping (direct AppDbContext)
│   │   └── UnitsService.cs                 # Unit CRUD (duplicate-code guard, block delete when users reference it) + group CRUD (clear/re-add UnitGroups)
│   ├── Templates/                    # SAQ + KRI templates
│   │   ├── SaqTemplatesService.cs           # SAQ template CRUD: headers, questions, options + lock/duplicate rules
│   │   └── KriTemplatesService.cs           # KRI template CRUD (headers + KRIs) + threshold config (colors/groups/bands) + lock/duplicate rules
│   └── Assessment/                   # Assessment wizard workflow
│       └── AssessmentService.cs             # Draft create/rename, SAQ+KRI template pick (non-Locked), schedule upsert, finalize (Active requires SAQ+KRI), delete (drafts only)
│
├── Database/
│   └── Seed.sql                       # Manual permission/role/unit/user inserts + legacy dbo schema draft
│
├── Migrations/                        # (absent) — schema per migration 20260812202536_UserPermissionControl on the live DB; files not present in this tree
│
├── Views/
│   ├── _ViewImports.cshtml  _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml          # Sidebar (permission-gated links) + header + Toast mixin
│   ├── Login/Index.cshtml             # Standalone page (Layout=null), AJAX login
│   ├── Login/AccessDenied.cshtml      # Standalone (Layout=null)
│   ├── Users/Index.cshtml             # DataTables grid + Add/Edit modals (Select2 + SweetAlert)
│   ├── Roles/Index.cshtml             # DataTables grid + Add/Edit modals (permission checkboxes)
│   ├── Units/Index.cshtml             # Tabs (Units | Unit Groups) + grids + Unit modals + Group modal (Select2 multi-select)
│   ├── Error/Index.cshtml             # Standalone (Layout=null), RequestId
│   ├── SaqTemplates/Index.cshtml    # DataTables grid + Add/Edit modals + Design modal (question cards + option editor)
│   ├── KriTemplates/Index.cshtml    # Tabs (KRI Templates | Threshold Colors | KRI Groups) + grids + Design/Color/Group/Bands modals
│   ├── Assessment/                  # Wizard flow: Index (DataTables grid) + Wizard (step bar + 5 step partials `_StepName|Saq|Kri|Schedule|Finalize.cshtml`)
│   └── Dashboard|Submissions|RiskRegisterTemplates/Index.cshtml  # Stubs
│
├── AI/
│   ├── Skills/ui-ux-pro-max.md        # AI UI generation prompt (tracked in git; currently deleted from the working tree)
│   └── Specs/{DESIGN,PROJECT-ANALYSIS}.md + {login,layout,branch,kri,error-page}/  # Design refs
│
└── wwwroot/
    ├── css/site.css                   # Stasis Enterprise design system
    ├── js/site.js                     # Sidebar/collapse/flyout/keyboard logic
    ├── js/modules/riskpulse.js        # Shared RiskPulse.* helpers (toast, postJson/getJson, serializeForm, populateSelect, showModal/hideModal, confirmDelete, initGrid)
    └── lib/                           # bootstrap, datatables, font-awesome, jquery, jquery-validation(-unobtrusive), select2, sweetalert2
```

---

## 4. Architecture Patterns

### 4.1 Pattern Set Currently Used

| # | Pattern | Where |
|---|---|---|
| 1 | **Classic MVC** (server-side Razor) | All controllers/views |
| 2 | **Service layer** (concrete classes via DI, Scoped) | `Services/` (`Login`/`Administration`/`Templates`/`Assessment`), registered in `Program.cs:22-30` |
| 3 | **EF Core + DbContext** directly inside services (no repository) | `UsersService`, `RolesService`, `DbAuthorizationService` |
| 4 | **Cookie auth + claim-based authorization** | `Program.cs:30-52`, `[Authorize(Policy=...)]` |
| 5 | **AJAX JSON endpoints** from controllers (not a Web API) | `Grid`/`Save`/`Login` actions |
| 6 | **DataTables grid fed by JSON** | `Views/{Users,Roles,SaqTemplates,KriTemplates}/Index.cshtml` |
| 7 | **ViewModel pattern** for page rendering | `*IndexViewModel` (Users, Roles, SAQ Templates, KRI Templates) |
| 8 | **DTO/result model** for service → controller | `LoginResultDto`, `*SaveDto`, `*DeleteRequestDto` (`Models/Dto`) |
| 9 | **Orchestrator service** composing lower services | `LoginOrchestratorService` |
| 10 | **Bootstrap 5 modal API** — programmatic open/close only through `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` in the shared module; no raw `bootstrap.Modal.getOrCreateInstance` or jQuery `$.fn.modal` in views (the vendored bundle has no jQuery modal API) | All interactive view script sections |
| 11 | **Shared JS module (`RiskPulse.*`)** — `wwwroot/js/modules/riskpulse.js` (loaded from `_Layout`) is the single home for cross-page helpers: `toastSuccess`/`toastError`/`toastGenericError`, `postJson`/`getJson`, `serializeForm`, `populateSelect`, `showModal`/`hideModal`, `confirmDelete`, `initGrid`. Views call the namespace and keep only validation/columns/wiring. | Users/Roles/SAQ/KRI/KRI-Config view script sections |

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

- **9 permissions** are declared once in code as constants in `Services/Login/PermissionCatalog.cs` (`PermissionCatalog.Dashboard | Submissions | Assessment | Users | Roles | Units | Saq | Kri | RiskRegister`) and referenced by:
  1. `Program.cs:40-52` — `AddPolicy($"Permission:{PermissionCatalog.X}")` … `RequireClaim("Permission", PermissionCatalog.X)`
  2. Controllers — `[Authorize(Policy = $"Permission:{PermissionCatalog.X}")]`
  3. `Views/Shared/_Layout.cshtml` — `User.HasClaim("Permission", PermissionCatalog.X)`
  4. `Services/Login/PermissionPageMapper.cs` — dict keys keyed by `PermissionCatalog.X`
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

Used by the four shell "Index" pages (Users, Roles, SAQ Templates, KRI Templates) and the login page.

```
Browser ──GET /Users/Index────────────────────────────► UsersController.Index
         ◄──HTML (full page)──────────────────────────── UsersController
                                                          │ UsersService.GetAllAsync()          │
                                                          │ UsersService.GetAllUnitsAsync()     │ EF Core
                                                          │ UsersService.GetAllRolesAsync()     │ (Include+AsNoTracking)
                                                          ▼                                   ▼
                                                        UsersIndexViewModel ──► PostgreSQL (riskpulse)
```

- **View model used:** `UsersIndexViewModel` — also `RolesIndexViewModel`, `SaqTemplatesIndexViewModel`, `KriTemplatesIndexViewModel`.
- **Rendering:** Razor view + `@Html.Raw(Json.Serialize(...))` to embed *initial dropdown data* (units/roles/permissions) directly into an inline `<script>` block — this is **not** an AJAX call; it is server-side JSON serialization injected into the page at render time.

```js
// Emitted by the Razor view (Views/Users/Index.cshtml:5-7)
var units = [{"unitId":1,"unitCode":"001","unitDesc":"Head Office"}, ...];
var roles = [{"roleId":1,"roleDesc":"IT Admin"}, ...];
var currentUserId = 1;
```

### 5.2 Flow B — AJAX JSON Grid (DataTables)

Used by all shell grids — `Users/Grid`, `Roles/Grid`, `SaqTemplates/Grid`, `KriTemplates/*Grid` (templates + colors + groups).

```
Browser (DataTables.ajax) ──GET /Users/Grid──────────► UsersController.Grid
        dataSrc:'data'                               │ GetAllAsync()
        ◄── { success:true, message:null,            │   .Select(u => new UserGridRowViewModel { ... })
              data:[ {id,username,unitId,            │
                       roleId,isActive}, ...] }       │
                                                      └──► EF Core → PostgreSQL
```

- **Grid library:** DataTables (client-side processing) — the whole row set is serialized and shipped to the browser in one response; paging/searching/sorting happen in the browser, not in SQL.
- **Payload shape:** named grid view models — `*GridRowViewModel` in `Models/ViewModel` (Users, Roles, SAQ, KRI) — projected in the controllers; camelCase JSON via the MVC web serializer defaults matches the DataTables `columns` config.
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
  - `Roles/Save` binds a dedicated **`RoleSaveDto`** (a DTO in `Models/Dto/`).
  - `Users/Save` binds a dedicated **`UserSaveDto`** (a DTO in `Models/Dto/`).
- **Return:** `ApiResponse<T>` envelope — success `{ success, message, data:{ id } }`, failure `{ success:false, message }`.

### 5.4 Flow D — AJAX Login (JSON)

```
Browser (loginForm) ──POST /Login/Login (JSON body)──► LoginController.Login
                        contentType:'application/json' │ null ?? ModelState check (LoginRequestDto)
                        ◄── { success:true,            │ LoginOrchestratorService.AuthenticateAsync
                               data:{ redirectUrl } }  │  1 AdAuthenticationService  (STUB: always true)
                                                       │  2 DbAuthorizationService    (user/role/perm graph)
                                                       │  3 build ClaimsPrincipal
                                                       ▲  PermissionPageMapper       (desc → (controller,action))
Post-success:                         HttpContext.SignInAsync(principal) → auth cookie
  window.location.href = response.data.redirectUrl ─┘
```

- **Payload style:** `application/json` via `JSON.stringify`, bound to `LoginRequestDto` with `[FromBody]` — consistent with Flows B/C.
- **Outcome:** On success the server sets the cookie via `SignInAsync` and returns `{ success, data:{ redirectUrl } }`; the browser navigates to `response.data.redirectUrl`.

### 5.5 Flow summary table

| Flow | Method/Route | Request body | Server binder | Response JSON | Data source |
|---|---|---|---|---|---|
| A — Page load | `GET /{Users,Roles}/Index` | — | — | HTML + embedded `Json.Serialize` | ViewModel from service |
| B — Grid | `GET /{Users,Roles}/Grid` | — | — | `{success:true, data:[…]}` (`UserGridRowViewModel` / `RoleGridRowViewModel`) | service → EF Core |
| C — Save | `POST /{Users,Roles}/Save` | JSON | `[FromBody]` (`UserSaveDto` / `RoleSaveDto`) | `{success,message,data:{id}}` | service → `SaveChanges` |
| D — Login | `POST /Login/Login` | JSON | `[FromBody]` (`LoginRequestDto`) | `{success, data:{redirectUrl}}` | orchestrator → cookie |

All AJAX responses use the shared **`ApiResponse<T>`** envelope (`Models/Dto/ApiResponse.cs`): `Success`, `Message`, `Data`, `Errors`. HTTP status stays `200`; outcome signalled by `success`. Grid keeps `data` as the array so DataTables `dataSrc:'data'` is unchanged; `id` / `redirectUrl` moved into `data`.

> **Note:** Flows A–C apply to SAQ Templates and KRI Templates unchanged — including the KRI threshold-config actions (Colors/Groups/Bands), which now live on the KriTemplates controller.

---

## 6. Layer Responsibility Map ("use grid/use JSON/use model where")

| Concern | Layer / File | Notes |
|---|---|---|
| **Grid** (data table UI) | DataTables in `Views/{Users,Roles,SaqTemplates,KriTemplates}/Index.cshtml` | client-side processing, AJAX `dataSrc:'data'` |
| **Grid data source** | `*Controller.Grid` → `*GridRowViewModel` (in `Models/ViewModel`) via `ApiResponse<T>` | camelCase (web JSON defaults) matches DataTables `columns`; not server-side processing |
| **JSON serialization (server→JS init)** | Razor `@Html.Raw(Json.Serialize(...))` | `Views/Users/Index.cshtml:5-7`, `Views/Roles/Index.cshtml:5` |
| **JSON produce/consume** | Controllers `Json(...)` + jQuery `$.ajax` | all AJAX in views' `@section Scripts` |
| **Model — EF entities** | `Data/Entries/*` | mapped 1:1 to `riskpulse.*` tables |
| **Model — view models** | `Models/ViewModel/*` | data shaped for a UI/view — `*IndexViewModel`, `*GridRowViewModel`, `*OptionViewModel` (Users/Roles/Login/Error/SAQ/KRI) |
| **Model — DTOs** | `Models/Dto/*` | data moving between layers/systems — `ApiResponse<T>`, `LoginRequestDto`, `LoginResultDto`, `*SaveDto`, `*DeleteRequestDto` |
| **Business logic** | `Services/*Service` | no repository layer; each service uses `AppDbContext` directly |
| **Data access** | `Data/AppDbContext` via services | `Include`/`AsNoTracking`/`SaveChanges` in services |
| **DB schema/DDL** | EF migration `20260812202536_UserPermissionControl` (files not in working tree) | `HasDefaultSchema("riskpulse")`; live DB provisioned manually via `Database/Seed.sql` |
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
Groups      (GroupId PK, GroupDesc)
UnitGroups  (UnitGroupId PK, GroupId FK→Groups, UnitId FK→Units)   [many-to-many join, unique GroupId+UnitId]
Roles       (RoleId PK, RoleDesc, DefaultPermissionId FK→Permissions)
RolePermissions (RolePermissionId PK, RoleId FK→Roles, PermissionId FK→Permissions)   [many-to-many join]
Users       (Id PK, Username, IsActive, UnitId FK→Units, RoleId FK→Roles)
SaqHeaders          (SaqHeaderId PK, SaqDesc, GroupId FK→Groups, SaqStatus varchar(32))
SaqQuestions        (QuestionId PK, SaqHeaderId FK→SaqHeaders, QuestionText, QuestionType varchar(32), AllowComment, DisplayOrder)
SaqQuestionOptions  (OptionId PK, QuestionId FK→SaqQuestions, OptionText, DisplayOrder)
KriHeaders          (KriHeaderId PK, KriHeaderDesc, GroupId FK→Groups, KriStatus varchar(32))
Kri                 (KriId PK, KriHeaderId FK→KriHeaders, KriDesc, AllowComment, KriThresholdGroupId FK→KriThresholdGroups)
KriThresholdGroups  (KriThresholdGroupId PK, KriThresholdGroupDesc)
KriThresholdColors  (ColorId PK, ColorDesc, HexCode)
KriThresholds       (KriThresholdId PK, KriThresholdGroupId FK→KriThresholdGroups, ColorId FK→KriThresholdColors, MinValue, MaxValue)
AssessmentHeaders   (AssessmentHeaderId PK, AssessmentName, AssessmentStatus varchar(32) — Draft/Active, SaqHeaderId FK→SaqHeaders, KriHeaderId FK→KriHeaders, RiskRegisterHeaderId nullable — Risk Register not built yet)
ScheduleHeaders     (ScheduleHeaderId PK, AssessmentHeaderId FK→AssessmentHeaders, ScheduleDesc, StartDate, EndDate)   [1:N — an assessment can be re-scheduled]
```

- **Enum→string conversion:** `Unit.UnitType`, `SaqStatus`, `QuestionType`, `KriStatus`, `AssessmentStatus` stored as `character varying(32)` (`AppDbContext.cs:18-71`).
- **Cascade/restrict rules** (`AppDbContext.cs:44-107`): `SaqQuestion→SaqQuestionOptions` cascade; `KriHeader→Kri` cascade with `Kri→KriThresholdGroup` restrict; `KriThresholdGroup→KriThresholds` cascade with `KriThreshold→KriThresholdColor` restrict; `AssessmentHeader→ScheduleHeaders` cascade with `AssessmentHeader→Saq/KriHeaders` restrict; `UnitGroup→Group/Unit` cascade with a unique `(GroupId, UnitId)` index; `SaqHeader/KriHeader→Group` restrict (a group in use by a template can't be deleted). AccessControl FKs (Users→Roles/Units, RolePermissions→Roles/Permissions) cascade by convention.
- **Implemented via:** EF Core migration `20260812202536_UserPermissionControl` (applied live; files not in the working tree). DB otherwise provisioned manually via `Database/Seed.sql`.

### 7.2 `Database/Seed.sql` — legacy content & risk

Seed.sql inserts the 9 permissions, 2 roles, 1 unit, and 1 test user. **However** lines 36+ contain an entire **legacy SQL Server schema** (`dbo.tblAssessmentModuleType`, `tblAssessmentHeader`, `tblSAQ*`, `tblKRI*`, `tblRiskRegister*` — `IDENTITY(1,1)`, `NVARCHAR`, `GETDATE()`; line 40 is bare DDL, lines 60+ block-commented) inside a "Do not run this manually" comment. This is not PostgreSQL-compatible and is a copy of a different-era design. It should be extracted to a separate reference document (see mismatch #6).

---

## 8. Pattern Mismatches & Inconsistencies

> Current **open** deviations only, with evidence and fix. Already-resolved items (save-model binding + server-side DataAnnotations, `ApiResponse<T>` envelope, login JSON standardization, `PermissionCatalog` single source, named grid view models `UserGridRowViewModel`/`RoleGridRowViewModel`, the DTO/ViewModel layer split, self-edit rule moved into `UsersService`) are reflected in §4–§6 and §10 and are not repeated here.

### 8.1 Architecture & layering

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 1 | **No repository / unit-of-work; no interface abstractions** — every service talks to `AppDbContext` directly and exposes concrete methods returning entities. The data layer is untestable and services can't be swapped or faked. | `Services/Administration/UsersService.cs:17-24`, `Services/Administration/RolesService.cs:16-25`, `Services/Administration/UnitsService.cs:14-21`, `Services/Login/DbAuthorizationService.cs:18-29` | Introduce `IUsersService`, `IRolesService`, `ILoginOrchestratorService`, `IAdAuthenticationService`, `IDbAuthorizationService` (optionally `IRepository<T>`/`IUnitOfWork`) and have services return DTOs, not entities. |
| 2 | **Concrete-class-only DI** — every service is registered as `AddScoped<Concrete>()`, so nothing can be mocked or swapped. | `Program.cs:22-29` | Register `AddScoped<IXxx, Xxx>()` against the interfaces from #1. |
| 3 | **DataTables runs client-side processing** — the full row set ships to the browser in one response; paging/search/sort run in JS, not SQL. Latent scale problem once Submissions hold real data. | `Views/Users/Index.cshtml:191-216`, `Views/Roles/Index.cshtml:220-243`, `Views/Units/Index.cshtml` | For large tables use `serverSide:true` and handle `start`/`length`/`search` at the Grid endpoints. |
| 4 | **Global/inline JS, no modules, no bundling** — page logic lives in `@section Scripts` with hand-rolled `$.ajax` calls; only vendored libs are static. | `wwwroot/js/modules/riskpulse.js` | ~Resolved — shared helpers extracted into the module; page scripts now call `RiskPulse.*` and hold only validation, column configs, and wiring (dedup was the fix; no bundling, no build step). |
| 5 | **`AdAuthenticationService` is a stub that always returns `true`** — any username/password is "valid" as long as the user exists in DB. | `Services/Login/AdAuthenticationService.cs:7-9` | Implement a real directory/identity-provider lookup (or explicitly dev-gate the stub). |

### 8.2 Data & persistence

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 6 | **Seed data is manual SQL that also embeds a legacy SQL Server schema** — `Database/Seed.sql` mixes live PostgreSQL inserts (permissions, roles, unit, test user) with a commented-out, non-PostgreSQL `dbo.tbl*` design (`IDENTITY(1,1)`, `NVARCHAR`, `GETDATE()`). | `Database/Seed.sql` (lines 1–33 live; 35–324 legacy `dbo.tbl*`) | Move seed into EF Core (`modelBuilder.HasData` / seed extension) and extract the legacy schema to a reference doc, out of executable SQL. |
| 7 | **No migrate/seed bootstrap at startup** — the app assumes `dotnet ef database update` was run externally; a fresh DB will fail at first query. | `Program.cs:12-13` (no `Db.Database.Migrate()`) | Add dev-only `Migrate()` (+ data seed) bootstrap, or document the required EF command in a README. |
| 8 | **No logging (`ILogger`) anywhere** — service/DB exceptions bubble with no trace and the catch blocks can't be audited. | all `Services/*` and `Controllers/*` | Inject `ILogger<T>` and log at service and catch boundaries. |

### 8.3 Security

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 9 | **No CSRF protection on cookie-auth state-changing endpoints** — `Users/Save`, `Roles/Save`, `Units` save/delete, and `Login/Login` are JSON POSTs authenticated by cookie, but the app has no antiforgery tokens (`[ValidateAntiForgeryToken]` / `@Html.AntiForgeryToken()` are absent everywhere). | `Controllers/UsersController.cs:50-51`, `Controllers/RolesController.cs:47-48`, `Controllers/UnitsController.cs:47-49`, `Controllers/LoginController.cs:35-37` | Emit antiforgery tokens in the views and add `[ValidateAntiForgeryToken]` on the POST actions; for `[FromBody]` JSON use `AddAntiforgery` + a header token. |
| 10 | **Database credentials committed to source** — the PostgreSQL connection string (`Server`, `Port`, user, `Password=123456`) is hard-coded in `appsettings.json` and tracked by git. | `appsettings.json:8-10` | Move credentials to user-secrets / environment variables; keep no secret (or a harmless dev value) in the repo. |

### 8.4 Code hygiene & minor

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| _(none open)_ | Hygiene items resolved — dead zero-FK fallbacks in `UsersService`, unused `UsersIndexViewModel.Users` / `RolesIndexViewModel.Roles`, dead `KriBandSaveDto.KriThresholdId`, unreferenced `_ValidationScriptsPartial`, empty `Infrastructure/` + `Validation/`, dead `Models\Auth\**` csproj exclusion, undefined `status-active`/`login-icon-circle` classes, and the tri-spelled brand name were all removed in the hygiene pass. | (removed) | (done) |

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
   ├── Login:      IAdAuthenticationService  → AdAuthenticationService (real impl)
   │              IDbAuthorizationService   → DB lookup
   │              ILoginOrchestratorService → composes above
   │              PermissionCatalog / PermissionPageMapper (single source, shared by all modules)
   ├── Administration: IUsersService / IRolesService
   ├── Templates:  ISaqTemplatesService / IKriTemplatesService
   └── Data access via IRepository<T> (or keep DbContext here behind service facades)
   │
Data — AppDbContext + EF Migrations + EF seed (HasData)
   │
PostgreSQL (riskpulse schema)
```

Key decisions for the target:
1. **DTOs in, ViewModels out** — inbound payloads are `*SaveDto` / `*DeleteRequestDto` / `LoginRequestDto` (`Models/Dto`); outbound UI data is `*IndexViewModel` / `*GridRowViewModel` / `*OptionViewModel` (`Models/ViewModel`); `ApiResponse<T>` is the shared envelope (already live).
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
| PostgreSQL + EF Core migrations | ✅ Live — schema per migration `20260812202536_UserPermissionControl` (files not in working tree; DB provisioned via `Database/Seed.sql`) |
| DataTables AJAX grids + JSON save flows | ✅ Working — every grid returns a named `*GridRowViewModel` (Users, Roles, SAQ headers/questions/options, KRI headers/items, KRI colors/groups/bands) |
| Bootstrap 5 modals (programmatic open/close) | ✅ Fixed — open/close via `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` in the shared module; no jQuery `$.fn.modal` or raw `getOrCreateInstance` in views |
| Design system (CSS) + AI specs | ✅ Complete |
| Project structure conventions | ✅ Complete — controllers flat & 1:1 with Views; services grouped by workflow; Models split `Dto`/`ViewModel`; entities in `Data/Entries`; Views folder=controller, file=action |
| Domain pages (Dashboard, Submissions, Assessment, Risk Register Templates) | ⬜ Stubs |
| SAQ Templates (grid, header CRUD, question/option designer, Locked immutable rule) | ✅ Implemented |
| KRI Templates (grid, header CRUD, split-pane KRI builder with value/comment/group, Locked immutable rule) | ✅ Implemented |
| KRI Config (threshold colors, groups, value-band editor) | ✅ Merged into KRI Templates as tabs |
| Repository / unit-of-work / interface services | ❌ Not started |
| DTOs & uniform API envelope | ✅ Complete — `ApiResponse<T>` envelope; `*Dto` inputs (`LoginRequestDto`, `LoginResultDto`, `*SaveDto`, `*DeleteRequestDto`) + `*ViewModel` outputs (`*IndexViewModel`, `*GridRowViewModel`, `*OptionViewModel`) split by layer |
| Server-side validation (DataAnnotations on save models) | ✅ On every save model (Users/Roles/Login/SAQ/KRI) — FK `[Range]`, `[Required]`, `[MinLength]`, `[RegularExpression]` |
| Permission single source (`PermissionCatalog`) | ✅ Resolved — policies, `[Authorize]`, sidebar, `PermissionPageMapper` all reference the catalog |
| Business rule placement | ✅ Service-side — self-edit, duplicate, not-found, locked-template, and default-permission rules all throw `InvalidOperationException` from services |
| Code hygiene (dead code, unused scaffolding, brand name) | ✅ Cleaned — see §8.4 |
| Shared JS module (`RiskPulse.*` in `wwwroot/js/modules/riskpulse.js`) | ✅ Dedup done — all 5 shell views use the helpers; generic-error toast single-sourced (§4.1 #11) |
| CSRF protection | ❌ Not started (§8 #9) |
| Secrets management | ❌ Credentials committed (§8 #10) |
| Real AD / identity provider | ❌ Stub (always true) (§8 #5) |
| Server-side grid processing | ❌ Not started (client-side only) (§8 #3) |
| Logging | ❌ Not started (§8 #8) |
| Tests / CI / Docker | ❌ Not started |

---

## 11. Next Logical Steps

1. ✅ **Fix validation & binding consistency** — `UserSaveDto`/`RoleSaveDto`, `[Required]`/`[RegularExpression]`, null-guard both `Save` actions (resolved).
2. ✅ **Introduce `ApiResponse<T>`** and adopt in `Grid`/`Save`/`Login`; login standardized to JSON `[FromBody]` (resolved).
3. ✅ **Single permission source** (`PermissionCatalog`) for policies/layout/mapper (resolved).
4. ✅ **Named grid view models** — `UserGridRowViewModel`, `RoleGridRowViewModel` replace anonymous grid projections (resolved).
5. ✅ **Move self-edit rule into `UsersService`** — `UpdateUserAsync(model, actingUserId)`; controller left thin (resolved).
6. ✅ **Fix Bootstrap 5 modal API** — replaced all 15 `$('#x').modal('show'/'hide')` calls with `bootstrap.Modal.getOrCreateInstance(...)` across Users/Roles/SAQ/KRI/KRI-Config (resolved).
7. ✅ **Close pattern deviations** — `ColorsGrid` now returns `KriColorGridRowViewModel`; default-permission rule moved into `RolesService`; `SaveBands` + all delete actions get the null-guard/ModelState block; DataAnnotations aligned (FK `[Range]` on `SaqHeaderId`/`KriHeaderId`, validation on `KriBandsSaveDto`/`KriBandSaveDto`, redundant `[Required]` removed); `form-select` dropped from `input-stasis` selects (resolved).
8. ✅ **Hygiene pass** — removed dead `UsersService` fallbacks, dead view-model props (`UsersIndexViewModel.Users`, `RolesIndexViewModel.Roles`), dead `KriBandSaveDto.KriThresholdId`, unreferenced `_ValidationScriptsPartial`, empty `Infrastructure/`/`Validation/` folders, dead `Models\Auth\**` csproj exclusion, undefined CSS classes; added KriConfig modal input resets; unified brand on "Risk Pulse" (resolved).
9. ✅ **JS dedup / shared module** — created `wwwroot/js/modules/riskpulse.js` (`RiskPulse.*`: toast helpers, `postJson`/`getJson` with auto generic-error toast, `serializeForm`, `populateSelect`, `showModal`/`hideModal`, `confirmDelete`, `initGrid`); loaded from `_Layout`; refactored all 5 shell views to use it (~250 duplicated lines removed, generic-error message now single-sourced). Login page intentionally left standalone (resolved).
10. ✅ **Restructure layers** — controllers flattened to `Controllers/` (12 classes, namespace `RiskPulse.Controllers`, thin 1:1 with views); `Models/` split into `Models/Dto/` (16 files, `*Dto` postfix) + `Models/ViewModel/` (19 files, `*ViewModel` postfix), `ApiResponse<T>` kept as the sole un-postfixed type; entities moved to `Data/Entries/`; services grouped by workflow (Login/Administration/Templates); Views verified folder=controller / file=action (resolved).
11. **Add CSRF protection** — antiforgery tokens + `[ValidateAntiForgeryToken]` on `Save`/`Login` POSTs (§8 #9).
12. **Move DB credentials out of source** — user-secrets / environment variables (§8 #10).
13. **Extract interfaces + register DI** — `IUsersService`, `IRolesService`, `IAdAuthenticationService`, `IDbAuthorizationService` (§8 #1–#2).
14. **Implement real AD** or dev-gate explicitly (§8 #5).
15. **EF seed + clean `Seed.sql`** of legacy SQL Server DDL (§8 #6).
16. **Add logging** at service boundaries (§8 #8).
17. Then build domain: keep this file current for Dashboard/Submissions with the standardized flow (server-side grid for submissions volume, §8 #3).