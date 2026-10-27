# Minor Project 5: Hotel Reservations Service Layer

An ASP.NET Core Web API (.NET 10) over the HotelReservationsDB database from Minor Project 4, split into data, business and service layers, plus a small HTML/JavaScript client that calls the API from a different origin (CORS).

- **Data layer** (`HotelReservations.Data`): one data object per table (RoomType, Room, Guest, Reservation) and one ADO.NET repository per table with SelectAll, SelectById, Insert, Update and Delete.
- **Business layer** (`HotelReservations.Business`): one manager per table. Insert and Update are combined into `Save` (id 0 = insert), plus Search, GetAvailable, GetForGuest and Cancel. Validation, double-booking and room-capacity rules live here.
- **Service layer** (`HotelReservations.Api`): one controller per service model with GET (all / by id), POST (insert or update) and DELETE. Business errors come back as 400/404 JSON problem details.
- **Client** (`HotelReservations.Client`): static page with Get all and Get by ID for every table.

## Layout

```
HotelReservations/
  Scripts/01_HotelReservationsDB_DDL.sql   creates HotelReservationsDB (RoomTypes, Rooms, Guests, Reservations)
  Scripts/02_HotelReservationsDB_DML.sql   sample data (64 rows)
  Code/HotelReservations.sln
  Code/HotelReservations.slnLaunch         Visual Studio profile that starts the API and client together
  Code/HotelReservations.Data/             data layer (data objects, ADO.NET helper, repositories)
  Code/HotelReservations.Business/         business layer (managers, business rules)
  Code/HotelReservations.Api/              service layer (controllers, service models, CORS)
  Code/HotelReservations.Client/           HTML/JS test client (wwwroot)
Documentation/
  MinorProject5_HotelReservationsAPI.docx  screenshots and write-up
  screenshots/
```

## Endpoints

| Verb   | Route                                           | Purpose                                     |
|--------|-------------------------------------------------|---------------------------------------------|
| GET    | `/api/{guests,rooms,roomtypes,reservations}`    | Get all                                     |
| GET    | `/api/{resource}/{id}`                          | Get by id                                   |
| POST   | `/api/{resource}`                               | Save: insert when `id` is 0, else update    |
| DELETE | `/api/{resource}/{id}`                          | Delete                                      |
| GET    | `/api/guests?search=text`                       | Search guests                               |
| GET    | `/api/guests/{id}/reservations`                 | A guest's reservations                      |
| GET    | `/api/rooms/available?checkIn=&checkOut=&guests=` | Free rooms for the dates and party size   |
| POST   | `/api/reservations/{id}/cancel`                 | Cancel a reservation                        |

## Running

1. **Database:** in SSMS, open and run `HotelReservations/Scripts/01_HotelReservationsDB_DDL.sql`, then `02_HotelReservationsDB_DML.sql`, against `.\SQLEXPRESS` (not needed if the Project 4 database already exists).
2. **Visual Studio 2022 (17.10 or later):** open `HotelReservations/Code/HotelReservations.sln`. In the toolbar's startup dropdown choose **API + Client** (from `HotelReservations.slnLaunch`) and press **F5**. Two browser tabs open: the API at http://localhost:5310/api/guests and the client at http://localhost:5311.
   - Older Visual Studio: right-click the solution > **Configure Startup Projects** > **Multiple startup projects**, set HotelReservations.Api and HotelReservations.Client to **Start**, then press F5.
3. **Or from a terminal**, in `HotelReservations/Code`: `dotnet run --project HotelReservations.Api`, then in a second terminal `dotnet run --project HotelReservations.Client`.
4. If your SQL Server instance has a different name, change `ConnectionStrings:HotelReservationsDB` in `HotelReservations.Api/appsettings.json`. If the client runs at a different address, add it to `Cors:AllowedOrigins` there.

The client accepts `#rooms` (Get all) or `#rooms/5` (Get by ID) in the URL, and `?api=http://host:port` to point at a different API.
