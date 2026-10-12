/*
    03_indexes.sql

    Indexes for the columns the API and procedures filter/join on, and for every foreign key added in 04
    (SQL Server does not index foreign keys automatically; without them each parent delete/lookup scans the child).
    Biggest wins: tblJobHistory.EnqId (checked once per dashboard row) and tblLineitemsHistory.Enqid (140k rows),
    which until now were full table scans.

    Safe to re-run (each index is created only if an index with that name does not exist). Run before 04.
    SQL Server Express has no ONLINE index builds: run in a quiet period (all tables are small, each build takes seconds).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @idx TABLE (name sysname NOT NULL, tbl sysname NOT NULL, keys nvarchar(400) NOT NULL, incl nvarchar(400) NULL);
INSERT @idx (name, tbl, keys, incl) VALUES
    -- Dashboard: OldsurveyENQ check per enquiry, and the history tab
    (N'IX_tblJobHistory_EnqId',                       N'tblJobHistory',                  N'[EnqId]',            N'[Field], [NewValue]'),
    -- Project summary: line item history (140k rows)
    (N'IX_tblLineitemsHistory_Enqid',                 N'tblLineitemsHistory',            N'[Enqid]',            NULL),
    -- Quotes / savings / specifications by item (also support the cascading foreign keys in 04)
    (N'IX_tblSupplierQuotes_EnquiryDetailsID',        N'tblSupplierQuotes',              N'[EnquiryDetailsID]', NULL),
    (N'IX_tblSupplierQuoteHistory_EnquiryID',         N'tblSupplierQuoteHistory',        N'[EnquiryID]',        NULL),
    (N'IX_tblSupplierQuotesforfirstround_EnquiryDetailsID', N'tblSupplierQuotesforfirstround', N'[EnquiryDetailsID]', NULL),
    (N'IX_tblSavingsDetails_EnquiryId',               N'tblSavingsDetails',              N'[EnquiryId]',        NULL),
    (N'IX_tblEnquirySpecification_EnqDetailsID',      N'tblEnquirySpecification',        N'[EnqDetailsID]',     NULL),
    (N'IX_tblEnquirySpecification_EnquiryID',         N'tblEnquirySpecification',        N'[EnquiryID]',        NULL),
    (N'IX_tblClintDeliveryAddress_Enqid',             N'tblClintDeliveryAddress',        N'[Enqid]',            NULL),
    (N'IX_EB_tblPauseExtend_EnquiryId',               N'EB_tblPauseExtend',              N'[EnquiryId]',        NULL),
    -- Supplier / client / division look-ups
    (N'IX_tblSupplierQuoteHistory_SupplierID',        N'tblSupplierQuoteHistory',        N'[SupplierID]',       NULL),
    (N'IX_tblSupplierUsers_SupplierId',               N'tblSupplierUsers',               N'[SupplierId]',       NULL),
    (N'IX_tblDivisionBrandsorproducts_Divisionid',    N'tblDivisionBrandsorproducts',    N'[Divisionid]',       NULL),
    (N'IX_tblDivision_ClientID',                      N'tblDivision',                    N'[ClientID]',         NULL),
    (N'IX_tblClient_Department',                      N'tblClient',                      N'[Department]',       NULL),
    (N'IX_tblClient_Userid',                          N'tblClient',                      N'[Userid]',           NULL),
    (N'IX_tblAgencyUsers_Userid',                     N'tblAgencyUsers',                 N'[Userid]',           NULL),
    (N'IX_tblEnquiryMaster_TemplateId',               N'tblEnquiryMaster',               N'[TemplateId]',       NULL),
    (N'IX_tblTypeofItem_SubitemId',                   N'tblTypeofItem',                  N'[SubitemId]',        NULL),
    -- Project summary: historical price search (heap, filtered by country + currency)
    (N'IX_TblNewSavingsdata_Country_Curr',            N'TblNewSavingsdata',              N'[Country], [Curr]',  NULL);

DECLARE @name sysname, @tbl sysname, @keys nvarchar(400), @incl nvarchar(400), @sql nvarchar(max);
DECLARE idx_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT name, tbl, keys, incl FROM @idx;
OPEN idx_cursor;
FETCH NEXT FROM idx_cursor INTO @name, @tbl, @keys, @incl;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NULL
        PRINT 'SKIP  ' + @name + ': table not found';
    ELSE IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.' + @tbl) AND name = @name)
        PRINT 'OK    ' + @name + ': already exists';
    ELSE
    BEGIN
        SET @sql = N'CREATE NONCLUSTERED INDEX ' + QUOTENAME(@name) + N' ON dbo.' + QUOTENAME(@tbl) + N' (' + @keys + N')'
                 + ISNULL(N' INCLUDE (' + @incl + N')', N'') + N';';
        EXEC (@sql);
        PRINT 'ADDED ' + @name;
    END
    FETCH NEXT FROM idx_cursor INTO @name, @tbl, @keys, @incl;
END
CLOSE idx_cursor; DEALLOCATE idx_cursor;

COMMIT TRANSACTION;
