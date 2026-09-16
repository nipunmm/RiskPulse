# Risk Reporting Excel Formats — Explanation for AI Reference

This document explains the structure, purpose, and data rules of every Excel report template
currently used in the manual risk reporting process described in the BRD. It covers three
template families: **KRI** (Key Risk Indicators), **SAQ** (Self-Assessment Questionnaire), and
**Risk Register**. Each family is a fixed layout with a header/metadata block and a data table.
The goal is to let an AI model understand exactly what each cell means, what is fixed reference
data vs. user input, and how the templates relate to each other, so it can design database
schemas, forms, and validation logic from them.

---

## Part A — KRI and SAQ Templates

### Common Pattern (KRI & SAQ)

1. **Row 1 (header/classification):** `Confidential - Internal` label, plus the report title (e.g. "Key Risk Indicators (KRIs) - 2025" or "Self-Assessment Questions (SAQs) - 2025").
2. **Metadata rows:**
   - `Month:` — reporting month (samples show "January").
   - `Branch:` or `Department:` — a dropdown-driven field where the submitter selects their branch or department from a predefined list.
   - `Prepared By:` — free-text name of the person who filled in the form (Maker).
   - `Reviewed By:` — free-text name of the person who reviewed/approved the form (Checker).
3. **Blank spacer row.**
4. **Table header row(s):** column titles, sometimes split across two rows (e.g. a "Thresholds" column expanding into Green/Amber/Red sub-columns).
5. **Data rows:** one row per indicator/question, each with a fixed `Ref No` code (e.g. `KRI B1`, `SAQ D3`) and fixed descriptive text. Only specific columns are user input (KRI Value / SAQ Answer, and Comments).
6. **A second sheet (`Sheet1`)** — hidden from normal use — containing the full list of branch or department names, feeding the dropdown/data-validation list on the `Branch:`/`Department:` cell.

**Design implication:** the `Ref No` + description pairs are a fixed master list of indicators/questions per report type and submitter type (Branch / Department / HR-Department). Store these as reference/lookup data; each monthly submission is a set of (Ref No, Value, Comment) records tied to a Branch/Department, Month, Preparer, and Reviewer.

### 1. Branch KRI (`Branch_KRI - Jan 2025.xlsx`)

**Purpose:** Monthly KRI submission for a **branch**.

Metadata: Month, Branch (dropdown), Prepared By, Reviewed By.
Table columns: `Ref No`, `Key Risk Indicators (KRIs)` (description), `KRI Value` (input, numeric), `Comments` (mandatory), `Thresholds` → `Green` / `Amber` / `Red` (fixed rating bands).

Indicators (`KRI B1`–`KRI B12`):
1. Number of operational risk incidents during the month (internal processes, people, systems, or external events). Thresholds: Green 0, Amber 0, Red 1+.
2. Number of near-miss incidents during the month. Thresholds: Green 0, Amber 1, Red 2+.
3. Number of vacant positions not replenished at month end. Thresholds: Green 0, Amber 1, Red 2+.
4. Number of compliance breaches identified during the month. Thresholds: Green 0, Amber 0, Red 1+.
5. Number of instances of cash shortages or excess during the month. Thresholds: Green 0, Amber 1, Red 2+.
6. Number of customer complaints per month. Thresholds: Green 0, Amber 1, Red 2+.
7. Number of instances of fake pawn articles detected. Thresholds: Green 0, Amber 1, Red 2+.
8. Number of instances of unplanned system downtime per month (help desk IDs must be provided). Thresholds: Green 0, Amber 1, Red 2+.
9. Number of instances of security guards reporting delays during the month. Thresholds: Green 0, Amber 1, Red 2+.
10. Number of data entry errors. Thresholds: Green 0, Amber 1, Red 2+.
11. Number of spam emails detected during the month (help desk IDs must be provided). Thresholds: Green 0, Amber 1, Red 2+.
12. Number of leasing facilities incepted with documents pending for more than 3 days. Thresholds: Green 0–4, Amber 5, Red 6+.

`Sheet1`: 52 branch names (Akkaraipattu … Deniyaya), placeholder "Select the branch from the list".

### 2. Branch SAQ (`Branch_SAQ - Jan 2025.xlsx`)

**Purpose:** Monthly control-compliance checklist for a **branch**.

Metadata: same as Branch KRI.
Table columns: `Ref No`, `Self-Assessment Questions (SAQs)`, `SAQ Answer` (input — Yes/No style), `Comments` (mandatory).

Questions (`SAQ B1`–`SAQ B11`):
1. Is a dual key control implemented for all safes and vaults within the branch?
2. Has temporary insurance cover been obtained for cash-in-transit and cash in the safe when limits under the regular insurance cover are exceeded?
3. Have there been occasions where security personnel were absent without a replacement at their post?
4. Was the CCTV system fully functioning throughout the month without any breakdowns?
5. Is there a monthly check to ensure the CCTV system, generator, alarm system, and cash counting machine are in good working condition?
6. Was monthly physical verification conducted for pawned articles?
7. Are all fire extinguishers currently in the branch not expired?
8. Have the cashiering guidelines issued by the Finance department been complied with?
9. Is there a business continuity plan (BCP) for the branch?
10. Are all registers currently up-to-date?
11. Has the branch conducted spot cash verifications during the month?

`Sheet1`: same 52-branch reference list.

### 3. Department KRI (`Department_KRI - Jan 2025.xlsx`)

**Purpose:** Monthly KRI submission for a **department** (generic, non-HR).

Metadata: Month, Department (dropdown), Prepared By, Reviewed By.
Table columns: same structure as Branch KRI.

Indicators (`KRI D1`–`KRI D6`):
1. Number of operational risk incidents during the month. Thresholds: Green 0, Amber 1, Red 2+.
2. Number of near-miss incidents during the month. Thresholds: Green 0, Amber 1, Red 2+.
3. Number of vacant positions not replenished within 60 days. Thresholds: Green 0, Amber 1, Red 2+.
4. Number of instances of unplanned system downtime per month (help desk IDs must be provided). Thresholds: Green 0, Amber 1, Red 2+.
5. Number of data entry errors. Thresholds: Green 0, Amber 1, Red 2+.
6. Number of spam emails detected during the month (help desk IDs must be provided). Thresholds: Green 0, Amber 1, Red 2+.

`Sheet1`: 24 department names (Administration, Central processing, Compliance, Credit operations, Credit risk, Customer relations, Factoring, Finance, Fixed deposits, Gold finance, HR, Information security and compliance, Insurance, Internal audit, IT, Legal, MIS, Marketing communication, Marketing, Operations, Process improvement, Recoveries, Treasury, Sustainability), placeholder "Select the department from the list".

### 4. Department SAQ (`Department_SAQ - Jan 2025.xlsx`)

**Purpose:** Monthly control-compliance checklist for a **department**.

Metadata: same as Department KRI.
Table columns: `Ref no`, `Self-Assessment Questions (SAQs)`, `SAQ Answer` (input), `Comments`.

Questions (`SAQ D1`–`SAQ D6`):
1. Is there a business continuity plan (BCP) for the department?
2. Do department members always comply with the organization's IT security policies and procedures?
3. Is there any delay in the submission of management reports?
4. Are there any unresolved IT issues that have been open for more than 30 days?
5. Are there any findings from Internal Audit, Regulatory Audit, or External Audit that have remained unresolved for more than two weeks?
6. Are there any departmental policies and procedures set to expire within the next 30 days?

`Sheet1`: same 24-department reference list.

### 5. KRI — HR (`KRI_HR - Jan 2025.xlsx`)

**Purpose:** Monthly KRI submission for the **HR department** — an extended Department KRI with two HR-only, organization-wide indicators. `Department:` is pre-filled "HR".

Indicators (`KRI D1`–`KRI D8`):
1. Number of operational risk incidents during the month. Thresholds: Green 0, Amber 1, Red 2+.
2. Number of near-miss incidents during the month. Thresholds: Green 0, Amber 1, Red 2+.
3. Number of vacant positions not replenished within 60 days (**only in HR department**). Thresholds: Green 0, Amber 1, Red 2+.
4. Number of instances of unplanned system downtime per month (help desk IDs must be provided). Thresholds: Green 0, Amber 1, Red 2+.
5. Number of data entry errors. Thresholds: Green 0, Amber 1, Red 2+.
6. Number of spam emails detected during the month (help desk IDs must be provided). Thresholds: Green 0, Amber 1, Red 2+.
7. **Number of vacant positions in all branches not replenished within 60 days.** (HR-only, org-wide.) Thresholds: Green 0, Amber 1, Red 2+.
8. **Number of vacant positions in all departments not replenished within 60 days.** (HR-only, org-wide.) Thresholds: Green 0, Amber 1, Red 2+.

`Sheet1`: 23 department names (same list, minus "Sustainability" in this file).

### KRI/SAQ Summary Table

| File | Report Type | Submitter Level | Ref No Prefix | # Items | Location Field |
|---|---|---|---|---|---|
| Branch_KRI | KRI | Branch | `KRI B#` | 12 | Branch (dropdown, 52 branches) |
| Branch_SAQ | SAQ | Branch | `SAQ B#` | 11 | Branch (dropdown, 52 branches) |
| Department_KRI | KRI | Department (generic) | `KRI D#` | 6 | Department (dropdown, 24 depts) |
| Department_SAQ | SAQ | Department (generic) | `SAQ D#` | 6 | Department (dropdown, 24 depts) |
| KRI_HR | KRI | Department (HR-specific) | `KRI D#` | 8 (incl. 2 HR-only, org-wide) | Department (pre-filled "HR") |

---

## Part B — Risk Register Template

Five sample files were provided, one per department/area: **Admin**, **Audit**, **BCP**
(Process Improvement Department), **BID**, and **Branding**. Unlike the KRI/SAQ files, these
are not five different formats — they are five **filled-in instances of one single Risk
Register template**, each populated with only the risk items relevant to that department. One
sample (`Admin`) was supplied as a legacy `.xls` file; the rest are `.xlsx`. All five otherwise
follow an identical structure.

### Risk Register Template Definition

**Header block (rows 1–3):**
- Row 1: `Confidential - Internal` label.
- Row 2: Title — `Risk Register - <Month> <Year>` (e.g. "Risk Register - January 2025").
- Row 3: The department/area name as a plain title (e.g. "Admin", "Internal Audit", "Process Improvement Department (BCP)", "BID", "Branding Department"). Unlike the KRI/SAQ templates, this is **not** a labeled field (no "Department:" prefix) or a dropdown — it is fixed text identifying which department's register the sheet holds. There are also no separate "Prepared By" / "Reviewed By" fields on this template.
- Blank spacer row.

**Table header (row 5, some columns span two header rows via merge):**

| Column | Meaning |
|---|---|
| `No.` | Fixed reference number of the **Main Risk Category** from an organization-wide risk taxonomy (not sequential within the sheet — e.g. 1 = Credit Risk, 2 = Strategic Risk, 3 = Operational Risk - Internal Processes, 11 = Compliance Risk, based on observed samples; other numbers exist for categories not present in these five samples). |
| `Main Risk category` | Name of the main risk category (fixed reference text, matched to `No.`). |
| `Sub category` | A finer-grained risk sub-category under the main category (fixed reference text). |
| `Risk trigger/Event` | The specific, granular risk item/metric being tracked (fixed reference text) — this is the actual line item a department reports against. |
| `Risk Appetite` | The fixed tolerance/limit set for that risk trigger (a number, ratio, percentage, or descriptive limit, e.g. `0.04`, `15% of capital base`, `3 locations`, `1`). This is reference data, not user input. |
| `Current position/Actual as at <date>` | **User input** — the actual measured value for that risk trigger as of month end. |
| `Remarks/Control activity with timing` | **User input, mandatory** — free-text explanation of control activity or commentary for that risk trigger. |

**Merging/grouping behaviour observed in the source files:** `No.` and `Main Risk category`
cells are merged vertically across every row that shares the same main category; `Sub category`
cells are merged across every row that shares the same sub-category. In most cases `Risk
Appetite`, `Current position`, and `Remarks` are entered per individual `Risk trigger/Event`
row, but in a few groups (e.g. a whole "Operational Risk - Internal Processes → Execution,
delivery and process management" block) the source spreadsheet merges `Current position` and
`Remarks` across multiple trigger rows as a formatting shortcut. **For system design, treat
each `Risk trigger/Event` as its own record with its own `Current Position` and `Remarks`
fields** — the merged cells in the source files are a spreadsheet convenience, not a business
rule requiring shared values.

### Master Risk Category / Sub-category / Trigger Data (as observed across the 5 samples)

| No. | Main Risk Category | Sub Category | Risk Trigger/Event | Risk Appetite | Seen in |
|---|---|---|---|---|---|
| 1 | Credit Risk | Default | Net NPL | 0.04 | BID |
| 1 | Credit Risk | Default | Gross NPL | 0.075 | BID |
| 1 | Credit Risk | Default | Leasing 90DPD NPL | 0.1 | BID |
| 1 | Credit Risk | Default | Loans 90DPD NPL | 0.12 | BID |
| 1 | Credit Risk | Default | Factoring 90DPD NPL | 0.12 | BID |
| 1 | Credit Risk | Concentration | Single Borrower | 15% of capital base | BID |
| 1 | Credit Risk | Concentration | Group Borrower | 20% of capital base | BID |
| 1 | Credit Risk | Concentration | Fast draft facility concentration | 0.125 | BID |
| 2 | Strategic Risk | Branding | Number of branding activities carried out during the month | 7 | Branding |
| 3 | Operational Risk - Internal Processes | Internal Fraud | Theft of company's physical assets | 0 | Admin |
| 3 | Operational Risk - Internal Processes | Internal control | Maintain a business continuity plan (BCP) o/a company-wide and for branches as per CBSL Direction No. 06 of 2020 | 1 | Admin |
| 3 | Operational Risk - Internal Processes | Internal control | Failure to establish and review internal controls in departments/branches with high or medium-high audit rating | 2 | Audit |
| 3 | Operational Risk - Internal Processes | Internal control | Updates on Audit Ratings of branches (tracking item, no numeric appetite) | — | Audit |
| 3 | Operational Risk - Internal Processes | External fraud | Robbery/theft | 0 | Admin |
| 3 | Operational Risk - Internal Processes | Execution, delivery and process management | Space constraints in the department | 3 locations | Admin, Audit, BCP, BID, Branding (appears in every sample — a common, org-wide risk trigger) |
| 3 | Operational Risk - Internal Processes | Execution, delivery and process management | Expired outsource agreements | 1 | Admin |
| 11 | Compliance Risk | Failure to comply with regulatory/statutory requirements | Maintain a business continuity plan (BCP) as per CBSL Direction No. 06 of 2020 | 1 | BCP |

**Observation:** the "Space constraints in the department" trigger (Main Category 3 →
Execution, delivery and process management) and the "maintain a BCP as per CBSL Direction No.
06 of 2020" trigger both recur across multiple departments — confirming that some risk triggers
are **shared/global** (assigned to every department) while others are **department-specific**
(e.g. NPL ratios only apply to BID; branding activity count only applies to Branding). The
system should support a many-to-many assignment of risk triggers to departments/areas, not a
fixed one-set-fits-all list.

### Special Case: Audit Register — Secondary Appendix Table

The `Audit` sample includes an additional table beneath the main risk grid, not present in the
other four samples:

| Department/Branch | Audit Rating | Received Date |
|---|---|---|

This is a supporting log specific to the Internal Audit register, used to record the audit
rating received per department/branch and the date it was received — feeding into the "Internal
control → Failure to establish and review internal controls…" risk trigger above. This should
be modeled as its own child table (one row per Department/Branch per audit cycle), linked to the
Audit risk register, rather than as part of the generic Risk Register template.

### File Naming / Workbook Convention

- One workbook per department/area per month: `Risk Register - <Department/Area> - <Month> <Year>.xlsx`.
- The single sheet in each workbook is named with a short code for that department/area (`Admin`, `Audit`, `BCP`, `BID`, `Branding`).
- One sample (`Admin`) was supplied in the legacy binary `.xls` format; the new system should standardize on `.xlsx` (or database-native storage) rather than carrying legacy binary spreadsheet formats forward.

### Risk Register — Data Model Implications

- **Master risk taxonomy table:** `No.` (Main Risk Category ID) → `Main Risk category` name — organization-wide, fixed.
- **Sub-category table:** linked to a Main Risk Category.
- **Risk trigger/event table:** linked to a Sub-category, carrying the fixed `Risk Appetite` value/description. Each trigger can be flagged as **global** (applies to all departments) or **department-specific** (applies only to named departments/areas) — a many-to-many mapping table between triggers and departments/areas is needed.
- **Monthly submission table:** one row per (Risk Trigger, Department/Area, Month/Year), holding the user-entered `Current Position/Actual` and mandatory `Remarks/Control activity`.
- **Audit-specific child table:** `Department/Branch`, `Audit Rating`, `Received Date` — only relevant to the Audit register.

---

## Cross-Template Summary (KRI, SAQ, Risk Register)

| Template Family | Granularity | Location Field | Fixed Reference Data | User-Entered Fields |
|---|---|---|---|---|
| KRI | Per Branch / Department (incl. HR variant) | Dropdown, labeled `Branch:`/`Department:` | Ref No, description, Green/Amber/Red thresholds | KRI Value, Comments (mandatory) |
| SAQ | Per Branch / Department | Dropdown, labeled `Branch:`/`Department:` | Ref No, question text | SAQ Answer, Comments (mandatory) |
| Risk Register | Per Department/Area, against a shared org-wide risk taxonomy | Fixed title text (not a labeled dropdown field) | No., Main Risk category, Sub category, Risk trigger/Event, Risk Appetite | Current Position/Actual, Remarks/Control activity (mandatory) |

All three families share the same underlying idea: a fixed, centrally-defined list of
reportable items (KRIs, SAQ questions, or risk triggers) that is customized per
department/branch, with only a small number of columns (`Value`/`Answer`/`Current Position`
plus mandatory `Comments`/`Remarks`) actually filled in by the Maker each month, followed by a
Checker review step (explicit `Reviewed By` field for KRI/SAQ; not present as a field in the
Risk Register samples, though the BRD's Maker/Checker workflow presumably still applies).
