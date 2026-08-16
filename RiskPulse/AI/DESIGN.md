---
name: Stasis Enterprise
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
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
  outline-variant: '#c4c6cf'
  surface-tint: '#465f88'
  primary: '#002046'
  on-primary: '#ffffff'
  primary-container: '#1b365d'
  on-primary-container: '#87a0cd'
  inverse-primary: '#aec7f7'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#182033'
  on-tertiary: '#ffffff'
  tertiary-container: '#2d354a'
  on-tertiary-container: '#969eb7'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d6e3ff'
  primary-fixed-dim: '#aec7f7'
  on-primary-fixed: '#001b3d'
  on-primary-fixed-variant: '#2e476f'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#dae2fd'
  tertiary-fixed-dim: '#bec6e0'
  on-tertiary-fixed: '#131b2e'
  on-tertiary-fixed-variant: '#3f465c'
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
The palette is rooted in a deep corporate blue to establish trust and stability. 

- **Primary:** Used for navigational headers, primary actions, and branding elements.
- **Surface & Backgrounds:** The main canvas is pure white (#FFFFFF) to maximize contrast for data readability. Subtle slate gray (#F8FAFC) is used exclusively for structural panels, sidebars, and grouping containers.
- **RAG System:** Functional colors (Red, Amber, Green) are strictly reserved for risk thresholds and status indicators. These colors must maintain high accessibility standards against white and light gray backgrounds.
- **Typography Colors:** Use a near-black (#0F172A) for headings and a slate gray (#475569) for body text to create a clear visual hierarchy.

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

- **Tonal Layering:** Depth is primarily communicated through color. The base background is white, while interactive components or sidebars sit on $neutral-color-hex (#F8FAFC).
- **Outlines:** Instead of shadows, use 1px solid borders (#E2E8F0) to define sections. 
- **Active Elevation:** A very subtle, high-diffusion shadow (0px 4px 12px rgba(0, 0, 0, 0.05)) is reserved only for temporary floating elements like dropdown menus, tooltips, or modal dialogs. This keeps the main interface feeling flat and structurally sound.

## Shapes
The shape language is conservative and geometric. 

- **Corner Radius:** A consistent 4px (Soft) radius is applied to buttons, input fields, and cards. This softens the "industrial" feel of the interface without appearing informal or consumer-grade.
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
- **Input Fields:** Use "Internal Labels" where the label sits above the field. Inputs have a 1px #CBD5E1 border, which turns #1B365D on focus with a 2px outer "halo" of 10% opacity primary color.
- **Search:** A prominent, global search bar in the header with a `/` keyboard shortcut hint.