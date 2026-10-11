# Layout — Application Shells

Single source of truth for tokens, typography and components: [`AI/DESIGN.md`](../../DESIGN.md)
(backed by `wwwroot/css/site.css`). This spec covers the page shells only.

## Standard Shell (every page except Dashboard)
Rendered by `Views/Shared/_Layout.cshtml`.

- **Top nav** — `.rp-topnav` sticky bar, 56px, translucent **sapphire/cyan glass**
  (`--navy-deep #0a192f → --navy-mid #112240` with `backdrop-filter: blur(18px)`,
  a steel hairline border `rgba(56,189,248,.28)`) and a soft downward shadow
  (`--shadow-navbar`). A `.rp-skip-link` is the first focusable element, revealing
  on focus and targeting `#main-content`. Contents inside `.rp-topnav-inner`:
  - Brand: `.rp-brand-badge` (orange-gradient icon in a glass tile) + `.rp-brand-name`
    (hidden ≤1199.98px).
  - `#rpNavToggle` hamburger (`.rp-nav-toggle`, visible ≤991.98px) toggles `#rpNav`.
  - `#rpNav` links (`.rp-nav-link`, active = brighter **steel-tint** glass pill):
    Dashboard, Submissions, Schedule; Templates and Administration are Bootstrap
    dropdowns restyled as `.rp-nav-menu` (glass panel, rounded, permission-gated
    children), each toggle showing a single bold `fa-chevron-down` caret.
  - `.rp-nav-right`: `.rp-user-chip` (initials avatar + username + role, hidden
    ≤991.98px) and `.rp-nav-logout` (steel ghost → steel-tint hover).
- **≤991.98px drawer**: `.rp-topnav-inner` wraps — toggle on the first row, `#rp-nav`
  expands full-width beneath (`.rp-nav.open`), `.rp-nav-right` drops to its own
  right-aligned row. Toggle logic lives in `js/site.js` (`toggleNav`/`closeNav`;
  Escape and link clicks close it). No localStorage state.
- **Page canvas** — `.rp-main` holds fixed decorative layers (`.rp-bg-pattern` dot
  overlay + `.rp-main-ambient` breathing glow) over `--page-bg`, then `.rp-page`
  (max-width 1320px, centered, 24px gutters) and `.rp-footer`.
- **Entrance** — `rp-shell-fade` (nav), `rp-topbar-rise`, `rp-page-fade` (content),
  one-time per load; all disabled under `prefers-reduced-motion`.

## Dashboard (standard shell)
The Dashboard uses **the same top-nav shell as every other page** — there is no
"Shell" ViewData flag and no dedicated sidebar. `Views/Dashboard/Index.cshtml`
opens with a `.rp-hero--ink` banner (title + welcome/period sub +
`Open Submissions` action), then KPI stat cards, status/progress + KRI health
cards, the attention list and the period-history table. All dashboard tables are
server-rendered (not DataTables — no pagination there). The old `.rp-dash-shell`
sidebar/topbar layout and its `site.js` drawer code have been removed.

## Standalone Shells (`Layout = null`)
- `Login/Index` → `.rp-login-shell` (two-column, see `Specs/login`).
- `Login/AccessDenied`, `Error/Index` → `.rp-shell-centered` with `.rp-panel
  rp-panel-message` (both set `Layout = null`).

## Rules
- There is no sidebar anywhere in the app — the only chrome is the top nav
  (`_Layout.cshtml`) plus `Layout = null` standalone pages.
- Page headers are `.rp-hero` banners (see `AI/DESIGN.md`), not `rp-page-header`.
- A script at the end of `_Layout` re-hosts every `.modal` as a direct child of
  `<body>` after render, so modal z-index escapes any page container.
