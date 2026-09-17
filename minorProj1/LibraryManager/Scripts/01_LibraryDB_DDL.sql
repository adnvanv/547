/* =====================================================================
   LibraryDB - DDL (Data Definition Language)
   Creates the LibraryDB database with two related tables:
       dbo.Authors  (parent)
       dbo.Books    (child, Books.AuthorId -> Authors.AuthorId)

   Run this script first, then 02_LibraryDB_DML.sql.
   WARNING: drops and recreates LibraryDB if it already exists.
   ===================================================================== */

USE master;
GO

IF DB_ID(N'LibraryDB') IS NOT NULL
BEGIN
    ALTER DATABASE LibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryDB;
END
GO

CREATE DATABASE LibraryDB;
GO

USE LibraryDB;
GO

/* ---------------------------------------------------------------------
   Authors
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Authors
(
    AuthorId    INT IDENTITY(1, 1) NOT NULL,
    FirstName   NVARCHAR(50)       NOT NULL,
    LastName    NVARCHAR(50)       NOT NULL,
    Country     NVARCHAR(50)       NULL,
    BirthYear   INT                NULL,

    CONSTRAINT PK_Authors PRIMARY KEY (AuthorId),
    CONSTRAINT CK_Authors_BirthYear CHECK (BirthYear BETWEEN 1000 AND 2100)
);
GO

/* ---------------------------------------------------------------------
   Books
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Books
(
    BookId          INT IDENTITY(1, 1) NOT NULL,
    Title           NVARCHAR(200)      NOT NULL,
    AuthorId        INT                NOT NULL,
    Genre           NVARCHAR(50)       NULL,
    PublishedYear   INT                NULL,
    Price           DECIMAL(8, 2)      NOT NULL CONSTRAINT DF_Books_Price   DEFAULT (0),
    InStock         BIT                NOT NULL CONSTRAINT DF_Books_InStock DEFAULT (1),

    CONSTRAINT PK_Books PRIMARY KEY (BookId),
    CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorId) REFERENCES dbo.Authors (AuthorId),
    CONSTRAINT CK_Books_Price CHECK (Price >= 0),
    CONSTRAINT CK_Books_PublishedYear CHECK (PublishedYear BETWEEN 1000 AND 2100)
);
GO

CREATE INDEX IX_Books_AuthorId ON dbo.Books (AuthorId);
GO

/* ---------------------------------------------------------------------
   Descriptions (MS_Description extended properties).
   The console application reads these to describe each table/column.
   --------------------------------------------------------------------- */
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Writers whose books the library carries.',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Authors';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Unique author ID',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Authors', @level2type = N'COLUMN', @level2name = N'AuthorId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Given name',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Authors', @level2type = N'COLUMN', @level2name = N'FirstName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Family name',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Authors', @level2type = N'COLUMN', @level2name = N'LastName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Country of origin',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Authors', @level2type = N'COLUMN', @level2name = N'Country';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Year of birth',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Authors', @level2type = N'COLUMN', @level2name = N'BirthYear';

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Book titles in the catalog; each belongs to one author.',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Unique book ID',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'BookId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Book title',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'Title';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Author who wrote the book',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'AuthorId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Literary genre',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'Genre';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Year first published',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'PublishedYear';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Retail price (USD)',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'Price';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'1 = copies on the shelf',
     @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Books', @level2type = N'COLUMN', @level2name = N'InStock';
GO
