/*
    01_preflight_checks.sql   (READ-ONLY - changes nothing)

    Run before 02-05. Lists every row that would stop a constraint from being created.
    Scripts 02-05 skip (and report) any constraint whose data is not clean, so they are safe to run anyway;
    this script just shows the detail up front.
*/
SET NOCOUNT ON;

PRINT '== Orphan / sentinel values for candidate foreign keys ==';
SELECT relation, total_rows, null_values, zero_values, orphan_rows
FROM (
    SELECT 'tblSupplierQuotes.EnquiryDetailsID -> tblEnquiryDetails' AS relation, COUNT(*) total_rows,
           SUM(CASE WHEN c.EnquiryDetailsID IS NULL THEN 1 ELSE 0 END) null_values, SUM(CASE WHEN c.EnquiryDetailsID = 0 THEN 1 ELSE 0 END) zero_values,
           SUM(CASE WHEN c.EnquiryDetailsID <> 0 AND p.Id IS NULL THEN 1 ELSE 0 END) orphan_rows
    FROM tblSupplierQuotes c LEFT JOIN tblEnquiryDetails p ON p.Id = c.EnquiryDetailsID
    UNION ALL
    SELECT 'tblEnquiryDetails.ProductCategoryId -> tblProductCategory', COUNT(*),
           SUM(CASE WHEN c.ProductCategoryId IS NULL THEN 1 ELSE 0 END), SUM(CASE WHEN c.ProductCategoryId = 0 THEN 1 ELSE 0 END),
           SUM(CASE WHEN c.ProductCategoryId <> 0 AND p.Id IS NULL THEN 1 ELSE 0 END)
    FROM tblEnquiryDetails c LEFT JOIN tblProductCategory p ON p.Id = c.ProductCategoryId
    UNION ALL
    SELECT 'tblEnquiryMaster.DeliveryCountry -> tblCountry', COUNT(*), 0,
           SUM(CASE WHEN c.DeliveryCountry = 0 THEN 1 ELSE 0 END), SUM(CASE WHEN c.DeliveryCountry <> 0 AND p.Id IS NULL THEN 1 ELSE 0 END)
    FROM tblEnquiryMaster c LEFT JOIN tblCountry p ON p.Id = c.DeliveryCountry
) x;

PRINT '== Supplier quotes whose line item no longer exists (left behind by EN_DeleteItem / EB_DeleteItem) ==';
SELECT q.Id AS QuoteId, q.EnquiryID, q.EnquiryDetailsID, q.SupplierID, q.ItemNumber
FROM tblSupplierQuotes q
WHERE NOT EXISTS (SELECT 1 FROM tblEnquiryDetails d WHERE d.Id = q.EnquiryDetailsID);

PRINT '== Line items pointing to a product category that no longer exists ==';
SELECT d.Id AS EnquiryDetailsId, d.EnquiryID, d.ItemNumber, d.ProductCategoryId
FROM tblEnquiryDetails d
WHERE d.ProductCategoryId <> 0 AND NOT EXISTS (SELECT 1 FROM tblProductCategory p WHERE p.Id = d.ProductCategoryId);

PRINT '== Enquiries with an unknown delivery country ==';
SELECT m.Id, m.EnqUId, m.DeliveryCountry
FROM tblEnquiryMaster m
WHERE m.DeliveryCountry <> 0 AND NOT EXISTS (SELECT 1 FROM tblCountry p WHERE p.Id = m.DeliveryCountry);

PRINT '== Duplicate login names (login picks one of these at random) ==';
SELECT u.UserName, COUNT(*) AS accounts,
       STRING_AGG(u.UserType + '#' + CAST(u.FkId AS varchar(12)) + CASE WHEN u.Status = 1 THEN ' (active)' ELSE ' (inactive)' END, ', ') AS accounts_detail
FROM tblUser u GROUP BY u.UserName HAVING COUNT(*) > 1;

PRINT '== Duplicate item numbers within one enquiry ==';
SELECT d.EnquiryID, d.ItemNumber, COUNT(*) AS items, STRING_AGG(CAST(d.Id AS varchar(12)), ', ') AS EnquiryDetailsIds
FROM tblEnquiryDetails d WHERE d.ItemNumber IS NOT NULL
GROUP BY d.EnquiryID, d.ItemNumber HAVING COUNT(*) > 1;

PRINT '== Duplicate supplier codes ==';
SELECT s.Suppliercode, COUNT(*) AS suppliers, STRING_AGG(CAST(s.Id AS varchar(12)) + ':' + s.SupplierName, ' | ') AS suppliers_detail
FROM tblSuppliersList s WHERE ISNULL(s.Suppliercode, '') <> ''
GROUP BY s.Suppliercode HAVING COUNT(*) > 1;

PRINT '== Heap tables (no primary key) whose key column is not unique / has NULLs (02 will skip these) ==';
SELECT 'tblFieldValues' AS [table], COUNT(*) - COUNT(DISTINCT FieldValueID) AS duplicate_keys FROM tblFieldValues
UNION ALL SELECT 'EBtblJobStatus', COUNT(*) - COUNT(DISTINCT StatusId) FROM EBtblJobStatus
UNION ALL SELECT 'tblJobStatus', COUNT(*) - COUNT(DISTINCT StatusId) FROM tblJobStatus
UNION ALL SELECT 'tblversiondata', COUNT(*) - COUNT(DISTINCT Versionid) FROM tblversiondata;
