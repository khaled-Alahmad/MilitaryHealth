-- Migration Script V2: Multiple System Enhancements
-- Target: Production Server (db30626)
-- Date: 2025-11-17

USE db30626;
GO

-- =====================================================
-- 1. FinalDecision Table - Add Supervisor Date Fields
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') AND name = 'ReceptionAddedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision] 
    ADD [ReceptionAddedAt] DATETIME NULL;
    PRINT 'Added ReceptionAddedAt column to FinalDecision table';
END
ELSE
BEGIN
    PRINT 'ReceptionAddedAt column already exists in FinalDecision table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') AND name = 'SupervisorAddedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision] 
    ADD [SupervisorAddedAt] DATETIME NULL;
    PRINT 'Added SupervisorAddedAt column to FinalDecision table';
END
ELSE
BEGIN
    PRINT 'SupervisorAddedAt column already exists in FinalDecision table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') AND name = 'SupervisorLastModifiedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision] 
    ADD [SupervisorLastModifiedAt] DATETIME NULL;
    PRINT 'Added SupervisorLastModifiedAt column to FinalDecision table';
END
ELSE
BEGIN
    PRINT 'SupervisorLastModifiedAt column already exists in FinalDecision table';
END
GO

-- =====================================================
-- 2. Consultations Table - Replace ReferredDoctor with ReferralReason
-- =====================================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultations]') AND name = 'ReferredDoctor')
BEGIN
    ALTER TABLE [dbo].[Consultations] 
    DROP COLUMN [ReferredDoctor];
    PRINT 'Removed ReferredDoctor column from Consultations table';
END
ELSE
BEGIN
    PRINT 'ReferredDoctor column does not exist in Consultations table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultations]') AND name = 'ReferralReason')
BEGIN
    ALTER TABLE [dbo].[Consultations] 
    ADD [ReferralReason] TEXT NULL;
    PRINT 'Added ReferralReason column to Consultations table';
END
ELSE
BEGIN
    PRINT 'ReferralReason column already exists in Consultations table';
END
GO

-- =====================================================
-- 3. Investigations Table - Add InvestigationReason
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Investigations]') AND name = 'InvestigationReason')
BEGIN
    ALTER TABLE [dbo].[Investigations] 
    ADD [InvestigationReason] TEXT NULL;
    PRINT 'Added InvestigationReason column to Investigations table';
END
ELSE
BEGIN
    PRINT 'InvestigationReason column already exists in Investigations table';
END
GO

-- =====================================================
-- 4. Applicants Table - Add QueueNumber
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Applicants]') AND name = 'QueueNumber')
BEGIN
    ALTER TABLE [dbo].[Applicants] 
    ADD [QueueNumber] INT NULL;
    PRINT 'Added QueueNumber column to Applicants table';
END
ELSE
BEGIN
    PRINT 'QueueNumber column already exists in Applicants table';
END
GO

-- =====================================================
-- 5. Auto-Generate QueueNumbers for Existing Records
-- =====================================================
-- Generate queue numbers based on CreatedAt order for existing records
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
-- 6. Create Trigger for Auto-Generating QueueNumber
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
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    
    -- Get the maximum QueueNumber from records created today only
    SELECT @MaxQueueNumber = ISNULL(MAX(QueueNumber), 0)
    FROM [dbo].[Applicants]
    WHERE CAST(CreatedAt AS DATE) = @Today
      AND ApplicantID NOT IN (SELECT ApplicantID FROM inserted);
    
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

PRINT 'Created trg_GenerateQueueNumber trigger for auto-generating queue numbers';
GO

-- =====================================================
-- Summary
-- =====================================================
PRINT '========================================';
PRINT 'Migration V2 completed successfully!';
PRINT 'Changes applied:';
PRINT '1. Added 3 date columns to FinalDecision';
PRINT '2. Replaced ReferredDoctor with ReferralReason in Consultations';
PRINT '3. Added InvestigationReason to Investigations';
PRINT '4. Added QueueNumber to Applicants with auto-generation trigger';
PRINT '========================================';
GO

