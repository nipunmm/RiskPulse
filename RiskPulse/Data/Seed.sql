-- =============================================================================
-- RISKPULSE COMPLETE DATABASE SEED SCRIPT
-- Database: sit | Schema: riskpulse
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS "riskpulse";
SET search_path TO "riskpulse";

-- -----------------------------------------------------------------------------
-- 1. SCHEMA MIGRATION / COLUMN VERIFICATION
-- -----------------------------------------------------------------------------
ALTER TABLE "riskpulse"."Roles" 
    ADD COLUMN IF NOT EXISTS "IsSystemRole" boolean NOT NULL DEFAULT false;

ALTER TABLE "riskpulse"."AssessmentUnits" 
    ADD COLUMN IF NOT EXISTS "UnitApprovedById" integer REFERENCES "riskpulse"."Users"("Id") ON DELETE RESTRICT,
    ADD COLUMN IF NOT EXISTS "UnitApprovedOn" timestamptz,
    ADD COLUMN IF NOT EXISTS "RiskReviewedById" integer REFERENCES "riskpulse"."Users"("Id") ON DELETE RESTRICT,
    ADD COLUMN IF NOT EXISTS "RiskReviewedOn" timestamptz,
    ADD COLUMN IF NOT EXISTS "RiskReviewerRemarks" text,
    ADD COLUMN IF NOT EXISTS "FinalApprovedById" integer REFERENCES "riskpulse"."Users"("Id") ON DELETE RESTRICT,
    ADD COLUMN IF NOT EXISTS "FinalApprovedOn" timestamptz,
    ADD COLUMN IF NOT EXISTS "FinalApproverRemarks" text;

-- -----------------------------------------------------------------------------
-- 2. BASE PERMISSIONS (9 Core Permissions)
-- -----------------------------------------------------------------------------
INSERT INTO "riskpulse"."Permissions" ("PermissionId", "PermissionDesc") VALUES
    (1, 'Dashboard'),
    (2, 'Submissions'),
    (3, 'Schedule'),
    (4, 'Users'),
    (5, 'Roles'),
    (6, 'Units'),
    (7, 'SAQ'),
    (8, 'KRI'),
    (9, 'Risk Register')
ON CONFLICT ("PermissionId") DO UPDATE 
    SET "PermissionDesc" = EXCLUDED."PermissionDesc";

SELECT setval(pg_get_serial_sequence('"riskpulse"."Permissions"', 'PermissionId'), (SELECT MAX("PermissionId") FROM "riskpulse"."Permissions"));

-- -----------------------------------------------------------------------------
-- 3. THE 5 SYSTEM-PROTECTED DEFAULT ROLES
-- DefaultPermissionId: 1 = Dashboard, 2 = Submissions
-- -----------------------------------------------------------------------------
INSERT INTO "riskpulse"."Roles" ("RoleId", "RoleDesc", "DefaultPermissionId", "IsSystemRole") VALUES
    (1, 'Administrator', 1, true),
    (2, 'Unit Initiator', 2, true),
    (3, 'Unit Approver', 2, true),
    (4, 'Risk Dept Reviewer', 1, true),
    (5, 'Risk Dept Approver', 1, true)
ON CONFLICT ("RoleId") DO UPDATE SET 
    "RoleDesc" = EXCLUDED."RoleDesc",
    "DefaultPermissionId" = EXCLUDED."DefaultPermissionId",
    "IsSystemRole" = EXCLUDED."IsSystemRole";

SELECT setval(pg_get_serial_sequence('"riskpulse"."Roles"', 'RoleId'), (SELECT MAX("RoleId") FROM "riskpulse"."Roles"));

-- -----------------------------------------------------------------------------
-- 4. MAP PERMISSIONS TO SYSTEM ROLES
-- -----------------------------------------------------------------------------
DELETE FROM "riskpulse"."RolePermissions" WHERE "RoleId" IN (1, 2, 3, 4, 5);

INSERT INTO "riskpulse"."RolePermissions" ("RoleId", "PermissionId") VALUES
    (1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8), (1, 9),
    (2, 1), (2, 2),
    (3, 1), (3, 2),
    (4, 1), (4, 2), (4, 3), (4, 6), (4, 7), (4, 8),
    (5, 1), (5, 2), (5, 3), (5, 6), (5, 7), (5, 8);

-- -----------------------------------------------------------------------------
-- 5. UNITS (Head Office, Branches)
-- -----------------------------------------------------------------------------
INSERT INTO "riskpulse"."Units" ("UnitId", "UnitCode", "UnitType", "UnitDesc") VALUES
    (1, 'HO-001', 'Department', 'Head Office - Risk Department'),
    (2, 'BR-001', 'Branch', 'Colombo Branch'),
    (3, 'BR-002', 'Branch', 'Kandy Branch')
ON CONFLICT ("UnitId") DO UPDATE SET
    "UnitCode" = EXCLUDED."UnitCode",
    "UnitType" = EXCLUDED."UnitType",
    "UnitDesc" = EXCLUDED."UnitDesc";

SELECT setval(pg_get_serial_sequence('"riskpulse"."Units"', 'UnitId'), (SELECT MAX("UnitId") FROM "riskpulse"."Units"));

-- -----------------------------------------------------------------------------
-- 6. INITIAL / TEST USERS FOR WORKFLOW TESTING
-- -----------------------------------------------------------------------------
INSERT INTO "riskpulse"."Users" ("Id", "Username", "IsActive", "UnitId", "RoleId") VALUES
    (1, 'admin', TRUE, 1, 1),
    (2, 'nipunmm', TRUE, 1, 1),
    (3, 'colombomaker', TRUE, 2, 2),
    (4, 'colombochecker', TRUE, 2, 3),
    (5, 'riskreviewer', TRUE, 1, 4),
    (6, 'riskapprover', TRUE, 1, 5)
ON CONFLICT ("Id") DO UPDATE SET
    "Username" = EXCLUDED."Username",
    "IsActive" = EXCLUDED."IsActive",
    "UnitId" = EXCLUDED."UnitId",
    "RoleId" = EXCLUDED."RoleId";

SELECT setval(pg_get_serial_sequence('"riskpulse"."Users"', 'Id'), (SELECT MAX("Id") FROM "riskpulse"."Users"));

-- -----------------------------------------------------------------------------
-- 7. WORKFLOW DICTIONARIES & 4-LEVEL STEPS
-- -----------------------------------------------------------------------------
INSERT INTO "riskpulse"."Workflows" ("WorkflowId", "WorkflowCode", "WorkflowName") VALUES
    (1, 'ASSESSMENT-UNIT', 'Assessment Unit Workflow'),
    (2, 'ASSESSMENT-ITEM', 'Assessment Item Workflow')
ON CONFLICT ("WorkflowId") DO UPDATE SET 
    "WorkflowCode" = EXCLUDED."WorkflowCode", 
    "WorkflowName" = EXCLUDED."WorkflowName";

SELECT setval(pg_get_serial_sequence('"riskpulse"."Workflows"', 'WorkflowId'), (SELECT MAX("WorkflowId") FROM "riskpulse"."Workflows"));

DELETE FROM "riskpulse"."WorkflowSteps" WHERE "WorkflowId" IN (1, 2);

INSERT INTO "riskpulse"."WorkflowSteps" ("WorkflowStepId", "WorkflowId", "StepCode", "StepLabel", "StepOrder", "IsInitial", "IsFinal") VALUES
    (1, 2, 'Pending', 'Pending Data Entry', 1, true, false),
    (2, 2, 'Submitted', 'Submitted', 2, false, false),
    (3, 2, 'Approved', 'Approved by Unit', 3, false, true);

INSERT INTO "riskpulse"."WorkflowSteps" ("WorkflowStepId", "WorkflowId", "StepCode", "StepLabel", "StepOrder", "IsInitial", "IsFinal") VALUES
    (10, 1, 'Pending', 'Pending Entry', 1, true, false),
    (11, 1, 'InProgress', 'In Progress', 2, false, false),
    (12, 1, 'UnitApproved', 'Unit Approved', 3, false, false),
    (13, 1, 'RiskReviewed', 'Risk Dept Reviewed', 4, false, false),
    (14, 1, 'FinalApproved', 'Final Approved', 5, false, true),
    (15, 1, 'Returned', 'Returned for Revision', 6, false, false);

SELECT setval(pg_get_serial_sequence('"riskpulse"."WorkflowSteps"', 'WorkflowStepId'), (SELECT MAX("WorkflowStepId") FROM "riskpulse"."WorkflowSteps"));
