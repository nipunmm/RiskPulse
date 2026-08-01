# RiskPulse - Project Analysis

> Generated from codebase analysis. Last reviewed: July 2026.

---

## Overview

**RiskPulse** (branded as **RiskIntel MIS**) is an enterprise risk management application built with ASP.NET Core MVC. The domain covers KRI (Key Risk Indicators), SAQ (Self-Assessment Questionnaires), branch-level risk submissions, and RAG (Red/Amber/Green) status tracking for high-stakes financial environments.

**Current Phase:** Early scaffolding — core framework is functional, but all domain pages are stubs and no database layer exists.

---

## Tech Stack

| Layer | Technology | Version / Details |
|---|---|---|
| Runtime | .NET | `net10.0` (cutting-edge preview) |
| Framework | ASP.NET Core MVC | `Microsoft.NET.Sdk.Web`, minimal hosting model |
| Language | C# | Implicit usings, nullable enabled |
| Auth | Cookie Authentication | `Microsoft.AspNetCore.Authentication.Cookies` |
| Template Engine | Razor Views (`.cshtml`) | Server-side rendered |
| CSS Framework | Bootstrap 5 | Vendored in `wwwroot/lib/bootstrap/` |
| Icons | Font Awesome | Vendored in `wwwroot/lib/font-awesome/` |
| JS | jQuery + jQuery Validation | Vendored in `wwwroot/lib/` |
| Fonts | Inter (variable), JetBrains Mono | Self-hosted `.woff2` |
| Custom CSS | "Stasis Enterprise" Design System | 728 lines, 92 CSS custom properties |
| Custom JS | Vanilla JS | 28 lines (sidebar toggle, keyboard shortcut) |

**NuGet Packages:** None — relies entirely on framework-provided libraries.

---

## Project Structure

```
RiskPulse/
├── RiskPulse.slnx                        # XML solution file (single project)
├── .github/workflows/                    # Empty — no CI/CD configured
│
└── RiskPulse/
    ├── Program.cs                        # Entry point — minimal hosting, DI, auth, routing
    ├── RiskPulse.csproj                  # net10.0, zero PackageReferences
    ├── appsettings.json                  # Standard logging config
    ├── appsettings.Development.json      # Dev logging overrides
    ├── Properties/launchSettings.json    # HTTP :5021, HTTPS :7110
    │
    ├── Controllers/                      # 7 MVC controllers
    │   ├── AuthController.cs             # Login (AJAX POST), Logout, AccessDenied
    │   ├── DashboardController.cs        # Stub
    │   ├── SubmissionsController.cs      # Stub
    │   ├── AssessmentControlController.cs# Stub
    │   ├── FormBuilderController.cs      # Stub
    │   ├── HomeController.cs             # Default ASP.NET template
    │   └── ErrorController.cs            # Global error handler
    │
    ├── Models/
    │   └── ErrorViewModel.cs             # RequestId, StatusCode, ErrorMessage, FriendlyMessage
    │
    ├── Services/AuthService/
    │   ├── IAuthService.cs               # Interface: AuthenticateAsync
    │   └── DevAuthService.cs             # Hardcoded admin/1234 for dev
    │
    ├── Views/
    │   ├── Auth/Login.cshtml             # Standalone login page (no layout), AJAX form
    │   ├── Dashboard/Index.cshtml        # Stub — heading + subtitle
    │   ├── Submissions/Index.cshtml      # Stub — heading + subtitle
    │   ├── AssessmentControl/Index.cshtml# Stub — heading + subtitle
    │   ├── FormBuilder/Index.cshtml      # Stub — heading + subtitle
    │   ├── Home/Index.cshtml             # Default template
    │   ├── Home/Privacy.cshtml           # Default template
    │   └── Shared/
    │       ├── _Layout.cshtml            # Main shell: sidebar + header + content
    │       ├── _Layout.cshtml.css        # Scoped layout CSS
    │       └── Error.cshtml              # Error page with status code + friendly message
    │
    ├── AI/
    │   ├── Skills/
    │   │   └── ui-ux-pro-max.md          # AI system prompt for UI/UX generation
    │   └── Specs/
    │       ├── DESIGN.md                 # Master design system spec
    │       ├── PROJECT-ANALYSIS.md       # This file
    │       ├── login/                    # Login page spec (DESIGN.md, code.html, screen.png)
    │       ├── layout/                   # App shell spec
    │       ├── branch/                   # Branch officer dashboard spec
    │       ├── kri/                      # KRI data entry spec
    │       └── error-page/               # Error page spec
    │
    └── wwwroot/
        ├── css/site.css                  # Stasis Enterprise design system (728 lines)
        ├── js/site.js                    # Sidebar toggle + search shortcut
        ├── fonts/                        # Inter, JetBrains Mono
        └── lib/                          # Bootstrap, Font Awesome, jQuery, jQuery Validation
```

---

## Architecture & Patterns

### Pattern: Classic MVC (Model-View-Controller)

Traditional ASP.NET Core MVC with server-side rendered Razor views. No SPA framework, no Web API layer.

### Middleware Pipeline (in order)

1. Exception handler (`/Error/Index`)
2. HTTPS redirection
3. Status code pages (`/Error/Index?statusCode={0}`)
4. Routing
5. Authentication
6. Authorization
7. Static files (`MapStaticAssets`)
8. Controller routes

### Service Layer Pattern

- Services registered via DI in `Program.cs` as **Scoped** lifetime
- Interface-based abstraction (`IAuthService` → `DevAuthService`)
- Currently only one service exists

### Authentication

- Cookie-based with form login
- Default route: `{controller=Auth}/{action=Login}/{id?}`
- Login is AJAX-based (jQuery `$.ajax` POST, returns JSON with `redirectUrl`)
- `[Authorize]` on all controllers except `AuthController` and `HomeController`
- Dev credentials: `admin` / `1234` (hardcoded in `DevAuthService`)

### What is NOT Present

- No repository pattern / Unit of Work
- No DTOs or ViewModels (beyond `ErrorViewModel`)
- No API controllers or Web API
- No background services or hosted services
- No custom middleware
- No FluentValidation or Data Annotations
- No database connection strings, EF Core, or ORM
- No Docker configuration
- No CI/CD pipelines
- No tests

---

## Design System: "Stasis Enterprise"

A Material Design 3-inspired token architecture for enterprise financial UI.

### Key Design Tokens (92 CSS custom properties)

| Category | Tokens |
|---|---|
| Color System | `--color-primary-*`, `--color-secondary-*`, `--color-tertiary-*`, `--color-error-*` |
| Surface Variants | `--color-surface-*` (5 elevation levels) |
| Outline Variants | `--color-outline-*` (4 weight levels) |
| RAG Status | `--color-rag-green`, `--color-rag-amber`, `--color-rag-red` |
| Layout | `--sidebar-width: 256px` |
| Typography | `--font-family-display`, `--font-family-body`, `--font-family-mono` |

### Typography Scale

| Class | Size | Weight | Use |
|---|---|---|---|
| `.font-display-lg` | 32px | 700 | Page titles |
| `.font-headline-md` | 24px | 600 | Section headers |
| `.font-headline-sm` | 18px | 600 | Card titles |
| `.font-body-lg` | 16px | 400 | Body text |
| `.font-body-md` | 14px | 400 | Default text |
| `.font-body-sm` | 12px | 400 | Captions |
| `.font-label-md` | 12px | 600 | Labels (uppercase) |
| `.font-data-mono` | 13px | 450 | Data values (JetBrains Mono) |

### Component Classes

- `.sidebar`, `.sidebar-nav`, `.sidebar-footer`, `.sidebar-logo` — Navigation
- `.top-header`, `.search-wrapper`, `.header-icon-btn` — Header
- `.card-stasis` — Cards with border, hover shadow
- `.btn-stasis-primary`, `.btn-stasis-secondary` — Buttons
- `.table-stasis` — Data tables (sticky headers, hover rows, monospace data)
- `.input-stasis` — Form inputs (focus halo effect)
- `.rag-green`, `.rag-amber`, `.rag-red` — RAG status indicators
- `.rag-dot-green`, `.rag-dot-amber`, `.rag-dot-red` — Circular RAG dots
- `.status-pill` — Full-round status badges
- `.notification-badge` — Header notification indicator

### Responsive Behavior

- Sidebar: fixed 256px on desktop, slide-in with `.open` class on mobile
- Search bar: hidden on mobile
- Content area: full width on mobile, max-width 1440px on desktop
- Submenu animation: staggered fadeIn keyframes

---

## AI-Assisted Development Workflow

The project uses a structured AI-assisted development approach via the `AI/` directory.

### `AI/Skills/ui-ux-pro-max.md`

A system prompt defining a 3-step workflow for AI-generated UI:
1. Identify Context (page type, user role, data density)
2. Define Design Tokens (color, typography, spacing)
3. Write Code (semantic HTML, token-based CSS, responsive)

Includes golden UX rules: accessibility (4.5:1 contrast), touch targets (44×44px), mobile-first, loading states.

### `AI/Specs/{page}/` directories

Each page spec contains:
- `DESIGN.md` — Full Stasis Enterprise design tokens + guidelines
- `code.html` — Tailwind CSS prototype (reference implementation)
- `screen.png` — Screenshot of the target design

**Important:** The `code.html` prototypes use **Tailwind CSS via CDN** for rapid prototyping, while the actual application uses **Bootstrap 5 + custom CSS**. Specs serve as visual reference targets to translate into the Razor/Bootstrap stack.

### Existing Page Specs

| Spec | Page | Key Elements |
|---|---|---|
| `login/` | Auth/Login | Centered card, geometric background, shield icon |
| `layout/` | _Layout | Dark sidebar, top nav, content canvas, bento grid |
| `branch/` | Dashboard (Branch) | Pending submissions, recent submissions table |
| `kri/` | KRI Data Entry | Metric rows, target vs actual, RAG dots, commentary |
| `error-page/` | Error | Status code display, friendly message, request ID |

---

## Domain Concepts

| Term | Description |
|---|---|
| **KRI** | Key Risk Indicator — measurable metrics for risk assessment |
| **SAQ** | Self-Assessment Questionnaire — compliance/self-evaluation forms |
| **RAG** | Red/Amber/Green — risk status visualization system |
| **Branch** | Organizational unit — officers submit risk data at branch level |
| **Submissions** | Risk data entries submitted by branch officers for review |
| **Assessment Control** | Oversight/management of risk assessments |
| **Form Builder** | Dynamic form creation tool (stub, not yet implemented) |

---

## Controller Routes

| Controller | Route | Auth Required |
|---|---|---|
| `AuthController` | `GET/POST /Auth/Login`, `POST /Auth/Logout`, `GET /Auth/AccessDenied` | No |
| `DashboardController` | `GET /Dashboard/Index` | Yes |
| `SubmissionsController` | `GET /Submissions/Index` | Yes |
| `AssessmentControlController` | `GET /AssessmentControl/Index` | Yes |
| `FormBuilderController` | `GET /FormBuilder/Index` | Yes |
| `HomeController` | `GET /Home/Index`, `GET /Home/Privacy` | No |
| `ErrorController` | `GET /Error/Index?statusCode=` | No |

---

## Maturity Assessment

| Aspect | Status |
|---|---|
| Core Framework | ✅ Functional |
| Authentication | ✅ Working (dev-only hardcoded) |
| Layout / Shell | ✅ Complete |
| Design System (CSS) | ✅ Complete (728 lines) |
| Design Specs (AI) | ✅ Complete (5 pages prototyped) |
| Domain Pages | ⬜ Stubs only |
| Database / ORM | ❌ Not started |
| API Layer | ❌ Not started |
| Service Layer | 🟡 One service (AuthService) |
| Data Models | ❌ Not started |
| Testing | ❌ Not started |
| CI/CD | ❌ Not started (empty workflows dir) |
| Docker | ❌ Not started |
| NuGet Dependencies | None (framework-only) |

---

## Next Logical Steps

1. **Database Setup** — Entity Framework Core, DbContext, migrations
2. **Data Models** — Risk assessments, submissions, KRI metrics, branches, users
3. **Complete View Stubs** — Implement pages per AI specs (branch, KRI, submissions, etc.)
4. **Expand Service Layer** — Business logic for risk calculations, RAG thresholds
5. **Repository/Service Pattern** — Data access abstraction
6. **ViewModels** — Typed models for view data binding
7. **Testing** — Unit and integration tests
8. **CI/CD** — GitHub Actions pipeline
9. **Docker** — Containerization for deployment
