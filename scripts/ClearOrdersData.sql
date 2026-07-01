SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT 'Removing Orders-module data...';

    DELETE FROM dbo.OrderAssignments;
    DELETE FROM dbo.OrderBuildRecordLinks;
    DELETE FROM dbo.OrderChecklistItems;
    DELETE FROM dbo.OrderLabels;
    DELETE FROM dbo.OrderNotes;
    DELETE FROM dbo.OrderStatusHistory;
    DELETE FROM dbo.OrderImportWarnings;
    DELETE FROM dbo.OrderImportBatches;
    DELETE FROM dbo.OrderRecords;

    DBCC CHECKIDENT ('dbo.OrderAssignments', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderBuildRecordLinks', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderChecklistItems', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderLabels', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderNotes', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderStatusHistory', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderImportWarnings', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderImportBatches', RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderRecords', RESEED, 0);

    COMMIT TRANSACTION;

    PRINT 'Orders data removed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
