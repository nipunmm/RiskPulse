--Run this SQL script against your PostgreSQL database (sit database, riskpulse schema) to populate initial permissions, roles, unit, and user assignment.
-- 1. Insert Base Permissions matching sidebar and modules
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Dashboard');
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Submissions') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Assessment Control') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Form Builder') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Users');
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Roles') ;

-- 2. Insert Roles
INSERT INTO "riskpulse"."Roles" ("RoleDesc") VALUES ('IT Admin');
INSERT INTO "riskpulse"."Roles" ("RoleDesc") VALUES ('Branch Officer');

-- 3. Insert Units (Head Office)
INSERT INTO "riskpulse"."Units" ("UnitCode", "UnitType", "UnitDesc") 
VALUES ('001', 'Branch', 'Head Office');

-- 4. Assign ALL Permissions to 'IT Admin' Role
INSERT INTO "riskpulse"."RolePermissions" ("RoleId", "PermissionId")
SELECT 
    (SELECT "RoleId" FROM "riskpulse"."Roles" WHERE "RoleDesc" = 'IT Admin'),
    "PermissionId"
FROM "riskpulse"."Permissions";

-- 6. Insert Test Users
INSERT INTO "riskpulse"."Users" ("Username", "IsActive", "UnitId", "RoleId")
VALUES (
    'nipunmm',
    TRUE,
    (SELECT "UnitId" FROM "riskpulse"."Units" WHERE "UnitCode" = '001'),
    (SELECT "RoleId" FROM "riskpulse"."Roles" WHERE "RoleDesc" = 'IT Admin')
);
