/*
    99_rollback.sql

    Removes everything scripts 02-05 created (only objects with the names those scripts use), returning the
    schema to its previous state. Data is not touched. Safe to re-run. (07 is reverted with
    ALTER DATABASE ... SET READ_COMMITTED_SNAPSHOT OFF.)
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @sql nvarchar(max);

-- 05 + 04: check constraints and foreign keys created by these scripts
SELECT @sql = STRING_AGG(CONVERT(nvarchar(max), N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
              + N' DROP CONSTRAINT ' + QUOTENAME(o.name) + N';'), NCHAR(10))
FROM sys.objects o JOIN sys.tables t ON t.object_id = o.parent_object_id
WHERE (o.type = 'C' AND o.name IN (N'CK_tblEnquiryDetails_Status', N'CK_tblEnquiryMaster_StepId', N'CK_tblEnquiryMaster_QuoteBy',
                                   N'CK_tblEnquiryMaster_JobType', N'CK_tblUser_UserType'))
   OR (o.type = 'F' AND o.name IN (
        N'FK_tblEnquiryDetails_tblEnquiryMaster', N'FK_tblSupplierQuotes_tblEnquiryMaster', N'FK_tblSavingsDetails_tblEnquiryMaster',
        N'FK_tblEnquirySpecification_tblEnquiryMaster', N'FK_tblAttachments_tblEnquiryMaster', N'FK_tblSurvey_tblEnquiryMaster',
        N'FK_tblClintDeliveryAddress_tblEnquiryMaster', N'FK_EB_tblPauseExtend_tblEnquiryMaster', N'FK_tblJobHistory_tblEnquiryMaster',
        N'FK_tblLineitemsHistory_tblEnquiryMaster', N'FK_tblSupplierQuoteHistory_tblEnquiryMaster',
        N'FK_tblEnquirySpecification_tblEnquiryDetails', N'FK_tblSupplierQuotesforfirstround_tblEnquiryDetails',
        N'FK_tblSavingsDetails_tblEnquiryDetails', N'FK_tblSupplierQuotes_tblEnquiryDetails',
        N'FK_tblSupplierQuotes_tblSuppliersList', N'FK_tblSupplierUsers_tblSuppliersList', N'FK_tblEnquiryMaster_tblSLA',
        N'FK_tblDivision_tblClientDetails', N'FK_tblTypeofItem_tblSubCategory', N'FK_tblClient_tblUser', N'FK_tblAgencyUsers_tblUser',
        N'FK_tblDivisionBrandsorproducts_tblDivision'));
IF @sql IS NOT NULL BEGIN EXEC (@sql); PRINT 'Dropped check constraints / foreign keys'; END

-- 05 + 03: unique and non-clustered indexes created by these scripts
SET @sql = NULL;
SELECT @sql = STRING_AGG(CONVERT(nvarchar(max), N'DROP INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N';'), NCHAR(10))
FROM sys.indexes i JOIN sys.tables t ON t.object_id = i.object_id
WHERE i.name IN (
    N'UQ_tblEnquiryMaster_EnqUId', N'UQ_tblCountry_Country',
    N'IX_tblJobHistory_EnqId', N'IX_tblLineitemsHistory_Enqid', N'IX_tblSupplierQuotes_EnquiryDetailsID', N'IX_tblSupplierQuoteHistory_EnquiryID',
    N'IX_tblSupplierQuotesforfirstround_EnquiryDetailsID', N'IX_tblSavingsDetails_EnquiryId', N'IX_tblEnquirySpecification_EnqDetailsID',
    N'IX_tblEnquirySpecification_EnquiryID', N'IX_tblClintDeliveryAddress_Enqid', N'IX_EB_tblPauseExtend_EnquiryId',
    N'IX_tblSupplierQuoteHistory_SupplierID', N'IX_tblSupplierUsers_SupplierId', N'IX_tblDivisionBrandsorproducts_Divisionid',
    N'IX_tblDivision_ClientID', N'IX_tblClient_Department', N'IX_tblClient_Userid', N'IX_tblAgencyUsers_Userid',
    N'IX_tblEnquiryMaster_TemplateId', N'IX_tblTypeofItem_SubitemId', N'IX_TblNewSavingsdata_Country_Curr');
IF @sql IS NOT NULL BEGIN EXEC (@sql); PRINT 'Dropped indexes'; END

-- 02: primary keys added to former heaps (tables become heaps again)
SET @sql = NULL;
SELECT @sql = STRING_AGG(CONVERT(nvarchar(max), N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
              + N' DROP CONSTRAINT ' + QUOTENAME(k.name) + N';'), NCHAR(10))
FROM sys.key_constraints k JOIN sys.tables t ON t.object_id = k.parent_object_id
WHERE k.type = 'PK' AND k.name IN (
    N'PK_tblJobStatus', N'PK_EBtblJobStatus', N'PK_TBLMANAGMENTTYPE', N'PK_tblcurrency', N'PK_TBLExceptionreason',
    N'PK_tblRegionalcatalogname', N'PK_tblFieldTypes', N'PK_tblAttachments', N'PK_tblSurvey', N'PK_tblDivisionBrandsorproducts',
    N'PK_tblpwdhistory', N'PK_TBLCATALOGLOGINDETAILS', N'PK_tblEnvironmentresult', N'PK_tblFieldValues', N'PK_tblversiondata',
    N'PK_tblProductCatalogue', N'PK_tblCatalog_MailDraft', N'PK_tblcatalogUsers', N'PK_catalog_tblenquirydetails',
    N'PK_tblenvironmentalimpact', N'PK_tblEnvironmentEmission', N'PK_tblenvironmentcomparison', N'PK_tblMessage', N'PK_EB_LiveFeed');
IF @sql IS NOT NULL BEGIN EXEC (@sql); PRINT 'Dropped primary keys'; END

COMMIT TRANSACTION;
