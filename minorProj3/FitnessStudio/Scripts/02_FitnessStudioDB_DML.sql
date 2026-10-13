/*  Minor Project 3 - Fitness Studio
    DML: sample data (4 plans + 8 instructors + 14 classes + 22 members = 48 rows).
    Run after 01_FitnessStudioDB_DDL.sql.
*/

USE FitnessStudioDB;
GO

INSERT INTO dbo.MembershipPlans (Name, MonthlyPrice, Description, IsActive) VALUES
(N'Basic',    29.99, N'Gym floor access during staffed hours.',                 1),
(N'Standard', 49.99, N'Gym floor plus up to 8 group classes per month.',        1),
(N'Premium',  79.99, N'Unlimited group classes and 24/7 gym access.',           1),
(N'Student',  24.99, N'Basic access at a discount with a valid student ID.',    1);
GO

INSERT INTO dbo.Instructors (FirstName, LastName, Email, Specialty, HireDate) VALUES
(N'Maya',    N'Patel',     N'maya.patel@fitstudio.com',     N'Yoga',              '2019-03-11'),
(N'Jordan',  N'Brooks',    N'jordan.brooks@fitstudio.com',  N'HIIT',              '2020-07-01'),
(N'Elena',   N'Garcia',    N'elena.garcia@fitstudio.com',   N'Pilates',           '2018-01-15'),
(N'Marcus',  N'Lee',       N'marcus.lee@fitstudio.com',     N'Strength Training', '2021-05-20'),
(N'Priya',   N'Shah',      N'priya.shah@fitstudio.com',     N'Spin',              '2022-02-07'),
(N'Tom',     N'Nguyen',    N'tom.nguyen@fitstudio.com',     N'Boxing',            '2017-09-30'),
(N'Hannah',  N'Okafor',    N'hannah.okafor@fitstudio.com',  N'Dance Cardio',      '2023-04-03'),
(N'Diego',   N'Ramirez',   N'diego.ramirez@fitstudio.com',  N'Mobility',          '2024-01-08');
GO

INSERT INTO dbo.FitnessClasses (Title, InstructorId, DayOfWeek, StartTime, DurationMinutes, Capacity, Room) VALUES
(N'Sunrise Yoga',         1, N'Monday',    '06:30', 60, 20, N'Studio A'),
(N'Power HIIT',           2, N'Monday',    '17:30', 45, 25, N'Studio B'),
(N'Core Pilates',         3, N'Tuesday',   '09:00', 50, 15, N'Studio A'),
(N'Barbell Basics',       4, N'Tuesday',   '18:00', 60, 12, N'Weight Room'),
(N'Rhythm Ride',          5, N'Wednesday', '06:00', 45, 30, N'Cycle Room'),
(N'Boxing Fundamentals',  6, N'Wednesday', '19:00', 60, 16, N'Studio B'),
(N'Dance Cardio Party',   7, N'Thursday',  '18:30', 50, 35, N'Studio A'),
(N'Stretch & Mobility',   8, N'Thursday',  '12:00', 30, 20, N'Studio C'),
(N'Vinyasa Flow',         1, N'Friday',    '17:00', 75, 20, N'Studio A'),
(N'Tabata Blast',         2, N'Friday',    '06:30', 30, 25, N'Studio B'),
(N'Reformer Pilates',     3, N'Saturday',  '10:00', 50, 10, N'Studio C'),
(N'Strength Circuit',     4, N'Saturday',  '08:00', 60, 18, N'Weight Room'),
(N'Climb Ride',           5, N'Sunday',    '09:00', 45, 30, N'Cycle Room'),
(N'Restorative Yoga',     1, N'Sunday',    '18:00', 60, 20, N'Studio A');
GO

INSERT INTO dbo.Members (FirstName, LastName, Email, Phone, JoinDate, PlanId) VALUES
(N'Olivia',   N'Johnson',   N'olivia.johnson@email.com',   N'555-0101', '2023-01-09', 3),
(N'Liam',     N'Smith',     N'liam.smith@email.com',       N'555-0102', '2023-02-14', 2),
(N'Emma',     N'Williams',  N'emma.williams@email.com',    N'555-0103', '2023-03-21', 1),
(N'Noah',     N'Brown',     N'noah.brown@email.com',       N'555-0104', '2023-04-02', 4),
(N'Ava',      N'Jones',     N'ava.jones@email.com',        N'555-0105', '2023-05-17', 3),
(N'Elijah',   N'Miller',    N'elijah.miller@email.com',    N'555-0106', '2023-06-30', 2),
(N'Sophia',   N'Davis',     N'sophia.davis@email.com',     N'555-0107', '2023-08-11', 2),
(N'James',    N'Wilson',    N'james.wilson@email.com',     N'555-0108', '2023-09-05', 1),
(N'Isabella', N'Moore',     N'isabella.moore@email.com',   N'555-0109', '2023-10-22', 3),
(N'Benjamin', N'Taylor',    N'ben.taylor@email.com',       N'555-0110', '2023-11-13', 4),
(N'Mia',      N'Anderson',  N'mia.anderson@email.com',     N'555-0111', '2024-01-04', 2),
(N'Lucas',    N'Thomas',    N'lucas.thomas@email.com',     N'555-0112', '2024-02-19', 1),
(N'Charlotte',N'Jackson',   N'charlotte.jackson@email.com',N'555-0113', '2024-03-08', 3),
(N'Henry',    N'White',     N'henry.white@email.com',      N'555-0114', '2024-04-27', 2),
(N'Amelia',   N'Harris',    N'amelia.harris@email.com',    N'555-0115', '2024-06-15', 4),
(N'Alexander',N'Martin',    N'alex.martin@email.com',      N'555-0116', '2024-07-01', 3),
(N'Harper',   N'Thompson',  N'harper.thompson@email.com',  N'555-0117', '2024-08-20', 1),
(N'Daniel',   N'Martinez',  N'daniel.martinez@email.com',  N'555-0118', '2024-10-03', 2),
(N'Evelyn',   N'Robinson',  N'evelyn.robinson@email.com',  N'555-0119', '2025-01-12', 3),
(N'Michael',  N'Clark',     N'michael.clark@email.com',    N'555-0120', '2025-03-25', 2),
(N'Abigail',  N'Lewis',     N'abigail.lewis@email.com',    NULL,        '2025-05-09', 1),
(N'Sam',      N'Johnston',  N'sam.johnston@email.com',     N'555-0122', '2025-08-18', 4);
GO
