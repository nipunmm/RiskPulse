-- Run this SQL against your PostgreSQL database (sit database, riskpulse schema)
-- to add the auto-generated template code columns with unique indexes.
-- Existing rows will have NULL codes; new templates receive codes like SAQ-20260827-0001 / KRI-20260827-0001.

ALTER TABLE riskpulse."SaqHeaders" ADD COLUMN "SaqCode" character varying(50) NULL;
ALTER TABLE riskpulse."KriHeaders" ADD COLUMN "KriCode" character varying(50) NULL;

CREATE UNIQUE INDEX "UX_SaqHeaders_SaqCode" ON riskpulse."SaqHeaders" ("SaqCode");
CREATE UNIQUE INDEX "UX_KriHeaders_KriCode" ON riskpulse."KriHeaders" ("KriCode");