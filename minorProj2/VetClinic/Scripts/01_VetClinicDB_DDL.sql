/* =====================================================================
   VetClinicDB - DDL (Data Definition Language)
   Creates the VetClinicDB database with four related tables:
       dbo.Veterinarians (admin-only: staff records)
       dbo.Owners        (pet owners / clients)
       dbo.Pets          (child of Owners)
       dbo.Appointments  (child of Pets and Veterinarians)

   Run this script first, then 02_VetClinicDB_DML.sql.
   WARNING: drops and recreates VetClinicDB if it already exists.
   ===================================================================== */

USE master;
GO

IF DB_ID(N'VetClinicDB') IS NOT NULL
BEGIN
    ALTER DATABASE VetClinicDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VetClinicDB;
END
GO

CREATE DATABASE VetClinicDB;
GO

USE VetClinicDB;
GO

/* ---------------------------------------------------------------------
   Veterinarians (edited only from the Administration page)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Veterinarians
(
    VeterinarianId  INT IDENTITY(1, 1) NOT NULL,
    FirstName       NVARCHAR(50)       NOT NULL,
    LastName        NVARCHAR(50)       NOT NULL,
    Specialty       NVARCHAR(100)      NULL,
    IsActive        BIT                NOT NULL CONSTRAINT DF_Veterinarians_IsActive DEFAULT (1),

    CONSTRAINT PK_Veterinarians PRIMARY KEY (VeterinarianId)
);
GO

/* ---------------------------------------------------------------------
   Owners
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Owners
(
    OwnerId     INT IDENTITY(1, 1) NOT NULL,
    FirstName   NVARCHAR(50)       NOT NULL,
    LastName    NVARCHAR(50)       NOT NULL,
    Email       NVARCHAR(100)      NULL,
    Phone       NVARCHAR(25)       NULL,
    City        NVARCHAR(50)       NULL,

    CONSTRAINT PK_Owners PRIMARY KEY (OwnerId)
);
GO

CREATE INDEX IX_Owners_LastName ON dbo.Owners (LastName, FirstName);
GO

/* ---------------------------------------------------------------------
   Pets (deleting an owner deletes their pets)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Pets
(
    PetId       INT IDENTITY(1, 1) NOT NULL,
    OwnerId     INT                NOT NULL,
    Name        NVARCHAR(50)       NOT NULL,
    Species     NVARCHAR(30)       NOT NULL,
    Breed       NVARCHAR(50)       NULL,
    BirthDate   DATE               NULL,
    WeightKg    DECIMAL(6, 2)      NULL,

    CONSTRAINT PK_Pets PRIMARY KEY (PetId),
    CONSTRAINT FK_Pets_Owners FOREIGN KEY (OwnerId) REFERENCES dbo.Owners (OwnerId) ON DELETE CASCADE,
    CONSTRAINT CK_Pets_WeightKg CHECK (WeightKg > 0)
);
GO

CREATE INDEX IX_Pets_OwnerId ON dbo.Pets (OwnerId);
GO

/* ---------------------------------------------------------------------
   Appointments (deleting a pet deletes its appointments;
   a veterinarian with appointments cannot be deleted)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Appointments
(
    AppointmentId   INT IDENTITY(1, 1) NOT NULL,
    PetId           INT                NOT NULL,
    VeterinarianId  INT                NOT NULL,
    ScheduledAt     DATETIME2(0)       NOT NULL,
    Reason          NVARCHAR(200)      NOT NULL,
    Status          NVARCHAR(20)       NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT (N'Scheduled'),
    Cost            DECIMAL(8, 2)      NULL,

    CONSTRAINT PK_Appointments PRIMARY KEY (AppointmentId),
    CONSTRAINT FK_Appointments_Pets FOREIGN KEY (PetId) REFERENCES dbo.Pets (PetId) ON DELETE CASCADE,
    CONSTRAINT FK_Appointments_Veterinarians FOREIGN KEY (VeterinarianId) REFERENCES dbo.Veterinarians (VeterinarianId),
    CONSTRAINT CK_Appointments_Status CHECK (Status IN (N'Scheduled', N'Completed', N'Cancelled')),
    CONSTRAINT CK_Appointments_Cost CHECK (Cost >= 0)
);
GO

CREATE INDEX IX_Appointments_PetId ON dbo.Appointments (PetId);
CREATE INDEX IX_Appointments_VeterinarianId ON dbo.Appointments (VeterinarianId);
GO
