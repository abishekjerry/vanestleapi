/*
    04_foreign_keys.sql

    The database currently has NO foreign keys, so nothing stops a row from pointing at an enquiry, item, supplier
    or user that does not exist. This script adds the relationships the application already relies on.

    Delete behaviour (chosen from what the stored procedures actually do):
      - Enquiries (tblEnquiryMaster) are only soft-deleted (Status = 0)          -> NO ACTION
      - Line items are hard-deleted by EN_DeleteItem / EB_DeleteItem, which delete the item first and its
        specifications afterwards, and never delete the item's quotes (that is where today's orphan quotes
        come from)                                                               -> ON DELETE CASCADE for item children
      - Look-up tables (suppliers, users, SLA, client details, sub-categories) are never deleted -> NO ACTION
      - Audit/history tables reference only the enquiry, so history is kept when an item is deleted.

    Each key is created WITH CHECK (existing rows validated, so the optimiser can trust it) and only when the data
    is clean; otherwise it is skipped with the number of offending rows (see 01 and 06). Safe to re-run.
    Run 03 first so every foreign key column is indexed.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @fk TABLE (name sysname NOT NULL, child sysname NOT NULL, child_col sysname NOT NULL,
                   parent sysname NOT NULL, parent_col sysname NOT NULL, on_delete nvarchar(20) NOT NULL);
INSERT @fk (name, child, child_col, parent, parent_col, on_delete) VALUES
    -- Enquiry (soft-deleted only)
    (N'FK_tblEnquiryDetails_tblEnquiryMaster',          N'tblEnquiryDetails',              N'EnquiryID',        N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblSupplierQuotes_tblEnquiryMaster',          N'tblSupplierQuotes',              N'EnquiryID',        N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblSavingsDetails_tblEnquiryMaster',          N'tblSavingsDetails',              N'EnquiryId',        N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblEnquirySpecification_tblEnquiryMaster',    N'tblEnquirySpecification',        N'EnquiryID',        N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblAttachments_tblEnquiryMaster',             N'tblAttachments',                 N'EnqId',            N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblSurvey_tblEnquiryMaster',                  N'tblSurvey',                      N'EnqId',            N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblClintDeliveryAddress_tblEnquiryMaster',    N'tblClintDeliveryAddress',        N'Enqid',            N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_EB_tblPauseExtend_tblEnquiryMaster',          N'EB_tblPauseExtend',              N'EnquiryId',        N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    -- Audit / history: enquiry only
    (N'FK_tblJobHistory_tblEnquiryMaster',              N'tblJobHistory',                  N'EnqId',            N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblLineitemsHistory_tblEnquiryMaster',        N'tblLineitemsHistory',            N'Enqid',            N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    (N'FK_tblSupplierQuoteHistory_tblEnquiryMaster',    N'tblSupplierQuoteHistory',        N'EnquiryID',        N'tblEnquiryMaster',  N'Id', N'NO ACTION'),
    -- Line item children: removed together with the item
    (N'FK_tblEnquirySpecification_tblEnquiryDetails',   N'tblEnquirySpecification',        N'EnqDetailsID',     N'tblEnquiryDetails', N'Id', N'CASCADE'),
    (N'FK_tblSupplierQuotesforfirstround_tblEnquiryDetails', N'tblSupplierQuotesforfirstround', N'EnquiryDetailsID', N'tblEnquiryDetails', N'Id', N'CASCADE'),
    (N'FK_tblSavingsDetails_tblEnquiryDetails',         N'tblSavingsDetails',              N'EnquiryDetailsId', N'tblEnquiryDetails', N'Id', N'CASCADE'),
    (N'FK_tblSupplierQuotes_tblEnquiryDetails',         N'tblSupplierQuotes',              N'EnquiryDetailsID', N'tblEnquiryDetails', N'Id', N'CASCADE'), -- skipped until 06 removes the orphan quotes
    -- Look-ups (never deleted)
    (N'FK_tblSupplierQuotes_tblSuppliersList',          N'tblSupplierQuotes',              N'SupplierID',       N'tblSuppliersList',  N'Id', N'NO ACTION'),
    (N'FK_tblSupplierUsers_tblSuppliersList',           N'tblSupplierUsers',               N'SupplierId',       N'tblSuppliersList',  N'Id', N'NO ACTION'),
    (N'FK_tblEnquiryMaster_tblSLA',                     N'tblEnquiryMaster',               N'TemplateId',       N'tblSLA',            N'Id', N'NO ACTION'),
    (N'FK_tblDivision_tblClientDetails',                N'tblDivision',                    N'ClientID',         N'tblClientDetails',  N'Id', N'NO ACTION'),
    (N'FK_tblTypeofItem_tblSubCategory',                N'tblTypeofItem',                  N'SubitemId',        N'tblSubCategory',    N'Id', N'NO ACTION'),
    (N'FK_tblClient_tblUser',                           N'tblClient',                      N'Userid',           N'tblUser',           N'Id', N'NO ACTION'),
    (N'FK_tblAgencyUsers_tblUser',                      N'tblAgencyUsers',                 N'Userid',           N'tblUser',           N'Id', N'NO ACTION');

DECLARE @name sysname, @child sysname, @cc sysname, @parent sysname, @pc sysname, @del nvarchar(20);
DECLARE @sql nvarchar(max), @orphans int, @child_type sysname, @parent_type sysname, @parent_unique bit;
DECLARE fk_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT name, child, child_col, parent, parent_col, on_delete FROM @fk;
OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @name, @child, @cc, @parent, @pc, @del;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @child_type = TYPE_NAME(user_type_id) FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.' + @child) AND name = @cc;
    SELECT @parent_type = TYPE_NAME(user_type_id) FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.' + @parent) AND name = @pc;
    -- the referenced column must be a primary key / unique key on its own
    SELECT @parent_unique = CASE WHEN EXISTS (
        SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        WHERE i.object_id = OBJECT_ID(N'dbo.' + @parent) AND i.is_unique = 1 AND i.has_filter = 0
          AND COL_NAME(ic.object_id, ic.column_id) = @pc
          AND (SELECT COUNT(*) FROM sys.index_columns x WHERE x.object_id = i.object_id AND x.index_id = i.index_id AND x.is_included_column = 0) = 1
    ) THEN 1 ELSE 0 END;

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = @name)
        PRINT 'OK    ' + @name + ': already exists';
    ELSE IF @child_type IS NULL OR @parent_type IS NULL
        PRINT 'SKIP  ' + @name + ': table or column not found';
    ELSE IF @child_type <> @parent_type
        PRINT 'SKIP  ' + @name + ': type mismatch (' + @child_type + ' vs ' + @parent_type + ')';
    ELSE IF @parent_unique = 0
        PRINT 'SKIP  ' + @name + ': ' + @parent + '.' + @pc + ' is not a primary/unique key (run 02)';
    ELSE
    BEGIN
        SET @sql = N'SELECT @orphans = COUNT(*) FROM dbo.' + QUOTENAME(@child) + N' c WHERE c.' + QUOTENAME(@cc)
                 + N' IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.' + QUOTENAME(@parent) + N' p WHERE p.' + QUOTENAME(@pc)
                 + N' = c.' + QUOTENAME(@cc) + N');';
        EXEC sp_executesql @sql, N'@orphans int OUTPUT', @orphans OUTPUT;

        IF @orphans > 0
            PRINT 'SKIP  ' + @name + ': ' + CAST(@orphans AS varchar(12)) + ' rows reference a missing ' + @parent + ' (see 01 / 06)';
        ELSE
        BEGIN
            SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@child) + N' WITH CHECK ADD CONSTRAINT ' + QUOTENAME(@name)
                     + N' FOREIGN KEY (' + QUOTENAME(@cc) + N') REFERENCES dbo.' + QUOTENAME(@parent) + N' (' + QUOTENAME(@pc) + N')'
                     + N' ON DELETE ' + @del + N';';
            EXEC (@sql);
            PRINT 'ADDED ' + @name + CASE WHEN @del = N'CASCADE' THEN ' (on delete cascade)' ELSE '' END;
        END
    END
    FETCH NEXT FROM fk_cursor INTO @name, @child, @cc, @parent, @pc, @del;
END
CLOSE fk_cursor; DEALLOCATE fk_cursor;

COMMIT TRANSACTION;
