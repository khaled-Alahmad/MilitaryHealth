USE db30626;
GO

-- =====================================================
-- Comprehensive Migration V3
-- Includes all new features:
-- 1. Export tracking fields in FinalDecision
-- 2. Eye exam new fields (WorstRefractionRight/Left)
-- 3. Queue number with daily reset
-- =====================================================

PRINT 'Starting Comprehensive Migration V3...';
PRINT '========================================';
GO

-- =====================================================
-- 1. Add Export Tracking Fields to FinalDecision
-- =====================================================

PRINT 'Section 1: Adding Export Tracking Fields...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'IsExportedToRecruitment')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [IsExportedToRecruitment] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added IsExportedToRecruitment column';
END
ELSE
BEGIN
    PRINT 'IsExportedToRecruitment column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'ExportedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [ExportedAt] DATETIME NULL;
    
    PRINT 'Added ExportedAt column';
END
ELSE
BEGIN
    PRINT 'ExportedAt column already exists';
END
GO

-- =====================================================
-- 2. Add Eye Exam Fields
-- =====================================================

PRINT 'Section 2: Adding Eye Exam Fields...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[EyeExam]') 
               AND name = 'WorstRefractionRight')
BEGIN
    ALTER TABLE [dbo].[EyeExam]
    ADD [WorstRefractionRight] VARCHAR(100) NULL;
    
    PRINT 'Added WorstRefractionRight column';
END
ELSE
BEGIN
    PRINT 'WorstRefractionRight column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[EyeExam]') 
               AND name = 'WorstRefractionLeft')
BEGIN
    ALTER TABLE [dbo].[EyeExam]
    ADD [WorstRefractionLeft] VARCHAR(100) NULL;
    
    PRINT 'Added WorstRefractionLeft column';
END
ELSE
BEGIN
    PRINT 'WorstRefractionLeft column already exists';
END
GO

-- =====================================================
-- 3. Queue Number with Daily Reset (if not exists)
-- =====================================================

PRINT 'Section 3: Queue Number Setup...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Applicants]') 
               AND name = 'QueueNumber')
BEGIN
    ALTER TABLE [dbo].[Applicants]
    ADD [QueueNumber] INT NULL;
    
    PRINT 'Added QueueNumber column';
    
    -- Generate queue numbers for existing records
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
END
ELSE
BEGIN
    PRINT 'QueueNumber column already exists';
END
GO

-- Drop and recreate trigger for daily reset
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

PRINT 'Created trg_GenerateQueueNumber trigger with daily reset';
GO

PRINT '========================================';
PRINT 'Comprehensive Migration V3 Completed!';
PRINT 'Summary:';
PRINT '- Export tracking fields added';
PRINT '- Eye exam worst refraction fields added';
PRINT '- Queue number with daily reset configured';
PRINT '========================================';
GO

