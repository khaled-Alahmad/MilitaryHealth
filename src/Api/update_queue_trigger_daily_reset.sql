USE db30626;
GO

-- =====================================================
-- Update QueueNumber Trigger for Daily Reset
-- This script updates the trigger to reset queue numbers daily
-- =====================================================

PRINT 'Starting Queue Number Trigger Update...';
GO

-- Drop existing trigger if it exists
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_GenerateQueueNumber')
BEGIN
    DROP TRIGGER [dbo].[trg_GenerateQueueNumber];
    PRINT 'Dropped existing trg_GenerateQueueNumber trigger';
END
GO

-- Create updated trigger with daily reset
CREATE TRIGGER [dbo].[trg_GenerateQueueNumber]
ON [dbo].[Applicants]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MaxQueueNumber INT;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    
    -- Get the maximum QueueNumber from records created TODAY only
    -- This ensures queue number resets to 1 each day
    SELECT @MaxQueueNumber = ISNULL(MAX(QueueNumber), 0)
    FROM [dbo].[Applicants]
    WHERE CAST(CreatedAt AS DATE) = @Today
      AND ApplicantID NOT IN (SELECT ApplicantID FROM inserted);
    
    -- Update inserted records with auto-incremented QueueNumber
    ;WITH NumberedInserted AS (
        SELECT 
            ApplicantID,
            ROW_NUMBER() OVER (ORDER BY ApplicantID) AS RowNum
        FROM inserted
    )
    UPDATE a
    SET a.QueueNumber = @MaxQueueNumber + n.RowNum
    FROM [dbo].[Applicants] a
    INNER JOIN NumberedInserted n ON a.ApplicantID = n.ApplicantID
    WHERE a.QueueNumber IS NULL;
END
GO

PRINT '========================================';
PRINT 'Queue Number Trigger Updated Successfully!';
PRINT 'Queue numbers will now reset to 1 each day';
PRINT '========================================';
GO

