USE db30626;
GO

-- =====================================================
-- Add New Fields to EyeExam Table
-- =====================================================

PRINT 'Starting Eye Exam Fields Addition...';
GO

-- Add WorstRefractionRight field
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[EyeExam]') 
               AND name = 'WorstRefractionRight')
BEGIN
    ALTER TABLE [dbo].[EyeExam]
    ADD [WorstRefractionRight] VARCHAR(100) NULL;
    
    PRINT 'Added WorstRefractionRight column to EyeExam table';
END
ELSE
BEGIN
    PRINT 'WorstRefractionRight column already exists';
END
GO

-- Add WorstRefractionLeft field
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[EyeExam]') 
               AND name = 'WorstRefractionLeft')
BEGIN
    ALTER TABLE [dbo].[EyeExam]
    ADD [WorstRefractionLeft] VARCHAR(100) NULL;
    
    PRINT 'Added WorstRefractionLeft column to EyeExam table';
END
ELSE
BEGIN
    PRINT 'WorstRefractionLeft column already exists';
END
GO

PRINT '========================================';
PRINT 'Eye Exam Fields Added Successfully!';
PRINT '========================================';
GO

