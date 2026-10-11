# Branch — Feature Design Notes

No dedicated UI exists for this feature yet; the module is a stub
(`RiskRegisterTemplates` is the only page-backed stub today). This spec tracks the
planned direction and does not describe shipped markup.

## Standing notes
- If/when implemented, follow the standard CRUD recipe (hero banner per the module
  hero map, `.rp-card` + `.table-responsive` grid via `RiskPulse.initGrid`,
  `.rp-hero-actions` primary action, `confirmDelete`, status pills).
- Design-system tokens/components: [`AI/DESIGN.md`](../../DESIGN.md).