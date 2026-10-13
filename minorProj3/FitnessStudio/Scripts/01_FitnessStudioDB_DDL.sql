/*  Minor Project 3 - Fitness Studio
    DDL: creates FitnessStudioDB and its four tables.
    Run this first, then 02_FitnessStudioDB_DML.sql.

    MembershipPlans  - admin-only table (edited on the Administration page)
    Members          - full CRUD + search (first name, last name, email, phone)
    Instructors      - full CRUD
    FitnessClasses   - full CRUD
*/

USE master;
GO

IF DB_ID(N'FitnessStudioDB') IS NOT NULL
BEGIN
    ALTER DATABASE FitnessStudioDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FitnessStudioDB;
END
GO

CREATE DATABASE FitnessStudioDB;
GO

USE FitnessStudioDB;
GO

CREATE TABLE dbo.MembershipPlans
(
    PlanId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MembershipPlans PRIMARY KEY,
    Name          NVARCHAR(50)  NOT NULL CONSTRAINT UQ_MembershipPlans_Name UNIQUE,
    MonthlyPrice  DECIMAL(8,2)  NOT NULL CONSTRAINT CK_MembershipPlans_Price CHECK (MonthlyPrice >= 0),
    Description   NVARCHAR(250) NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_MembershipPlans_IsActive DEFAULT (1)
);
GO

CREATE TABLE dbo.Members
(
    MemberId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Members PRIMARY KEY,
    FirstName  NVARCHAR(50)  NOT NULL,
    LastName   NVARCHAR(50)  NOT NULL,
    Email      NVARCHAR(100) NOT NULL CONSTRAINT UQ_Members_Email UNIQUE,
    Phone      NVARCHAR(20)  NULL,
    JoinDate   DATE          NOT NULL CONSTRAINT DF_Members_JoinDate DEFAULT (CAST(GETDATE() AS DATE)),
    PlanId     INT           NOT NULL CONSTRAINT FK_Members_MembershipPlans
                                 REFERENCES dbo.MembershipPlans (PlanId)
);
GO

CREATE INDEX IX_Members_LastName ON dbo.Members (LastName);
GO

CREATE TABLE dbo.Instructors
(
    InstructorId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Instructors PRIMARY KEY,
    FirstName     NVARCHAR(50)  NOT NULL,
    LastName      NVARCHAR(50)  NOT NULL,
    Email         NVARCHAR(100) NOT NULL CONSTRAINT UQ_Instructors_Email UNIQUE,
    Specialty     NVARCHAR(50)  NOT NULL,
    HireDate      DATE          NOT NULL
);
GO

CREATE TABLE dbo.FitnessClasses
(
    ClassId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FitnessClasses PRIMARY KEY,
    Title            NVARCHAR(80) NOT NULL,
    InstructorId     INT          NOT NULL CONSTRAINT FK_FitnessClasses_Instructors
                                      REFERENCES dbo.Instructors (InstructorId),
    DayOfWeek        NVARCHAR(10) NOT NULL CONSTRAINT CK_FitnessClasses_Day CHECK
                                      (DayOfWeek IN (N'Monday', N'Tuesday', N'Wednesday', N'Thursday',
                                                     N'Friday', N'Saturday', N'Sunday')),
    StartTime        TIME(0)      NOT NULL,
    DurationMinutes  INT          NOT NULL CONSTRAINT CK_FitnessClasses_Duration CHECK (DurationMinutes BETWEEN 15 AND 180),
    Capacity         INT          NOT NULL CONSTRAINT CK_FitnessClasses_Capacity CHECK (Capacity BETWEEN 1 AND 100),
    Room             NVARCHAR(30) NOT NULL
);
GO
