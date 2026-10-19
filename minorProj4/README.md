# Minor Project 4: Hotel Reservations

An ASP.NET Core MVC web application (.NET 10) for a small hotel's front desk, backed by a new SQL Server database through a separate ADO.NET data layer project.

- **Guests, Rooms, Reservations**: full create, read, update and delete. Guests can be searched by ID, first name, last name, email or city, using contains / starts with / ends with matching (parameterized SQL `LIKE`). Reservations are checked for double bookings and room capacity.
- **Administration**: room types and nightly rates can only be changed here, after turning on Administrator mode in Settings.
- **Settings**: theme (light / dark / match system), accent color, language (English / Spanish), date format, show cancelled reservations, delete confirmation, administrator mode. Saved in a browser cookie.

## Layout

```
HotelReservations/
  Scripts/01_HotelReservationsDB_DDL.sql   creates HotelReservationsDB (RoomTypes, Rooms, Guests, Reservations)
  Scripts/02_HotelReservationsDB_DML.sql   sample data (64 rows)
  Code/HotelReservations.sln
  Code/HotelReservations.Data/             data layer (models, ADO.NET helper, repositories)
  Code/HotelReservations.Web/              MVC user interface (controllers, views)
Documentation/
  MinorProject4_HotelReservations.docx     screenshots and write-up
  screenshots/
```

## Running

1. Run `HotelReservations/Scripts/01_HotelReservationsDB_DDL.sql`, then `02_HotelReservationsDB_DML.sql`, against `.\SQLEXPRESS`.
2. From `HotelReservations/Code`, run `dotnet run --project HotelReservations.Web` (or open `HotelReservations.sln` in Visual Studio). NuGet packages are restored automatically.
3. Browse to http://localhost:5299.
4. If your SQL Server instance has a different name, change `ConnectionStrings:HotelReservationsDB` in `HotelReservations.Web/appsettings.json`.
