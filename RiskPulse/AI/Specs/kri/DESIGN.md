# KRI — Feature Design Notes

Design-system tokens/components: [`AI/DESIGN.md`](../../DESIGN.md). This spec only
carries KRI-specific deltas.

## Module specifics
- **Hero:** `rp-hero--violet` (KRI is violet in the per-module hero map — ink base with
  a violet corner glow).
- **Designer:** KRI templates use the shared template-designer card list + read-only
  preview modal pattern (same as SAQ). Items carry flat `GreenLimit`/`AmberLimit`/
  `RedLimit` ints; RAG is computed in code, never persisted.
- **RAG display:** threshold values are shown with `tabular-nums`; the current level
  uses `RiskPulse.pill()` with the RAG kind. `.rp-level-dot` is the circular RAG
  indicator for small cells.
- **Grid:** `RiskPulse.initGrid` (paginated — `pageLength:10`, size selector,
  stock info line), columns keyed to `KriGridRowViewModel` JSON exactly.
- **Entry screen** (`Views/KriEntry.cshtml`): violet hero + per-question RAG states;
  answers are saved via the shared `postJson` transport.

## Conventions carried forward
- Tables wrapped in `.table-responsive` inside `.rp-card`.
- Delete confirms via `RiskPulse.confirmDelete` (title "Please Confirm", Yes/No).
- Status shows (any) as pills via `RiskPulse.statusPill`.