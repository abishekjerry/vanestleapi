/*
    06_data_cleanup_REQUIRES_APPROVAL.sql

    Data fixes that unblock the remaining constraints. These change or delete business data, so:
      - by default the script is a DRY RUN: it shows what it would do and ROLLS BACK;
      - set @Apply = 1 only after the business owner has approved each section;
      - every row that is deleted is first copied to a backup table (cleanup_* ) so it can be restored.
    After applying, re-run 04 and 05: the constraints they skipped will then be created.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Apply bit = 0;   -- <<< 1 = commit the changes, 0 = dry run (default)

BEGIN TRANSACTION;

------------------------------------------------------------------------------------------------------------------
-- A. Supplier quotes whose line item was deleted (EN_DeleteItem / EB_DeleteItem never removed them).
--    They can never be shown or awarded. Removing them lets FK_tblSupplierQuotes_tblEnquiryDetails
--    (ON DELETE CASCADE) be created, so this cannot happen again.
------------------------------------------------------------------------------------------------------------------
-- (the join keeps SELECT INTO from copying the IDENTITY property of Id, so later inserts work)
IF OBJECT_ID(N'dbo.cleanup_tblSupplierQuotes_orphans', N'U') IS NULL
    SELECT q.*, SYSDATETIME() AS cleanup_date INTO dbo.cleanup_tblSupplierQuotes_orphans
    FROM dbo.tblSupplierQuotes q LEFT JOIN dbo.tblEnquiryDetails d ON d.Id = q.EnquiryDetailsID
    WHERE 1 = 0;

INSERT dbo.cleanup_tblSupplierQuotes_orphans
SELECT q.*, SYSDATETIME()
FROM dbo.tblSupplierQuotes q LEFT JOIN dbo.tblEnquiryDetails d ON d.Id = q.EnquiryDetailsID
WHERE d.Id IS NULL;

DELETE q FROM dbo.tblSupplierQuotes q
WHERE NOT EXISTS (SELECT 1 FROM dbo.tblEnquiryDetails d WHERE d.Id = q.EnquiryDetailsID);
PRINT 'A. Orphan supplier quotes removed (backed up to cleanup_tblSupplierQuotes_orphans): ' + CAST(@@ROWCOUNT AS varchar(12));

------------------------------------------------------------------------------------------------------------------
-- B. Duplicate login names (REPORT ONLY - needs a decision per user).
--    Login (UserSignon) picks one of these accounts arbitrarily. Rename or deactivate the extra accounts, then
--    replace the existing non-unique index with a unique one:
--        DROP INDEX IX_tblUser_UserName ON dbo.tblUser;
--        CREATE UNIQUE NONCLUSTERED INDEX UQ_tblUser_UserName ON dbo.tblUser (UserName);
------------------------------------------------------------------------------------------------------------------
SELECT 'B. duplicate login' AS issue, u.UserName, u.Id AS UserId, u.UserType, u.FkId, u.Status
FROM dbo.tblUser u
WHERE u.UserName IN (SELECT UserName FROM dbo.tblUser GROUP BY UserName HAVING COUNT(*) > 1)
ORDER BY u.UserName, u.Id;

------------------------------------------------------------------------------------------------------------------
-- C. Duplicate item numbers inside one enquiry (REPORT ONLY).
--    Caused by the API numbering new items as COUNT + 1 after an item was deleted (fixed in the API: MAX + 1).
--    Quotes and savings are matched by ItemNumber, so renumber the duplicate after checking its quotes, then:
--        CREATE UNIQUE NONCLUSTERED INDEX UQ_tblEnquiryDetails_EnquiryID_ItemNumber
--            ON dbo.tblEnquiryDetails (EnquiryID, ItemNumber) WHERE ItemNumber IS NOT NULL;
------------------------------------------------------------------------------------------------------------------
SELECT 'C. duplicate item number' AS issue, d.EnquiryID, d.ItemNumber, d.Id AS EnquiryDetailsId, d.ItemName,
       (SELECT COUNT(*) FROM dbo.tblSupplierQuotes q WHERE q.EnquiryDetailsID = d.Id) AS quotes
FROM dbo.tblEnquiryDetails d
WHERE EXISTS (SELECT 1 FROM dbo.tblEnquiryDetails x WHERE x.EnquiryID = d.EnquiryID AND x.ItemNumber = d.ItemNumber AND x.Id <> d.Id)
ORDER BY d.EnquiryID, d.ItemNumber, d.Id;

------------------------------------------------------------------------------------------------------------------
-- D. Duplicate supplier codes (REPORT ONLY). Correct the codes, then:
--        CREATE UNIQUE NONCLUSTERED INDEX UQ_tblSuppliersList_Suppliercode
--            ON dbo.tblSuppliersList (Suppliercode) WHERE Suppliercode IS NOT NULL AND Suppliercode <> N'';
------------------------------------------------------------------------------------------------------------------
SELECT 'D. duplicate supplier code' AS issue, s.Suppliercode, s.Id, s.SupplierName, s.Country, s.Status
FROM dbo.tblSuppliersList s
WHERE ISNULL(s.Suppliercode, N'') <> N''
  AND s.Suppliercode IN (SELECT Suppliercode FROM dbo.tblSuppliersList WHERE ISNULL(Suppliercode, N'') <> N'' GROUP BY Suppliercode HAVING COUNT(*) > 1)
ORDER BY s.Suppliercode, s.Id;

------------------------------------------------------------------------------------------------------------------
-- E. Columns that use 0 to mean "none" (REPORT ONLY - needs an application change first).
--    A foreign key rejects 0. Converting 0 -> NULL is the fix, but the API and procedures compare these columns
--    with 0 (e.g. "SupplierId != 0"), so they must be updated in the same release:
--      tblEnquiryMaster: DivisionId, ClientId, ClientContactId, PmgEntity, DeliveryCountry, CreatedBy, MANAGEMENTFEETYPEID
--      tblEnquiryDetails: SupplierId, ProductCategoryId     tblClient: Department     tblJobHistory: UserId
------------------------------------------------------------------------------------------------------------------
SELECT 'E. zero used as "none"' AS issue, x.col, x.zero_rows FROM (
    SELECT 'tblEnquiryMaster.ClientContactId' col, COUNT(*) zero_rows FROM dbo.tblEnquiryMaster WHERE ClientContactId = 0
    UNION ALL SELECT 'tblEnquiryMaster.DivisionId', COUNT(*) FROM dbo.tblEnquiryMaster WHERE DivisionId = 0
    UNION ALL SELECT 'tblEnquiryMaster.ClientId', COUNT(*) FROM dbo.tblEnquiryMaster WHERE ClientId = 0
    UNION ALL SELECT 'tblEnquiryMaster.PmgEntity', COUNT(*) FROM dbo.tblEnquiryMaster WHERE PmgEntity = 0
    UNION ALL SELECT 'tblEnquiryMaster.DeliveryCountry', COUNT(*) FROM dbo.tblEnquiryMaster WHERE DeliveryCountry = 0
    UNION ALL SELECT 'tblEnquiryMaster.CreatedBy', COUNT(*) FROM dbo.tblEnquiryMaster WHERE CreatedBy = 0
    UNION ALL SELECT 'tblEnquiryDetails.SupplierId', COUNT(*) FROM dbo.tblEnquiryDetails WHERE SupplierId = 0
    UNION ALL SELECT 'tblEnquiryDetails.ProductCategoryId', COUNT(*) FROM dbo.tblEnquiryDetails WHERE ProductCategoryId = 0
    UNION ALL SELECT 'tblClient.Department', COUNT(*) FROM dbo.tblClient WHERE Department = 0
    UNION ALL SELECT 'tblJobHistory.UserId', COUNT(*) FROM dbo.tblJobHistory WHERE UserId = 0
) x;

------------------------------------------------------------------------------------------------------------------
-- F. Brands -> Division (DECISION NEEDED). ClientHierarchy_CRUD hard-deletes divisions. Choose one:
--      NO ACTION : deleting a division that still has brands fails (protects data, admin must remove brands first)
--      CASCADE   : deleting a division also deletes its brands
--    ALTER TABLE dbo.tblDivisionBrandsorproducts WITH CHECK ADD CONSTRAINT FK_tblDivisionBrandsorproducts_tblDivision
--        FOREIGN KEY (Divisionid) REFERENCES dbo.tblDivision (ID) ON DELETE NO ACTION;   -- or CASCADE
------------------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------------------------------------------------
-- G. Backup / temporary tables not referenced by any procedure, view or the API (ARCHIVE, then drop):
--      tbldivision_02042026, tblDivisionBrandsorproducts_02042026, tbldivisionold, tempdivision,
--      temp_productcategory_20220826, tttblproductcategory, tblentity_New, tblTemp, tblWebAdmin_old_09052024,
--      tblClient_28072016 (still mapped in the API model but unused - remove from AppDbContext first)
------------------------------------------------------------------------------------------------------------------

IF @Apply = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT 'Changes COMMITTED. Now re-run 04_foreign_keys.sql and 05_check_and_unique_constraints.sql.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'DRY RUN - all changes rolled back. Set @Apply = 1 after approval.';
END
