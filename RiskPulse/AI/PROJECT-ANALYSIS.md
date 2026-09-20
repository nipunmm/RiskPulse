# RiskPulse — Project Architecture & Data-Flow Analysis

> Generated from codebase analysis. Last reviewed: September 2026.

---

## 1. Overview

**RiskPulse** is an enterprise risk management application built with **ASP.NET Core MVC**. The domain targets KRI (Key Risk Indicators), SAQ (Self-Assessment Questionnaires), schedule/template orchestration, branch-level risk submissions, and RAG (Red/Amber/Green) status tracking for high-stakes financial environments.

**Current Phase:** Access-control scaffolding complete (cookie auth + claims, user/role/permission management with DataTables grids). SAQ and KRI template designers implemented — headers with unique generated codes, question/item designers, Locked-immutability rules; each template header is **linked to a required unit group or unit** (`GroupId FK→Groups` XOR `UnitId FK→Units`, shown in the grids via an assignment label). **Schedule wizard implemented** — step flow (schedule type → SAQ templates → KRI templates → finalize) with per-step AJAX persistence, multi-template selection with a card/checkbox picker (search + pager + selection chips) and read-only SAQ/KRI previews. **Workflow engine implemented** — DB-driven `Workflow`/`WorkflowStep` dictionaries; activating a schedule auto-creates an assessment (header → one `AssessmentUnit` per targeted unit → one `AssessmentItem` per template) with statuses driven by workflow step codes rather than enums. **Submissions implemented** — own-unit grid, per-item SAQ/KRI entry (draft save, submit, approve), unit authorize, all gated on workflow step codes. **Dashboard implemented** — fully server-rendered landing page (hero KPIs, status distribution from workflow steps, KRI RAG snapshot over the latest period, needs-attention list, period history), own-unit scoped. Remaining domain page (Risk Register templates) is a **stub**. PostgreSQL persistence is live via EF Core; there is **no `Migrations/` folder on disk and no committed seed artifact** (`Migrations/` and `Database/` are git-ignored, so seed rows exist only in the dev DB). Structure is convention-aligned: controllers are **flat** in `Controllers/` (thin, 1:1 with `Views/{Controller}/`), services are **grouped by workflow** (`Login`/`Administration`/`Templates`/`Schedule`/`Assessment`/`Dashboard`/`Utilities`), and `Models/` is split by layer into `Models/Dto/` (inter-system data), `Models/ViewModel/` (UI-shaped data), and `Models/Enum/` (domain enums, persisted as strings).

**Notable recent refactors (since the previous review):** the Assessment wizard was replaced by the **Schedule wizard** (`ScheduleController`/`ScheduleService`/`Views/Schedule/`, permission `Schedule`), assessment creation is now **triggered automatically** when a schedule is activated rather than built step-by-step, the KRI threshold-config subsystem (colors/groups/bands) was **removed** (KRI items now carry flat `GreenLimit`/`AmberLimit`/`RedLimit` ints), a `CodeGeneratorService` issues unique `SAQ-`/`KRI-`/`SCH-`/`ASM-` codes, and the workflow-status model went **DB-driven** via `Workflow`/`WorkflowStep`.

---

## 2. Technology Stack

| Layer | Technology | Version / Details |
|---|---|---|
| Runtime | .NET | `net10.0` (target framework, `Microsoft.NET.Sdk.Web`) |
| Language | C# | Implicit usings, nullable enabled |
| Framework | ASP.NET Core MVC | Minimal hosting model (`Program.cs`) |
| ORM | Entity Framework Core | `Microsoft.EntityFrameworkCore` **10.0.10** |
| DB Provider | Npgsql (PostgreSQL) | `Npgsql.EntityFrameworkCore.PostgreSQL` **10.0.3** |
| Database | PostgreSQL | Schema `riskpulse`, DB `sit` (localhost:5432); `Search Path=riskpulse` |
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
| Scaffolding | EF Core Migrations | **No `Migrations/` folder exists on disk** (git-ignored via `.gitignore` `**/Migrations/`); schema is provisioned in the dev DB; seed rows live only in the dev DB |

**NuGet packages:** `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Tools`, `Npgsql.EntityFrameworkCore.PostgreSQL`. Nothing else. No tests, no CI (`.github/workflows` is an empty placeholder).

---

## 3. Current Project Structure

```
RiskPulse.slnx                                   # XML solution (single project)
RiskPulse/
├── Program.cs                                   # Bootstrap: DbContext, DI, auth policies, pipeline
├── RiskPulse.csproj                             # net10.0, 3 EF/Npgsql package refs
├── appsettings.json                             # ConnString: Server=localhost;Port=5432;DB=sit;schema=riskpulse (COMMITTED — debt §8.3 #9)
├── appsettings.Development.json                 # Logging overrides
├── Properties/launchSettings.json
│
├── Controllers/                  # Flat — thin, 1:1 with Views/{ControllerName}/; routes follow class names
│   ├── LoginController.cs        # GET/POST Index, POST Login (JSON [FromBody]), Logout, AccessDenied (AllowAnonymous)
│   ├── UsersController.cs        # Index (View), Grid (JSON incl. role desc), Save (JSON [FromBody]), Delete (JSON)
│   ├── RolesController.cs        # Index (View), Grid (JSON incl. permissionIds/descs), Save (JSON [FromBody]), Delete (JSON)
│   ├── UnitsController.cs        # 2-tab Units page: UnitGrid/SaveUnit/DeleteUnit + GroupGrid/SaveGroup/DeleteGroup (JSON)
│   ├── SaqTemplatesController.cs # Grid/Save/Delete headers + QuestionsGrid/SaveQuestion/DeleteQuestion (JSON)
│   ├── KriTemplatesController.cs # Grid/Save/Delete headers + KrisGrid/SaveKri/DeleteKri (JSON); KRI limits on the item
│   ├── ScheduleController.cs     # Schedule wizard: Index/Grid, Wizard (4 steps), SaveSchedule/SaveSaq/SaveKri, SaqPreview/KriPreview, Finalize, Delete (JSON)
│   ├── SubmissionsController.cs  # Index + Grid (JSON) + Detail/SaqEntry/KriEntry (views) + SaveSaq/SaveKri/SubmitItem/ApproveItem/AuthorizeUnit (JSON), own-unit scoped
│   ├── DashboardController.cs    # GET Index → server-rendered DashboardViewModel, [Authorize(Policy="Permission:Dashboard")]
│   ├── RiskRegisterTemplatesController.cs # Stub, [Authorize(Policy="Permission:Risk Register")]
│   ├── ErrorController.cs        # GET /Error/Index (no auth)
│   └── ControllerHelpers.cs      # Static helpers: ValidateModel, TryExecute, TrySave, TryDelete (null-guard + ModelState + ApiResponse)
│
├── Data/
│   ├── AppDbContext.cs               # DbContext: schema, 21 DbSets, enum→string conversions, FK/cascade config
│   ├── Entries/                      # EF entities — flat 1:1 mirror of riskpulse.* tables (no domain grouping)
│   │   ├── User.cs  Role.cs  Permission.cs  RolePermission.cs  Unit.cs
│   │   ├── Group.cs  UnitGroup.cs      # Unit grouping (Group 1—N UnitGroup N—1 Unit, unique GroupId+UnitId)
│   │   ├── SaqHeader.cs  SaqQuestion.cs  SaqQuestionOption.cs
│   │   ├── KriHeader.cs  Kri.cs        # Kri carries flat GreenLimit/AmberLimit/RedLimit (threshold subsystem removed)
│   │   ├── Schedule.cs  ScheduleItem.cs        # schedule header + polymorphic (ItemType+ItemId) template links
│   │   ├── AssessmentHeader.cs  AssessmentUnit.cs  AssessmentItem.cs   # auto-created on schedule activation
│   │   ├── SaqAssessmentAnswer.cs  KriAssessmentValue.cs             # submission answers/values
│   │   └── Workflow.cs  WorkflowStep.cs        # DB-driven workflow/step dictionary (statuses = step codes)
│   └── Extensions/
│       └── DbSetExtensions.cs        # EnsureUniqueAsync (duplicate → InvalidOperationException), ToOptionListAsync (→ OptionViewModel)
│
├── Models/
│   ├── Dto/                        # Data that moves between layers/systems (`*Dto` postfix; ApiResponse excepted)
│   │   ├── ApiResponse.cs          # Shared JSON envelope { success, message, data, errors } — only DTO with file-scoped namespace
│   │   ├── LoginResultDto.cs  LoginRequestDto.cs  UserAuthorizationDto.cs
│   │   ├── UserSaveDto.cs  RoleSaveDto.cs  UnitSaveDto.cs  GroupSaveDto.cs  DeleteRequestDto.cs  SaveResultDto.cs
│   │   ├── Saq*.cs                 # SaqHeaderSaveDto, SaqQuestionSaveDto, SaqOptionSaveDto
│   │   ├── Kri*.cs                 # KriHeaderSaveDto, KriSaveDto
│   │   ├── Schedule*.cs            # ScheduleSaveDto, ScheduleTemplatesSaveDto, ScheduleFinalizeDto
│   │   └── Submission*.cs          # SaveSaqAnswersDto (+SaqAnswerSaveDto), SaveKriValuesDto (+KriValueSaveDto), ItemTransitionDto, UnitAuthorizeDto
│   ├── ViewModel/                  # Data shaped specifically for a UI/view (38 files, `*ViewModel` postfix)
│   │   ├── UsersIndexViewModel.cs  RolesIndexViewModel.cs  UnitsIndexViewModel.cs  ErrorViewModel.cs
│   │   ├── UserGridRowViewModel.cs  RoleGridRowViewModel.cs  UnitGridRowViewModel.cs  GroupGridRowViewModel.cs  OptionViewModel.cs (Value/Label/Code)
│   │   ├── Saq*.cs                 # SaqTemplatesIndexViewModel, SaqGridRowViewModel, SaqQuestionGridRowViewModel, SaqOptionGridRowViewModel, SaqStatusOptionViewModel, SaqPreviewViewModel
│   │   ├── Kri*.cs                 # KriTemplatesIndexViewModel, KriGridRowViewModel, KriItemGridRowViewModel, KriStatusOptionViewModel, KriPreviewViewModel
│   │   ├── Schedule*.cs            # ScheduleGridRowViewModel, ScheduleWizardViewModel
│   │   ├── Submission*.cs          # SubmissionGridRowViewModel, AssessmentDetailViewModel, AssessmentItemRowViewModel (StepCode-driven), SaqEntryViewModel, SaqEntryQuestionViewModel, KriEntryViewModel, KriEntryValueViewModel (G/A/R limits)
│   │   └── Dashboard*.cs           # DashboardViewModel, DashboardKpiViewModel, DashboardStatusSliceViewModel, DashboardAttentionRowViewModel, AssessmentProgressRowViewModel, PeriodHistoryRowViewModel, KriSnapshotViewModel
│   └── Enum/                       # Domain enums, persisted as varchar(32)
│       ├── UnitType.cs  QuestionType.cs  SaqStatus.cs  KriStatus.cs
│       └── ScheduleType.cs  ScheduleStatus.cs  ScheduleItemType.cs  AssessmentStatus.cs
│
├── Services/
│   ├── Login/                        # Authentication + authorization
│   │   ├── AdAuthenticationService.cs     # STUB — ValidateCredentialsAsync always returns true
│   │   ├── DbAuthorizationService.cs       # Loads user+role+permissions+unit into UserAuthorizationDto
│   │   ├── LoginOrchestratorService.cs     # AD check → DB lookup → claims principal + redirect route
│   │   ├── PermissionCatalog.cs            # Single source for permission constants (policies, layout, mapper) — includes Schedule
│   │   └── PermissionPageMapper.cs         # Static: PermissionDesc → (Controller, Action); default = Dashboard
│   ├── Administration/               # Users + roles + units CRUD
│   │   ├── UsersService.cs                 # CRUD for users + self-edit guard (direct AppDbContext), username unique
│   │   ├── RolesService.cs                 # CRUD for roles + permission mapping + default-permission rule (direct AppDbContext)
│   │   └── UnitsService.cs                 # Unit CRUD (duplicate guard, block delete when referenced) + group CRUD (≥2 units, clear/re-add UnitGroups)
│   ├── Templates/                    # SAQ + KRI templates
│   │   ├── SaqTemplatesService.cs           # SAQ header CRUD (Group XOR Unit rule, lock rules, unique SaqCode) + question/option designer (dup question guard)
│   │   └── KriTemplatesService.cs           # KRI header CRUD + item designer (limits sort rule, dup KRI guard, lock rules, unique KriCode)
│   ├── Schedule/                     # Schedule wizard engine
│   │   └── ScheduleService.cs               # Draft create/rename (SCH code), multi-template SAQ/KRI pick (Active only), finalize (Active requires SAQ+KRI; triggers assessment creation), draft-only delete
│   ├── Assessment/                   # Assessment auto-creation + submissions workflow
│   │   ├── AssessmentService.cs             # CreateAssessmentAsync(scheduleId): header→units→items from workflow IsInitial steps
│   │   └── SubmissionsService.cs            # Own-unit grid + detail; SAQ/KRI entry (draft save); submit/approve/authorize step transitions; own-unit scoping
│   ├── Dashboard/                   # Server-rendered landing page
│   │   └── DashboardService.cs             # Own-unit KPIs, status slices (from WorkflowStep rows), KRI RAG snapshot, needs-attention list, period history
│   └── Utilities/
│       └── CodeGeneratorService.cs         # Unique codes: SAQ-/KRI-/SCH-/ASM-{yyyyMMdd}-{0001..} with 5 retry attempts
│
├── Views/
│   ├── _ViewImports.cshtml  _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml          # Sidebar (permission-gated links incl. Templates + Administration submenus) + frosted topbar + shell polish
│   ├── Login/Index.cshtml             # Standalone page (Layout=null), AJAX login
│   ├── Login/AccessDenied.cshtml      # Standalone (Layout=null)
│   ├── Users/Index.cshtml             # DataTables grid + Add/Edit modals (Select2 + SweetAlert)
│   ├── Roles/Index.cshtml             # DataTables grid + Add/Edit modals (permission checkboxes)
│   ├── Units/Index.cshtml             # Tabs (Units | Unit Groups) + grids + modals (Select2 multi-select for groups)
│   ├── Error/Index.cshtml             # Standalone (Layout=null), RequestId
│   ├── SaqTemplates/Index.cshtml    # DataTables grid + modals + designer modal (question cards + option editor)
│   ├── KriTemplates/Index.cshtml    # DataTables grid + modals + designer modal (KRI items with Green/Amber/Red limits)
│   ├── Schedule/                     # Wizard flow: Index (DataTables grid) + Wizard (stepper + 4 step partials `_StepScheduleType|Saq|Kri|Finalize.cshtml`; template card picker + preview modal)
│   ├── Submissions/               # Index (DataTables grid) + Detail (item table) + SaqEntry + KriEntry (draft save/submit, RAG dots)
│   ├── Dashboard/Index.cshtml     # Server-rendered landing page (no DataTables/JS)
│   └── RiskRegisterTemplates/Index.cshtml  # Stub
│
├── AI/
│   ├── DESIGN.md                      # Stasis Enterprise design-system spec
│   ├── PROJECT-ANALYSIS.md            # This document
│   └── Specs/{login,layout,kri,error-page,branch}/   # Per-feature DESIGN.md + code.html + screen.png
│
└── wwwroot/
    ├── css/site.css                   # Stasis Enterprise design system
    ├── js/site.js                     # Sidebar/collapse/submenu/flyout logic
    ├── js/modules/riskpulse.js        # Shared RiskPulse.* helpers (see §4.1 #11)
    └── lib/                           # bootstrap, datatables, font-awesome, jquery, select2, sweetalert2
```

---

## 4. Architecture Patterns

### 4.1 Pattern Set Currently Used

| # | Pattern | Where |
|---|---|---|
| 1 | **Classic MVC** (server-side Razor) | All controllers/views |
| 2 | **Service layer** (concrete classes via DI, Scoped) | `Services/` (`Login`/`Administration`/`Templates`/`Schedule`/`Assessment`/`Dashboard`/`Utilities`), 13 `AddScoped` registrations in `Program.cs:26-38` |
| 3 | **EF Core + DbContext directly inside services** (no repository) | `UsersService`, `RolesService`, `UnitsService`, all template/schedule/assessment services |
| 4 | **Cookie auth + claim-based authorization** | `Program.cs:39-60`, `[Authorize(Policy=...)]` |
| 5 | **AJAX JSON endpoints** from controllers (not a Web API) | `Grid`/`Save`/`Login`/wizard/submission actions |
| 6 | **DataTables grid fed by JSON** | `Views/{Users,Roles,Units,SaqTemplates,KriTemplates,Schedule,Submissions}/Index.cshtml` |
| 7 | **ViewModel pattern** for page rendering | `*IndexViewModel`/`*WizardViewModel`/`*EntryViewModel` |
| 8 | **DTO/result model** for service → controller | `*SaveDto`, `*FinalizeDto`, `ItemTransitionDto`, `UnitAuthorizeDto`, `DeleteRequestDto` (`Models/Dto`) |
| 9 | **Orchestrator service** composing lower services | `LoginOrchestratorService`; `ScheduleService` composes `CodeGeneratorService` + `AssessmentService` |
| 10 | **Bootstrap 5 modal API** — programmatic open/close only through `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` in the shared module; the layout re-hosts `.modal` nodes as direct children of `<body>` so the Bootstrap backdrop never paints over them | All interactive view script sections + `_Layout.cshtml:226-228` |
| 11 | **Shared JS module (`RiskPulse.*`)** — `wwwroot/js/modules/riskpulse.js` (loaded from `_Layout`) is the single home for cross-page helpers: `toastSuccess`/`toastError`/`toastGenericError`, `escapeHtml`, `postJson`/`getJson` (auto generic-error toast, `ApiResponse.success` routing, optional `$trigger` flight lock + `handlers.complete`), `serializeForm`, `populateSelect`, `initSelect2` (rp theme, `data-color` swatches + `data-kind` pills, modal `dropdownParent`), `showModal`/`hideModal`, `confirmDelete`, `initGrid`, `pill`, `statusPill`, `statusKind`, `validationError`, `clearFieldErrors`. Views call the namespace and keep only validation/columns/wiring. | Users/Roles/Units/SAQ/KRI/Schedule/Submissions view script sections |
| 12 | **Two-tab page pattern** — a single Index view with Bootstrap tabs, one grid per tab, each tab's CRUD hitting its own JSON endpoints | `Views/Units/Index.cshtml` (Units \| Unit Groups) |
| 13 | **Server-persisted wizard pattern** — a 4-step stepper (`data-step=1..4` + a locked `data-step="rr"` Risk Register placeholder) where each step posts its own AJAX endpoint (`SaveSchedule`/`SaveSaq`/`SaveKri`) before advancing; a JS `state` object tracks `completed`/`frontier`; step 4 posts `Finalize` with `status: 'Draft' \| 'Active'`; non-drafts are read-only (`CanEdit`) | `Views/Schedule/Wizard.cshtml` + `_StepScheduleType|_StepSaq|_StepKri|_StepFinalize.cshtml` + `ScheduleController` |
| 14 | **DB-driven workflow (statuses = step codes)** — statuses are `WorkflowStep.StepCode` rows of `ASSESSMENT-UNIT`/`ASSESSMENT-ITEM` workflows, not enums; transitions (`Submitted`/`Approved`/`Authorized`) are business rules in the service; the enum `AssessmentStatus` is write-once at creation and never read for logic | `Workflow`/`WorkflowStep` entities; `SubmissionsService` (`SubmissionsService.cs:12-16`); `AssessmentService.CreateAssessmentAsync` (`AssessmentService.cs:75-83`) |
| 15 | **Polymorphic item linking** — `ScheduleItem`/`AssessmentItem` reference SAQ or KRI headers via `(ItemType, ItemId)` with **no FK**; ambiguity resolved by `ItemType` (`Saq`/`Kri`) | `AppDbContext.cs:124-140, 232-263` |
| 16 | **Auto-assessment on schedule activation** — `ScheduleService.FinalizeAsync(Active)` calls `AssessmentService.CreateAssessmentAsync`, which expands schedule items to per-unit/per-template assessment rows using the workflows' initial steps | `ScheduleService.cs:259-285`; `AssessmentService.cs:22-73` |
| 17 | **Code-generation for domain keys** — `CodeGeneratorService` issues `{SAQ|KRI|SCH|ASM}-{yyyyMMdd}-{0001..}` codes with up to 5 collision retries; unique indexes back each code | `Services/Utilities/CodeGeneratorService.cs`; `AppDbContext.cs:52-53, 98-99, 164-165` |

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

- **9 permissions** are declared once in code as constants in `Services/Login/PermissionCatalog.cs` (`Dashboard | Submissions | Schedule | Users | Roles | Units | Saq | Kri | RiskRegister`) and referenced by:
  1. `Program.cs:49-60` — `AddPolicy($"Permission:{PermissionCatalog.X}")` … `RequireClaim("Permission", PermissionCatalog.X)`
  2. Controllers — `[Authorize(Policy = $"Permission:{PermissionCatalog.X}")]`
  3. `Views/Shared/_Layout.cshtml` — `User.HasClaim("Permission", PermissionCatalog.X)`
  4. `Services/Login/PermissionPageMapper.cs` — dict keys keyed by `PermissionCatalog.X` (Schedule→Schedule/Index)
- The **data source** remains the DB `riskpulse.Permissions.PermissionDesc` (live DB seed rows; no seed script is committed) — constant values must match those rows exactly. One value contains a space: `"Risk Register"`.
- At login, `DbAuthorizationService` loads User → Role → RolePermissions → Permissions, and `LoginOrchestratorService` writes each `PermissionDesc` as a `Claim("Permission", ...)` plus `Name`, `NameIdentifier` (int user Id), `Role`, `DefaultPage`, `Unit` claims into the auth cookie. `DefaultPage` = the role's default permission desc (fallback `PermissionCatalog.Dashboard`).

---

## 5. Data-Flow Patterns (Frontend → Backend → Database)

There are **six** distinct request/response flows in use today.

### 5.0 Common request/response conventions

- **All AJAX** endpoints return `application/json`.
- **All responses** are `HTTP 200 OK` regardless of outcome — success is signalled by a `success: true/false` flag in the JSON body.
- **Client validation** happens first (SweetAlert toasts + `RiskPulse.validationError`), then a `POST` to the server, then another `Toast.fire()` on the returned message.
- Controllers use `ControllerHelpers.ValidateModel` / `TryExecute` / `TrySave` / `TryDelete` — null-guard, ModelState check, `InvalidOperationException` message passthrough, generic-error fallback, all wrapped in the `ApiResponse<T>` envelope.

### 5.1 Flow A — Server-rendered page load (MVC + ViewModel)

Used by the shell "Index" pages (Users, Roles, Units, SAQ Templates, KRI Templates, Schedule), the Schedule Index/Wizard pages, Submissions (Index/Detail/SaqEntry/KriEntry), the Dashboard, and the login page.

```
Browser ──GET /Schedule/Wizard?id=N──────────────► ScheduleController.Wizard
         ◄──HTML (full page)────────────────────── ScheduleController
                                                      │ ScheduleService.GetWizardAsync(N)  │
                                                      │ SaqTemplatesService / KriTemplatesService
                                                      ▼                                  ▼
                                                    ScheduleWizardViewModel ──► PostgreSQL (riskpulse)
```

- **View model used:** `ScheduleWizardViewModel` (also `*IndexViewModel` for the shell pages, `SaqEntryViewModel`/`KriEntryViewModel` for the entry pages, `DashboardViewModel` for the landing page).
- **Rendering:** Razor view + `@Html.Raw(Json.Serialize(...))` to embed *initial dropdown data* (unit/role options, template options, selected ids, completion flags) directly into an inline `<script>` block — this is **not** an AJAX call; it is server-side JSON serialization injected into the page at render time.
- **Status options:** `SaqStatusOptionViewModel.GetAll()` / `KriStatusOptionViewModel.GetAll()` return `Value`/`Label` for every enum member **except `Locked`** — Locked is a system-set state (enforced by the service) and is never offered as a selectable status in the add/edit modals.

### 5.2 Flow B — AJAX JSON Grid (DataTables)

Used by all shell grids — `Users/Grid`, `Roles/Grid`, `Units/UnitGrid|GroupGrid`, `SaqTemplates/Grid|QuestionsGrid`, `KriTemplates/Grid|KrisGrid`, `Schedule/Grid`, `Submissions/Grid`.

```
Browser (DataTables.ajax) ──GET /Schedule/Grid─────► ScheduleController.Grid
        dataSrc:'data'                             │ ScheduleService.GetAllAsync()
        ◄── { success:true, message:null,          │   .Select(s => new ScheduleGridRowViewModel { ... })
              data:[ {scheduleId,scheduleCode,     │
                       scheduleType, scheduleStatus,│
                       ...}, ...] }                  │
                                                      └──► EF Core → PostgreSQL
```

- **Grid library:** DataTables (client-side processing) — the whole row set is serialized and shipped to the browser in one response; paging/searching/sorting happen in the browser, not in SQL.
- **Payload shape:** named grid view models — `*GridRowViewModel` in `Models/ViewModel`, projected in the services; camelCase JSON via the MVC web serializer defaults matches the DataTables `columns` config.
- **Role grid JSON** additionally includes nested arrays `permissionIds` / `permissionDescs` for the edit modal.
- **Ordering:** user/role/unit/group grids sort ascending by Id; SAQ template, KRI template, schedule, and assessment grids sort **descending** (`OrderByDescending`) so the newest records appear first.

### 5.3 Flow C — AJAX JSON Form Submit (Create / Update)

Used by `Users/Save`, `Roles/Save`, `Units/SaveUnit|SaveGroup`, `SaqTemplates/Save|SaveQuestion`, `KriTemplates/Save|SaveKri`, `Schedule/SaveSchedule`, `*/Delete`.

```
Browser (jQuery serializeArray) ──POST /Schedule/SaveSchedule──► ScheduleController.SaveSchedule
  { contentType:'application/json',                            │ null ?? ModelState check
    data: JSON.stringify(payload) }                            │ business rule (one-time vs recurring)
    ◄── { success:true, message:"Schedule draft created.",     ▼
          data:{ id:12, code:"SCH-20260920-0001" } }            ScheduleService.CreateOrUpdateScheduleAsync
                                                                ├─ CodeGeneratorService (new)
                                                                ├─ RequireDraftAsync (edit)
                                                                └─ DbContext.SaveChangesAsync()  → PostgreSQL
```

- **Model binding:** `[FromBody]` deserializes the JSON body into dedicated `*SaveDto` types (`UserSaveDto`, `RoleSaveDto`, `UnitSaveDto`, `GroupSaveDto`, `SaqHeaderSaveDto`, `SaqQuestionSaveDto`, `KriHeaderSaveDto`, `KriSaveDto`, `ScheduleSaveDto`).
- **Return:** `ApiResponse<T>` envelope via `ControllerHelpers.TrySave` / `TryDelete` — success `{ success, message, data:{ id, code } }`, failure `{ success:false, message }`.

### 5.4 Flow D — AJAX Login (JSON)

```
Browser (loginForm) ──POST /Login/Login (JSON body)──► LoginController.Login
                        contentType:'application/json' │ null ?? ModelState check (LoginRequestDto)
                        ◄── { success:true,            │ LoginOrchestratorService.AuthenticateAsync
                               data:{ redirectUrl } }  │  1 AdAuthenticationService  (STUB: always true)
                                                         │  2 DbAuthorizationService    (user/role/perm graph)
                                                         │  3 build ClaimsPrincipal
                                                         ▲  PermissionPageMapper       (desc → (controller, action))
Post-success:                         HttpContext.SignInAsync(principal) → auth cookie
  window.location.href = response.data.redirectUrl ─┘
```

- **Payload style:** `application/json` via `JSON.stringify`, bound to `LoginRequestDto` with `[FromBody]` — consistent with Flows B/C.
- **Outcome:** On success the server sets the cookie via `SignInAsync` and returns `{ success, data:{ redirectUrl } }`; the browser navigates to `response.data.redirectUrl`.

### 5.5 Flow E — Schedule wizard step persistence + assessment auto-creation

Each wizard step persists independently via its own AJAX endpoint, then the stepper advances (JS `state.completed`/`state.frontier`).

```
Browser (wizard JS) ──POST /Schedule/SaveSchedule  { scheduleId:0, scheduleType, startDate, endDate | startMonth, recurringDay } ──► SaveSchedule → CreateOrUpdateScheduleAsync (SCH code)
                    ──POST /Schedule/SaveSaq       { scheduleId, templateHeaderIds:[...] } ──► SaveSaq → SetSaqTemplatesAsync (Active-only)
                    ──POST /Schedule/SaveKri       { scheduleId, templateHeaderIds:[...] } ──► SaveKri → SetKriTemplatesAsync (Active-only)
                    ──POST /Schedule/Finalize      { scheduleId, status:'Draft'|'Active' }  ──► Finalize → FinalizeAsync
                                                                                                  └─ status Active && ≥1 SAQ && ≥1 KRI
                                                                                                     → AssessmentService.CreateAssessmentAsync(scheduleId)
                                                                                                        header(Pending, ASM code) → AssessmentUnit per unit
                                                                                                        → AssessmentItem per template (workflow IsInitial steps)
```

- **Draft rules (service-side):** schedule-type edit and template pick all run through `RequireDraftAsync` (edit = Draft only); `FinalizeAsync(Active)` requires ≥1 SAQ and ≥1 KRI template; `DeleteAsync` allows drafts only. Non-draft wizard pages render with `CanEdit = false` and the JS blocks navigation.
- **timestamptz gotcha:** `Schedule.StartDate/EndDate/StartMonth` are `timestamptz`; the JSON binder yields `DateTimeKind.Unspecified`, so `ScheduleService.CreateOrUpdateScheduleAsync` wraps values in `DateTime.SpecifyKind(..., DateTimeKind.Utc)` before saving (`ScheduleService.cs:188-196`).
- **Recurring schedules** cannot be activated yet: `CreateAssessmentAsync` throws `"Recurring schedules are not supported yet; only one-time schedules can be activated."` (`AssessmentService.cs:29-32`).

### 5.6 Flow F — Submissions workflow (draft → submit → approve → authorize)

Each assessment item and assessment unit carries a `WorkflowStepId`; transitions are gated on step codes in `SubmissionsService`.

```
Browser (entry JS) ──POST /Submissions/SaveSaq       { assessmentItemId, answers:[{questionId, optionId, comment}] }  ──► SaveSaqAnswersAsync (upsert)
                  ──POST /Submissions/SaveKri       { assessmentItemId, values:[{kriId, value, comment}] }         ──► SaveKriValuesAsync (upsert)
                  ──POST /Submissions/SubmitItem    { assessmentItemId }                                            ──► SubmitItemAsync (requires complete saq/kri)
                  ──POST /Submissions/ApproveItem   { assessmentItemId }                                            ──► ApproveItemAsync (item must be Submitted)
                  ──POST /Submissions/AuthorizeUnit { assessmentUnitId }                                            ──► AuthorizeUnitAsync (all items Approved)
```

- **Own-unit scoping:** every read/save path filters through the acting user's `UnitId`; foreign items/units throw `"The requested item/assessment was not found."`.
- **Editable rule:** a unit is editable while its step ≠ Authorized and the item step ∉ {Submitted, Approved}.
- **Step codes (constants in `SubmissionsService.cs:12-16`):** `Submitted`, `Approved` (items); `Authorized` (unit). Workflows are `"ASSESSMENT-ITEM"` and `"ASSESSMENT-UNIT"`. Completeness is enforced with `EnsureCompleteAsync` (SAQ: every question answered; KRI: every value present).

### 5.7 Flow summary table

| Flow | Method/Route | Request body | Server binder | Response JSON | Data source |
|---|---|---|---|---|---|
| A — Page load | `GET /{Users,Roles,Units,SaqTemplates,KriTemplates,Schedule}/Index`, `Schedule/Wizard`, `Submissions/{Index,Detail,SaqEntry,KriEntry}`, `Dashboard/Index` | — | — | HTML + embedded `Json.Serialize` | ViewModel from service |
| B — Grid | `GET /{Users,Roles}/Grid`, `Units/{UnitGrid,GroupGrid}`, `SaqTemplates/{Grid,QuestionsGrid}`, `KriTemplates/{Grid,KrisGrid}`, `Schedule/Grid`, `Submissions/Grid` | — | — | `{success:true, data:[…]}` (`*GridRowViewModel`) | service → EF Core |
| C — Save/Delete | `POST /{Users,Roles}/Save`, `Units/{SaveUnit,SaveGroup}`, `SaqTemplates/{Save,SaveQuestion}`, `KriTemplates/{Save,SaveKri}`, `Schedule/SaveSchedule`, `*/Delete` | JSON | `[FromBody]` (`*SaveDto` / `DeleteRequestDto`) | `{success,message,data:{id,code}}` | service → `SaveChanges` |
| D — Login | `POST /Login/Login` | JSON | `[FromBody]` (`LoginRequestDto`) | `{success, data:{redirectUrl}}` | orchestrator → cookie |
| E — Wizard | `POST /Schedule/{SaveSchedule,SaveSaq,SaveKri,Finalize}` | JSON | `[FromBody]` (`ScheduleSaveDto` / `ScheduleTemplatesSaveDto` / `ScheduleFinalizeDto`) | `{success,message,data}` | service → `SaveChanges` (+ auto-assessment) |
| F — Submissions | `POST /Submissions/{SaveSaq,SaveKri,SubmitItem,ApproveItem,AuthorizeUnit}` | JSON | `[FromBody]` (`SaveSaqAnswersDto` / `SaveKriValuesDto` / `ItemTransitionDto` / `UnitAuthorizeDto`) | `{success,message}` | service → `SaveChanges` |

All AJAX responses use the shared **`ApiResponse<T>`** envelope (`Models/Dto/ApiResponse.cs`): `Success`, `Message`, `Data`, `Errors`. HTTP status stays `200`; outcome signalled by `success`. Grid keeps `data` as the array so DataTables `dataSrc:'data'` is unchanged; `id` / `code` / `redirectUrl` move into `data`.

---

## 6. Layer Responsibility Map ("use grid/use JSON/use model where")

| Concern | Layer / File | Notes |
|---|---|---|
| **Grid** (data table UI) | DataTables in `Views/{Users,Roles,Units,SaqTemplates,KriTemplates,Schedule,Submissions}/Index.cshtml` | client-side processing, AJAX `dataSrc:'data'` |
| **Grid data source** | `*Controller.Grid` → `*GridRowViewModel` (in `Models/ViewModel`) via `ApiResponse<T>` | camelCase (web JSON defaults) matches DataTables `columns`; not server-side processing |
| **JSON serialization (server→JS init)** | Razor `@Html.Raw(Json.Serialize(...))` | Users/Roles/Units/SAQ/KRI/Schedule/Submissions pages (options, selected ids, completion flags) |
| **JSON produce/consume** | Controllers `Json(...)` + jQuery `RiskPulse.postJson/getJson` | all AJAX in views' `@section Scripts` via the shared module |
| **Model — EF entities** | `Data/Entries/*` | mapped 1:1 to `riskpulse.*` tables |
| **Model — view models** | `Models/ViewModel/*` | data shaped for a UI/view — `*IndexViewModel`, `*GridRowViewModel`, `*WizardViewModel`, `*EntryViewModel`, `*PreviewViewModel`, `*OptionViewModel` |
| **Model — DTOs** | `Models/Dto/*` | data moving between layers/systems — `ApiResponse<T>`, `LoginRequestDto`, `LoginResultDto`, `UserAuthorizationDto`, `*SaveDto`, `*FinalizeDto`, `ItemTransitionDto`, `UnitAuthorizeDto`, `DeleteRequestDto`, `SaveResultDto` |
| **Model — enums** | `Models/Enum/*` | `UnitType`, `QuestionType`, `SaqStatus`, `KriStatus`, `ScheduleType`, `ScheduleStatus`, `ScheduleItemType`, `AssessmentStatus`; persisted as `varchar(32)` |
| **Business logic** | `Services/*Service` | no repository layer; each service uses `AppDbContext` directly; business rules throw `InvalidOperationException` (duplicate, self-edit, locked-template, group-vs-unit XOR, active-requires-SAQ+KRI, draft-only edits, workflow-step transitions, own-unit scoping) |
| **Controller boilerplate** | `Controllers/ControllerHelpers.cs` | `ValidateModel`, `TryExecute`, `TrySave`, `TryDelete` — null-guard, ModelState, ApiResponse envelope, exception→message passthrough |
| **Data access** | `Data/AppDbContext` via services | `Include`/`AsNoTracking`/`ExecuteDeleteAsync`/`SaveChanges` in services; `DbSetExtensions.EnsureUniqueAsync`/`ToOptionListAsync` |
| **DB schema/DDL** | **No `Migrations/` folder on disk** (git-ignored via `.gitignore` `**/Migrations/`); the `riskpulse` schema exists only in the dev DB | `HasDefaultSchema("riskpulse")`; schema is provisioned in the live DB outside the repo |
| **Seed data** | None in repo (no `Database/` folder, no `Seed.sql`) | permissions/roles/workflow-steps/unit/test-user rows live only in the dev DB; apply from the live DB or re-create manually |
| **Client validation** | `validateUserPayload`/`validateRolePayload`/`validateLoginForm` + `RiskPulse.validationError` in views | hand-rolled, not DataAnnotations-driven; server DataAnnotations are the source of truth |
| **Auth policies** | `PermissionCatalog` (single source) → `Program.cs` + `[Authorize]` + sidebar `HasClaim` + `PermissionPageMapper` | constant values must match `LoginOrchestratorService` claims + DB `Permissions` rows |
| **Auth cookie claims** | `LoginOrchestratorService` | `Name`, `NameIdentifier`, `Role`, `DefaultPage`, `Unit`, `Permission*` |
| **Workflow statuses** | `WorkflowStep.StepCode`/`StepLabel` for `ASSESSMENT-UNIT` and `ASSESSMENT-ITEM` workflows | statuses are DB rows, not enums; `IsInitial` seeds new assessments; `StepOrder` drives the Dashboard status slices and pill ordering |
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

SaqHeaders          (SaqHeaderId PK, SaqDesc, GroupId FK→Groups (nullable), UnitId FK→Units (nullable),
                     SaqStatus varchar(32), SaqCode unique)
SaqQuestions        (QuestionId PK, SaqHeaderId FK→SaqHeaders, QuestionText, QuestionType varchar(32), AllowComment, DisplayOrder)
SaqQuestionOptions  (OptionId PK, QuestionId FK→SaqQuestions, OptionText, DisplayOrder)

KriHeaders          (KriHeaderId PK, KriHeaderDesc, GroupId FK→Groups (nullable), UnitId FK→Units (nullable),
                     KriStatus varchar(32), KriCode unique)
Kri                 (KriId PK, KriHeaderId FK→KriHeaders, KriDesc, AllowComment, GreenLimit int, AmberLimit int, RedLimit int)

Schedules           (ScheduleId PK, ScheduleCode, ScheduleStatus varchar(32), ScheduleType varchar(32),
                     StartDate timestamptz?, EndDate timestamptz?, StartMonth timestamptz?, RecurringDay int?,
                     RiskRegisterHeaderId int? (placeholder — Risk Register not built))
ScheduleItems       (ScheduleItemId PK, ScheduleId FK→Schedules, ItemType varchar(32) — Saq/Kri, ItemId (polymorphic, no FK))
                     [unique (ScheduleId, ItemType, ItemId)]

Workflows           (WorkflowId PK, WorkflowCode unique, WorkflowName)
WorkflowSteps       (WorkflowStepId PK, WorkflowId FK→Workflows, StepCode, StepLabel, StepOrder, IsInitial, IsFinal)
                     [unique (WorkflowId, StepCode)]      # codes: Pending/InProgress/Submitted/Approved/Authorized

AssessmentHeaders   (AssessmentHeaderId PK, ScheduleId FK→Schedules (Restrict), AssessmentCode unique,
                     PeriodStart timestamptz?, PeriodEnd timestamptz?, AssessmentStatus varchar(32) — Pending only)
AssessmentUnits     (AssessmentUnitId PK, AssessmentHeaderId FK→AssessmentHeaders (Cascade),
                     UnitId FK→Units (Restrict), WorkflowStepId FK→WorkflowSteps (Restrict),
                     AuthorizedById FK→Users (nullable, Restrict), AuthorizedOn timestamptz?)
                     [unique (AssessmentHeaderId, UnitId)]
AssessmentItems     (AssessmentItemId PK, AssessmentUnitId FK→AssessmentUnits (Cascade),
                     ItemType varchar(32) — Saq/Kri, ItemId (polymorphic, no FK),
                     WorkflowStepId FK→WorkflowSteps (Restrict),
                     SubmittedById FK→Users (nullable, Restrict), SubmittedOn timestamptz?,
                     ApprovedById FK→Users (nullable, Restrict), ApprovedOn timestamptz?)
                     [unique (AssessmentUnitId, ItemType, ItemId)]
SaqAssessmentAnswers (SaqAssessmentAnswerId PK, AssessmentItemId FK→AssessmentItems (Cascade),
                     QuestionId FK→SaqQuestions (Restrict), OptionId FK→SaqQuestionOptions (Restrict), Comment?)
                     [unique (AssessmentItemId, QuestionId)]
KriAssessmentValues (KriAssessmentValueId PK, AssessmentItemId FK→AssessmentItems (Cascade),
                     KriId FK→Kris (Restrict), Value, Comment?)
                     [unique (AssessmentItemId, KriId)]
```

- **Enum→string conversion:** `Unit.UnitType`, `Unit.SaqStatus`, `QuestionType`, `KriStatus`, `ScheduleStatus`, `ScheduleType`, `ScheduleItemType` (×2), `AssessmentStatus` stored as `character varying(32)` (`AppDbContext.cs:18-141, 155-171, 232-237`).
- **Unique indexes:** `SaqCode`, `KriCode`, `AssessmentCode`, `ScheduleItem.(ScheduleId,ItemType,ItemId)`, `WorkflowCode`, `WorkflowStep.(WorkflowId,StepCode)`, `AssessmentUnit.(AssessmentHeaderId,UnitId)`, `AssessmentItem.(AssessmentUnitId,ItemType,ItemId)`, `SaqAssessmentAnswer.(AssessmentItemId,QuestionId)`, `KriAssessmentValue.(AssessmentItemId,KriId)`, `UnitGroup.(GroupId,UnitId)` (`AppDbContext.cs:38-39, 52-53, 98-99, 136-139, 164-165, 182-183, 199-200, 226-229, 259-262, 283-284, 300-301`). **No index on `Schedule.ScheduleCode`** (unlike the template/assessment codes).
- **Cascade/restrict rules** (`AppDbContext.cs:26-302`): `SaqHeader→SaqQuestions→SaqQuestionOptions` cascade with `SaqHeader→Group/Unit` restrict; `KriHeader→Kri` cascade with `KriHeader→Group/Unit` restrict; `Schedule→ScheduleItems` cascade; `AssessmentHeader→Schedule` **Restrict**; `Workflow→WorkflowSteps` cascade; `AssessmentHeader→AssessmentUnits→AssessmentItems` cascade with `AssessmentUnit→Unit/WorkflowStep/AuthorizedBy` restrict and `AssessmentItem→WorkflowStep/SubmittedBy/ApprovedBy` restrict; `AssessmentItem→answers/values` cascade with answer→Question/Option and value→Kri restrict; `UnitGroup→Group/Unit` cascade with a unique `(GroupId, UnitId)` index; `SaqHeader/KriHeader→Group` and `SaqHeader/KriHeader→Unit` restrict (a group or unit in use by a template can't be deleted; exactly one of GroupId/UnitId must be set — enforced as a business rule in the service). AccessControl FKs (Users→Roles/Units, RolePermissions→Roles/Permissions) cascade by convention.
- **Implemented via:** the schema was provisioned from the previous (now git-ignored) migration set; **no `Migrations/` folder exists on disk today** and `dotnet ef database update` has nothing to run against in the repo. Apply the schema/seed from the live DB or re-create manually (mismatch #5).

### 7.2 Seed data — no `Seed.sql` in the repo

No seed artifact is committed or present locally: there is **no `Database/` folder and no `Seed.sql`**. The dev DB holds the 9 permissions, 2 roles, 1 unit, 1 test user, and the two workflows' step dictionaries (`ASSESSMENT-UNIT`, `ASSESSMENT-ITEM`) directly. Any fresh DB needs these rows re-applied manually (ideally as an EF `HasData` seed or a committed SQL script — see mismatch #5/#6).

---

## 8. Pattern Mismatches & Inconsistencies

> Current **open** deviations only, with evidence and fix. Already-resolved items (save-model binding + server-side DataAnnotations, `ApiResponse<T>` envelope, login JSON standardization, `PermissionCatalog` single source, named grid view models, the DTO/ViewModel/Enum layer split, the shared JS module, Mod as the canonical shell pattern, the Assessment→Schedule wizard refactor, and the workflow-step/auto-assessment model) are reflected in §4–§6 and §10 and are not repeated here.

### 8.1 Architecture & layering

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 1 | **No repository / unit-of-work; no interface abstractions** — every service talks to `AppDbContext` directly and exposes concrete methods returning entities (or view models). The data layer is untestable and services can't be swapped or faked. | `Services/Administration/UsersService.cs`, `Services/Administration/RolesService.cs`, `Services/Administration/UnitsService.cs`, `Services/Login/DbAuthorizationService.cs`, `Services/Schedule/ScheduleService.cs`, `Services/Assessment/AssessmentService.cs`, `Services/Utilities/CodeGeneratorService.cs` | Introduce `IUsersService`, `IRolesService`, `IUnitsService`, `IScheduleService`, `IAssessmentService`, `ISubmissionsService`, `IDashboardService`, `ICodeGeneratorService`, `IAdAuthenticationService`, `IDbAuthorizationService`, `ILoginOrchestratorService` (optionally `IRepository<T>`/`IUnitOfWork`) and have services return DTOs, not entities. |
| 2 | **Concrete-class-only DI** — all 13 services are registered as `AddScoped<Concrete>()`, so nothing can be mocked or swapped. | `Program.cs:26-38` | Register `AddScoped<IXxx, Xxx>()` against the interfaces from #1. |
| 3 | **DataTables runs client-side processing** — the full row set ships to the browser in one response; paging/search/sort run in JS, not SQL. Latent scale problem for Submissions once real branch data collects. | All grid `Index.cshtml` views | For large tables use `serverSide:true` and handle `start`/`length`/`search` at the Grid endpoints. |
| 4 | **`AdAuthenticationService` is a stub that always returns `true`** — any username/password is "valid" as long as the user exists in DB. | `Services/Login/AdAuthenticationService.cs` | Implement a real directory/identity-provider lookup (or explicitly dev-gate the stub). |

### 8.2 Data & persistence

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 5 | **No seed artifact and no migration set for a fresh DB** — permissions/roles/unit/test-user/workflow-step rows exist only in the dev DB; `Migrations/` is git-ignored and absent on disk. | no `Database/`, no `Seed.sql`, no `Migrations/`; `.gitignore:365-368` | Move the schema into EF Core (`dotnet ef migrations add`) **and** seed via `modelBuilder.HasData` so a fresh DB is reproducible end-to-end. |
| 6 | **No migrate/seed bootstrap at startup** — the app assumes the DB was provisioned externally; a fresh DB will fail at first query. | `Program.cs:16-17` (no `Db.Database.Migrate()`) | Add dev-only `Migrate()` (+ data seed) bootstrap, or document the manual provisioning + seed-resync step in a README. |
| 7 | **No logging (`ILogger`) anywhere** — service/DB exceptions bubble with no trace and the catch blocks can't be audited. | all `Services/*` and `Controllers/*` | Inject `ILogger<T>` and log at service and catch boundaries. |

### 8.3 Security

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 8 | **No CSRF protection on cookie-auth state-changing endpoints** — `Users/Save`, `Roles/Save`, `Units` save/delete, `SaqTemplates`/`KriTemplates` save/delete, the schedule wizard `SaveSchedule|SaveSaq|SaveKri|Finalize|Delete`, the submissions `SaveSaq|SaveKri|SubmitItem|ApproveItem|AuthorizeUnit`, and `Login/Login` are JSON POSTs authenticated by cookie, but the app has no antiforgery tokens (`[ValidateAntiForgeryToken]` / `@Html.AntiForgeryToken()` are absent everywhere). | `Controllers/UsersController.cs`, `Controllers/RolesController.cs`, `Controllers/UnitsController.cs`, `Controllers/ScheduleController.cs`, `Controllers/SubmissionsController.cs`, `Controllers/LoginController.cs` | Emit antiforgery tokens in the views and add `[ValidateAntiForgeryToken]` on the POST actions; for `[FromBody]` JSON use `AddAntiforgery` + a header token. |
| 9 | **Database credentials committed to source** — the PostgreSQL connection string (`Server`, `Port`, user, `Password=123456`) is hard-coded in `appsettings.json` and tracked by git. | `appsettings.json:9` | Move credentials to user-secrets / environment variables; keep no secret (or a harmless dev value) in the repo. |

### 8.4 Code hygiene & minor

| # | Mismatch | Evidence | Recommended fix |
|---|---|---|---|
| 10 | **Assessment-structure enums are half-wired** — `AssessmentStatus` is written once (`Pending`) at assessment creation and never read for logic (behavior is entirely workflow-step-driven); `WorkflowStep.IsFinal` is defined but never referenced in code. | `AssessmentService.cs:52`; `Data/Entries/WorkflowStep.cs:22` | Either delete the enum + `IsFinal` column, or wire them into UI state ("Authorized" pills could reuse a final-step flag). |
| 11 | **No unique index on `Schedule.ScheduleCode`** (present on `SaqCode`/`KriCode`/`AssessmentCode`). | `AppDbContext.cs:113-122` vs `:49-53`, `:95-99`, `:161-165` | `AddIndex(ScheduleCode).IsUnique()` for parity; `CodeGeneratorService` already retries on collision. |
| 12 | **`OptionViewModel`/grid data relies on the picker card pattern in the wizard** — the template selector (`createTemplatePicker`) is ~210 lines of bespoke JS embedded in `Wizard.cshtml` rather than the shared module. | `Views/Schedule/Wizard.cshtml:322-533` | If reused by the Risk Register wizard later, extract into `riskpulse.js` as a reusable picker helper. |

---

## 9. Recommended / Target Architecture

A minimal, incremental target that fixes every mismatch above **without** a rewrite:

```
Views (.cshtml + DataTables/Select2/SweetAlert + card pickers)
   │  GET page (ViewModels + Json.Serialize init data)
   ▼  AJAX JSON (grid + save + login + wizard + submissions) — all through ApiResponse<T>
Controllers — thin: bind SaveModels, call services, return ApiResponse<T>
   │
Services (interface + concrete, Scoped DI)
   ├── Login:      IAdAuthenticationService  → AdAuthenticationService (real impl)
   │              IDbAuthorizationService   → DB lookup
   │              ILoginOrchestratorService → composes above
   │              PermissionCatalog / PermissionPageMapper (single source, shared by all modules)
   ├── Administration: IUsersService / IRolesService / IUnitsService
   ├── Templates:  ISaqTemplatesService / IKriTemplatesService
   ├── Schedule:   IScheduleService
   ├── Assessment: IAssessmentService / ISubmissionsService
   ├── Dashboard:  IDashboardService
   ├── Utilities:  ICodeGeneratorService
   └── Data access via IRepository<T> (or keep DbContext here behind service facades)
   │
Data — AppDbContext + EF Migrations (regenerate & commit) + EF seed (HasData)
   │
PostgreSQL (riskpulse schema)
```

Key decisions for the target:
1. **DTOs in, ViewModels out** — inbound payloads are `*SaveDto` / `*FinalizeDto` / `ItemTransitionDto` / `UnitAuthorizeDto` / `DeleteRequestDto` / `LoginRequestDto` (`Models/Dto`); outbound UI data is `*IndexViewModel` / `*GridRowViewModel` / `*WizardViewModel` / `*OptionViewModel` (`Models/ViewModel`); `ApiResponse<T>` is the shared envelope (already live).
2. **Uniform JSON contract** — `{ success, message, data, errors }` via `ApiResponse<T>` (already live).
3. **Server-side validation is the source of truth** — DataAnnotations on save models + custom validators; client JS mirrors it for UX only (already live).
4. **Single source for permissions/policies** — `PermissionCatalog` shared by `Program.cs`, sidebar, and `PermissionPageMapper` (already live).
5. **EF seed via `HasData`** so a fresh DB provisions seed rows + workflow dictionaries (mismatch #5/#6).
6. **Real AD provider** behind `IAdAuthenticationService` (or explicitly dev-gated) (mismatch #4).
7. **Logging** (`ILogger`) at service boundaries (mismatch #7).
8. **Server-side DataTables** when row counts grow (mismatch #3).
9. **Anti-forgery on all state-changing POSTs** (mismatch #8) and **secrets out of source** (mismatch #9).

---

## 10. Maturity Assessment

| Aspect | Status |
|---|---|
| Core framework / layout / login | ✅ Complete |
| Cookie auth + claims + permission policies | ✅ Complete |
| User / Role / Permission CRUD (models, services, views) | ✅ Complete — incl. self-edit rule in `UsersService` (controller left thin) |
| Units page (Unit CRUD \| Unit Group CRUD, Select2 group→unit assignment) | ✅ Complete — two-tab Administration page; group requires ≥2 units; delete guards for units/groups referenced by users or templates |
| SAQ Templates (grid, header CRUD, question/option designer, Locked immutable rule, Group XOR Unit link, unique code) | ✅ Implemented |
| KRI Templates (grid, header CRUD, item builder with Green/Amber/Red limits + comment flag, Locked immutable rule, Group XOR Unit link, unique code) | ✅ Implemented (threshold-config subsystem removed) |
| Schedule wizard (schedule type → SAQ templates → KRI templates → finalize, SCH code, multi-select card pickers + previews) | ✅ Implemented — per-step AJAX persistence, draft edit/re-save, activate rules (SAQ+KRI required), draft-only delete, locked Risk Register step placeholder |
| Assessment auto-creation on schedule activation (header → per-unit → per-item rows, workflow IsInitial steps) | ✅ Implemented — recurring schedules blocked for now |
| Workflow engine (DB-driven `Workflow`/`WorkflowStep` statuses) | ✅ Implemented — statuses are step codes, not enums |
| Submissions (fill/submit/approve/authorize, own-unit) | ✅ Implemented — grid, detail, SAQ entry, KRI entry (RAG dots from limits), workflow step transitions via `SubmissionsService`; maker/checker separation on completion |
| Dashboard (landing page) | ✅ Implemented — fully server-rendered: hero KPIs, status distribution (from workflow steps), KRI RAG snapshot (latest period), needs-attention list, period history |
| PostgreSQL + EF Core | ⚠️ Live in dev DB only — no `Migrations/`, no seed artifact in the repo; fresh DBs are not reproducible without manual work (§8 #5/#6) |
| DataTables AJAX grids + JSON save flows | ✅ Working — every grid returns a named `*GridRowViewModel` |
| Bootstrap 5 modals (programmatic open/close) | ✅ Fixed — open/close via `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` in the shared module; modals re-hosted under `<body>` |
| Design system (CSS) + AI specs | ✅ Complete |
| Project structure conventions | ✅ Complete — controllers flat & 1:1 with Views; services grouped by workflow; Models split `Dto`/`ViewModel`/`Enum`; entities in `Data/Entries` |
| Domain page: Risk Register Templates | ⬜ Stub |
| DTOs & uniform API envelope | ✅ Complete — `ApiResponse<T>` envelope; `*Dto` inputs + `*ViewModel` outputs split by layer |
| Server-side validation (DataAnnotations on save models) | ✅ On every save model |
| Permission single source (`PermissionCatalog`) | ✅ Resolved — policies, `[Authorize]`, sidebar, `PermissionPageMapper` all reference the catalog |
| Business rule placement | ✅ Service-side — duplicate, not-found, locked-template, group-vs-unit XOR, default-permission-must-be-assigned, draft-only-edit, active-requires-SAQ+KRI, workflow transitions, own-unit scoping |
| Shared JS module (`RiskPulse.*` in `wwwroot/js/modules/riskpulse.js`) | ✅ Dedup done — all shell views + wizard + submissions use the helpers; generic-error toast single-sourced |
| Status dropdowns (Locked excluded, RAG pill options) | ✅ Done — `GetAll()` filters `Locked`; `statusKind` tags options; template/schedule grids newest-first |
| Repository / unit-of-work / interface services | ❌ Not started (§8 #1/#2) |
| CSRF protection | ❌ Not started (§8 #8) |
| Secrets management | ❌ Credentials committed (§8 #9) |
| Real AD / identity provider | ❌ Stub (always true) (§8 #4) |
| Server-side grid processing | ❌ Not started (client-side only) (§8 #3) |
| Logging | ❌ Not started (§8 #7) |
| Tests / CI / Docker | ❌ Not started |

---

## 11. Next Logical Steps

**Resolved under the current phase (reflected in §4–§6/§10):**
1. ✅ Shared JS module + `ApiResponse<T>` + named grid view models + `ControllerHelpers` standardized across all shells.
2. ✅ Hygenic layer split — controllers flat, `Models/{Dto,ViewModel,Enum}`, entities in `Data/Entries`, services grouped by workflow; dead scaffolding removed.
3. ✅ **Assessment wizard → Schedule wizard refactor** — `Schedule` entity (not `ScheduleHeader`), `ScheduleController`/`ScheduleService`/`Views/Schedule/`, permission `Schedule`, multi-template card picker with SAQ/KRI previews.
4. ✅ **Workflow-step model** — `Workflow`/`WorkflowStep` dictionaries; submissions gated on step codes; auto-assessment on schedule activation.
5. ✅ **Template-key refactor** — `CodeGeneratorService` + unique `SaqCode`/`KriCode`/`AssessmentCode`; KRI threshold-config subsystem removed in favor of flat `GreenLimit`/`AmberLimit`/`RedLimit` RAG.

**Open (from §8):**
6. **Add CSRF protection** — antiforgery tokens + `[ValidateAntiForgeryToken]` on `Save`/`Login`/schedule-wizard/submissions POSTs (§8 #8).
7. **Move DB credentials out of source** — user-secrets / environment variables (§8 #9).
8. **Extract interfaces + register DI** — `IUsersService`, `IRolesService`, `IUnitsService`, `ISaqTemplatesService`, `IKriTemplatesService`, `IScheduleService`, `IAssessmentService`, `ISubmissionsService`, `IDashboardService`, `ICodeGeneratorService`, auth-service interfaces (§8 #1–#2).
9. **Commit EF migrations + `HasData` seed** so `dotnet ef database update` produces a fully-working DB including permissions/roles/unit/test user/workflow steps (§8 #5–#6).
10. **Implement real AD** or dev-gate explicitly (§8 #4).
11. **Add logging at service boundaries** (§8 #7).
12. **Then build domain:** the Risk Register module (its wizard step + auto-generated register rows), and move grids to server-side processing as submissions volume grows (§8 #3). Keep this file current for each finished step.