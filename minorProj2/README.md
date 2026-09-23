# Minor Project 2: Vet Clinic Manager

A WPF desktop application (.NET 10) for a small veterinary clinic, backed by a SQL Server database.

- **Owners, Pets, Appointments**: full create, read, update and delete. Owners can be searched by ID, first name, last name, email, phone or city (SQL `LIKE`).
- **Administration**: veterinarian records can only be changed here, after turning on Administrator mode in Settings.
- **Settings**: theme (Windows / Light / Dark), text size, delete confirmation, administrator mode, connection string.

## Layout

```
VetClinic/
  Scripts/01_VetClinicDB_DDL.sql   creates VetClinicDB (Owners, Pets, Appointments, Veterinarians)
  Scripts/02_VetClinicDB_DML.sql   sample data
  Code/VetClinic.sln
  Code/VetClinic.Data/             data layer (ADO.NET repositories, Microsoft.Data.SqlClient)
  Code/VetClinic.App/              WPF user interface
Documentation/
  MinorProject2_VetClinic.docx     screenshots and write-up
```

## Running

1. Run `VetClinic/Scripts/01_VetClinicDB_DDL.sql`, then `02_VetClinicDB_DML.sql`, against `.\SQLEXPRESS`.
2. `dotnet run --project VetClinic/Code/VetClinic.App` (or open `VetClinic.sln` in Visual Studio). NuGet packages are restored automatically.
3. If your SQL Server instance has a different name, change the connection string on the Settings tab.
