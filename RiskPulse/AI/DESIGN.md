---
name: Risk Pulse
version: 2.0 (Sapphire Glass Edition)
# NOTE: steel-deep, success, info and chart-2 are AA-retuned from the raw
# client Sapphire spec (#0284c7 / #059669 / #0284c7 / #f58220) which failed
# WCAG 2.2 AA as text/chart marks. warning and error are held at their AA values.
mixing-methods:
  - Tonal Tinting & Shading (Derived Hue Scales)
  - Alpha Opacity Blending (Glassmorphism & Radial Glows)
  - 3-Step Semantic Wash (Base / Wash / Border)
  - Gradient Interpolation (Linear & Radial Lighting)
colors:
  navy-deep: '#0a192f'
  navy-mid: '#112240'
  ink-raised: '#1e3a8a'
  steel: '#38bdf8'
  steel-deep: '#0369a1'
  steel-wash: '#e0f2fe'
  orange: '#f58220'
  orange-hover-start: '#e07514'
  orange-hover-end: '#ffa048'
  orange-soft: '#ffb37a'
  orange-deep: '#c2410c'
  orange-wash: '#fff3e6'
  page-bg: '#f0f4f8'
  surface: '#ffffff'
  surface-soft: '#f8fafc'
  surface-soft-2: '#f1f5f9'
  ink: '#0f172a'
  text-2: '#334155'
  text-2b: '#1e293b'
  text-muted: '#475569'
  text-disabled: '#94a3b8'
  border: '#cbd5e1'
  form-border: '#0284c7'
  focus: '#0284c7'
  focus-on-dark: '#38bdf8'
  success: '#047857'
  success-wash: '#ecfdf5'
  success-border: '#a7f3d0'
  warning: '#b45309'
  warning-text: '#92400e'
  warning-wash: '#fef3c7'
  warning-border: '#fde68a'
  error: '#b91c1c'
  error-bright: '#ef4444'
  error-wash: '#fef2f2'
  error-border: '#fecaca'
  info: '#0369a1'
  info-wash: '#e0f2fe'
  info-border: '#bae6fd'
  violet: '#7c3aed'
  violet-wash: '#f3e8ff'
  violet-border: '#ddd6fe'
  neutral: '#64748b'
  neutral-wash: '#f1f5f9'
  neutral-border: '#e2e8f0'
  chart-1: '#0284c7'
  chart-2: '#c2410c'
  chart-3: '#059669'
  chart-4: '#7c3aed'
  chart-5: '#db2777'
  chart-6: '#475569'
  seq-1: '#e0f2fe'
  seq-2: '#7dd3fc'
  seq-3: '#0284c7'
  seq-4: '#1e3a8a'
  seq-5: '#0a192f'
typography:
  font-sans: 'Segoe UI Variable Text / Segoe UI / system-ui stack'
  font-display: 'Segoe UI Variable Display / Segoe UI Semibold / system stack'
  font-numeric: 'Segoe UI Variable Text / Roboto / Arial (tabular-nums)'
  font-mono: 'Cascadia Mono / Consolas / Liberation Mono / Courier New'
  display-lg:
    fontSize: clamp(1.5rem, 2.4vw, 1.875rem)
    fontWeight: '700'
    letterSpacing: -0.028em
  display-md:
    fontSize: clamp(1.25rem, 1.8vw, 1.5rem)
    fontWeight: '700'
    letterSpacing: -0.024em
  headline-md:
    fontSize: clamp(1.125rem, 1.5vw, 1.25rem)
    fontWeight: '700'
    letterSpacing: -0.018em
  body-lg: { fontSize: 1rem, lineHeight: 1.6 }
  body-md: { fontSize: 0.875rem, lineHeight: 1.6 }
  body-sm: { fontSize: 0.8125rem, lineHeight: 1.55 }
  body-xs: { fontSize: 0.75rem, lineHeight: 1.5 }
  label-md:
    fontSize: 0.6875rem
    fontWeight: '800'
    transform: uppercase
    letterSpacing: 0.07em
  stat-value:
    fontSize: 1.75rem
    fontWeight: '700'
    fontVariantNumeric: tabular-nums
rounded:
  pill: 999px
  card: 20px
  panel: 16px
  hero: 12px
  control: 12px
  sm: 10px
  xs: 8px
spacing:
  base: 4px
  gutter: 1rem
  container-max: 1320px
  nav-height: 56px
  target-min: 44px
elevation:
  navbar: 0 18px 42px -34px rgba(10, 10, 24, 0.90)
  card: 0 18px 42px -38px rgba(15, 23, 42, 0.35)
  hero: 0 26px 54px -38px rgba(10, 10, 24, 0.92)
  menu: 0 22px 48px -28px rgba(15, 23, 42, 0.50)
  drawer: -30px 0 70px -30px rgba(10, 10, 24, 0.68)
z-index:
  sticky: 20
  backdrop: 1040
  modal: 1050
  select: 1061
  loading: 1080
---

## Brand & Style
Risk Pulse follows the **SIYAPATHA brand** and the **UI Style Guide — Central Workflow
Notification and Dashboard Platform**. Hybrid theme: dark sapphire/cyan glass chrome (top nav,
hero banners, login brand panel, overlays) over light content surfaces (canvas `#f0f4f8`,
white cards) so dense tables and forms stay legible. One action color: **Siyapatha orange
`#f58220`**. Mood: calm, modern operations console with generous radii and soft,
large-blur shadows. (Theme: **Sapphire Glass Edition**.)

## Accessibility Contract (WCAG 2.2 AA, all pairs verified)
Rules every component must follow:
1. Text is at least **4.5:1** (large text 3:1). Non-text UI (control borders, focus rings,
   icons, chart marks, status dots) is at least **3:1**.
2. **Color is never the only signal.** Every status uses color + icon + visible label.
3. Orange is a fill, never a text color on light surfaces. Orange text on light uses
   `orange-deep`, and only on `surface`, `page-bg` or `orange-wash`.
4. Text on `orange` is `navy-mid` or `navy-deep` (white only on `orange-deep`).
5. `text-disabled` and `border` are decorative/inactive only (WCAG-exempt). Never use them
   for content, placeholders, timestamps or input boundaries.
6. `steel` (`#38bdf8`) is **dark-chrome only** (icons, rules, glows, focus-on-dark). It fails
   3:1 on light; on light surfaces use `steel-deep` (`#0369a1`).
7. Interactive targets are at least 44px (`target-min`) — except the compact top-nav controls
   (nav links, user chip, logout), in-page action buttons (`.rp-btn`) at 32px, grid controls
   (DataTables search/length) at 34px, and modal form controls at 36px, which still clear
   WCAG 2.2 AA 2.5.8 (24px); focus is always visible.
8. Danger CTA fills use `error` (`#b91c1c`, white text); `error-bright` is for dots, marks and
   accents only — white on it is below 4.5:1.

### Verified contrast
| Pair | Ratio | Use |
|---|---|---|
| ink on surface / page-bg | 17.9 / 16.2 | Headings, body |
| text-2 on page-bg / surface-soft-2 | 9.4 / 9.5 | Secondary copy |
| text-muted on surface / page-bg / surface-soft-2 / steel-wash | 7.6 / 6.9 / 6.9 / 6.6 | Hints, timestamps, placeholders |
| steel-deep on surface / page-bg / steel-wash | 5.9 / 5.4 / 5.2 | Links, info text, on-light icons |
| navy-deep on orange | 6.8 | Primary button text |
| navy-deep on orange-hover-start / hover (#ffa048) | 5.6 / 8.7 | Button hover states |
| white on orange-deep | 5.2 | Pressed button |
| orange-deep on surface / page-bg / orange-wash | 5.2 / 4.7 / 4.7 | Orange text on light |
| orange on navy-deep / ink-raised | 6.8 / 4.0 | Active nav (non-text) |
| white on navy-deep / navy-mid / ink-raised | 17.6 / 15.8 / 10.4 | On-dark text, icons, body |
| success on surface / page-bg / success-wash | 5.5 / 5.0 / 5.2 | Success text, pill |
| warning-text on warning-wash / surface-soft-2 | 6.4 / 6.5 | Warning text, pill |
| error on surface / page-bg / error-wash | 6.5 / 6.0 / 5.9 | Error text, pill |
| violet on surface / violet-wash | 5.7 / 4.8 | Violet pill |
| white on error | 6.5 | Danger button text |
| form-border on surface / page-bg | 4.1 / 3.7 | Input boundary (non-text) |
| orange-deep rim on surface | 5.2 | Primary button boundary (non-text) |
| focus on surface / focus-on-dark on navy-deep | 4.1 / 8.2 | Focus ring (non-text) |
| steel-deep on surface / page-bg | 5.9 / 5.4 | On-light icons, links (non-text) |
| chart-1..6 on surface | 4.1 / 5.2 / 3.8 / 5.7 / 4.6 / 7.6 | Chart marks (non-text, all ≥3:1) |
| navy-deep text on seq-1..2 / white on seq-4..5 | 10.6 / 17.6 | Heatmap cells; seq-3 text must be large |

## Colors
- **Chrome (dark sapphire glass):** `navy-deep` → `navy-mid` gradient; `ink-raised` for
  raised/hover layers; `steel` (cyan `#38bdf8`) for **dark-chrome only** — secondary chrome,
  rails, glows and `focus-on-dark`; on-dark text and icons are **white**;
  `steel-wash` for selected rows, hover and info backgrounds.
- **Action (Siyapatha orange):** `orange` primary CTA, active nav, spinner;
  `orange-hover-start/end` (`#e07514` → `#ffa048`) is the hover gradient ramp; `orange-deep`
  pressed state and orange text on light; `orange-soft` on dark (login feature icons);
  `orange-wash` subtle background (selected/notice).
- **Accent on light:** `steel-deep` (`#0369a1`, also `info`) for links, on-light icons and
  info text. `focus` / `form-border` are `#0284c7` (Sapphire sky) — non-text only.
- **Neutrals:** canvas `page-bg` with a `steel-deep` radial glow (`900px 400px at 85% -10%`);
  `surface` white; `ink` headings; `text-2` / `text-muted` secondary; `border` (`#cbd5e1`)
  hairlines only (decorative); `form-border` for every input, select and checkbox boundary.
- **Semantic (3-step wash — base / wash / border):** `success` teal-green (`#047857`),
  `warning` amber (`#b45309` for dots/fills, `warning-text` `#92400e` for text), `error`
  (`#b91c1c`; `error-bright` `#ef4444` for dots and marks only), `info` = `steel-deep`,
  `violet` single category accent, `neutral` for inactive dots. Each role carries a `-wash`
  (banner/pill background) and a `-border` (pill, notice and mark hairline), e.g.
  `success-wash` / `success-border`. Pills, `.rp-notice-*` and semantic marks consume the trio.
- **Chart palette (ordered):** `chart-1..6`. Maximum **6 series**; group extras into
  "Other" or use small multiples. Every series also gets a distinct marker shape or line
  dash and a direct label or legend entry. `chart-2` is the darkened `#c2410c` (raw orange
  `#f58220` fails 3:1 as a mark).
- **Sequential ramp (heatmaps):** `seq-1..5`, light to dark, monotonic in luminance. Print
  the value in each cell; use `navy-deep` text on seq-1..2 and white on seq-4..5. `seq-3`
  (`#0284c7`) passes neither text color at 4.5:1 — show its value in large text only.
- **Hero variants:** `.rp-hero--ink|blue|violet|teal|slate|green|orange|gold`. Common ink
  base with a module-tinted radial glow. `--ink`/`--blue`/`--slate` use the cyan `steel`
  glow; `--violet` uses `rgb(from var(--violet) r g b / .34)`; `--teal`/`--green` use
  `success`; `--gold` uses `warning`; `--orange` uses `orange`.
- **Glass utilities:** `.rp-glass-dark` (dark sapphire glass, cyan rim), `.rp-glass-light`
  and `.rp-glass-card` (frosted light surfaces).

### Token rules
- One token per role. Role tokens may intentionally share a value where the roles differ
  (`steel-deep` = `info` = `#0369a1`; `focus` = `form-border` = `#0284c7`;
  `steel-wash` = `info-wash` = `seq-1`; `steel` = `focus-on-dark` = `#38bdf8`) but each keeps
  its own name so a role can be re-tuned independently.
- No unused tokens in `:root`. Add a token only with its first consumer.
- No hard-coded colors in rules: gradients and rings reference tokens
  (`var(--navy-deep)`, `rgb(from var(--steel) r g b / .x)` or a `--steel-rgb` triplet).
- Legacy names are removed (see Migration).

## Typography
- System font stacks only, no webfonts. Display sizes use tight negative tracking. Every
  numeric surface (grids, pills, `input[type=number|date|time]`) uses `tabular-nums`.
- Minimum body size 12px (`body-xs`); never below. Text honors browser zoom to 200%.
- Utilities: `.rp-display-lg|md`, `.rp-headline-md`, `.rp-body-lg|md|sm|xs`,
  `.rp-label-md`, `.rp-stat-value`, `.rp-mono`.

## Layout & Shells
- **Standard shell:** sticky glass top nav (`.rp-topnav`, 56px) → `.rp-main` with ambient
  glow → centered `.rp-page` (max 1320px) → slim `.rp-footer`. Nav: brand, primary links,
  Templates/Administration dropdowns (`.rp-nav-menu`), user chip, logout. Dropdown toggles
  use a single `fa-chevron-down .rp-caret` that rotates 180° when open. Mobile (≤991px)
  collapses to a drawer via `#rpNavToggle`/`site.js`. Every page uses this shell,
  including Dashboard.
- **Standalone shells:** `.rp-shell-centered` (error/access) and `.rp-login-page`
  (brand panel + `.rp-login-panel`) render with `Layout = null`.
- Provide a "Skip to content" link as the first focusable element.

## Elevation & Shapes
- Soft large-blur, negative-spread shadows (`--shadow-*`). Radii: pills 999px, cards 20px,
  panels/hero 16/12px, controls 12px, small 10/8px.

## Components
- **Hero:** `.rp-hero` ink banner, per-module glow, `<h1>` + `.rp-hero-sub`, primary
  action in `.rp-hero-actions` (static below 576px), back link `.rp-hero-back`.
- **Cards:** `.rp-card` + `.rp-card-head/body`; grids are card + `.rp-grid-toolbar` +
  `.table-responsive`.
- **Data tables:** `.rp-table`; compact sticky uppercase header, hover row, `tabular-nums`,
  paginated (`pageLength:10`, selector `10/25/50/All`). DataTables DOM is
  `rp-dt-top` (length left / search right) + table + `rp-dt-bottom` (info left / pager right).
  Row hover/selected uses `steel-wash` with `text-2b` or `ink` text (never `text-muted` for
  primary cells).
- **Buttons:** `rp-btn-primary` is orange glass (`orange-hover-start → orange →
  orange-hover-end`), **`navy-deep` text**, **1px `orange-deep` rim at 50% alpha** (gives a
  5.2:1 boundary against white cards), hover lifts 1px, pressed uses `orange-deep` fill with
  white text. Depth is a neutral navy seat shadow (`--shadow-btn`), never a colored glow.
  `rp-btn-secondary` white with `form-border` rim. `rp-btn-edit/delete` quiet
  outline with `form-border`. `rp-btn-danger` uses `error` fill with white text.
  Disabled buttons use `text-disabled` and are not the only cue (also `aria-disabled`).
- **Status pills:** `.rp-pill` + kind (`success|warning|danger|info|violet|neutral`).
  Anatomy: icon + label, always both. Text color and background:
  success `success` on `success-wash` (border `success-border`), warning `warning-text` on
  `warning-wash` (border `warning-border`), danger `error` on `error-wash` (border
  `error-border`), info `steel-deep` on `info-wash` (border `info-border`), violet `violet`
  on `violet-wash` (border `violet-border`), neutral `text-2b` on `neutral-wash` (border
  `neutral-border`).
  Icons: success `fa-circle-check`, warning `fa-triangle-exclamation`, danger
  `fa-circle-xmark`, info `fa-circle-info`, neutral `fa-circle-minus`.
  Built by `RiskPulse.pill()/statusPill()`.
- **RAG indicators:** `.rp-level-dot` uses `success` / `warning` / `error-bright` /
  `neutral`. A dot is never alone: it is always followed by text ("Low", "Medium", "High")
  or carries an `aria-label` plus a shape (● low, ▲ medium, ■ high) in dense grids and
  exports (PDF, print, grayscale).
- **Forms:** `.rp-label` + `.rp-input` (12px radius, **`form-border`** boundary,
  `focus` 2px ring with 2px offset, placeholder `text-muted`). Errors: `.rp-input-error`
  uses `error` border, an error icon and message text below the field linked via
  `aria-describedby` (managed by `RiskPulse.validationError` / `clearFieldErrors`).
  Selects are select2 with the `rp` theme.
- **Focus:** `:focus-visible` ring 2px `focus` on light, `focus-on-dark` on dark chrome.
  Never `outline: none` without a replacement.
- **Wizard:** `.rp-stepper` (dots + connectors) and `.rp-wizard-panel`; completed, current
  and upcoming steps differ by icon/number and label weight, not color alone. Picker cards
  `.rp-picker`/`.rp-pick-*`, preview modal `.rp-preview-*`.
- **Modal:** Bootstrap 5, gradient header, soft shadow, footer actions; opened only via
  `RiskPulse.showModal/hideModal`; traps focus, closes on Esc, returns focus to trigger.
  Modal controls use a compact enterprise density (`.modal .rp-input` 36px, tighter
  label/gutter spacing) so multi-field forms stay short. The SAQ/KRI designer
  (`modal-xl` + `.rp-design-split`) is pinned to the viewport height — its left question
  list and right editor scroll internally, so the page never scrolls.
- **Dialog:** `RiskPulse.toastSuccess/toastError` render centered SweetAlert2 dialogs
  (`.rp-swal-alert`, `.rp-swal-confirm`), announced with `role="alertdialog"`. Buttons use
  SweetAlert2's built-in styling, recolored to the RP palette through Swal's CSS variables
  (`--swal2-confirm-button-*` / `--swal2-cancel-button-*`) - no custom button classes.
- **Confirmation:** `RiskPulse.confirmDelete`, title "Please Confirm", Yes / No.
- **Loading:** `#rp-loading-overlay` (glass card + orange spinner), `role="status"` with
  "Loading" text; spinner honors `prefers-reduced-motion`.
- **Charts:** use `chart-1..6` in order, markers + dashes per series, gridlines `border`,
  axis text `text-2`, direct labels preferred, tooltip `navy-deep` with white text. Offer
  a data table alternative for each chart.

## Standalone Pages
- **Login:** two-column `.rp-login-shell`: ink/steel brand panel (mark, eyebrow, headline,
  feature checklist with `orange-soft` icons) and `.rp-login-panel` card (eyebrow,
  "Welcome back", icon inputs, full-width orange Sign in, secured-by footer). Auth
  failures surface as SweetAlert2 error dialogs.
- **Error / Access Denied:** centered `.rp-panel rp-panel-message` with brand mark,
  `error` / `warning` mark, headline, copy, optional request-ID chip, `Return to Home` +
  `Logout`.

## Motion
- 150-200ms ease-out for hover/focus; no motion longer than 400ms.
- `@media (prefers-reduced-motion: reduce)` disables lifts, rotations and spinners
  (replace spinner with static icon + text).

## Migration (v1 → v2)
| Old | New |
|---|---|
| `--blue`, `--blue-bright`, `--indigo`, `--glass-tint` | `--steel-deep` / `--steel` |
| `--focus-border`, `--info` | `--focus`, `--info` (separate role tokens) |
| `--blue-hover-start/-end` | `--orange-hover-start/-end` (`#e07514`, `#ff9a3d`) |
| `--text-faint` | `--text-disabled` (inactive only) or `--text-muted` (real content) |
| `--text-muted` `#64748b` | `#5b6b82` |
| `--form-border` `#cbd8e8` | `#6b829e` (inputs); `#dbe4f0` stays as `--border` |
| `--success` `#15803d`, `--success-2` | `--success` `#0f766e` |
| `--warning`, `--amber`, `--warning-2` | `--warning` (dot/fill), `--warning-text` |
| `--error-2` | `--error-bright` |
| `--purple` | `--violet` |
| `--info-bg` | `--info-wash` |
| `.rp-hero--navy` | `.rp-hero--ink` (steel glow); gold glow is `.rp-hero--gold` |
| Violet glow `rgba(124,58,237,.34)` | `rgba(109, 70, 184, .34)` |
| Literals `#12132b`, `rgba(91,133,170,…)` | `var(--navy-deep)`, steel token |
| Single chart color `--violet` | `--chart-1..6`, `--seq-1..5` |

## Migration (Executive Slate → Sapphire Glass)
| Old (slate) | New (sapphire) | Note |
|---|---|---|
| `navy-deep/-mid`, `ink-raised` `#182232/#243248/#32445e` | `#0a192f/#112240/#1e3a8a` | deeper sapphire chrome |
| `steel` `#6b829e` | `#38bdf8` | cyan — **dark-chrome only** |
| `steel-deep` `#2f5d8a` | `#0369a1` | **AA retune** (raw `#0284c7` fails as text on light) |
| `steel-wash` `#e9f0f8` | `#e0f2fe` | |
| `focus`/`form-border` `#2f5d8a/#6b829e` | `#0284c7` | sky ring + boundary |
| `focus-on-dark` `#a1b8d6` | `#38bdf8` | |
| `orange-hover-end` `#ff9a3d` | `#ffa048` | |
| `page-bg` `#f4f6f9` | `#f0f4f8` | steel-deep radial glow |
| `text-2/2b/muted`, `border` `#475569/#334155/#5b6b82/#dbe4f0` | `#334155/#1e293b/#475569/#cbd5e1` | |
| `success` `#0f766e` | `#047857` | **AA retune** (raw `#059669` fails as text) |
| `warning` / `warning-text` `#b45309/#854d0e` | `#b45309/#92400e` | warning held (raw `#d97706` fails) |
| `error` `#b91c1c` (kept), `error-bright` `#dc3545`, `error-wash` `#fee2e2` | `#b91c1c/#ef4444/#fef2f2` | `error` held (raw `#dc2626` fails on wash) |
| `info` `#2f5d8a` | `#0369a1` | = `steel-deep` |
| `violet` `#6d46b8` | `#7c3aed` | + violet hero glow |
| `neutral-wash` `#eef2f7` | `#f1f5f9` | |
| `chart-1/3/4/5` | `#0284c7/#059669/#7c3aed/#db2777` | chart-2 held at `#c2410c` (raw `#f58220` fails as mark) |
| `seq-1..5` `#e9f0f8…#182232` | `#e0f2fe…#0a192f` | `seq-3` cell value in large text only |
| `--steel` on-light icons/pattern | `--steel-deep` | cyan fails 3:1 on light |
| `--steel-rgb` `107,130,158` | `56,189,248` | + new `--steel-deep-rgb`, `--border-rgb`; `--navy-deep/-mid-rgb` updated |