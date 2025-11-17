-- Fix Script: Generate QueueNumbers and Fix Trigger
-- Target: Production Server (db30626)
-- Date: 2025-11-17

USE db30626;
GO

-- =====================================================
-- 1. Fix: Generate QueueNumbers for Existing Records
-- =====================================================
PRINT 'Generating QueueNumbers for existing applicants...';

;WITH NumberedApplicants AS (
    SELECT 
        ApplicantID,
        ROW_NUMBER() OVER (ORDER BY ISNULL(CreatedAt, GETDATE()), ApplicantID) AS RowNum
    FROM [dbo].[Applicants]
    WHERE [QueueNumber] IS NULL
)
UPDATE [dbo].[Applicants]
SET [QueueNumber] = n.RowNum
FROM [dbo].[Applicants] a
INNER JOIN NumberedApplicants n ON a.ApplicantID = n.ApplicantID;

PRINT 'Generated QueueNumbers for existing applicants';
GO

-- =====================================================
-- 2. Fix: Drop and Recreate Trigger
-- =====================================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_GenerateQueueNumber')
BEGIN
    DROP TRIGGER [dbo].[trg_GenerateQueueNumber];
    PRINT 'Dropped existing trg_GenerateQueueNumber trigger';
END
GO

CREATE TRIGGER [dbo].[trg_GenerateQueueNumber]
ON [dbo].[Applicants]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MaxQueueNumber INT;
    
    -- Get the maximum QueueNumber from existing records
    SELECT @MaxQueueNumber = ISNULL(MAX(QueueNumber), 0)
    FROM [dbo].[Applicants]
    WHERE ApplicantID NOT IN (SELECT ApplicantID FROM inserted);
    
    -- Update inserted records with auto-incremented QueueNumber using CTE
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

PRINT '✓ Created trg_GenerateQueueNumber trigger successfully';
GO

-- =====================================================
-- 3. Verify Results
-- =====================================================
PRINT '========================================';
PRINT 'Verification:';
PRINT '========================================';

-- Check QueueNumbers
SELECT 
    COUNT(*) AS TotalApplicants,
    COUNT(QueueNumber) AS ApplicantsWithQueueNumber,
    MIN(QueueNumber) AS MinQueueNumber,
    MAX(QueueNumber) AS MaxQueueNumber
FROM [dbo].[Applicants];

PRINT '========================================';
PRINT 'Fix completed successfully!';
PRINT '========================================';
GO

