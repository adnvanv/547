/* =====================================================================
   LibraryDB - DML (Data Manipulation Language)
   Loads sample data: 11 authors + 25 books = 36 records.

   Run after 01_LibraryDB_DDL.sql. Safe to re-run: existing rows are
   cleared and the original IDs are restored.
   ===================================================================== */

USE LibraryDB;
GO

SET NOCOUNT ON;

-- Children first, because Books.AuthorId references Authors.
DELETE FROM dbo.Books;
DELETE FROM dbo.Authors;

/* ---------------------------------------------------------------------
   Authors (11 rows). Octavia Butler intentionally has no books yet, so
   she can be deleted without violating the foreign key.
   --------------------------------------------------------------------- */
SET IDENTITY_INSERT dbo.Authors ON;

INSERT INTO dbo.Authors (AuthorId, FirstName, LastName, Country, BirthYear)
VALUES
    ( 1, N'Jane',       N'Austen',          N'United Kingdom', 1775),
    ( 2, N'George',     N'Orwell',          N'United Kingdom', 1903),
    ( 3, N'Harper',     N'Lee',             N'United States',  1926),
    ( 4, N'Gabriel',    N'García Márquez',  N'Colombia',       1927),
    ( 5, N'Toni',       N'Morrison',        N'United States',  1931),
    ( 6, N'Haruki',     N'Murakami',        N'Japan',          1949),
    ( 7, N'Chinua',     N'Achebe',          N'Nigeria',        1930),
    ( 8, N'Ursula K.',  N'Le Guin',         N'United States',  1929),
    ( 9, N'Fyodor',     N'Dostoevsky',      N'Russia',         1821),
    (10, N'Chimamanda', N'Ngozi Adichie',   N'Nigeria',        1977),
    (11, N'Octavia',    N'Butler',          N'United States',  1947);

SET IDENTITY_INSERT dbo.Authors OFF;

/* ---------------------------------------------------------------------
   Books (25 rows)
   --------------------------------------------------------------------- */
SET IDENTITY_INSERT dbo.Books ON;

INSERT INTO dbo.Books (BookId, Title, AuthorId, Genre, PublishedYear, Price, InStock)
VALUES
    ( 1, N'Pride and Prejudice',            1, N'Romance',               1813,  9.99, 1),
    ( 2, N'Sense and Sensibility',          1, N'Romance',               1811,  8.99, 1),
    ( 3, N'Emma',                           1, N'Romance',               1815,  9.49, 0),
    ( 4, N'Nineteen Eighty-Four',           2, N'Dystopian',             1949, 12.99, 1),
    ( 5, N'Animal Farm',                    2, N'Satire',                1945,  8.49, 1),
    ( 6, N'To Kill a Mockingbird',          3, N'Southern Gothic',       1960, 14.99, 1),
    ( 7, N'Go Set a Watchman',              3, N'Fiction',               2015, 16.99, 0),
    ( 8, N'One Hundred Years of Solitude',  4, N'Magical Realism',       1967, 15.99, 1),
    ( 9, N'Love in the Time of Cholera',    4, N'Romance',               1985, 14.49, 1),
    (10, N'Beloved',                        5, N'Historical Fiction',    1987, 13.99, 1),
    (11, N'Song of Solomon',                5, N'Fiction',               1977, 13.49, 1),
    (12, N'The Bluest Eye',                 5, N'Fiction',               1970, 11.99, 0),
    (13, N'Norwegian Wood',                 6, N'Fiction',               1987, 15.49, 1),
    (14, N'Kafka on the Shore',             6, N'Magical Realism',       2002, 16.49, 1),
    (15, N'1Q84',                           6, N'Fantasy',               2009, 19.99, 1),
    (16, N'Things Fall Apart',              7, N'Historical Fiction',    1958, 10.99, 1),
    (17, N'No Longer at Ease',              7, N'Fiction',               1960, 11.49, 0),
    (18, N'A Wizard of Earthsea',           8, N'Fantasy',               1968, 10.49, 1),
    (19, N'The Left Hand of Darkness',      8, N'Science Fiction',       1969, 12.49, 1),
    (20, N'The Dispossessed',               8, N'Science Fiction',       1974, 12.99, 1),
    (21, N'Crime and Punishment',           9, N'Psychological Fiction', 1866, 11.99, 1),
    (22, N'The Brothers Karamazov',         9, N'Philosophical Fiction', 1880, 14.99, 0),
    (23, N'Notes from Underground',         9, N'Philosophical Fiction', 1864,  7.99, 1),
    (24, N'Half of a Yellow Sun',          10, N'Historical Fiction',    2006, 15.99, 1),
    (25, N'Americanah',                    10, N'Fiction',               2013, 16.99, 1);

SET IDENTITY_INSERT dbo.Books OFF;

-- Make new rows continue numbering right after the sample data.
DBCC CHECKIDENT (N'dbo.Authors', RESEED, 11) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Books',   RESEED, 25) WITH NO_INFOMSGS;
GO

/* ---------------------------------------------------------------------
   Verify
   --------------------------------------------------------------------- */
SELECT 'Authors' AS TableName, COUNT(*) AS RecordCount FROM dbo.Authors
UNION ALL
SELECT 'Books', COUNT(*) FROM dbo.Books;

SELECT b.BookId, b.Title, a.FirstName + N' ' + a.LastName AS Author, b.PublishedYear, b.Price
FROM dbo.Books AS b
JOIN dbo.Authors AS a ON a.AuthorId = b.AuthorId
ORDER BY b.BookId;
GO
