# Login — Two-Column Standalone Page

Single source of truth for tokens/typography: [`AI/DESIGN.md`](../../DESIGN.md).
This spec covers `Views/Login/Index.cshtml` only.

## Shell
`Layout = null`; `body.rp-login-page` loads only `site.css` + FontAwesome (+ jQuery,
SweetAlert2, `riskpulse.js` for the form — **no Bootstrap bundle**, so never call the
modal helpers here).

- `.rp-login-ambient` — fixed breathing glow behind everything.
- `.rp-login-shell` — two-column grid (brand | form), vertically/horizontally
  centered, generous gap. **≤991.98px it collapses to a single centered column**
  (max-width 460px), the feature list `.rp-login-features` is hidden and the brand
  panel centers its text.

## Left — Brand Panel (`.rp-login-brand-panel`)
Dark **sapphire glass** panel (`.rp-glass-dark` recipe: `rgba(255,255,255,.14→.04)`
gradient + `rgba(255,255,255,.18)` rim + `blur(14px)`, radius 20px) over the
`.rp-login-page` cool gradient (`--navy-deep #0a192f → --navy-mid #112240` + sapphire-cyan/orange glow):

- `.rp-login-brandmark` — orange-gradient glass tile with shield icon (ink glyph) and a
  looping orange `.rp-login-sonar` ring.
- `.rp-login-eyebrow.rp-login-eyebrow-light` "Risk Management Platform".
- `<h1>` Risk Pulse + `.rp-login-brand-copy` description.
- `.rp-login-features` checklist (3 items, `fa-circle-check`).

## Right — Form Panel (`.rp-login-panel`)
White rounded card (`.rp-panel` language, `--shadow-card`):

- `.rp-login-header`: eyebrow "Secure Access", `<h1>` "Welcome back", subtitle.
- `#loginForm` (`rp-login-form`, `novalidate`), two `.rp-login-field` blocks —
  `.rp-label` + `.rp-login-input-wrapper` (`.rp-login-input-icon` FA icon +
  `#username` / `#password` with class `rp-input rp-login-input`).
- `#loginBtn` — `rp-btn rp-btn-primary rp-login-submit`, label in `.btn-text`.
- `.rp-login-footer` — lock icon + "Authorized personnel only."

## Behavior (inline script, three SECTION banners)
1. **Validation** — `validateLoginForm()`: username required + lowercase-letters
   regex, password required; failures via `RiskPulse.validationError($el, msg)`
   (error dialog + red `.rp-input-error` highlight + focus).
2. **Transport** — `RiskPulse.postJson(Url.Action("Login","Login"), {username,password},
   handlers, $btn)`. The view **swaps only the `.btn-text` label** to a spinner;
   it must never pre-disable the button (the module owns the flight lock) —
   `handlers.complete` restores the label. Success redirects to
   `res.data.redirectUrl`; failure shows `RiskPulse.toastError(res.message)`.
3. **Wiring** — form `submit` handler prevents default and runs 1 → 2.

## Related
- Auth/transport failures anywhere surface as **centered SweetAlert2 error
  dialogs** (`RiskPulse.toastError` — legacy name; no toast pills).
- `Login/AccessDenied` and `Error/Index` keep the centered `.rp-shell-centered`
  treatment (see `Specs/error-page`).
