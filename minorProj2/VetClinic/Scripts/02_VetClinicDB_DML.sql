/* =====================================================================
   VetClinicDB - DML (Data Manipulation Language)
   Seeds sample data. Run after 01_VetClinicDB_DDL.sql.
   ===================================================================== */

USE VetClinicDB;
GO

SET NOCOUNT ON;

INSERT INTO dbo.Veterinarians (FirstName, LastName, Specialty, IsActive) VALUES
    (N'Maria',   N'Alvarez',  N'General Practice', 1),
    (N'James',   N'Okafor',   N'Surgery',          1),
    (N'Priya',   N'Raman',    N'Dermatology',      1),
    (N'Thomas',  N'Becker',   N'Exotic Animals',   0);

INSERT INTO dbo.Owners (FirstName, LastName, Email, Phone, City) VALUES
    (N'Emily',    N'Carter',    N'emily.carter@example.com',   N'(614) 555-0101', N'Columbus'),
    (N'Daniel',   N'Nguyen',    N'dnguyen@example.com',        N'(614) 555-0142', N'Dublin'),
    (N'Sophia',   N'Martinez',  N'sophia.m@example.com',       N'(740) 555-0188', N'Delaware'),
    (N'Michael',  N'Johnson',   N'mjohnson@example.com',       N'(614) 555-0199', N'Columbus'),
    (N'Olivia',   N'Brown',     N'olivia.brown@example.com',   N'(937) 555-0113', N'Dayton'),
    (N'Ethan',    N'Carter',    N'ethan.carter@example.com',   N'(614) 555-0175', N'Westerville'),
    (N'Ava',      N'Wilson',    N'ava.wilson@example.com',     N'(513) 555-0164', N'Cincinnati'),
    (N'Noah',     N'Patel',     N'noah.patel@example.com',     N'(614) 555-0120', N'Hilliard');

INSERT INTO dbo.Pets (OwnerId, Name, Species, Breed, BirthDate, WeightKg) VALUES
    (1, N'Biscuit',  N'Dog',     N'Golden Retriever',  '2019-04-12', 31.50),
    (1, N'Mochi',    N'Cat',     N'Siamese',           '2021-08-03',  4.20),
    (2, N'Rex',      N'Dog',     N'German Shepherd',   '2017-11-20', 38.00),
    (3, N'Luna',     N'Cat',     N'Maine Coon',        '2020-02-14',  6.80),
    (4, N'Kiwi',     N'Bird',    N'Cockatiel',         '2022-06-01',  0.09),
    (5, N'Pepper',   N'Rabbit',  N'Holland Lop',       '2023-01-25',  1.60),
    (6, N'Max',      N'Dog',     N'Beagle',            '2018-09-09', 11.30),
    (7, N'Shadow',   N'Cat',     N'Domestic Shorthair', '2016-05-30', 5.10),
    (8, N'Nugget',   N'Hamster', N'Syrian',            '2024-03-10',  0.15),
    (8, N'Daisy',    N'Dog',     N'Corgi',             '2020-12-24', 12.40);

INSERT INTO dbo.Appointments (PetId, VeterinarianId, ScheduledAt, Reason, Status, Cost) VALUES
    (1,  1, '2026-08-14 09:00', N'Annual wellness exam',        N'Completed', 85.00),
    (2,  3, '2026-08-20 13:30', N'Skin rash on neck',           N'Completed', 120.00),
    (3,  2, '2026-09-02 10:15', N'Hip X-rays',                  N'Completed', 240.00),
    (4,  1, '2026-09-18 15:00', N'Vaccinations',                N'Completed', 65.00),
    (5,  4, '2026-07-11 11:00', N'Beak and nail trim',          N'Completed', 40.00),
    (7,  1, '2026-09-28 09:30', N'Dental cleaning',             N'Scheduled', NULL),
    (8,  3, '2026-10-01 14:00', N'Allergy follow-up',           N'Scheduled', NULL),
    (10, 2, '2026-10-05 08:45', N'Spay surgery',                N'Scheduled', NULL),
    (6,  1, '2026-09-10 16:00', N'Check-up',                    N'Cancelled', NULL);
GO
