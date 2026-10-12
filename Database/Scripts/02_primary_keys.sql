/*
    02_primary_keys.sql

    Adds a clustered PRIMARY KEY to tables that are currently heaps (no key at all) and that already have a
    natural/identity key column. Without a key, rows can be duplicated silently, foreign keys cannot point at the
    table, and every lookup is a full table scan.

    Safe to re-run: each key is created only if the table has no primary key yet, the column is NOT NULL and its
    values are unique; otherwise the table is skipped with a message. Runs as one transaction.
    Backup / temp tables (e.g. *_02042026, temp*, *old*) are intentionally excluded - see 06 (archive them instead).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @keys TABLE (tbl sysname NOT NULL, col sysname NOT NULL);
INSERT @keys (tbl, col) VALUES
    -- look-up tables used by the API joins
    (N'tblJobStatus', N'StatusId'), (N'EBtblJobStatus', N'StatusId'), (N'TBLMANAGMENTTYPE', N'ID'),
    (N'tblcurrency', N'ID'), (N'TBLExceptionreason', N'id'), (N'tblRegionalcatalogname', N'Id'),
    (N'tblFieldTypes', N'FieldTypeID'),
    -- transactional tables
    (N'tblAttachments', N'Id'), (N'tblSurvey', N'Id'), (N'tblDivisionBrandsorproducts', N'ID'),
    (N'tblpwdhistory', N'id'), (N'TBLCATALOGLOGINDETAILS', N'ID'), (N'tblEnvironmentresult', N'Id'),
    (N'tblFieldValues', N'FieldValueID'), (N'tblversiondata', N'Versionid'),
    (N'tblProductCatalogue', N'Id'), (N'tblCatalog_MailDraft', N'Id'), (N'tblcatalogUsers', N'id'),
    (N'catalog_tblenquirydetails', N'Id'), (N'tblenvironmentalimpact', N'id'), (N'tblEnvironmentEmission', N'Id'),
    (N'tblenvironmentcomparison', N'id'), (N'tblMessage', N'id'), (N'EB_LiveFeed', N'Id');

DECLARE @tbl sysname, @col sysname, @sql nvarchar(max), @dups int, @nullable bit;
DECLARE keys_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT tbl, col FROM @keys;
OPEN keys_cursor;
FETCH NEXT FROM keys_cursor INTO @tbl, @col;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NULL
        PRINT 'SKIP  ' + @tbl + ': table not found';
    ELSE IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.' + @tbl) AND type = 'PK')
        PRINT 'OK    ' + @tbl + ': already has a primary key';
    ELSE
    BEGIN
        SELECT @nullable = is_nullable FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.' + @tbl) AND name = @col;
        SET @sql = N'SELECT @dups = COUNT(*) - COUNT(DISTINCT ' + QUOTENAME(@col) + N') FROM dbo.' + QUOTENAME(@tbl) + N';';
        EXEC sp_executesql @sql, N'@dups int OUTPUT', @dups OUTPUT;

        IF @nullable IS NULL
            PRINT 'SKIP  ' + @tbl + ': column ' + @col + ' not found';
        ELSE IF @nullable = 1
            PRINT 'SKIP  ' + @tbl + ': ' + @col + ' allows NULL';
        ELSE IF @dups > 0
            PRINT 'SKIP  ' + @tbl + ': ' + CAST(@dups AS varchar(12)) + ' duplicate ' + @col + ' values (see 01)';
        ELSE
        BEGIN
            SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@tbl) + N' ADD CONSTRAINT ' + QUOTENAME(N'PK_' + @tbl)
                     + N' PRIMARY KEY CLUSTERED (' + QUOTENAME(@col) + N');';
            EXEC (@sql);
            PRINT 'ADDED PK_' + @tbl + ' (' + @col + ')';
        END
    END
    FETCH NEXT FROM keys_cursor INTO @tbl, @col;
END
CLOSE keys_cursor; DEALLOCATE keys_cursor;

COMMIT TRANSACTION;
