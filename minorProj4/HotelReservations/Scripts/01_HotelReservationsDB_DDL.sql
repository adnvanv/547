/*  Minor Project 4 - Hotel Reservations
    DDL: creates HotelReservationsDB and its four tables.
    Run this first, then 02_HotelReservationsDB_DML.sql.

    RoomTypes     - admin-only table (edited on the Administration page)
    Guests        - full CRUD + search (first name, last name, email, city)
    Rooms         - full CRUD
    Reservations  - full CRUD
*/

USE master;
GO

IF DB_ID(N'HotelReservationsDB') IS NOT NULL
BEGIN
    ALTER DATABASE HotelReservationsDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HotelReservationsDB;
END
GO

CREATE DATABASE HotelReservationsDB;
GO

USE HotelReservationsDB;
GO

CREATE TABLE dbo.RoomTypes
(
    RoomTypeId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RoomTypes PRIMARY KEY,
    Name          NVARCHAR(50)  NOT NULL CONSTRAINT UQ_RoomTypes_Name UNIQUE,
    NightlyRate   DECIMAL(8,2)  NOT NULL CONSTRAINT CK_RoomTypes_Rate CHECK (NightlyRate >= 0),
    MaxOccupancy  INT           NOT NULL CONSTRAINT CK_RoomTypes_MaxOccupancy CHECK (MaxOccupancy BETWEEN 1 AND 12),
    Description   NVARCHAR(250) NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_RoomTypes_IsActive DEFAULT (1)
);
GO

CREATE TABLE dbo.Rooms
(
    RoomId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rooms PRIMARY KEY,
    RoomNumber      NVARCHAR(10)  NOT NULL CONSTRAINT UQ_Rooms_RoomNumber UNIQUE,
    Floor           INT           NOT NULL CONSTRAINT CK_Rooms_Floor CHECK (Floor BETWEEN 1 AND 50),
    RoomTypeId      INT           NOT NULL CONSTRAINT FK_Rooms_RoomTypes
                                      REFERENCES dbo.RoomTypes (RoomTypeId),
    IsSmoking       BIT           NOT NULL CONSTRAINT DF_Rooms_IsSmoking DEFAULT (0),
    IsOutOfService  BIT           NOT NULL CONSTRAINT DF_Rooms_IsOutOfService DEFAULT (0),
    Notes           NVARCHAR(250) NULL
);
GO

CREATE TABLE dbo.Guests
(
    GuestId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Guests PRIMARY KEY,
    FirstName  NVARCHAR(50)  NOT NULL,
    LastName   NVARCHAR(50)  NOT NULL,
    Email      NVARCHAR(100) NOT NULL CONSTRAINT UQ_Guests_Email UNIQUE,
    Phone      NVARCHAR(20)  NULL,
    City       NVARCHAR(50)  NULL,
    Country    NVARCHAR(50)  NOT NULL CONSTRAINT DF_Guests_Country DEFAULT (N'USA'),
    CreatedOn  DATE          NOT NULL CONSTRAINT DF_Guests_CreatedOn DEFAULT (CAST(GETDATE() AS DATE))
);
GO

CREATE INDEX IX_Guests_LastName ON dbo.Guests (LastName);
GO

CREATE TABLE dbo.Reservations
(
    ReservationId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reservations PRIMARY KEY,
    GuestId        INT           NOT NULL CONSTRAINT FK_Reservations_Guests
                                     REFERENCES dbo.Guests (GuestId),
    RoomId         INT           NOT NULL CONSTRAINT FK_Reservations_Rooms
                                     REFERENCES dbo.Rooms (RoomId),
    CheckInDate    DATE          NOT NULL,
    CheckOutDate   DATE          NOT NULL,
    NumberOfGuests INT           NOT NULL CONSTRAINT CK_Reservations_NumberOfGuests CHECK (NumberOfGuests >= 1),
    Status         NVARCHAR(20)  NOT NULL CONSTRAINT DF_Reservations_Status DEFAULT (N'Booked')
                                     CONSTRAINT CK_Reservations_Status
                                     CHECK (Status IN (N'Booked', N'Checked In', N'Checked Out', N'Cancelled')),
    Notes          NVARCHAR(250) NULL,
    CONSTRAINT CK_Reservations_Dates CHECK (CheckOutDate > CheckInDate)
);
GO

CREATE INDEX IX_Reservations_RoomId_Dates ON dbo.Reservations (RoomId, CheckInDate, CheckOutDate);
GO
