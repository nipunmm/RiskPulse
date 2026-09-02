# RiskPulse — Project Architecture & Data-Flow Analysis

> Generated from codebase analysis. Last reviewed: August 2026.

---

## 1. Overview

**RiskPulse** is an enterprise risk management application built with **ASP.NET Core MVC**. The domain targets KRI (Key Risk Indicators), SAQ (Self-Assessment Questionnaires), branch-level risk submissions, and RAG (Red/Amber/Green) status tracking for high-stakes financial environments.

**Current Phase:** Access-control scaffolding complete — authentication (cookie + claims), user/role/permission management (working CRUD + DataTables grids). SAQ Templates and KRI Templates implemented — headers, item designers, threshold-config tabs (merged into KRI Templates), and Locked-immutability rules. Each template header is **linked to a required unit group or unit** (`GroupId FK→Groups` XOR `UnitId FK→Units`, selected in the add/edit modals and shown in the grid). **Units page implemented** — two-tab Administration page (Unit CRUD | Unit Group CRUD) under the new `Units` permission with Select2 group→unit assignment. **Assessment wizard implemented** — step flow (name → SAQ → KRI → schedule → finalize) with per-step AJAX persistence, draft edit/re-save through the stages, and activate rules. Remaining domain pages (Dashboard, Submissions, Risk Register templates) are **stubs**. PostgreSQL persistence is live via EF Core; the schema was formerly created by EF migration `20260816161300_UserPermissionControl`, which was then **removed from the repo** — the DB is now provisioned manually via `Database/Seed.sql`, which also carries a legacy SQL Server `dbo.tbl*` draft. Structure is convention-aligned: controllers are **flat** in `Controllers/` (thin, 1:1 with `Views/{Controller}/`), services are **grouped by workflow** (`Login`/`Administration`/`Templates`/`Assessment`), and `Models/` is split by layer distinction into `Models/Dto/` (inter-system data, `*Dto` postfix), `Models/ViewModel/` (UI-shaped data, `*ViewModel` postfix), and `Models/Enum/` (domain enums, persisted as strings).

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
| View Engine | Razor `.cshtml` | Server-rendered, sections for Styles/Scripts |
| Grid | DataTables | `jquery.dataTables.min.js` + `dataTables.bootstrap5.min.js` (client-side processing, AJAX JSON source) |
| JS | jQuery | AJAX, DOM, form `serializeArray` |
| UI Feedback | SweetAlert2 | `Swal.mixin` toast pattern on every page |
| Dropdowns | Select2 | First-party `rp` theme (`select2-container--rp` in `site.css`), search at ≥10 options, `dropdownParent` bound to modal, colored options via `data-color`/`data-kind` |
| CSS | Bootstrap 5 + "Stasis Enterprise" | Custom design tokens in `wwwroot/css/site.css` |
| Icons | Font Awesome 6 | `all.min.css` |
| Fonts | Inter, JetBrains Mono | Self-hosted `.woff2` |
| Client Validation | jQuery Validate (vendored) + custom JS rules | Manual `validateXxx()` functions, not unobtrusive tags |
| Scaffolding | EF Core Migrations | Migration `20260816161300_UserPermissionControl` **removed from the repo** (commit `f8cacd2`); DB provisioned manually via `Database/Seed.sql` |

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
│   ├── LoginController.cs        # GET/POST Index, POST Login (JSON [FromBody]), Logout, AccessDenied (AllowAnonymous)
│   ├── UsersController.cs        # Index (View), Grid (JSON incl. unit/role descs), Save (JSON [FromBody]), Delete (JSON)
│   ├── RolesController.cs        # Index (View), Grid (JSON incl. permissionIds/descs), Save (JSON [FromBody]), Delete (JSON)
│   ├── UnitsController.cs        # 2-tab Units page: UnitGrid/SaveUnit/DeleteUnit + GroupGrid/SaveGroup/DeleteGroup (JSON)
│   ├── SaqTemplatesController.cs # Grid/Save/Delete headers + QuestionsGrid/SaveQuestion/DeleteQuestion (JSON)
│   ├── KriTemplatesController.cs # Templates + config: Grid/Save/Delete headers, KrisGrid/SaveKri/DeleteKri, Colors/Groups/Bands CRUD (JSON)
│   ├── RiskRegisterTemplatesController.cs # Stub, [Authorize(Policy="Permission:Risk Register")]
│   ├── DashboardController.cs    # Stub, [Authorize(Policy="Permission:Dashboard")]
│   ├── SubmissionsController.cs  # Stub, [Authorize(Policy="Permission:Submissions")]
│   ├── AssessmentController.cs   # Wizard: Index (grid), Wizard (step flow), SaveName/SaveSaq/SaveKri/SaveSchedule/Finalize/Delete (JSON)
│   ├── ErrorController.cs        # GET /Error/Index
│   └── ControllerHelpers.cs      # Static helpers: ValidateModel, TryExecute, TrySave, TryDelete (null-guard + ModelState + ApiResponse)
│
├── Data/
│   ├── AppDbContext.cs               # DbContext: schema, DbSets, enum→string conversions, FK/cascade config
│   ├── Entries/                      # EF entities — flat 1:1 mirror of riskpulse.* tables (no domain grouping)
│   │   ├── User.cs  Role.cs  Permission.cs  RolePermission.cs  Unit.cs
│   │   ├── Group.cs  UnitGroup.cs   # Unit grouping (Group 1—N UnitGroup N—1 Unit, unique GroupId+UnitId)
│   │   ├── SaqHeader.cs  SaqQuestion.cs  SaqQuestionOption.cs
│   │   ├── KriHeader.cs  Kri.cs  KriThresholdGroup.cs  KriThresholdColor.cs  KriThreshold.cs
│   │   └── AssessmentHeader.cs  ScheduleHeader.cs   # Assessment wizard entities
│   └── Extensions/
│       └── DbSetExtensions.cs        # EnsureUniqueAsync (duplicate → InvalidOperationException), ToOptionListAsync (→ OptionViewModel)
│
├── Models/
│   ├── Dto/                        # Data that moves between layers/systems (23 files, `*Dto` postfix)
│   │   ├── ApiResponse.cs          # Shared JSON envelope { success, message, data, errors }
│   │   ├── LoginResultDto.cs  LoginRequestDto.cs  UserAuthorizationDto.cs
│   │   ├── UserSaveDto.cs  RoleSaveDto.cs  UnitSaveDto.cs  GroupSaveDto.cs  DeleteRequestDto.cs  SaveResultDto.cs
│   │   ├── Saq*.cs                 # SaqHeaderSaveDto, SaqQuestionSaveDto, SaqOptionSaveDto
│   │   ├── Kri*.cs                 # KriHeaderSaveDto, KriSaveDto, KriColorSaveDto, KriThresholdGroupSaveDto, KriBandsSaveDto, KriBandSaveDto
│   │   └── Assessment*.cs          # AssessmentNameSaveDto, AssessmentTemplateSaveDto, AssessmentFinalizeDto, ScheduleSaveDto
│   ├── ViewModel/                  # Data shaped specifically for a UI/view (24 files, `*ViewModel` postfix)
│   │   ├── UsersIndexViewModel.cs  RolesIndexViewModel.cs  UnitsIndexViewModel.cs  ErrorViewModel.cs
│   │   ├── UserGridRowViewModel.cs  RoleGridRowViewModel.cs  UnitGridRowViewModel.cs  GroupGridRowViewModel.cs  OptionViewModel.cs
│   │   ├── Saq*.cs                 # SaqTemplatesIndexViewModel, SaqGridRowViewModel, SaqQuestionGridRowViewModel, SaqOptionGridRowViewModel, SaqStatusOptionViewModel
│   │   ├── Kri*.cs                 # KriTemplatesIndexViewModel, KriStatusOptionViewModel, KriGridRowViewModel, KriItemGridRowViewModel, KriGroupGridRowViewModel, KriColorGridRowViewModel, KriColorOptionViewModel, KriBandGridRowViewModel
│   │   └── Assessment*.cs          # AssessmentGridRowViewModel, AssessmentWizardViewModel
│   └── Enum/                       # Domain enums, persisted as varchar(32)
│       ├── UnitType.cs  QuestionType.cs  SaqStatus.cs  KriStatus.cs  AssessmentStatus.cs
│
├── Services/
│   ├── Login/                        # Authentication + authorization
│   │   ├── AdAuthenticationService.cs     # STUB — ValidateCredentialsAsync always returns true
│   │   ├── DbAuthorizationService.cs       # Loads user+role+permissions+unit into UserAuthorizationDto
│   │   ├── LoginOrchestratorService.cs     # Orchestrates: AD check → DB lookup → claims principal
│   │   ├── PermissionCatalog.cs            # Single source for permission constants (policies, layout, mapper)
│   │   └── PermissionPageMapper.cs         # Static: PermissionDesc → (Controller, Action)
│   ├── Administration/               # Users + roles + units CRUD
│   │   ├── UsersService.cs                 # CRUD for users + self-edit guard (direct AppDbContext)
│   │   ├── RolesService.cs                 # CRUD for roles + permission mapping + default-permission rule (direct AppDbContext)
│   │   └── UnitsService.cs                 # Unit CRUD (duplicate guard, block delete when referenced) + group CRUD (≥2 units, clear/re-add UnitGroups)
│   ├── Templates/                    # SAQ + KRI templates
│   │   ├── SaqTemplatesService.cs           # SAQ header CRUD (Group XOR Unit rule, lock rules) + question/option designer (dup question guard)
│   │   └── KriTemplatesService.cs           # KRI header CRUD + KRI items + threshold config (colors/groups/bands) + lock/duplicate rules
│   └── Assessment/                   # Assessment wizard workflow
│       └── AssessmentService.cs             # Draft create/rename, SAQ+KRI template pick (non-Locked), schedule upsert (UTC timestamptz), finalize (Active requires SAQ+KRI), delete (drafts only)
│
├── Database/
│   └── Seed.sql                       # Manual permission/role/unit/user inserts (lines 1–35 live) + legacy dbo schema draft (line 39+, non-PostgreSQL)
│
├── Views/
│   ├── _ViewImports.cshtml  _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml          # Sidebar (permission-gated links incl. Templates + Administration submenus) + frosted topbar + shell polish
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
│   ├── DESIGN.md                      # Stasis Enterprise design-system spec
│   ├── PROJECT-ANALYSIS.md            # This document
│   ├── Skills/                        # Empty (folder placeholder; file tracked then removed from the working tree)
│   └── Specs/{login,layout,kri,error-page,branch}/   # Per-feature DESIGN.md + code.html + screen.png
│
└── wwwroot/
    ├── css/site.css                   # Stasis Enterprise design system
    ├── js/site.js                     # Sidebar/collapse/submenu/keyboard logic
    ├── js/modules/riskpulse.js        # Shared RiskPulse.* helpers (see §4.1 #11)
    └── lib/                           # bootstrap, datatables, font-awesome, jquery, select2, sweetalert2
```

---

## 4. Architecture Patterns

### 4.1 Pattern Set Currently Used

| # | Pattern | Where |
|---|---|---|
| 1 | **Classic MVC** (server-side Razor) | All controllers/views |
| 2 | **Service layer** (concrete classes via DI, Scoped) | `Services/` (`Login`/`Administration`/`Templates`/`Assessment`), registered in `Program.cs:23-31` |
| 3 | **EF Core + DbContext** directly inside services (no repository) | `UsersService`, `RolesService`, `UnitsService`, `DbAuthorizationService` |
| 4 | **Cookie auth + claim-based authorization** | `Program.cs:32-53`, `[Authorize(Policy=...)]` |
| 5 | **AJAX JSON endpoints** from controllers (not a Web API) | `Grid`/`Save`/`Login`/wizard actions |
| 6 | **DataTables grid fed by JSON** | `Views/{Users,Roles,Units,SaqTemplates,KriTemplates,Assessment}/Index.cshtml` |
| 7 | **ViewModel pattern** for page rendering | `*IndexViewModel` (Users, Roles, Units, SAQ Templates, KRI Templates, Assessment Wizard) |
| 8 | **DTO/result model** for service → controller | `LoginResultDto`, `*SaveDto`, `*DeleteRequestDto` (`Models/Dto`) |
| 9 | **Orchestrator service** composing lower services | `LoginOrchestratorService` |
| 10 | **Bootstrap 5 modal API** — programmatic open/close only through `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` in the shared module; no raw `bootstrap.Modal.getOrCreateInstance` or jQuery `$.fn.modal` in views (the vendored bundle has no jQuery modal API); the layout re-hosts `.modal` nodes as direct children of `<body>` so the Bootstrap backdrop never paints over them | All interactive view script sections + `_Layout.cshtml:226-228` |
| 11 | **Shared JS module (`RiskPulse.*`)** — `wwwroot/js/modules/riskpulse.js` (loaded from `_Layout`) is the single home for cross-page helpers: `toastSuccess`/`toastError`/`toastGenericError`, `escapeHtml`, `postJson`/`getJson` (auto generic-error toast, `ApiResponse.success` routing, optional `$trigger` flight lock + `handlers.complete`), `serializeForm` (checkbox booleans + numeric coercion), `populateSelect`, `initSelect2` (rp theme, `data-color` swatches + `data-kind` pills, modal `dropdownParent`), `showModal`/`hideModal`, `confirmDelete`, `initGrid`, `pill`, `statusPill`, `statusKind`, `validationError`, `clearFieldErrors`. Views call the namespace and keep only validation/columns/wiring. | Users/Roles/Units/SAQ/KRI/KRI-Config/Assessment view script sections |
| 12 | **Two-tab page pattern** — a single Index view with Bootstrap tabs, one grid per tab, each tab's CRUD hitting its own JSON endpoints | `Views/Units/Index.cshtml` (Units \| Unit Groups), `Views/KriTemplates/Index.cshtml` (Templates \| Threshold Colors \| KRI Groups) |
| 13 | **Server-persisted wizard pattern** — a 5-step stepper (`data-step=1..5` + a locked `data-step="rr"` Risk Register placeholder) where each step posts its own AJAX endpoint (`SaveName`/`SaveSaq`/`SaveKri`/`SaveSchedule`) before advancing; a JS `state` object tracks `completed`/`frontier`; step 5 posts `Finalize` with `status: 'Draft' \| 'Active'`; non-drafts are read-only (`CanEdit`) | `Views/Assessment/Wizard.cshtml` + `_StepName|Saq|Kri|Schedule|Finalize.cshtml` + `AssessmentController` |

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

Default entry route is **Login/Index**. The sidebar (`_Layout.cshtml`) gates each link with `User.HasClaim("Permission", "<X>")`, grouping page links under collapsible **Templates** and **Administration** submenus.

### 4.3 Authorization Model

- **9 permissions** are declared once in code as constants in `Services/Login/PermissionCatalog.cs` (`PermissionCatalog.Dashboard | Submissions | Assessment | Users | Roles | Units | Saq | Kri | RiskRegister`) and referenced by:
  1. `Program.cs:44-53` — `AddPolicy($"Permission:{PermissionCatalog.X}")` … `RequireClaim("Permission", PermissionCatalog.X)`
  2. Controllers — `[Authorize(Policy = $"Permission:{PermissionCatalog.X}")]`
  3. `Views/Shared/_Layout.cshtml` — `User.HasClaim("Permission", PermissionCatalog.X)`
  4. `Services/Login/PermissionPageMapper.cs` — dict keys keyed by `PermissionCatalog.X`
- The **data source** remains the DB `riskpulse.Permissions.PermissionDesc` (`Database/Seed.sql`) — constant values must match those rows exactly. One value contains a space: `"Risk Register"`.
- At login, `DbAuthorizationService` loads User → Role → RolePermissions → Permissions, and `LoginOrchestratorService` writes each `PermissionDesc` as a `Claim("Permission", ...)` plus `Name`, `NameIdentifier`, `Role`, `DefaultPage`, `Unit` claims into the auth cookie.

---

## 5. Data-Flow Patterns (Frontend → Backend → Database)

There are **five** distinct request/response flows in use today.

### 5.0 Common request/response conventions

- **All AJAX** endpoints return `application/json`.
- **All responses** are `HTTP 200 OK` regardless of outcome — success is signalled by a `success: true/false` flag in the JSON body.
- **Client validation** happens first (SweetAlert toasts + `RiskPulse.validationError`), then a `POST` to the server, then another `Toast.fire()` on the returned message.
- Controllers use `ControllerHelpers.ValidateModel` / `TryExecute` / `TrySave` / `TryDelete` — null-guard, ModelState check, `InvalidOperationException` message passthrough, generic-error fallback, all wrapped in the `ApiResponse<T>` envelope.

### 5.1 Flow A — Server-rendered page load (MVC + ViewModel)

Used by the shell "Index" pages (Users, Roles, Units, SAQ Templates, KRI Templates), the Assessment Index/Wizard pages, and the login page.

```
Browser ──GET /Users/Index────────────────────────────► UsersController.Index
         ◄──HTML (full page)──────────────────────────── UsersController
                                                          │ UsersService.GetGridRowsAsync()  │
                                                          │ UsersService.GetAllRolesAsync()  │ EF Core
                                                          │ UnitsService.GetAllUnitsAsync()  │ (Include+AsNoTracking)
                                                          ▼                                  ▼
                                                        UsersIndexViewModel ──► PostgreSQL (riskpulse)
```

- **View model used:** `UsersIndexViewModel` — also `RolesIndexViewModel`, `UnitsIndexViewModel`, `SaqTemplatesIndexViewModel`, `KriTemplatesIndexViewModel`, `AssessmentWizardViewModel`.
- **Rendering:** Razor view + `@Html.Raw(Json.Serialize(...))` to embed *initial dropdown data* (units/roles/permissions/groups/colors/statuses) directly into an inline `<script>` block — this is **not** an AJAX call; it is server-side JSON serialization injected into the page at render time.
- **Status options:** `SaqStatusOptionViewModel.GetAll()` / `KriStatusOptionViewModel.GetAll()` return `Value`/`Label` for every enum member **except `Locked`** — Locked is a system-set state (enforced by the service) and is never offered as a selectable status in the add/edit modals.

### 5.2 Flow B — AJAX JSON Grid (DataTables)

Used by all shell grids — `Users/Grid`, `Roles/Grid`, `Units/UnitGrid|GroupGrid`, `SaqTemplates/Grid|QuestionsGrid`, `KriTemplates/Grid|KrisGrid|ColorsGrid|GroupsGrid|BandsGrid`, `Assessment/Grid`.

```
Browser (DataTables.ajax) ──GET /Users/Grid──────────► UsersController.Grid
        dataSrc:'data'                               │ GetAllAsync()
        ◄── { success:true, message:null,            │   .Select(u => new UserGridRowViewModel { ... })
              data:[ {id,username,unitId,            │
                       roleId,isActive}, ...] }       │
                                                      └──► EF Core → PostgreSQL
```

- **Grid library:** DataTables (client-side processing) — the whole row set is serialized and shipped to the browser in one response; paging/searching/sorting happen in the browser, not in SQL.
- **Payload shape:** named grid view models — `*GridRowViewModel` in `Models/ViewModel` (Users, Roles, Units, Groups, SAQ, KRI, KRI-config, Assessment) — projected in the services; camelCase JSON via the MVC web serializer defaults matches the DataTables `columns` config.
- **Role grid JSON** additionally includes nested arrays `permissionIds` / `permissionDescs` for the edit modal.
- **Ordering:** user/role/unit/groups grids sort ascending by Id; SAQ template, KRI template, and assessment grids sort **descending** (`OrderByDescending`) so the newest records appear first.

### 5.3 Flow C — AJAX JSON Form Submit (Create / Update)

Used by `Users/Save`, `Roles/Save`, `Units/SaveUnit|SaveGroup`, `SaqTemplates/Save|SaveQuestion`, `KriTemplates/Save|SaveKri|SaveColor|SaveGroup|SaveBands`, `Assessment/Delete`.

```
Browser (jQuery serializeArray) ──POST /Users/Save──────────► UsersController.Save
  { contentType:'application/json',                        │ null ?? ModelState check
    data: JSON.stringify(payload) }                        │ business rule (can't edit self)
    ◄── { success:true, message:"User saved..",            ▼
          data:{ id:12 } }                                  UsersService.CreateUserAsync/UpdateUserAsync
                                                             ├─ duplicate checks
                                                             └─ _db.SaveChangesAsync()  → PostgreSQL
```

- **Model binding:** `[FromBody]` deserializes the JSON body into dedicated `*SaveDto` types (`UserSaveDto`, `RoleSaveDto`, `UnitSaveDto`, `GroupSaveDto`, `SaqHeaderSaveDto`, `SaqQuestionSaveDto`, `KriHeaderSaveDto`, `KriSaveDto`, `KriColorSaveDto`, `KriThresholdGroupSaveDto`, `KriBandsSaveDto`).
- **Return:** `ApiResponse<T>` envelope via `ControllerHelpers.TrySave` / `TryDelete` — success `{ success, message, data:{ id } }`, failure `{ success:false, message }`.

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

### 5.5 Flow E — Assessment wizard step persistence

Each wizard step persists independently via its own AJAX endpoint, then the stepper advances (JS `state.completed`/`state.frontier`).

```
Browser (wizard JS) ──POST /Assessment/SaveName     { assessmentHeaderId:0, assessmentName }    ──► SaveName    → CreateDraftAsync / UpdateNameAsync
                    ──POST /Assessment/SaveSaq      { assessmentHeaderId, templateHeaderId }    ──► SaveSaq     → SetSaqTemplateAsync (rejects Locked)
                    ──POST /Assessment/SaveKri      { assessmentHeaderId, templateHeaderId }    ──► SaveKri     → SetKriTemplateAsync (rejects Locked)
                    ──POST /Assessment/SaveSchedule { assessmentHeaderId, scheduleDesc, startDate, endDate } ──► SaveSchedule → UpsertScheduleAsync
                    ──POST /Assessment/Finalize     { assessmentHeaderId, status:'Draft'|'Active' } ──► Finalize    → FinalizeAsync
```

- **Draft rules (service-side):** renaming, template pick, and schedule upsert all run through `RequireDraftAsync` (edit = Draft only); `FinalizeAsync(Active)` requires both SAQ and KRI templates; `DeleteAsync` allows drafts only. Non-draft wizard pages render with `CanEdit = false` and the JS blocks navigation.
- **timestamptz gotcha:** `ScheduleHeader.StartDate/EndDate` are `timestamptz`; the JSON binder yields `DateTimeKind.Unspecified`, so `AssessmentService.UpsertScheduleAsync` wraps values in `DateTime.SpecifyKind(..., DateTimeKind.Utc)` before saving.

### 5.6 Flow summary table

| Flow | Method/Route | Request body | Server binder | Response JSON | Data source |
|---|---|---|---|---|---|
| A — Page load | `GET /{Users,Roles,Units,SaqTemplates,KriTemplates,Assessment}/Index` + `/Assessment/Wizard` | — | — | HTML + embedded `Json.Serialize` | ViewModel from service |
| B — Grid | `GET /{Users,Roles}/Grid`, `Units/{UnitGrid,GroupGrid}`, `SaqTemplates/{Grid,QuestionsGrid}`, `KriTemplates/*Grid`, `Assessment/Grid` | — | — | `{success:true, data:[…]}` (`*GridRowViewModel`) | service → EF Core |
| C — Save/Delete | `POST /{Users,Roles}/Save`, `Units/SaveUnit|SaveGroup`, `SaqTemplates/Save|SaveQuestion`, `KriTemplates/Save|SaveKri|SaveColor|SaveGroup|SaveBands`, `*/Delete` | JSON | `[FromBody]` (`*SaveDto` / `DeleteRequestDto`) | `{success,message,data:{id}}` | service → `SaveChanges` |
| D — Login | `POST /Login/Login` | JSON | `[FromBody]` (`LoginRequestDto`) | `{success, data:{redirectUrl}}` | orchestrator → cookie |
| E — Wizard | `POST /Assessment/SaveName|SaveSaq|SaveKri|SaveSchedule|Finalize` | JSON | `[FromBody]` (`AssessmentNameSaveDto` / `AssessmentTemplateSaveDto` / `ScheduleSaveDto` / `AssessmentFinalizeDto`) | `{success,message,data}` | service → `SaveChanges` |

All AJAX responses use the shared **`ApiResponse<T>`** envelope (`Models/Dto/ApiResponse.cs`): `Success`, `Message`, `Data`, `Errors`. HTTP status stays `200`; outcome signalled by `success`. Grid keeps `data` as the array so DataTables `dataSrc:'data'` is unchanged; `id` / `redirectUrl` moved into `data`.

---

## 6. Layer Responsibility Map ("use grid/use JSON/use model where")

| Concern | Layer / File | Notes |
|---|---|---|
| **Grid** (data table UI) | DataTables in `Views/{Users,Roles,Units,SaqTemplates,KriTemplates,Assessment}/Index.cshtml` | client-side processing, AJAX `dataSrc:'data'` |
| **Grid data source** | `*Controller.Grid` → `*GridRowViewModel` (in `Models/ViewModel`) via `ApiResponse<T>` | camelCase (web JSON defaults) matches DataTables `columns`; not server-side processing |
| **JSON serialization (server→JS init)** | Razor `@Html.Raw(Json.Serialize(...))` | Users/Roles/Units/SAQ/KRI Index pages (statuses, units, groups, colors) |
| **JSON produce/consume** | Controllers `Json(...)` + jQuery `RiskPulse.postJson/getJson` | all AJAX in views' `@section Scripts` via the shared module |
| **Model — EF entities** | `Data/Entries/*` | mapped 1:1 to `riskpulse.*` tables |
| **Model — view models** | `Models/ViewModel/*` | data shaped for a UI/view — `*IndexViewModel`, `*GridRowViewModel`, `*OptionViewModel` |
| **Model — DTOs** | `Models/Dto/*` | data moving between layers/systems — `ApiResponse<T>`, `LoginRequestDto`, `LoginResultDto`, `UserAuthorizationDto`, `*SaveDto`, `*DeleteRequestDto`, `SaveResultDto` |
| **Model — enums** | `Models/Enum/*` | `UnitType`, `QuestionType`, `SaqStatus`, `KriStatus`, `AssessmentStatus`; persisted as `varchar(32)` |
| **Business logic** | `Services/*Service` | no repository layer; each service uses `AppDbContext` directly; business rules throw `InvalidOperationException` (duplicate, self-edit, locked-template, group-vs-unit XOR, active-requires-SAQ+KRI, draft-only edits) |
| **Controller boilerplate** | `Controllers/ControllerHelpers.cs` | `ValidateModel`, `TryExecute`, `TrySave`, `TryDelete` — null-guard, ModelState, ApiResponse envelope, exception→message passthrough |
| **Data access** | `Data/AppDbContext` via services | `Include`/`AsNoTracking`/`ExecuteDeleteAsync`/`SaveChanges` in services; `DbSetExtensions.EnsureUniqueAsync`/`ToOptionListAsync` |
| **DB schema/DDL** | No `Migrations/` folder — migration `20260816161300_UserPermissionControl` was removed from the repo (commit `f8cacd2`) | `HasDefaultSchema("riskpulse")`; live DB provisioned manually via `Database/Seed.sql` |
| **Seed data** | `Database/Seed.sql` (manual) | lines 1–35 live (9 permissions, 2 roles, 1 unit, 1 test user); NOT an EF `HasData` seed — see mismatch #6 |
| **Client validation** | `validateUserPayload`/`validateRolePayload`/`validateLoginForm` + `RiskPulse.validationError` in views | hand-rolled, not DataAnnotations-driven; server DataAnnotations are the source of truth |
| **Auth policies** | `PermissionCatalog` (single source) → `Program.cs` + `[Authorize]` + sidebar `HasClaim` + `PermissionPageMapper` | constant values must match `LoginOrchestratorService` claims + DB `Permissions` rows |
| **Auth cookie claims** | `LoginOrchestratorService` | `Name`, `NameIdentifier`, `Role`, `DefaultPage`, `Unit`, `Permission*` |
| **Status options** | `SaqStatusOptionViewModel.GetAll()` / `KriStatusOptionViewModel.GetAll()` | exclude `Locked` (system-set only); views tag `<option>`s via `statusKind` for pill chips in Select2 |

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
SaqHeaders          (SaqHeaderId PK, SaqDesc, GroupId FK→Groups (nullable), UnitId FK→Units (nullable), SaqStatus varchar(32))
SaqQuestions        (QuestionId PK, SaqHeaderId FK→SaqHeaders, QuestionText, QuestionType varchar(32), AllowComment, DisplayOrder)
SaqQuestionOptions  (OptionId PK, QuestionId FK→SaqQuestions, OptionText, DisplayOrder)
KriHeaders          (KriHeaderId PK, KriHeaderDesc, GroupId FK→Groups (nullable), UnitId FK→Units (nullable), KriStatus varchar(32))
Kri                 (KriId PK, KriHeaderId FK→KriHeaders, KriDesc, AllowComment, KriThresholdGroupId FK→KriThresholdGroups)
KriThresholdGroups  (KriThresholdGroupId PK, KriThresholdGroupDesc)
KriThresholdColors  (ColorId PK, ColorDesc, HexCode)
KriThresholds       (KriThresholdId PK, KriThresholdGroupId FK→KriThresholdGroups, ColorId FK→KriThresholdColors, MinValue, MaxValue)
AssessmentHeaders   (AssessmentHeaderId PK, AssessmentName, AssessmentStatus varchar(32) — Draft/Active, SaqHeaderId FK→SaqHeaders, KriHeaderId FK→KriHeaders, RiskRegisterHeaderId nullable — Risk Register not built yet)
ScheduleHeaders     (ScheduleHeaderId PK, AssessmentHeaderId FK→AssessmentHeaders, ScheduleDesc, StartDate, EndDate)   [1:N — an assessment can be re-scheduled]
```

- **Enum→string conversion:** `Unit.UnitType`, `SaqStatus`, `QuestionType`, `KriStatus`, `AssessmentStatus` stored as `character varying(32)` (`AppDbContext.cs:11-153`).
- **Cascade/restrict rules** (`AppDbContext.cs:25-152`): `SaqHeader→SaqQuestions→SaqQuestionOptions` cascade; `KriHeader→Kri` cascade with `Kri→KriThresholdGroup` restrict; `KriThresholdGroup→KriThresholds` cascade with `KriThreshold→KriThresholdColor` restrict; `AssessmentHeader→ScheduleHeaders` cascade with `AssessmentHeader→Saq/KriHeaders` restrict; `UnitGroup→Group/Unit` cascade with a unique `(GroupId, UnitId)` index; `SaqHeader/KriHeader→Group` and `SaqHeader/KriHeader→Unit` restrict (a group or unit in use by a template can't be deleted; exactly one of GroupId/UnitId must be set — enforced as a business rule in the service). AccessControl FKs (Users→Roles/Units, RolePermissions→Roles/Permissions) cascade by convention.
- **Implemented via:** migration `20260816161300_UserPermissionControl` applied to the live DB but **removed from the repo** (commit `f8cacd2`, "fix/remove-migrations"). DB is otherwise provisioned manually via `Database/Seed.sql`.

### 7.2 `Database/Seed.sql` — legacy content & risk

Seed.sql inserts the 9 permissions, 2 roles, 1 unit, and 1 test user (lines 1–35, live). **However** line 39+ contain an entire **legacy SQL Server schema** (`dbo.tblAssessmentModuleType`, `tblAssessmentHeader`, `tblSAQ*`, `tblKRI*`, `tblRiskRegister*` — `IDENTITY(1,1)`, `NVARCHAR`, `GETDATE()`; line 40 is bare DDL, line 60 onwards is block-commented) inside a "Do not run this manually" comment. This is not PostgreSQL-compatible and is a copy of a different-era design. It should be extracted to a separate reference document (see mismatch #6).

---

## 8. Pattern Mismatches & Inconsistencies

> Current **open** deviations only, with evidence and fix. Already-resolved items (save-model binding + server-side DataAnnotations, `ApiResponse<T>` envelope, login JSON standardization, `PermissionCatalog` single source, named grid view models, the DTO/ViewModel/Enum layer split, self-edit rule moved into `UsersService`, the shared JS module, Units page, Assessment wizard, and status-dropdown/grid-ordering refinements) are reflected in §4–§6 and §10 and are not repeated here.

### 8.1 Architecture & layering

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 1 | **No repository / unit-of-work; no interface abstractions** — every service talks to `AppDbContext` directly and exposes concrete methods returning entities. The data layer is untestable and services can't be swapped or faked. | `Services/Administration/UsersService.cs:10-17`, `Services/Administration/RolesService.cs:11-18`, `Services/Administration/UnitsService.cs:10-17`, `Services/Login/DbAuthorizationService.cs:8-15` | Introduce `IUsersService`, `IRolesService`, `ILoginOrchestratorService`, `IAdAuthenticationService`, `IDbAuthorizationService` (optionally `IRepository<T>`/`IUnitOfWork`) and have services return DTOs, not entities. |
| 2 | **Concrete-class-only DI** — every service is registered as `AddScoped<Concrete>()`, so nothing can be mocked or swapped. | `Program.cs:23-31` | Register `AddScoped<IXxx, Xxx>()` against the interfaces from #1. |
| 3 | **DataTables runs client-side processing** — the full row set ships to the browser in one response; paging/search/sort run in JS, not SQL. Latent scale problem once Submissions hold real data. | `Views/Users/Index.cshtml` (grid config), `Views/Roles/Index.cshtml`, `Views/Units/Index.cshtml`, `Views/Assessment/Index.cshtml:57-77` | For large tables use `serverSide:true` and handle `start`/`length`/`search` at the Grid endpoints. |
| 4 | **Global/inline JS, no modules, no bundling** — page logic lives in `@section Scripts` with hand-rolled `$.ajax` calls; only vendored libs are static. | `wwwroot/js/modules/riskpulse.js` | ~Resolved — shared helpers extracted into the module; page scripts now call `RiskPulse.*` and hold only validation, column configs, and wiring (dedup was the fix; no bundling, no build step). |
| 5 | **`AdAuthenticationService` is a stub that always returns `true`** — any username/password is "valid" as long as the user exists in DB. | `Services/Login/AdAuthenticationService.cs:4-9` | Implement a real directory/identity-provider lookup (or explicitly dev-gate the stub). |

### 8.2 Data & persistence

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 6 | **Seed data is manual SQL that also embeds a legacy SQL Server schema** — `Database/Seed.sql` mixes live PostgreSQL inserts (permissions, roles, unit, test user) with a non-PostgreSQL `dbo.tbl*` design (`IDENTITY(1,1)`, `NVARCHAR`, `GETDATE()`). | `Database/Seed.sql` (lines 1–35 live; line 39+ legacy `dbo.tbl*`; line 40 is bare DDL, rest block-commented) | Move seed into EF Core (`modelBuilder.HasData` / seed extension) and extract the legacy schema to a reference doc, out of executable SQL. |
| 7 | **No migrate/seed bootstrap at startup** — the app assumes the DB was provisioned externally; a fresh DB will fail at first query. Migrations were removed from the repo, so there is no `dotnet ef database update` path either. | `Program.cs:12-14` (no `Db.Database.Migrate()`), no `Migrations/` folder | Add dev-only `Migrate()` (+ data seed) bootstrap and regenerate a baseline migration, or document the manual Seed.sql step in a README. |
| 8 | **No logging (`ILogger`) anywhere** — service/DB exceptions bubble with no trace and the catch blocks can't be audited. | all `Services/*` and `Controllers/*` | Inject `ILogger<T>` and log at service and catch boundaries. |

### 8.3 Security

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 9 | **No CSRF protection on cookie-auth state-changing endpoints** — `Users/Save`, `Roles/Save`, `Units` save/delete, `SaqTemplates`/`KriTemplates` save/delete, the wizard `SaveName|SaveSaq|SaveKri|SaveSchedule|Finalize`, and `Login/Login` are JSON POSTs authenticated by cookie, but the app has no antiforgery tokens (`[ValidateAntiForgeryToken]` / `@Html.AntiForgeryToken()` are absent everywhere). | `Controllers/UsersController.cs:46-63`, `Controllers/RolesController.cs:40-52`, `Controllers/UnitsController.cs:40-74`, `Controllers/LoginController.cs:34-51`, `Controllers/AssessmentController.cs:54-142` | Emit antiforgery tokens in the views and add `[ValidateAntiForgeryToken]` on the POST actions; for `[FromBody]` JSON use `AddAntiforgery` + a header token. |
| 10 | **Database credentials committed to source** — the PostgreSQL connection string (`Server`, `Port`, user, `Password=123456`) is hard-coded in `appsettings.json` and tracked by git. | `appsettings.json:9` | Move credentials to user-secrets / environment variables; keep no secret (or a harmless dev value) in the repo. |

### 8.4 Code hygiene & minor

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| _(none open)_ | Hygiene items resolved — dead zero-FK fallbacks in `UsersService`, unused `UsersIndexViewModel.Users` / `RolesIndexViewModel.Roles`, dead `KriBandSaveDto.KriThresholdId`, unreferenced `_ValidationScriptsPartial`, empty `Infrastructure/` + `Validation/`, dead `Models\Auth\**` csproj exclusion, undefined `status-active`/`login-icon-circle` classes, and the tri-spelled brand name were all removed in the hygiene pass. | (removed) | (done) |
| _(resolved)_ | `Specs/layout/DESIGN.md` lacked the teal accent family that `Specs/login/DESIGN.md` and `site.css` already had, and had no "Layout shell polish" section documenting the sidebar gradient, sonar brandmark, teal active rail, frosted topbar, or canvas gradient. | `Specs/layout/DESIGN.md` YAML block ended at `surface-variant` (line 50); no shell polish section existed | Updated `Specs/layout/DESIGN.md` to include the full accent family YAML block + a "Layout shell polish" section covering sidebar gradient/sonar, teal nav states, frosted topbar, canvas gradient/ambient, cards, typography, and `prefers-reduced-motion` |
| _(resolved)_ | `Locked` was offered as a selectable status in the SAQ/KRI add/edit modals, but it is a system-set state (service rejects edits/deletes of Locked templates). Status dropdowns now exclude it, and the per-view `statusOptions` wrappers that re-derived `kind` via `RiskPulse.statusKind` were removed since the option list is already known. Template grids (SAQ, KRI) and the assessment grid now order **newest-first** (`OrderByDescending` on the header Id) so freshly-created records appear at the top. | `SaqStatusOptionViewModel.GetAll()` / `KriStatusOptionViewModel.GetAll()` now `.Where(s => s != Status.Locked)`; `SaqTemplatesService.GetHeaderRowsAsync` / `KriTemplatesService.GetHeaderRowsAsync` `OrderByDescending`; `Views/{Saq,Kri}Templates/Index.cshtml` dropped the `statusOptions` mapping and switched grid `order` to `desc` | Done — Locked remains visible only as a grid pill (`statusPill` → warning) and via the service guards. |

---

## 9. Recommended / Target Architecture

A minimal, incremental target that fixes every mismatch above **without** a rewrite:

```
Views (.cshtml + DataTables/Select2/SweetAlert)
   │  GET page (ViewModels + Json.Serialize init data)
   ▼  AJAX JSON (grid + save + login + wizard) — all through ApiResponse<T>
Controllers — thin: bind SaveModels, call services, return ApiResponse<T>
   │
Services (interface + concrete, Scoped DI)
   ├── Login:      IAdAuthenticationService  → AdAuthenticationService (real impl)
   │              IDbAuthorizationService   → DB lookup
   │              ILoginOrchestratorService → composes above
   │              PermissionCatalog / PermissionPageMapper (single source, shared by all modules)
   ├── Administration: IUsersService / IRolesService / IUnitsService
   ├── Templates:  ISaqTemplatesService / IKriTemplatesService
   ├── Assessment: IAssessmentService
   └── Data access via IRepository<T> (or keep DbContext here behind service facades)
   │
Data — AppDbContext + EF Migrations (regenerate baseline) + EF seed (HasData)
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
| Units page (Unit CRUD \| Unit Group CRUD, Select2 group→unit assignment) | ✅ Complete — two-tab Administration page; group requires ≥2 units; delete guards for units/groups referenced by users or templates |
| PostgreSQL + EF Core | ✅ Live — schema formerly created by migration `20260816161300_UserPermissionControl` (removed from repo, commit `f8cacd2`); DB provisioned manually via `Database/Seed.sql` |
| DataTables AJAX grids + JSON save flows | ✅ Working — every grid returns a named `*GridRowViewModel` (Users, Roles, Units, Groups, SAQ headers/questions/options, KRI headers/items, KRI colors/groups/bands, Assessment) |
| Bootstrap 5 modals (programmatic open/close) | ✅ Fixed — open/close via `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` in the shared module; modals re-hosted under `<body>`; no jQuery `$.fn.modal` or raw `getOrCreateInstance` in views |
| Design system (CSS) + AI specs | ✅ Complete |
| Project structure conventions | ✅ Complete — controllers flat & 1:1 with Views; services grouped by workflow; Models split `Dto`/`ViewModel`/`Enum`; entities in `Data/Entries`; Views folder=controller, file=action |
| Domain pages (Dashboard, Submissions, Risk Register Templates) | ⬜ Stubs |
| SAQ Templates (grid, header CRUD, question/option designer, Locked immutable rule, Group XOR Unit link) | ✅ Implemented |
| KRI Templates (grid, header CRUD, KRI builder with value/comment/group, Locked immutable rule, Group XOR Unit link) | ✅ Implemented |
| KRI Config (threshold colors, groups, value-band editor) | ✅ Merged into KRI Templates as tabs |
| Assessment wizard (name → SAQ → KRI → schedule → finalize) | ✅ Implemented — per-step AJAX persistence, draft edit/re-save, activate rules, draft-only delete, locked Risk Register step placeholder |
| Repository / unit-of-work / interface services | ❌ Not started |
| DTOs & uniform API envelope | ✅ Complete — `ApiResponse<T>` envelope; `*Dto` inputs (`LoginRequestDto`, `LoginResultDto`, `UserAuthorizationDto`, `*SaveDto`, `*DeleteRequestDto`) + `*ViewModel` outputs (`*IndexViewModel`, `*GridRowViewModel`, `*OptionViewModel`) split by layer |
| Server-side validation (DataAnnotations on save models) | ✅ On every save model (Users/Roles/Login/Units/Groups/SAQ/KRI/Assessment/Schedule) — FK `[Range]`, `[Required]`, `[StringLength]`, `[RegularExpression]` |
| Permission single source (`PermissionCatalog`) | ✅ Resolved — policies, `[Authorize]`, sidebar, `PermissionPageMapper` all reference the catalog |
| Business rule placement | ✅ Service-side — self-edit, duplicate, not-found, locked-template, group-vs-unit XOR, default-permission, draft-only-edit, active-requires-SAQ+KRI rules all throw `InvalidOperationException` from services |
| Code hygiene (dead code, unused scaffolding, brand name) | ✅ Cleaned — see §8.4 |
| Shared JS module (`RiskPulse.*` in `wwwroot/js/modules/riskpulse.js`) | ✅ Dedup done — all shell views + wizard use the helpers; generic-error toast single-sourced (§4.1 #11) |
| Status dropdowns (Locked excluded, RAG pill options) | ✅ Done — `GetAll()` filters `Locked`; `statusKind` tags options; template/assessment grids newest-first |
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
6. ✅ **Fix Bootstrap 5 modal API** — replaced all `$('#x').modal('show'/'hide')` calls with the programmatic API in the shared module (resolved).
7. ✅ **Close pattern deviations** — `ColorsGrid` now returns `KriColorGridRowViewModel`; default-permission rule moved into `RolesService`; `SaveBands` + all delete actions get the null-guard/ModelState block; DataAnnotations aligned; `form-select` dropped from `input-stasis` selects (resolved).
8. ✅ **Hygiene pass** — removed dead `UsersService` fallbacks, dead view-model props, dead `KriBandSaveDto.KriThresholdId`, unreferenced `_ValidationScriptsPartial`, empty `Infrastructure/`/`Validation/` folders, dead csproj exclusion, undefined CSS classes; unified brand on "Risk Pulse" (resolved).
9. ✅ **JS dedup / shared module** — created `wwwroot/js/modules/riskpulse.js` (`RiskPulse.*`: toast helpers, `postJson`/`getJson`, `escapeHtml`, `serializeForm`, `populateSelect`, `initSelect2`, `showModal`/`hideModal`, `confirmDelete`, `initGrid`, `pill`, `statusPill`, `statusKind`, `validationError`, `clearFieldErrors`); loaded from `_Layout`; refactored all shell views to use it; generic-error message single-sourced (resolved).
10. ✅ **Restructure layers** — controllers flattened to `Controllers/` (thin 1:1 with views); `Models/` split into `Models/Dto/`, `Models/ViewModel/`, and `Models/Enum/`; `ApiResponse<T>` kept as the sole un-postfixed type; entities moved to `Data/Entries/`; services grouped by workflow (Login/Administration/Templates); Views verified folder=controller / file=action (resolved).
11. ✅ **Units page** — two-tab Administration page (Unit CRUD | Unit Group CRUD) under the new `Units` permission, with Select2 multi-select group→unit assignment, a ≥2-units group rule, and delete guards for units/groups referenced by users or templates (resolved).
12. ✅ **Assessment wizard** — step flow (name → SAQ → KRI → schedule → finalize) with per-step AJAX persistence (`SaveName`/`SaveSaq`/`SaveKri`/`SaveSchedule`), draft edit/re-save through the stages, `Finalize` with `Draft`/`Active`, activate rules (SAQ + KRI required), draft-only deletion, and a locked Risk Register step placeholder (resolved).
13. ✅ **Status dropdowns + grid ordering refinements** — `Locked` excluded from SAQ/KRI status options (system-set only); per-view `statusOptions` mapping removed; SAQ/KRI/Assessment grids order newest-first (resolved).
14. **Add CSRF protection** — antiforgery tokens + `[ValidateAntiForgeryToken]` on `Save`/`Login`/wizard POSTs (§8 #9).
15. **Move DB credentials out of source** — user-secrets / environment variables (§8 #10).
16. **Extract interfaces + register DI** — `IUsersService`, `IRolesService`, `IUnitsService`, `IAssessmentService`, `IAdAuthenticationService`, `IDbAuthorizationService` (§8 #1–#2).
17. **Implement real AD** or dev-gate explicitly (§8 #5).
18. **EF seed + clean `Seed.sql`** of legacy SQL Server DDL; regenerate a baseline migration so `dotnet ef database update` works (§8 #6–#7).
19. **Add logging** at service boundaries (§8 #8).
20. Then build domain: keep this file current for Dashboard/Submissions with the standardized flow (server-side grid for submissions volume, §8 #3).