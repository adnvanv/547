# Minor Project 3: Fitness Studio

An ASP.NET Core Razor Pages web application (.NET 10) for a small gym, backed by a SQL Server database through an Entity Framework Core data layer.

- **Members, Instructors, Classes**: full create, read, update and delete. Members can be searched by ID, first name, last name, email or phone, using contains / starts with / ends with matching (EF Core `Contains`/`StartsWith`/`EndsWith`, translated to SQL `LIKE`).
- **Administration**: membership plans can only be changed here, after turning on Administrator mode in Settings.
- **Settings**: theme (light / dark / match system), accent color, date format, 12/24-hour clock, delete confirmation, administrator mode. Saved in a browser cookie.

## Layout

```
FitnessStudio/
  Scripts/01_FitnessStudioDB_DDL.sql   creates FitnessStudioDB (MembershipPlans, Members, Instructors, FitnessClasses)
  Scripts/02_FitnessStudioDB_DML.sql   sample data (48 rows)
  Code/FitnessStudio.slnx
  Code/FitnessStudio.Data/             data layer (EF Core entities, DbContext, services)
  Code/FitnessStudio.Web/              Razor Pages user interface
Documentation/
  MinorProject3_FitnessStudio.docx     screenshots and write-up
  screenshots/
```

## Running

1. Run `FitnessStudio/Scripts/01_FitnessStudioDB_DDL.sql`, then `02_FitnessStudioDB_DML.sql`, against `.\SQLEXPRESS`.
2. From `FitnessStudio/Code`, run `dotnet run --project FitnessStudio.Web` (or open `FitnessStudio.slnx` in Visual Studio). NuGet packages are restored automatically.
3. Browse to http://localhost:5139.
4. If your SQL Server instance has a different name, change `ConnectionStrings:FitnessStudioDB` in `FitnessStudio.Web/appsettings.json`.
