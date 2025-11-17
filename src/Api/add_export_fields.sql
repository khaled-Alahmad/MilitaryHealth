USE db30626;
GO

-- =====================================================
-- Add Export Tracking Fields to FinalDecision
-- Script adds fields to track export status to recruitment
-- =====================================================

PRINT 'Starting Export Fields Addition...';
GO

-- Add IsExportedToRecruitment field
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'IsExportedToRecruitment')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [IsExportedToRecruitment] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added IsExportedToRecruitment column to FinalDecision table';
END
ELSE
BEGIN
    PRINT 'IsExportedToRecruitment column already exists';
END
GO

-- Add ExportedAt field
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'ExportedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [ExportedAt] DATETIME NULL;
    
    PRINT 'Added ExportedAt column to FinalDecision table';
END
ELSE
BEGIN
    PRINT 'ExportedAt column already exists';
END
GO

PRINT '========================================';
PRINT 'Export Fields Added Successfully!';
PRINT '========================================';
GO

