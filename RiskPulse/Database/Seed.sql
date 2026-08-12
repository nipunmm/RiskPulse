--Run this SQL script against your PostgreSQL database (sit database, riskpulse schema) to populate initial permissions, roles, unit, and user assignment.
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Dashboard');
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Submissions') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Assessment Control') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Form Builder') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Users');
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Roles') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('SAQ');
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('KRI');
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('Risk Register') ;
INSERT INTO "riskpulse"."Permissions" ("PermissionDesc") VALUES ('KRI Config') ;


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



--------------------------------------------------------Do not run this manully (needs to create clss and run via the ef core)
CREATE TABLE dbo.tblAssessmentModuleType
(
    ModuleTypeId INT IDENTITY(1,1) PRIMARY KEY,
    ModuleName VARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    DisplayOrder INT NOT NULL DEFAULT(1),
    IsActive BIT NOT NULL DEFAULT(1)
);

INSERT INTO dbo.tblAssessmentModuleType
(
    ModuleName,
    Description,
    DisplayOrder
)
VALUES
('SAQ', 'Self Assessment Questionnaire', 1),
('KRI', 'Key Risk Indicator', 2),
('Risk Register', 'Risk Register', 3);

/*==============================================================
    ASSESSMENT HEADER
==============================================================*/

CREATE TABLE dbo.tblAssessmentHeader
(
    AssessmentHeaderId INT IDENTITY(1,1) PRIMARY KEY,

    AssessmentName NVARCHAR(200) NOT NULL,

    AssessmentMonth TINYINT NOT NULL,

    AssessmentYear SMALLINT NOT NULL,

    BranchId INT NOT NULL,

    Status VARCHAR(20) NOT NULL DEFAULT('Draft'),

    CreatedBy INT NOT NULL,

    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),

    ModifiedBy INT NULL,

    ModifiedDate DATETIME NULL
);

/*==============================================================
    COMMON MODULE HEADER
==============================================================*/

CREATE TABLE dbo.tblModuleHeader
(
    ModuleHeaderId INT IDENTITY(1,1) PRIMARY KEY,

    ModuleTypeId INT NOT NULL,

    AssessmentPeriod DATE NULL,

    VersionNo INT NOT NULL DEFAULT(1),

    Status VARCHAR(20) NOT NULL DEFAULT('Draft'),

    CreatedBy INT NOT NULL,

    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),

    CONSTRAINT FK_tblModuleHeader_ModuleType
        FOREIGN KEY(ModuleTypeId)
        REFERENCES dbo.tblAssessmentModuleType(ModuleTypeId)
);

/*==============================================================
    ASSESSMENT MODULE
==============================================================*/

CREATE TABLE dbo.tblAssessmentModule
(
    AssessmentModuleId INT IDENTITY(1,1) PRIMARY KEY,

    AssessmentHeaderId INT NOT NULL,

    ModuleHeaderId INT NOT NULL,

    DisplayOrder INT NOT NULL DEFAULT(1),

    CONSTRAINT FK_tblAssessmentModule_AssessmentHeader
        FOREIGN KEY(AssessmentHeaderId)
        REFERENCES dbo.tblAssessmentHeader(AssessmentHeaderId),

    CONSTRAINT FK_tblAssessmentModule_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblModuleHeader(ModuleHeaderId)
);

/*==============================================================
    SAQ HEADER
==============================================================*/

CREATE TABLE dbo.tblSAQHeader
(
    ModuleHeaderId INT PRIMARY KEY,

    CONSTRAINT FK_tblSAQHeader_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblModuleHeader(ModuleHeaderId)
);

/*==============================================================
    SAQ QUESTION
==============================================================*/

CREATE TABLE dbo.tblSAQQuestion
(
    QuestionId INT IDENTITY(1,1) PRIMARY KEY,

    ModuleHeaderId INT NOT NULL,

    QuestionText NVARCHAR(MAX) NOT NULL,

    QuestionType VARCHAR(30) NOT NULL,

    IsRequired BIT NOT NULL DEFAULT(0),

    DisplayOrder INT NOT NULL,

    CONSTRAINT FK_tblSAQQuestion_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblSAQHeader(ModuleHeaderId)
);

/*==============================================================
    SAQ QUESTION OPTION
==============================================================*/

CREATE TABLE dbo.tblSAQQuestionOption
(
    OptionId INT IDENTITY(1,1) PRIMARY KEY,

    QuestionId INT NOT NULL,

    OptionText NVARCHAR(300) NOT NULL,

    OptionValue NVARCHAR(100),

    DisplayOrder INT,

    CONSTRAINT FK_tblSAQQuestionOption_Question
        FOREIGN KEY(QuestionId)
        REFERENCES dbo.tblSAQQuestion(QuestionId)
);

/*==============================================================
    SAQ SUBMISSION
==============================================================*/

CREATE TABLE dbo.tblSAQSubmission
(
    SubmissionId INT IDENTITY(1,1) PRIMARY KEY,

    ModuleHeaderId INT NOT NULL,

    SubmittedBy INT NOT NULL,

    SubmittedDate DATETIME DEFAULT(GETDATE()),

    Status VARCHAR(20) DEFAULT('Submitted'),

    CONSTRAINT FK_tblSAQSubmission_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblSAQHeader(ModuleHeaderId)
);

/*==============================================================
    SAQ ANSWER
==============================================================*/

CREATE TABLE dbo.tblSAQAnswer
(
    AnswerId INT IDENTITY(1,1) PRIMARY KEY,

    SubmissionId INT NOT NULL,

    QuestionId INT NOT NULL,

    OptionId INT NULL,

    AnswerText NVARCHAR(MAX),

    Comment NVARCHAR(MAX),

    CreatedDate DATETIME DEFAULT(GETDATE()),

    CONSTRAINT FK_tblSAQAnswer_Submission
        FOREIGN KEY(SubmissionId)
        REFERENCES dbo.tblSAQSubmission(SubmissionId),

    CONSTRAINT FK_tblSAQAnswer_Question
        FOREIGN KEY(QuestionId)
        REFERENCES dbo.tblSAQQuestion(QuestionId),

    CONSTRAINT FK_tblSAQAnswer_Option
        FOREIGN KEY(OptionId)
        REFERENCES dbo.tblSAQQuestionOption(OptionId)
);

/*==============================================================
    KRI HEADER
==============================================================*/

CREATE TABLE dbo.tblKRIHeader
(
    ModuleHeaderId INT PRIMARY KEY,

    CONSTRAINT FK_tblKRIHeader_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblModuleHeader(ModuleHeaderId)
);

/*==============================================================
    KRI DETAIL
==============================================================*/

CREATE TABLE dbo.tblKRIDetail
(
    KRIDetailId INT IDENTITY(1,1) PRIMARY KEY,

    ModuleHeaderId INT NOT NULL,

    IndicatorName NVARCHAR(300),

    Threshold DECIMAL(18,2),

    CurrentValue DECIMAL(18,2),

    RiskLevel VARCHAR(20),

    Remarks NVARCHAR(MAX),

    CONSTRAINT FK_tblKRIDetail_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblKRIHeader(ModuleHeaderId)
);

/*==============================================================
    RISK REGISTER HEADER
==============================================================*/

CREATE TABLE dbo.tblRiskRegisterHeader
(
    ModuleHeaderId INT PRIMARY KEY,

    CONSTRAINT FK_tblRiskRegisterHeader_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblModuleHeader(ModuleHeaderId)
);

/*==============================================================
    RISK REGISTER DETAIL
==============================================================*/

CREATE TABLE dbo.tblRiskRegisterDetail
(
    RiskId INT IDENTITY(1,1) PRIMARY KEY,

    ModuleHeaderId INT NOT NULL,

    RiskDescription NVARCHAR(MAX),

    Cause NVARCHAR(MAX),

    Impact NVARCHAR(MAX),

    Probability INT,

    Severity INT,

    RiskScore INT,

    MitigationPlan NVARCHAR(MAX),

    OwnerId INT,

    Status VARCHAR(20),

    CONSTRAINT FK_tblRiskRegisterDetail_ModuleHeader
        FOREIGN KEY(ModuleHeaderId)
        REFERENCES dbo.tblRiskRegisterHeader(ModuleHeaderId)
);
