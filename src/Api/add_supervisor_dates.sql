USE db30626;  -- أو اسم قاعدة بياناتك المحلية
GO

-- =====================================================
-- Add Supervisor Date Fields to FinalDecision
-- إضافة حقول التواريخ من المشرف والاستقبال
-- =====================================================

PRINT 'Starting to add Supervisor and Reception date fields...';
GO

-- إضافة تاريخ الإضافة من الاستقبال
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'ReceptionAddedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [ReceptionAddedAt] DATETIME NULL;
    
    PRINT '✅ Added ReceptionAddedAt column';
END
ELSE
BEGIN
    PRINT 'ℹ️ ReceptionAddedAt column already exists';
END
GO

-- إضافة تاريخ الإضافة من المشرف
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'SupervisorAddedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [SupervisorAddedAt] DATETIME NULL;
    
    PRINT '✅ Added SupervisorAddedAt column';
END
ELSE
BEGIN
    PRINT 'ℹ️ SupervisorAddedAt column already exists';
END
GO

-- إضافة تاريخ آخر تعديل من المشرف
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[FinalDecision]') 
               AND name = 'SupervisorLastModifiedAt')
BEGIN
    ALTER TABLE [dbo].[FinalDecision]
    ADD [SupervisorLastModifiedAt] DATETIME NULL;
    
    PRINT '✅ Added SupervisorLastModifiedAt column';
END
ELSE
BEGIN
    PRINT 'ℹ️ SupervisorLastModifiedAt column already exists';
END
GO

-- التحقق من الأعمدة الجديدة
PRINT '========================================';
PRINT 'Verifying columns...';
PRINT '========================================';

SELECT 
    COLUMN_NAME AS 'Column Name',
    DATA_TYPE AS 'Data Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'FinalDecision'
  AND COLUMN_NAME IN ('ReceptionAddedAt', 'SupervisorAddedAt', 'SupervisorLastModifiedAt', 'IsExportedToRecruitment', 'ExportedAt')
ORDER BY COLUMN_NAME;

PRINT '========================================';
PRINT '✅ All Supervisor date fields added successfully!';
PRINT '========================================';
GO

