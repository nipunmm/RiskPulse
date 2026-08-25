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
- **Surface & Backgrounds:** The main canvas is pure white (#FFFFFF) to maximize contrast for data readability. Subtle slate gray (#F8FAFC) is used exclusively for structural panels, sidebars, and grouping containers.
- **RAG System:** Functional colors (Red, Amber, Green) are strictly reserved for risk thresholds and status indicators. These colors must maintain high accessibility standards against white and light gray backgrounds.
- **Typography Colors:** Use a near-black (#0F172A) for headings and a slate gray (#475569) for body text to create a clear visual hierarchy.
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

- **Tonal Layering:** Depth is primarily communicated through color. The base background is white, while interactive components or sidebars sit on surface containers (#F2F4F6).
- **Outlines:** Instead of shadows, use 1px solid borders (#E2E8F0) to define sections. 
- **Active Elevation:** A very subtle, high-diffusion shadow (0px 4px 12px rgba(0, 0, 0, 0.05)) is reserved only for temporary floating elements like dropdown menus, tooltips, or modal dialogs. This keeps the main interface feeling flat and structurally sound.

## Shapes
The shape language is conservative and geometric. 

- **Corner Radius:** 4px (Soft) for buttons and input fields. 0.5rem (8px) for cards and panels.
- **Icons:** Use 2px stroke weight icons with slightly rounded caps to match the component radius.
- **Status Pills:** Status badges use a 100px (full pill) radius to clearly differentiate them from interactive buttons.

## Components
- **Data Tables:** The core of the system. Use a "Zebra" row style only on hover. Headers must be "sticky" with a 1px bottom border. Numerical columns should use the `data-mono` font and be right-aligned.
- **Buttons:** 
  - *Primary:* Solid #1B365D with white text.
  - *Secondary:* Ghost style with #1B365D border and text.
  - *Destructive:* Solid #DC2626 for irreversible risk actions.
- **Risk Indicators (RAG):** Small circular indicators or full-width subtle background tints (10% opacity) within table cells.
- **Analytical Widgets:** White cards with a 1px border. Charts should use a refined color palette that avoids clashing with RAG status colors (use blues and grays for non-risk data).
- **Input Fields:** Use "Internal Labels" where the label sits above the field. Inputs have a 1px #CBD5E1 border, which turns #0f766e (teal) on focus with a 2px outer "halo" of `rgba(15, 118, 110, 0.14)`. Error state: `#dc2626` border + red halo.
- **Standalone Pages (Login / Access Denied / Error):** Centered `.rp-shell-centered` on a soft cool gradient (#F0FDFA → #F7F9FB → #EFF6FF) with a faint teal/navy dot pattern (`.rp-bg-pattern`). The `.rp-panel` card gets a 3px teal top stripe; brand/mark icons use `accent-strong`. Access Denied uses a gold warning mark; the Error page keeps the red error mark. Login loads `jquery + sweetalert2 + riskpulse.js` (no Bootstrap bundle — don't call modal helpers there).
- **Login Transport:** Uses `RiskPulse.postJson()` for AJAX and `RiskPulse.toastError()` for error display. The button label is swapped via `handlers.complete` — the view must NOT manually disable `$trigger` before calling `postJson` (the module owns the flight lock).
