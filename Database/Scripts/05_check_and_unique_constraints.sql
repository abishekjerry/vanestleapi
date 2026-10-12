/*
    05_check_and_unique_constraints.sql

    CHECK constraints for coded columns and UNIQUE keys for natural identifiers.
    Each constraint is added only when every existing row already satisfies it; otherwise it is skipped with the
    number of offending rows. Safe to re-run.

    Notes
    - tblEnquiryDetails.Status holds codes from TWO lists: tblJobStatus (Enquiry/Catalog) and EBtblJobStatus
      (E-Bidding, e.g. -1 Scheduled, -2 Draft). A foreign key to either table would be wrong, so the column gets a
      range check covering both lists (bounds read from the two tables when the script runs).
    - UQ_tblEnquiryMaster_EnqUId is a filtered index (EnqUId is NULL for a moment while an enquiry is created).
      Filtered indexes require QUOTED_IDENTIFIER/ANSI_NULLS ON for writers: verified ON for all 345 procedures,
      triggers and functions, and for the API (SqlClient default).
    - tblUser.UserName and tblSuppliersList.Suppliercode are NOT made unique here: they contain duplicates that
      must be resolved by the business first (see 06).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
BEGIN TRANSACTION;

DECLARE @minStatus int = (SELECT MIN(v) FROM (SELECT MIN(StatusId) FROM dbo.tblJobStatus UNION ALL SELECT MIN(StatusId) FROM dbo.EBtblJobStatus) x(v));
DECLARE @maxStatus int = (SELECT MAX(v) FROM (SELECT MAX(StatusId) FROM dbo.tblJobStatus UNION ALL SELECT MAX(StatusId) FROM dbo.EBtblJobStatus) x(v));

DECLARE @rules TABLE (seq int IDENTITY, name sysname NOT NULL, tbl sysname NOT NULL, kind varchar(10) NOT NULL,
                      definition nvarchar(400) NOT NULL, violations_sql nvarchar(max) NOT NULL);
INSERT @rules (name, tbl, kind, definition, violations_sql) VALUES
    (N'CK_tblEnquiryDetails_Status', N'tblEnquiryDetails', 'CHECK',
        N'([Status] IS NULL OR [Status] BETWEEN ' + CAST(@minStatus AS nvarchar(12)) + N' AND ' + CAST(@maxStatus AS nvarchar(12)) + N')',
        N'SELECT @v = COUNT(*) FROM dbo.tblEnquiryDetails WHERE NOT ([Status] IS NULL OR [Status] BETWEEN ' + CAST(@minStatus AS nvarchar(12)) + N' AND ' + CAST(@maxStatus AS nvarchar(12)) + N')'),
    (N'CK_tblEnquiryMaster_StepId', N'tblEnquiryMaster', 'CHECK',
        N'([StepId] IS NULL OR [StepId] BETWEEN 0 AND 5)',
        N'SELECT @v = COUNT(*) FROM dbo.tblEnquiryMaster WHERE NOT ([StepId] IS NULL OR [StepId] BETWEEN 0 AND 5)'),
    (N'CK_tblEnquiryMaster_QuoteBy', N'tblEnquiryMaster', 'CHECK',
        N'([SupplierQuoteAmountPriceOrQuantity] IS NULL OR [SupplierQuoteAmountPriceOrQuantity] IN (1, 2))',
        N'SELECT @v = COUNT(*) FROM dbo.tblEnquiryMaster WHERE NOT ([SupplierQuoteAmountPriceOrQuantity] IS NULL OR [SupplierQuoteAmountPriceOrQuantity] IN (1, 2))'),
    (N'CK_tblEnquiryMaster_JobType', N'tblEnquiryMaster', 'CHECK',
        N'([JobType] IS NULL OR [JobType] IN (N''Enquiry'', N''E-Bidding'', N''Catalog''))',
        N'SELECT @v = COUNT(*) FROM dbo.tblEnquiryMaster WHERE NOT ([JobType] IS NULL OR [JobType] IN (N''Enquiry'', N''E-Bidding'', N''Catalog''))'),
    (N'CK_tblUser_UserType', N'tblUser', 'CHECK',
        N'([UserType] IN (N''Agency'', N''Client'', N''Supplier''))',
        N'SELECT @v = COUNT(*) FROM dbo.tblUser WHERE NOT ([UserType] IN (N''Agency'', N''Client'', N''Supplier'')) OR [UserType] IS NULL'),
    (N'UQ_tblEnquiryMaster_EnqUId', N'tblEnquiryMaster', 'UNIQUE',
        N'CREATE UNIQUE NONCLUSTERED INDEX [UQ_tblEnquiryMaster_EnqUId] ON dbo.tblEnquiryMaster ([EnqUId]) WHERE [EnqUId] IS NOT NULL',
        N'SELECT @v = ISNULL(SUM(n - 1), 0) FROM (SELECT COUNT(*) n FROM dbo.tblEnquiryMaster WHERE EnqUId IS NOT NULL GROUP BY EnqUId HAVING COUNT(*) > 1) d'),
    (N'UQ_tblCountry_Country', N'tblCountry', 'UNIQUE',
        N'CREATE UNIQUE NONCLUSTERED INDEX [UQ_tblCountry_Country] ON dbo.tblCountry ([Country])',
        N'SELECT @v = ISNULL(SUM(n - 1), 0) FROM (SELECT COUNT(*) n FROM dbo.tblCountry GROUP BY Country HAVING COUNT(*) > 1) d');

DECLARE @seq int = 1, @max int = (SELECT MAX(seq) FROM @rules);
DECLARE @name sysname, @tbl sysname, @kind varchar(10), @def nvarchar(400), @vsql nvarchar(max), @v int, @sql nvarchar(max);
WHILE @seq <= @max
BEGIN
    SELECT @name = name, @tbl = tbl, @kind = kind, @def = definition, @vsql = violations_sql FROM @rules WHERE seq = @seq;

    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = @name)
       OR EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.' + @tbl) AND name = @name)
        PRINT 'OK    ' + @name + ': already exists';
    ELSE
    BEGIN
        EXEC sp_executesql @vsql, N'@v int OUTPUT', @v OUTPUT;
        IF @v > 0
            PRINT 'SKIP  ' + @name + ': ' + CAST(@v AS varchar(12)) + ' existing rows violate it';
        ELSE
        BEGIN
            SET @sql = CASE WHEN @kind = 'CHECK'
                            THEN N'ALTER TABLE dbo.' + QUOTENAME(@tbl) + N' WITH CHECK ADD CONSTRAINT ' + QUOTENAME(@name) + N' CHECK ' + @def + N';'
                            ELSE @def + N';' END;
            EXEC (@sql);
            PRINT 'ADDED ' + @name + CASE WHEN @kind = 'CHECK' THEN ' ' + @def ELSE '' END;
        END
    END
    SET @seq += 1;
END

COMMIT TRANSACTION;
