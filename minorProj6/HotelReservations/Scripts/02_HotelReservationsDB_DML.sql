/*  Minor Project 5 - Hotel Reservations service layer (database reused from Minor Project 4)
    DML: sample data (6 room types, 16 rooms, 20 guests, 22 reservations = 64 rows).
    Run after 01_HotelReservationsDB_DDL.sql.
*/

USE HotelReservationsDB;
GO

SET NOCOUNT ON;

INSERT INTO dbo.RoomTypes (Name, NightlyRate, MaxOccupancy, Description, IsActive) VALUES
(N'Standard Queen',     129.00, 2, N'One queen bed, work desk, city view.', 1),
(N'Standard Double',    149.00, 4, N'Two double beds, ideal for small families.', 1),
(N'Accessible Queen',   129.00, 2, N'ADA-compliant room with roll-in shower.', 1),
(N'Deluxe King',        189.00, 2, N'King bed, sitting area and espresso machine.', 1),
(N'Junior Suite',       249.00, 3, N'King bed plus sofa bed and a separate living area.', 1),
(N'Family Suite',       299.00, 6, N'Two bedrooms, kitchenette and two bathrooms.', 1);

INSERT INTO dbo.Rooms (RoomNumber, Floor, RoomTypeId, IsSmoking, IsOutOfService, Notes) VALUES
(N'101', 1, 3, 0, 0, N'Closest to the elevator.'),
(N'102', 1, 1, 0, 0, NULL),
(N'103', 1, 1, 0, 0, NULL),
(N'104', 1, 2, 0, 0, NULL),
(N'105', 1, 2, 1, 0, N'Designated smoking room (patio access).'),
(N'201', 2, 1, 0, 0, NULL),
(N'202', 2, 2, 0, 0, NULL),
(N'203', 2, 4, 0, 0, N'Corner room with two windows.'),
(N'204', 2, 4, 0, 1, N'Carpet replacement until Oct 31.'),
(N'205', 2, 3, 0, 0, NULL),
(N'301', 3, 4, 0, 0, NULL),
(N'302', 3, 4, 0, 0, NULL),
(N'303', 3, 5, 0, 0, N'Lake view.'),
(N'304', 3, 5, 0, 0, NULL),
(N'401', 4, 6, 0, 0, N'Penthouse level, private balcony.'),
(N'402', 4, 6, 0, 0, NULL);

INSERT INTO dbo.Guests (FirstName, LastName, Email, Phone, City, Country, CreatedOn) VALUES
(N'Emma',      N'Thompson',  N'emma.thompson@example.com',   N'312-555-0101', N'Chicago',      N'USA',     '2025-02-14'),
(N'Liam',      N'Garcia',    N'liam.garcia@example.com',     N'512-555-0102', N'Austin',       N'USA',     '2025-03-02'),
(N'Olivia',    N'Martinez',  N'olivia.martinez@example.com', N'305-555-0103', N'Miami',        N'USA',     '2025-04-19'),
(N'Noah',      N'Johnson',   N'noah.johnson@example.com',    N'206-555-0104', N'Seattle',      N'USA',     '2025-05-07'),
(N'Ava',       N'Brown',     N'ava.brown@example.com',       N'617-555-0105', N'Boston',       N'USA',     '2025-06-21'),
(N'Ethan',     N'Davis',     N'ethan.davis@example.com',     N'303-555-0106', N'Denver',       N'USA',     '2025-07-11'),
(N'Sophia',    N'Wilson',    N'sophia.wilson@example.com',   N'416-555-0107', N'Toronto',      N'Canada',  '2025-08-30'),
(N'Mason',     N'Anderson',  N'mason.anderson@example.com',  N'612-555-0108', N'Minneapolis',  N'USA',     '2025-09-15'),
(N'Isabella',  N'Taylor',    N'isabella.taylor@example.com', N'415-555-0109', N'San Francisco',N'USA',     '2025-10-03'),
(N'Lucas',     N'Moore',     N'lucas.moore@example.com',     N'020-555-0110', N'London',       N'UK',      '2025-11-22'),
(N'Mia',       N'Jackson',   N'mia.jackson@example.com',     N'404-555-0111', N'Atlanta',      N'USA',     '2025-12-09'),
(N'James',     N'White',     N'james.white@example.com',     N'602-555-0112', N'Phoenix',      N'USA',     '2026-01-17'),
(N'Charlotte', N'Harris',    N'charlotte.harris@example.com',N'514-555-0113', N'Montreal',     N'Canada',  '2026-02-05'),
(N'Benjamin',  N'Clark',     N'ben.clark@example.com',       N'215-555-0114', N'Philadelphia', N'USA',     '2026-03-12'),
(N'Amelia',    N'Lewis',     N'amelia.lewis@example.com',    N'503-555-0115', N'Portland',     N'USA',     '2026-04-01'),
(N'Henry',     N'Walker',    N'henry.walker@example.com',    N'615-555-0116', N'Nashville',    N'USA',     '2026-05-26'),
(N'Harper',    N'Hall',      N'harper.hall@example.com',     N'+49 30 5550117',N'Berlin',      N'Germany', '2026-06-14'),
(N'Alexander', N'Young',     N'alex.young@example.com',      N'313-555-0118', N'Detroit',      N'USA',     '2026-07-08'),
(N'Evelyn',    N'King',      N'evelyn.king@example.com',     N'702-555-0119', N'Las Vegas',    N'USA',     '2026-08-19'),
(N'Daniel',    N'Wright',    N'daniel.wright@example.com',   N'614-555-0120', N'Columbus',     N'USA',     '2026-09-27');

INSERT INTO dbo.Reservations (GuestId, RoomId, CheckInDate, CheckOutDate, NumberOfGuests, Status, Notes) VALUES
( 1,  8, '2026-09-04', '2026-09-07', 2, N'Checked Out', N'Anniversary trip.'),
( 2,  4, '2026-09-11', '2026-09-13', 3, N'Checked Out', NULL),
( 3, 13, '2026-09-18', '2026-09-21', 3, N'Checked Out', N'Late checkout approved.'),
( 4,  2, '2026-09-25', '2026-09-27', 1, N'Cancelled',   N'Flight cancelled.'),
( 5, 15, '2026-10-01', '2026-10-05', 5, N'Checked Out', NULL),
( 6, 11, '2026-10-09', '2026-10-12', 2, N'Checked Out', NULL),
( 7,  1, '2026-10-16', '2026-10-21', 2, N'Checked In',  N'Requested extra pillows.'),
( 8,  7, '2026-10-17', '2026-10-20', 4, N'Checked In',  NULL),
( 9, 12, '2026-10-18', '2026-10-22', 2, N'Checked In',  NULL),
(10, 14, '2026-10-19', '2026-10-24', 3, N'Booked',      N'Arriving after 10 PM.'),
(11,  3, '2026-10-22', '2026-10-24', 2, N'Booked',      NULL),
(12, 16, '2026-10-23', '2026-10-26', 6, N'Booked',      N'Family reunion.'),
(13,  6, '2026-10-24', '2026-10-27', 1, N'Booked',      NULL),
(14, 10, '2026-10-30', '2026-11-02', 2, N'Booked',      N'Needs accessible parking.'),
(15,  8, '2026-11-02', '2026-11-04', 2, N'Booked',      NULL),
(16, 13, '2026-11-06', '2026-11-09', 3, N'Booked',      NULL),
(17, 15, '2026-11-12', '2026-11-16', 4, N'Booked',      N'Airport shuttle requested.'),
(18,  5, '2026-11-13', '2026-11-15', 2, N'Booked',      N'Smoking room requested.'),
(19, 11, '2026-11-20', '2026-11-23', 2, N'Booked',      NULL),
(20,  2, '2026-11-25', '2026-11-28', 2, N'Booked',      N'Thanksgiving weekend.'),
( 1, 12, '2026-12-18', '2026-12-22', 2, N'Booked',      N'Returning guest.'),
( 4,  4, '2026-12-27', '2027-01-02', 4, N'Booked',      N'New Year''s stay.');
GO

SELECT 'RoomTypes' AS TableName, COUNT(*) AS Rows FROM dbo.RoomTypes
UNION ALL SELECT 'Rooms', COUNT(*) FROM dbo.Rooms
UNION ALL SELECT 'Guests', COUNT(*) FROM dbo.Guests
UNION ALL SELECT 'Reservations', COUNT(*) FROM dbo.Reservations;
GO
