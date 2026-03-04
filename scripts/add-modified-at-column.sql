-- Add ModifiedAt column to Applicants (for system-managed update timestamp).
-- Run this against your database if the EF migration was not applied.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Applicants') AND name = 'ModifiedAt'
)
BEGIN
    ALTER TABLE dbo.Applicants ADD ModifiedAt datetime NULL;
END
GO

-- Register the migration so EF Core does not try to apply it again.
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260304120000_AddApplicantModifiedAt')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260304120000_AddApplicantModifiedAt', N'9.0.8');
END
GO
