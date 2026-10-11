# Error & Access-Denied Pages

Design-system tokens: [`AI/DESIGN.md`](../../DESIGN.md). Standalone shells per
`Specs/layout`. Both views set `Layout = null` and render only `site.css` +
FontAwesome.

## Shared structure (`.rp-shell-centered`)
`body.rp-shell-centered` centers `.rp-panel.rp-panel-message` over `--page-bg` +
`.rp-bg-pattern`:

1. **Brand row** — `.rp-mark-brand` orange gradient shield tile + `rp-headline-md` "Risk Pulse" +
   muted eyebrow line ("System Error" / "Access Control").
2. **Outcome mark** — `.rp-mark-error` (red `fa-triangle-exclamation`) on the Error
   page, `.rp-mark-warning` (gold `fa-lock`) on Access Denied.
3. **Headline + copy** — `rp-display-lg` h1 + `rp-body-lg rp-text-muted` explanation.
4. **Request chip** — `.rp-request-chip` with `RequestId` in `rp-mono`, rendered
   only when `ViewData["RequestId"]` is set.
5. **Actions** — full-width stacked `.rp-btn-primary` "Return to Home" (driven by
   the `DefaultPage` claim via `PermissionPageMapper`) + `.rp-btn-secondary`
   Logout. Error page additionally offers a Send/Report link and "Reference" hint.

## Rules
- Grayscale/brand only — no form controls, no tables, no navigation shell.
- Copy stays calm and actionable; never leak stack traces to users (handled by
  `UseExceptionHandler` → `/Error/Index`).