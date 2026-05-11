-- Add CostRate column to MfgEquipment table
-- This column stores the hourly rate / 小时费率 for each equipment

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'CostRate'
    AND Object_ID = Object_ID(N'MfgEquipment')
)
BEGIN
    ALTER TABLE MfgEquipment
    ADD CostRate DECIMAL(18, 4) NULL;

    PRINT 'CostRate column added to MfgEquipment.';
END
ELSE
BEGIN
    PRINT 'CostRate column already exists in MfgEquipment.';
END
GO
