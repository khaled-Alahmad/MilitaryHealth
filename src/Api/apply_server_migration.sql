-- Migration: Add new fields to Applicants table and remove Hearing from InternalExam
-- Target: Production Server
-- Date: 2025-11-17

-- Add new columns to Applicants table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Applicants]') AND name = 'MotherName')
BEGIN
    ALTER TABLE [dbo].[Applicants] 
    ADD [MotherName] VARCHAR(100) NULL;
    PRINT 'Added MotherName column to Applicants table';
END
ELSE
BEGIN
    PRINT 'MotherName column already exists in Applicants table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Applicants]') AND name = 'DateOfBirth')
BEGIN
    ALTER TABLE [dbo].[Applicants] 
    ADD [DateOfBirth] DATETIME NULL;
    PRINT 'Added DateOfBirth column to Applicants table';
END
ELSE
BEGIN
    PRINT 'DateOfBirth column already exists in Applicants table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Applicants]') AND name = 'RecruitmentCenter')
BEGIN
    ALTER TABLE [dbo].[Applicants] 
    ADD [RecruitmentCenter] VARCHAR(200) NULL;
    PRINT 'Added RecruitmentCenter column to Applicants table';
END
ELSE
BEGIN
    PRINT 'RecruitmentCenter column already exists in Applicants table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Applicants]') AND name = 'BloodType')
BEGIN
    ALTER TABLE [dbo].[Applicants] 
    ADD [BloodType] VARCHAR(10) NULL;
    PRINT 'Added BloodType column to Applicants table';
END
ELSE
BEGIN
    PRINT 'BloodType column already exists in Applicants table';
END
GO

-- Remove Hearing column from InternalExam table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InternalExam]') AND name = 'Hearing')
BEGIN
    ALTER TABLE [dbo].[InternalExam] 
    DROP COLUMN [Hearing];
    PRINT 'Removed Hearing column from InternalExam table';
END
ELSE
BEGIN
    PRINT 'Hearing column does not exist in InternalExam table';
END
GO

PRINT 'Schema migration completed successfully on production server!';
GO

