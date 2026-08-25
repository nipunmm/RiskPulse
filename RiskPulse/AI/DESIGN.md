---
name: Stasis Enterprise
colors:
  surface: '#f7f9fb'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#44474e'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#74777f'
  outline-variant: '#e0e3e5'
  primary: '#002046'
  on-primary: '#ffffff'
  primary-container: '#1b365d'
  on-primary-container: '#87a0cd'
  inverse-primary: '#aec7f7'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  accent: '#0f766e'
  accent-strong: '#115e59'
  accent-container: '#ccfbf1'
  on-accent-container: '#134e4a'
  accent-soft: '#f0fdfa'
  accent-border: '#14b8a6'
  accent-connector: '#0d9488'
  accent-ring: 'rgba(15, 118, 110, 0.14)'
  accent-on-dark: '#5eead4'
  gold: '#b45309'
  gold-container: '#fef3c7'
  on-gold-container: '#78350f'
  gold-border: '#fcd34d'
  success: '#15803d'
  success-container: '#dcfce7'
  on-success-container: '#14532d'
  success-border: '#86efac'
  danger: '#dc2626'
  danger-hover: '#b91c1c'
  danger-container: '#fee2e2'
  on-danger-container: '#991b1b'
  neutral-container: '#f1f5f9'
  on-neutral-container: '#475569'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 24px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
  data-mono:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '450'
    lineHeight: 18px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  base: 4px
  xs: 8px
  sm: 16px
  md: 24px
  lg: 32px
  xl: 48px
  container-max: 1440px
  gutter: 20px
---

## Brand & Style
The design system is engineered for high-stakes financial environments where clarity, precision, and perceived security are paramount. The brand personality is institutional yet modern—avoiding unnecessary decorative flourishes in favor of functional elegance. 

The aesthetic follows a **refined Corporate Minimalism** approach. It utilizes a structured grid and a high-contrast interface to reduce cognitive load during complex risk assessment tasks. The emotional goal is to evoke a sense of "calm authority," ensuring users feel in control of volatile data through a rigorous and predictable UI language.

## Colors
The palette is rooted in a deep corporate blue to establish trust and stability. A teal accent family harmonizes with the navy (blue+green) and supplies the modern interaction layer without becoming "more blue."

- **Primary:** Used for navigational headers, primary actions, and branding elements.
- **Accent (Teal):** Reserved for modify/in-progress interactivity — Edit buttons, input/checkbox focus rings, wizard active step, grid header underline, active pagination, active tab underline, sidebar active icon, modal top stripe, selected designer items. It never appears on status pills or RAG cells.
- **Semantic Accents:** Gold (warning/pending/locked), Green (success/active), Red (destructive only), Slate (inactive/neutral).
- **Surface & Backgrounds:** The main canvas is `#f7f9fb` for data readability. Surface containers (`#f2f4f6` → `#e0e3e5`) are used for structural panels, sidebars, and grouping containers.
- **RAG System:** Functional colors (Red, Amber, Green) are strictly reserved for risk thresholds and status indicators. These colors must maintain high accessibility standards against white and light gray backgrounds.
- **Typography Colors:** Use `#191c1e` (on-surface) for headings and `#44474e` (on-surface-variant) for body text to create a clear visual hierarchy.
- **Contrast baseline:** text-on-tint ≥ 7:1, text-on-white ≥ 5:1, non-text boundaries ≥ 3:1.

## Typography
This design system prioritizes legibility in data-dense environments. 

- **Primary Typeface:** Inter is utilized for its exceptional clarity and neutral tone. It handles complex interface labels and long-form reporting with ease.
- **Secondary Typeface:** JetBrains Mono is introduced specifically for numerical data, financial figures, and IDs within tables to ensure character alignment and prevent reading errors.
- **Hierarchy:** Large display sizes are kept tight with negative letter-spacing for a modern feel. Labels use uppercase styling with increased weight to distinguish them from interactive data.

## Layout & Spacing
The layout employs a **fixed-fluid hybrid grid**. On desktop, the main content is constrained to a 1440px container to prevent line lengths from becoming unreadable on ultra-wide monitors.

- **Rhythm:** A 4px baseline grid ensures vertical consistency.
- **Margins:** Sidebars and utility panels use 24px (md) internal padding. 
- **Density:** The system supports "Standard" and "Compact" views. Compact view reduces vertical padding in tables from 16px to 8px for expert users handling large datasets.
- **Reflow:** For tablet viewports, the secondary sidebar collapses into an icon-only rail, and data tables transition to a horizontal scroll or card-stacking model depending on column count.

## Elevation & Depth
In alignment with the minimalist professional style, this design system avoids heavy shadows. 

- **Tonal Layering:** Depth is primarily communicated through color. The base background is `#f7f9fb`, while interactive components or sidebars sit on surface containers (`#f2f4f6` → `#eceef0`).
- **Outlines:** Instead of shadows, use 1px solid borders (`#e0e3e5`) to define sections. 
- **Cards:** `border-radius: 0.5rem` + soft elevation (`0 1px 2px rgba(15,23,42,0.03), 0 4px 16px -8px rgba(15,23,42,0.06)`).
- **Active Elevation:** A very subtle, high-diffusion shadow is reserved only for temporary floating elements like dropdown menus, tooltips, or modal dialogs (`0 8px 24px rgba(0, 0, 0, 0.08)`).

## Shapes
The shape language is conservative and geometric. 

- **Corner Radius:** 4px (Soft) for buttons and input fields. 0.5rem (8px) for cards and modals. This softens the "industrial" feel of the interface without appearing informal or consumer-grade.
- **Icons:** Use 2px stroke weight icons with slightly rounded caps to match the component radius.
- **Status Pills:** Status badges use a 100px (full pill) radius to clearly differentiate them from interactive buttons.

## Components
- **Data Tables:** The core of the system. Use a "Zebra" row style only on hover (`--accent-soft`). Headers must be "sticky" with a 2px bottom border (`--accent-border`). Numerical columns should use the `data-mono` font and be right-aligned.
- **Buttons:** 
  - *Primary:* Solid `#1b365d` with white text.
  - *Secondary:* Ghost style with `#1b365d` border and text.
  - *Edit:* Quiet ghost — `accent-strong` text, `accent` border, transparent fill; hover → `accent-container` bg + `accent-strong` border + 1px lift.
  - *Delete:* Quiet ghost — `on-danger-container` text, `--danger` border, transparent fill; hover → `danger-container` bg + `danger-hover` border + 1px lift.
  - *Destructive:* Solid `#dc2626` for irreversible risk actions.
- **Status Pills:** `rp-pill` with kind variants — `success` (green), `warning` (gold), `danger` (red), `neutral` (slate). Built by `RiskPulse.pill(label, kind)` / `RiskPulse.statusPill(status)`.
- **Risk Indicators (RAG):** Small circular indicators or full-width subtle background tints (10% opacity) within table cells.
- **Analytical Widgets:** White cards with a 1px border. Charts should use a refined color palette that avoids clashing with RAG status colors (use blues and grays for non-risk data).
- **Input Fields:** Use "Internal Labels" where the label sits above the field (`rp-label`). Inputs have a 1px `#cbd5e1` border, which turns `#0f766e` (teal) on focus with a 2px outer "halo" of `rgba(15, 118, 110, 0.14)`. Error state: `#dc2626` border + red halo.
- **Modals:** Bootstrap 5 vendored bundle. Top stripe `3px solid var(--accent)`. Open/close via `RiskPulse.showModal(id)` / `RiskPulse.hideModal(formEl)` — never `$.fn.modal()`.
- **User Profile (Topbar):** Gradient avatar circle (`primary-container → accent`), username from `User.Identity?.Name`, role from `ClaimTypes.Role`, logout link with danger-red hover. Hidden on mobile (`< 768px`).
- **Sidebar Toggle:** `fa-chevron-right` icon, rotates 90° (→ down) on expand via `.expanded .rp-sidebar-caret`.
- **Standalone Pages (Login / Access Denied / Error):** Centered `.rp-shell-centered` on a soft cool gradient (`#f0fdfa → #f7f9fb → #eff6ff`) with a faint teal/navy dot pattern (`.rp-bg-pattern`). The `.rp-panel` card gets a 3px teal top stripe; brand/mark icons use `accent-strong`. Access Denied uses a gold warning mark; the Error page keeps the red error mark.
