/*
    behaviour_tests.sql

    Verifies, after scripts 02-05, that the application's own procedures still work with the constraints and that
    the constraints reject bad data. EVERY test runs in a transaction that is ROLLED BACK - no data is changed.
    Run it on a UAT/restored copy (it briefly locks the rows it touches). All rows must report PASS.
*/
SET NOCOUNT ON;
SET XACT_ABORT OFF;
DECLARE @r TABLE (test nvarchar(200), expected nvarchar(20), outcome nvarchar(20), detail nvarchar(400));
DECLARE @item int, @enq int, @n1 int, @n2 int, @n3 int, @msg nvarchar(400);

-- T1: EN_DeleteItem on an item that has quotes, specifications and savings -> must succeed, children cascade away
SELECT TOP 1 @item = d.Id, @enq = d.EnquiryID FROM tblEnquiryDetails d
WHERE (SELECT COUNT(*) FROM tblEnquiryDetails x WHERE x.EnquiryID = d.EnquiryID) > 1
  AND EXISTS (SELECT 1 FROM tblSupplierQuotes q WHERE q.EnquiryDetailsID = d.Id)
ORDER BY (SELECT COUNT(*) FROM tblEnquirySpecification s WHERE s.EnqDetailsID = d.Id) DESC,
         (SELECT COUNT(*) FROM tblSavingsDetails s WHERE s.EnquiryDetailsId = d.Id) DESC;
SELECT @n1 = COUNT(*) FROM tblSupplierQuotes WHERE EnquiryDetailsID = @item;
SELECT @n2 = COUNT(*) FROM tblEnquirySpecification WHERE EnqDetailsID = @item;
SELECT @n3 = COUNT(*) FROM tblSavingsDetails WHERE EnquiryDetailsId = @item;
BEGIN TRAN;
BEGIN TRY
    EXEC EN_DeleteItem @EnqDetailsId = @item, @EnqId = @enq;
    INSERT @r SELECT 'T1 EN_DeleteItem (item ' + CAST(@item AS varchar) + ': ' + CAST(@n1 AS varchar) + ' quotes, ' + CAST(@n2 AS varchar) + ' specs, ' + CAST(@n3 AS varchar) + ' savings)', 'success',
        CASE WHEN NOT EXISTS (SELECT 1 FROM tblEnquiryDetails WHERE Id = @item) THEN 'success' ELSE 'not deleted' END,
        'children left: quotes=' + CAST((SELECT COUNT(*) FROM tblSupplierQuotes WHERE EnquiryDetailsID = @item) AS varchar)
        + ' specs=' + CAST((SELECT COUNT(*) FROM tblEnquirySpecification WHERE EnqDetailsID = @item) AS varchar)
        + ' savings=' + CAST((SELECT COUNT(*) FROM tblSavingsDetails WHERE EnquiryDetailsId = @item) AS varchar);
END TRY BEGIN CATCH INSERT @r VALUES ('T1 EN_DeleteItem', 'success', 'FAILED', ERROR_MESSAGE()); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

-- T2: EB_DeleteItem (E-Bidding) on an item with specs
SELECT TOP 1 @item = d.Id, @enq = d.EnquiryID FROM tblEnquiryDetails d JOIN tblEnquiryMaster m ON m.Id = d.EnquiryID
WHERE m.JobType = 'E-Bidding' AND (SELECT COUNT(*) FROM tblEnquiryDetails x WHERE x.EnquiryID = d.EnquiryID) > 1
ORDER BY (SELECT COUNT(*) FROM tblEnquirySpecification s WHERE s.EnqDetailsID = d.Id) DESC;
BEGIN TRAN;
BEGIN TRY
    EXEC EB_DeleteItem @EnqDetailsId = @item, @EnqId = @enq;
    INSERT @r VALUES ('T2 EB_DeleteItem (item ' + CAST(@item AS varchar) + ')', 'success', 'success', '');
END TRY BEGIN CATCH INSERT @r VALUES ('T2 EB_DeleteItem', 'success', 'FAILED', ERROR_MESSAGE()); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

-- T3: soft delete of an enquiry (DeleteEnquiries)
SELECT TOP 1 @enq = Id FROM tblEnquiryMaster WHERE JobType = 'Enquiry' AND Status = 1 ORDER BY Id DESC;
BEGIN TRAN;
BEGIN TRY
    EXEC DeleteEnquiries @Id = @enq;
    INSERT @r VALUES ('T3 DeleteEnquiries (soft delete ' + CAST(@enq AS varchar) + ')', 'success', 'success', '');
END TRY BEGIN CATCH INSERT @r VALUES ('T3 DeleteEnquiries', 'success', 'FAILED', ERROR_MESSAGE()); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

-- T4: UpdateJobStatus (deletes + rebuilds savings) on an enquiry that has savings
SELECT TOP 1 @enq = s.EnquiryId FROM tblSavingsDetails s JOIN tblEnquiryDetails d ON d.EnquiryID = s.EnquiryId ORDER BY s.EnquiryId DESC;
SELECT TOP 1 @n1 = d.Status FROM tblEnquiryDetails d WHERE d.EnquiryID = @enq;
BEGIN TRAN;
BEGIN TRY
    EXEC UpdateJobStatus @EnquiryID = @enq, @ModifiedBy = 1, @Status = @n1;
    INSERT @r VALUES ('T4 UpdateJobStatus (enquiry ' + CAST(@enq AS varchar) + ', status ' + CAST(@n1 AS varchar) + ')', 'success', 'success', '');
END TRY BEGIN CATCH INSERT @r VALUES ('T4 UpdateJobStatus', 'success', 'FAILED', ERROR_MESSAGE()); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

-- T5..T9: constraints must reject bad data
BEGIN TRAN;
BEGIN TRY INSERT tblEnquiryDetails (EnquiryID, Status) VALUES (999999999, 0); INSERT @r VALUES ('T5 item for a non-existent enquiry', 'rejected', 'ACCEPTED', ''); END TRY
BEGIN CATCH INSERT @r VALUES ('T5 item for a non-existent enquiry', 'rejected', 'rejected', LEFT(ERROR_MESSAGE(), 120)); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

BEGIN TRAN;
BEGIN TRY UPDATE TOP (1) tblEnquiryDetails SET Status = 99; INSERT @r VALUES ('T6 status 99 (unknown code)', 'rejected', 'ACCEPTED', ''); END TRY
BEGIN CATCH INSERT @r VALUES ('T6 status 99 (unknown code)', 'rejected', 'rejected', LEFT(ERROR_MESSAGE(), 120)); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

BEGIN TRAN;
BEGIN TRY UPDATE TOP (1) tblEnquiryMaster SET EnqUId = (SELECT TOP 1 EnqUId FROM tblEnquiryMaster WHERE EnqUId IS NOT NULL ORDER BY Id) WHERE Id = (SELECT MAX(Id) FROM tblEnquiryMaster);
      INSERT @r VALUES ('T7 duplicate enquiry number (EnqUId)', 'rejected', 'ACCEPTED', ''); END TRY
BEGIN CATCH INSERT @r VALUES ('T7 duplicate enquiry number (EnqUId)', 'rejected', 'rejected', LEFT(ERROR_MESSAGE(), 120)); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

BEGIN TRAN;
BEGIN TRY INSERT tblSupplierQuotes (EnquiryID, EnquiryDetailsID, SupplierID) SELECT TOP 1 EnquiryID, Id, 999999999 FROM tblEnquiryDetails;
      INSERT @r VALUES ('T8 quote for a non-existent supplier', 'rejected', 'ACCEPTED', ''); END TRY
BEGIN CATCH INSERT @r VALUES ('T8 quote for a non-existent supplier', 'rejected', 'rejected', LEFT(ERROR_MESSAGE(), 120)); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

-- T9: the API's create flow - master with EnqUId NULL, then detail, then EnqUId set - twice in one transaction
BEGIN TRAN;
BEGIN TRY
    DECLARE @m1 int, @m2 int;
    INSERT tblEnquiryMaster (DivisionId, ClientContactId, DeliveryCountry, PmgEntity, CreatedBy, ModifiedBy, ServerTime, Status, JobType, StepId)
    SELECT TOP 1 DivisionId, ClientContactId, DeliveryCountry, PmgEntity, CreatedBy, ModifiedBy, GETDATE(), 0, 'Enquiry', 1 FROM tblEnquiryMaster ORDER BY Id DESC;
    SET @m1 = SCOPE_IDENTITY();
    INSERT tblEnquiryMaster (DivisionId, ClientContactId, DeliveryCountry, PmgEntity, CreatedBy, ModifiedBy, ServerTime, Status, JobType, StepId)
    SELECT TOP 1 DivisionId, ClientContactId, DeliveryCountry, PmgEntity, CreatedBy, ModifiedBy, GETDATE(), 0, 'Enquiry', 1 FROM tblEnquiryMaster ORDER BY Id DESC;
    SET @m2 = SCOPE_IDENTITY();
    INSERT tblEnquiryDetails (EnquiryID, Status) VALUES (@m1, 0), (@m2, 0);
    UPDATE tblEnquiryMaster SET EnqUId = 'ENQ' + RIGHT('000000' + CAST(Id AS varchar), 6), Status = 1 WHERE Id IN (@m1, @m2);
    INSERT @r VALUES ('T9 create flow (2 enquiries, NULL EnqUId first)', 'success', 'success', '');
END TRY BEGIN CATCH INSERT @r VALUES ('T9 create flow', 'success', 'FAILED', ERROR_MESSAGE()); END CATCH
IF @@TRANCOUNT > 0 ROLLBACK;

SELECT test, expected, outcome, CASE WHEN expected = outcome THEN 'PASS' ELSE 'FAIL' END AS result, detail FROM @r;
