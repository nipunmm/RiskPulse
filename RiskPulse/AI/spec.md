# RiskPulse — Implementation Specification

**Product:** Risk Pulse — automated risk reporting system for Siyapatha Finance PLC
**Source requirements:** `BRD-Riskreporting2025-AI.md`, `Risk-Reporting-Formats-Explained.md`
**Basis:** direct analysis of the code in this repository (paths/line numbers cited throughout)
**Status date:** reflects the working tree at time of writing
**Related docs:** `AGENTS.md` (technical rules & code patterns), `DESIGN.md` (UI design system). **This file is the source of truth for what the system does.**

> This document describes (1) what the system is required to do, (2) what is actually implemented in code, and (3) what remains pending. Where the BRD asks for something and the code does not yet deliver it, the item is marked **PENDING** with a reference to the gap.

---

## 1. Purpose & Scope

RiskPulse replaces Siyapatha's manual, email/Excel-based risk reporting process (`BRD §Executive Summary`, `§Current Solution`). It is a ASP.NET Core MVC web application that centralises data collection, review/approval, and reporting for four monthly report families:

| Report family | Cadence | BRD scope | Implemented today |
|---|---|---|---|
| **KRI** — Key Risk Indicators | Monthly | ✅ in scope | ✅ implemented |
| **SAQ** — Self-Assessment Questionnaire | Monthly | ✅ in scope | ✅ implemented |
| **Risk Register** | Monthly | ✅ in scope | 🟡 stub page only |
| **Loss Event Report** (RCA / RIA formats) | Monthly | ✅ in scope | ❌ not started |

### 1.1 Audience / user profiles (BRD)

The BRD defines two functional modules and five user profiles:

- **Module A — Risk management staff:** view all reports across branches/departments; download reports in Excel; add/edit/remove KRI, SAQ and Risk Trigger/Event (Risk Register).
- **Module B — Branch/Department staff:** view only their own branch/department reports; download past reports; enter and submit all risk reports.

Profiles: Branch/Dept **Maker**, Branch/Dept **Checker**, Risk Dept **Maker**, Risk Dept **Checker**, **Auditor** (read-only).

**Implemented equivalent:** access is DB-driven via `Role` → `Permission` claims (`PermissionCatalog`, `Services/Administration/RolesService.cs`) rather than hard-coded profiles. Maker/checker separation is enforced inside the Submissions workflow step engine (§5.6). The BRD's fixed "2 Makers + 2 Checkers per unit / 8 risk staff / Auditor" head-count model is **not** encoded as a constraint.

### 1.2 Entity terminology

- **Unit** = a branch or department (`UnitType { Branch, Department }`, `Models/Enum/UnitType.cs`).
- **Group** = a named set of units used to target multiple units at once (`Data/Entries/Group.cs`, `UnitGroup.cs`).
- **Template** = the centrally-defined master list: `SaqHeader`+`SaqQuestion`+`SaqQuestionOption`, or `KriHeader`+`Kri` (`Data/Entries/`).
- **Assessment Activation** = a reporting cycle config that targets units and references one Assessment Template (`Data/Entries/AssessmentActivation.cs`).
- **Assessment** = the snapshot instance created when an activation is activated; the fillable work item (`AssessmentHeader` → `AssessmentUnit` → `AssessmentItem`).
- **Submission** = a user-facing view over an assessment unit and its SAQ/KRI items.

---

## 2. Architecture (as built)

```
Views (.cshtml + DataTables / Select2 / SweetAlert2 / Bootstrap 5)
  │  GET page (ViewModels + `Json.Serialize` bootstrap data)
  ▼  AJAX JSON  →  ApiResponse<T>  { success, message, data, errors }
Controllers  (thin; bind SaveDto, delegate, return ApiResponse)
  │
Services  (concrete classes, Scoped DI, talk to AppDbContext directly)
  ├─ Login:        AdAuthenticationService, DbAuthorizationService, LoginOrchestratorService,
  │                PermissionCatalog, PermissionPageMapper
  ├─ Administration: UsersService, RolesService, UnitsService
  ├─ Templates:    SaqTemplatesService, KriTemplatesService, AssessmentTemplatesService
  ├─ Assessment:   AssessmentActivationService, AssessmentService, SubmissionsService
  ├─ Dashboard:    DashboardService
  ├─ Monitoring:   MonitorService
  └─ Utilities:    CodeGeneratorService
  │
Data  AppDbContext (EF Core 10) → PostgreSQL, schema `riskpulse`
```

**Key architectural facts (verified in code):**

- Registration/DI and the full request pipeline live in `Program.cs:13-86`. The 14 services are registered at `Program.cs:26-39`.
- Cookie authentication (20-min sliding expiry, HttpOnly) with permission claims; login flow in `Services/Login/LoginOrchestratorService.cs`.
- Authorisation is policy-per-permission: `Permission:<name>` policies (`Program.cs:49-61`) driven by `PermissionCatalog` (`Services/Login/PermissionCatalog.cs`); page routing by `PermissionPageMapper` (`Services/Login/PermissionPageMapper.cs`).
- Enums persisted as `varchar(32)` via `HasConversion<string>()` in `Data/AppDbContext.cs`.
- JSON contract: inbound `Models/Dto/*`, outbound `Models/ViewModel/*`, shared `ApiResponse<T>` envelope; `JsonStringEnumConverter` enabled (`Program.cs:20-25`).
- No repository/unit-of-work layer, no service interfaces, no AutoMapper — services are concrete and use `AppDbContext` directly. **PENDING.**
- Business rules live in services, never controllers (`Controllers/ControllerHelpers.cs` standardises null-guard/ModelState/TryExecute handling).

### 2.1 Permissions (single source of truth)

`PermissionCatalog` defines exactly ten permission strings (`Services/Login/PermissionCatalog.cs`): `Dashboard`, `Submissions`, `Assessment Activation`, `Users`, `Roles`, `Units`, `SAQ`, `KRI`, `Assessment Templates`, `Risk Register`. These must match the DB `Permissions.PermissionDesc` seed and the `[Authorize(Policy = "Permission:…")]` attributes, the nav in `_Layout.cshtml`, and `PermissionPageMapper`.

---

## 3. Domain Model (implemented entities)

All entities live in `Data/Entries/` and are registered in `Data/AppDbContext.cs` (DbSets at `:315-355`, relationships in `OnModelCreating` at `:11-313`).

### Administration / security
- `User`, `Role`, `Permission`, `RolePermission` — user/role/permission administration.
- `Unit` (`UnitType`), `Group`, `UnitGroup` (many-to-many, unique `(GroupId, UnitId)`).

### Templates
- `SaqHeader` (`SaqCode` unique, `SaqStatus`) → `SaqQuestion` (`QuestionType`, `AllowComment`) → `SaqQuestionOption`.
- `KriHeader` (`KriCode` unique, `KriStatus`) → `Kri` (flat `GreenLimit`/`AmberLimit`/`RedLimit`, `AllowComment`).
- `SaqHeader`/`KriHeader` link to **exactly one** of `Group` or `Unit` (`Restrict` FKs); the XOR rule is enforced in the service, not the DB.

### Assessment activation / assessment
- `AssessmentActivation` (`AssessmentActivationStatus { Draft, Active, Completed }`, `AssessmentType { OneTime, Recurring }`, `ActivationCode`, `int? AssessmentTemplateId` Restrict, `timestamptz` dates).
- `AssessmentTemplate` → `AssessmentTemplateItem` — polymorphic `(ItemType {Saq,Kri}, ItemId)` with no FK; cascade from `AssessmentTemplate`.
- `AssessmentHeader` (`AssessmentCode` unique, `AssessmentStatus`, `AssessmentActivationId` Restrict).
- `AssessmentUnit` → one per targeted unit; tracks `WorkflowStep`, and the Unit/Risk/Final approver FKs + timestamps.
- `AssessmentItem` (polymorphic `ItemType`+`ItemId`, per template) → `SaqAssessmentAnswer`, `KriAssessmentValue`.
- `Workflow`, `WorkflowStep` — DB-driven status dictionaries. **Statuses are step codes, not enums.**

### 3.1 Code generators

`CodeGeneratorService` (`Services/Utilities/CodeGeneratorService.cs`) issues unique keys `{SAQ|KRI|AST|ASM|ACT}-yyyyMMdd-0001` (`AST` = assessment template, `ACT` = assessment activation).

> **PENDING (parity):** `AssessmentActivation.ActivationCode` has no unique index, unlike `SaqCode`/`KriCode`/`AssessmentCode`.

---

## 4. Functional Modules — Status

Legend: ✅ implemented · 🟡 partial/stub · ❌ pending

| # | Module | Status | Primary files |
|---|---|---|---|
| 5.1 | Login & Authorization | ✅ | `LoginController.cs`, `Services/Login/*` |
| 5.2 | Users / Roles / Units administration | ✅ | `UsersController.cs`, `RolesController.cs`, `UnitsController.cs`, `Services/Administration/*` |
| 5.3 | SAQ Template designer | ✅ | `SaqTemplatesController.cs`, `Services/Templates/SaqTemplatesService.cs`, `Views/SaqTemplates/Index.cshtml` |
| 5.4 | KRI Template designer | ✅ | `KriTemplatesController.cs`, `Services/Templates/KriTemplatesService.cs`, `Views/KriTemplates/Index.cshtml` |
| 5.4.1 | Assessment Template designer (compose SAQ + KRI) | ✅ | `AssessmentTemplatesController.cs`, `Services/Templates/AssessmentTemplatesService.cs`, `Views/AssessmentTemplates/Index.cshtml` |
| 5.5 | Assessment Activation (grid + modal → snapshot assessment) | ✅ (one-time) | `AssessmentController.cs`, `Services/Assessment/AssessmentActivationService.cs`, `Views/Assessment/Activation.cshtml` |
| 5.6 | Submissions (fill → review → approve; card board) | ✅ | `SubmissionsController.cs`, `Services/Assessment/SubmissionsService.cs`, `Views/Submissions/*` |
| 5.7 | Dashboard | ✅ | `DashboardController.cs`, `Services/Dashboard/DashboardService.cs`, `Views/Dashboard/Index.cshtml` |
| 5.7.1 | Monitoring (cross-unit status matrix) | ✅ | `MonitorController.cs`, `Services/Monitoring/MonitorService.cs`, `Views/Monitor/Index.cshtml` |
| 5.8 | Risk Register Templates | 🟡 stub | `RiskRegisterTemplatesController.cs`, `Views/RiskRegisterTemplates/Index.cshtml` |
| 5.9 | Loss Event Report (RCA/RIA) | ❌ | — |
| 5.10 | Excel export / reporting | ❌ | — |
| 5.11 | Email notifications | ❌ | — |
| 5.12 | Audit trail | ❌ | — |

---

## 5. Functional Specification by Module

### 5.1 Login & Authorization — ✅
- Login page (`Views/Login/Index.cshtml`, standalone `Layout = null`) posts `LoginRequestDto` to `LoginController.Login` (`LoginController.cs:36`).
- `AdAuthenticationService.ValidateCredentialsAsync` is a **stub that always returns `true`** (`Services/Login/AdAuthenticationService.cs`) — dev only.
- `DbAuthorizationService.GetUserDetailsAsync` loads the user's roles/permissions (`Services/Login/DbAuthorizationService.cs`).
- `LoginOrchestratorService` composes auth + authorization, issues the cookie with claims `Permission`, `Role`, and the user's page permission (`Services/Login/LoginOrchestratorService.cs`).
- Unauthenticated → `/Login/Index`; forbidden → `/Login/AccessDenied` (`Program.cs:42-43`).
- **PENDING:** real AD/LDAP provider or explicit dev gating.

### 5.2 Users / Roles / Units administration — ✅
- **Users** (`UsersController.cs`): `Index`, `Grid`, `Save` (`UserSaveDto`), `Delete`. Service enforces a self-edit rule (`UsersService.UpdateUserAsync`).
- **Roles** (`RolesController.cs`): grid + create/update/delete; permission assignment validated against `PermissionCatalog` (`RolesService`). The permission assigned as a role's default must be among its granted permissions.
- **Units** (`UnitsController.cs`): two-tab page — **Unit** CRUD (`UnitGrid`/`SaveUnit`/`DeleteUnit`) and **Unit Group** CRUD (`GroupGrid`/`SaveGroup`/`DeleteGroup`, `GroupSaveDto`). A group requires ≥2 units; deletes are guarded when units/groups are referenced by users or templates (`UnitsService.cs`).
- Views: `Views/Users/Index.cshtml`, `Views/Roles/Index.cshtml`, `Views/Units/Index.cshtml` (DataTables + Bootstrap modals + Select2; per `AGENTS.md` conventions).

### 5.3 SAQ Template designer — ✅
- One master set of SAQ questions with options; header carries `SaqCode` (unique), status (`Active`/`Inactive`/`Locked`), and a **Group XOR Unit** assignment.
- Grid + full save: `SaqTemplatesController.Grid/SaveFull/Delete/QuestionsGrid` (`SaqTemplatesController.cs:42-64`).
- Service (`SaqTemplatesService.cs`): `GetHeaderRowsAsync`, `SaveFullAsync`, `DeleteHeaderAsync`, `GetQuestionRowsAsync`, `GetSaqPreviewAsync`.
- Business rules: unique code; **`Locked` templates are immutable**; exactly one of Group/Unit must be set; duplicate-name guard via `DbSetExtensions.EnsureUniqueAsync`.
- Views: `Views/SaqTemplates/Index.cshtml` — grid + full-screen `modal-xl` designer (header + question/option list) + read-only preview modal.
- **PENDING (BRD alignment):** the master SAQ data (Ref No, question text) is admin-authored rather than seeded from the fixed 52-branch/24-department reference lists in `Risk-Reporting-Formats-Explained.md §Part A`. Branch vs Department question sets (SAQ B1–B11 vs SAQ D1–D6) are represented as separate templates, not as fixed reference data.

### 5.3.1 KRI Template designer — ✅
- Same shape as SAQ (`KriTemplatesController.cs`, `KriTemplatesService.cs`, `Views/KriTemplates/Index.cshtml`).
- KRI items carry flat `GreenLimit`/`AmberLimit`/`RedLimit` ints and an `AllowComment` flag; RAG is computed in code (KRI entry view + `DashboardService`). The former threshold-configuration subsystem was removed.
- **PENDING (BRD alignment):** the fixed KRI master lists (KRI B1–B12, KRI D1–D6, HR variant D1–D8) are templates rather than seeded reference data.

### 5.4.1 Assessment Template designer — ✅
Files: `AssessmentTemplatesController.cs`, `Services/Templates/AssessmentTemplatesService.cs`, `Views/AssessmentTemplates/Index.cshtml`.
- Composes an **assessment template** — a reusable set of SAQ and/or KRI templates — under the nav **Templates → Assessment Templates** (permission `Assessment Templates`).
- Grid + modal CRUD: `AssessmentTemplatesController.Grid/Save/Delete`; `Index` injects the selectable SAQ/KRI options server-side. Description is **optional** and intentionally **not shown in the grid** (code + SAQ/KRI counts + status only).
- SAQ/KRI are chosen with the shared `RiskPulse.createChecklistPicker` — compact **tabbed** SAQ/KRI checkbox lists (search + Select all/Clear + live `n/N selected` count) sized for a dense modal (`.rp-soft-tabs` + `.rp-check-picker`) — and each row can be **previewed read-only before/without selecting** via the shared `RiskPulse.templatePreview` modal (`SaqPreview`/`KriPreview` actions on this controller, reusing `SaqTemplatesService.GetSaqPreviewAsync`/`KriTemplatesService.GetKriPreviewAsync`).
- Service (`AssessmentTemplatesService.cs`): `GetHeaderRowsAsync`, `GetSaqOptionsAsync`, `GetKriOptionsAsync`, `SaveAsync`, `DeleteAsync`.
- Schema: `AssessmentTemplate` (unique `AssessmentTemplateCode` `AST-…` via `CodeGeneratorService`, description, `IsActive`) + `AssessmentTemplateItem` (polymorphic `ItemType` `Saq`/`Kri` + `ItemId`, no FK, cascade from header, unique `(AssessmentTemplateId, ItemType, ItemId)`).
- Business rules: at least one SAQ or KRI required; item set is reconciled (final-state) on save; selected templates must still exist.
- **DONE:** assessment templates now drive activation — an activation references one template and snapshots its items into the real assessment (§5.5).

### 5.5 Assessment Activation (the `Assessment` module) — ✅ (one-time)
Files: `AssessmentController.cs`, `Services/Assessment/AssessmentActivationService.cs`, `Views/Assessment/Activation.cshtml`.
- Nav **Assessment** dropdown with the single child **Assessment Activation** (`Views/Assessment/Activation.cshtml`, action `Activation` → `/Assessment/Activation`; `/Assessment` redirects to it); controller/permission are `Assessment` / `Assessment Activation`.
- Page is a DataTables grid of activations plus one create/edit **modal** (Assessment Template `<select>` + one-time Start/End dates, `AssessmentActivationSaveDto`).
- Grid row actions are a **single Edit** button (Completed rows show none); every other action lives in the modal footer:
  - **Save** (`Save`) — label "Save Draft" for a new/draft row, "Save Changes" otherwise.
  - **Activate** (`SaveAndActivate`) — save-then-activate in one call; shown only for Draft.
  - **Disable** (`SetEnabled { enabled: false }`) / **Enable** (`SetEnabled { enabled: true }`) — shown for Active / Disabled rows.
  - **Delete** — shown only for Draft.
- Editing is allowed on any non-completed activation (Draft/Active/Disabled); disable/enable and delete act on the saved record by id. Activation snapshots the real assessment from the referenced Assessment Template (see §5.5.1 / `AssessmentService`).
- **One-time only:** the module offers no Recurring type; generation is one-time. The recurring generator/hosted service/`SyncNow`/`Close` were removed (the `AssessmentType` enum value remains for future use).
- **Status** enum `AssessmentActivationStatus { Draft, Active, Completed, Disabled }` (persisted varchar).
- `AssessmentActivationService` methods: `GetAllAsync`, `GetTemplateOptionsAsync`, `CreateOrUpdateAsync`, `FinalizeAsync`, `SetEnabledAsync`, `DeleteAsync`.
- Rules: edit/re-save allowed on any non-completed activation; draft-only delete; disable/enable for Active/Disabled; `timestamptz` fields coerced to UTC (`CreateOrUpdateAsync`).

### 5.5.1 Assessment snapshot creation — ✅
`AssessmentService.CreateAssessmentAsync(int assessmentActivationId)` (`Services/Assessment/AssessmentService.cs`) creates, on activation, a **snapshot** from the activation's Assessment Template:
`AssessmentHeader` (code `ASM-…`) → one `AssessmentUnit` per targeted unit (direct `UnitId` or all `UnitGroup` members) → one `AssessmentItem` per SAQ/KRI template item in the template, all at the respective workflow's `IsInitial` step. The snapshot copies membership, so later template edits do not affect generated assessments.
- Lifecycle state comes from `WorkflowStep.StepCode` rows of the `ASSESSMENT-UNIT` and `ASSESSMENT-ITEM` workflows (catalog in `Services/Assessment/WorkflowStepCodes.cs`; e.g. `Pending`/`InProgress`/`UnitApproved`/`RiskReviewed`/`FinalApproved`/`Returned`).
- `AssessmentStatus` is written once (`Pending`) and never read for logic — do not build logic on it.
- **PENDING:** `WorkflowStep.IsFinal` is defined but unused; `AssessmentStatus` is half-wired.

### 5.6 Submissions — ✅
Files: `SubmissionsController.cs`, `Services/Assessment/SubmissionsService.cs`, `Views/Submissions/{Index,Detail,SaqEntry,KriEntry}.cshtml`.
- **Card board** (own-unit scoping for non-risk staff): `SubmissionsController.Index` is server-rendered from `SubmissionsService.GetIndexAsync(userId, userRole)` → `SubmissionsIndexViewModel` with `Pending` / `InProgress` / `Completed` columns (bucketed by unit workflow step code); each card links to `Detail`. No DataTables grid.
- Detail: `GetDetailAsync` (assessment unit + items + progress).
- Entry: `GetSaqEntryAsync`, `SaveSaqAnswersAsync`; `GetKriEntryAsync`, `SaveKriValuesAsync`; KRI RAG dots computed from the item's limits.
- Workflow transitions: `SubmitItemAsync`, `ApproveItemAsync` (item-level maker→checker), then unit-level `UnitApproveAsync`, `RiskReviewAsync`, `FinalApproveAsync`, `ReturnAssessmentAsync` (`SubmissionsService.cs:377-507`).
- **Maker/checker separation** enforced on completion; own-unit scoping enforced server-side.
- **PENDING:** server-side grid processing for scale; Excel export of submitted data (see 5.10).

### 5.7 Dashboard — ✅
- `DashboardController.Index` → `DashboardService.GetDashboardAsync(userId)` (`Services/Dashboard/DashboardService.cs:24`).
- Fully **server-rendered** (no DataTables): hero KPIs, status distribution (from workflow steps), KRI RAG snapshot (latest period), "needs attention" list, period-history cards. View models: `DashboardViewModel`, `DashboardKpiViewModel`, `DashboardStatusSliceViewModel`, `DashboardAttentionRowViewModel`, `KriSnapshotViewModel`, `PeriodHistoryRowViewModel`.
- `Views/Dashboard/Index.cshtml` uses the standard top-nav shell.
- **PENDING:** BRD "role-based dashboards for different profiles" is only partially differentiated (server-rendered scope, no distinct Risk-Department vs Branch dashboard layouts).

### 5.7.1 Monitoring (cross-unit status matrix) — ✅
Files: `MonitorController.cs`, `Services/Monitoring/MonitorService.cs`, `Views/Monitor/Index.cshtml`.
- Reached at `/Monitor`, listed under the nav **Assessment** dropdown (gated by the `Monitoring` permission); **Risk Department (Reviewer/Approver) + Administrator only** — branches never see other units.
- Fully **server-rendered** cross-unit matrix (`MonitorService.GetMonitorAsync`): **rows = units** (grouped Branches then Departments, sticky first column), **columns = assessments** (newest period first, capped to the latest 12 with an assessment filter to focus one), **cells = that unit's `AssessmentUnit` status** as a colour-coded pill (`rp-pill-{kind}` from the unit workflow step) with a hover tooltip (`StepLabel` + `submitted/total`) and an overdue marker (`PeriodEnd < now` and not `FinalApproved`).
- Clicking a cell opens the existing `/Submissions/Detail/{assessmentUnitId}`; the page is read-only (all workflow actions stay in Submissions).
- Summary KPIs (units, completed, in progress, not started, overdue), a status legend/slice row (from the `ASSESSMENT-UNIT` workflow steps), a footer row of completed/target units per assessment, and `Unit type` + `Assessment` filters applied via GET query params (`assessmentHeaderId`, `unitType`).
- View models: `MonitorIndexViewModel`, `MonitorAssessmentColumnViewModel`, `MonitorUnitRowViewModel`, `MonitorCellViewModel`, `MonitorSummaryViewModel`, `MonitorStatusSliceViewModel`.
- **PENDING:** server-side processing / column virtualisation if assessment history grows large; matrix is limited to the newest 12 assessments by default.


### 5.8 Risk Register Templates — 🟡 stub
- `RiskRegisterTemplatesController.Index` returns a placeholder view (`RiskRegisterTemplatesController.cs:10`); `Views/RiskRegisterTemplates/Index.cshtml` renders a hero + note only.
- Permission `Risk Register` is fully wired (catalog, policy `Program.cs:59`, page mapper, nav).
- **PENDING — full feature.** Per `Risk-Reporting-Formats-Explained.md §Part B`, the target model needs:
  1. **Master risk taxonomy** — `Main Risk Category` (`No.` + name, org-wide fixed).
  2. **Sub-category** table (FK to main category).
  3. **Risk trigger/event** table (FK to sub-category) carrying fixed `Risk Appetite`; each trigger flagged **global** (all units) or **unit-specific**, via a many-to-many trigger↔unit mapping.
  4. **Monthly submission** row per `(Trigger, Unit, Period)` holding user-entered `Current Position/Actual` + mandatory `Remarks/Control activity`.
  5. **Audit-specific child table** (`Department/Branch`, `Audit Rating`, `Received Date`), only for the Audit register.
- None of these entities exist in `Data/Entries/` or `AppDbContext.cs` today.

### 5.9 Loss Event Report (RCA / RIA) — ❌ pending
BRD `§Loss Event Report — Monthly` specifies the same process as KRI/SAQ with RCA and RIA attach formats. No entity, template, page, permission, or nav entry exists.

### 5.10 Excel export / reporting — ❌ pending
BRD `§Reporting` and `§Success Criteria` require: download reports in Excel (Module A all units; Module B own unit + past reports), plus the three named reports — **Branch/Department Maker input**, **Checker authorizing**, **Risk Department**, and a **summary Excel download** screen; and the specific reports:
1. Branch/Department submission + approval report (with timestamps),
2. Department/Branch KRI,
3. Department/Branch SAQ,
4. Audit-trail report.

No Excel library (e.g. ClosedXML/EPPlus) is referenced; no export endpoint exists. **PENDING.**

### 5.11 Email notifications — ❌ pending
BRD `§System Architecture → Integration Layer` requires deadline and approval email notifications. Not implemented.

### 5.12 Audit trails — ❌ pending
BRD `§Security & Access Control` requires logging of all submissions/modifications/approvals with user + timestamp. Some per-record actor columns exist (`AssessmentUnit.UnitApprovedById`, `RiskReviewedById`, `FinalApprovedById`; `AssessmentItem.SubmittedById`, `ApprovedById`), but there is **no general audit log / change history** and no `ILogger` usage at service boundaries.

---

## 6. Non-Functional Requirements — Status

| BRD NFR | Target | Status |
|---|---|---|
| Role-based access control | Strict RBAC | ✅ Permission-claim policies (`Program.cs:49-60`, `PermissionCatalog`) |
| Maker/Checker separation | 2+2 per unit, approval before submit | 🟡 Enforced as workflow steps in Submissions, not as head-count |
| Audit trails | All changes logged w/ user + time | ❌ (partial actor columns only) |
| Data encryption (storage + transit) | Encryption standards | 🟡 HTTPS/HSTS in dev; DB-level encryption not configured |
| Uptime ≥ 99.9% | Availability | ⚪ Not enforced in code (deployment concern) |
| Response ≤ 3s | Per submission | ❌ No server-side grid processing / caching strategies |
| 5-year history / daily backups | Storage + backup | ❌ operational, not in repo |
| Scalable user/unit growth | Add units/users without degradation | 🟡 structurally supported; grids are client-side only |
| CSRF protection | All state-changing POSTs | ❌ |
| Secrets out of source | user-secrets/env | ❌ connection string committed in `appsettings.json` |
| Tests / CI | — | ❌ none |

---

## 7. Requirement Traceability (BRD → code)

| BRD requirement | Code location | Status |
|---|---|---|
| Secure username/password login | `LoginController.cs`, `Services/Login/*` | ✅ (AD stub) |
| Role-based access, 5 profiles | `PermissionCatalog`, `RolesService`, DB roles | 🟡 (roles, not fixed profiles) |
| User id scalability | `UsersService` CRUD | ✅ |
| SAQ monthly collection | `SaqTemplatesService`, `SubmissionsController.SaqEntry/SaveSaq` | ✅ |
| KRI monthly collection | `KriTemplatesService`, `SubmissionsController.KriEntry/SaveKri` | ✅ |
| Risk Register monthly | `RiskRegisterTemplatesController` | 🟡 stub |
| Loss Event Report | — | ❌ |
| Maker input → Checker authorise → Risk review | `SubmissionsService` workflow transitions | ✅ |
| View all reports (Risk Dept) / own (Branch) | `SubmissionsService.GetIndexAsync(userId, userRole)` (card board) | ✅ |
| Download Excel (all + past) | — | ❌ |
| Summary report Excel screen | — | ❌ |
| Branch/Dept maker input screen | `Views/Submissions/SaqEntry.cshtml`, `KriEntry.cshtml` | ✅ |
| Checker authorizing screen | `Views/Submissions/Detail.cshtml` (transitions) | ✅ |
| Risk Department screen | `Views/Dashboard/Index.cshtml`, Submissions card board | 🟡 |
| Audit trail log | — | ❌ |
| Email notifications | — | ❌ |

---

## 7.1 Pending backlog (prioritised)

1. **Risk Register Templates end-to-end** — taxonomy/sub-category/trigger entities, global-vs-unit trigger mapping, monthly submission + mandatory remarks, Audit child table, designer UI, wizard step unlock. (Largest remaining domain feature.)
2. **Excel export/reporting** — the four named reports + past-report download (Module B), one reusable export service.
3. **Loss Event Report (RCA/RIA)** — template family + collection flow.
4. **Audit trail** — append-only change log + `ILogger` at service boundaries.
5. **Email notifications** — deadlines/approvals (integration layer).
6. **Security hardening** — CSRF tokens on all state-changing POSTs; move DB credentials out of `appsettings.json`.
7. **Real AD provider** — replace the always-true stub (or formally dev-gate it).
8. **Reproducible database** — commit EF migrations + `HasData` seed so a fresh DB provisions the `riskpulse` schema, permissions, roles, workflows, and units.
9. **Service interfaces + DI** — introduce `I*Service` abstraction (currently concrete classes only).
10. **Server-side data processing** — switch DataTables grids to server-side when row counts grow.
11. **Recurring activation generation** — removed; the activation module is one-time only (was monthly day-of-month, seeded on activation by `ScheduleGenerationHostedService` + `SyncNow`).
12. **Tests / CI / Docker** — none exist.

---

## 8. Code Conventions (binding for new work)

- Controllers are thin and flat (`Controllers/`), 1:1 with `Views/<Controller>/`; business rules live in `Services/`.
- Services grouped by workflow under `Services/`; DTOs in `Models/Dto/`, view models in `Models/ViewModel/`, enums in `Models/Enum/`; entities in `Data/Entries/` mirrored to tables.
- New CRUD feature recipe: entity + `DbSet` + enum mapping → `*SaveDto` (DataAnnotations = server-side truth; `Id == 0` means new) → service (grid `AsNoTracking`, `SaveResultDto`) → controller (`ControllerHelpers.TrySave/TryDelete`, `ApiResponse`) → register service in `Program.cs` → `Index.cshtml` with DataTables + `@Html.Raw(Json.Serialize(...))` init data.
- New enum-valued property ⇒ `HasConversion<string>()` + `HasMaxLength(32)` in `AppDbContext.OnModelCreating`.
- Shared frontend helpers live only in `wwwroot/js/modules/riskpulse.js` (`RiskPulse.*`); views hold only validation rules, DataTables column config, and event wiring.
- Permissions are single-source: update `PermissionCatalog`, the `Program.cs` policy, `PermissionPageMapper`, the DB seed, the controller, the view, and nav links together.

---

## 9. Known Gaps / Debt

- `Migrations/` exists on disk but is not committed (git-ignored); `Data/Seed.sql` seeds the data rows — fresh DBs need manual provisioning.
- Connection string with credentials is committed in `appsettings.json`.
- `AssessmentStatus` enum and `WorkflowStep.IsFinal` are dead/half-wired.
- No unique index on `AssessmentActivation.ActivationCode`.
- No interfaces/repository layer, no logging, no CSRF, no tests/CI.
