-- =============================================
-- 工艺管理模块 - 添加 Site 及扩展字段
-- 在现有表上执行 ALTER TABLE
-- =============================================

-- 1. MfgCategory
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgCategory') AND name = 'Description')
    ALTER TABLE MfgCategory ADD Description NVARCHAR(500) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgCategory') AND name = 'Site')
    ALTER TABLE MfgCategory ADD Site NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgCategory_Site' AND object_id = OBJECT_ID('MfgCategory'))
    CREATE INDEX IX_MfgCategory_Site ON MfgCategory(Site);
GO

-- 2. MfgProcess
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgProcess') AND name = 'Description')
    ALTER TABLE MfgProcess ADD Description NVARCHAR(500) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgProcess') AND name = 'Site')
    ALTER TABLE MfgProcess ADD Site NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgProcess') AND name = 'StdTimeMin')
    ALTER TABLE MfgProcess ADD StdTimeMin DECIMAL(18,2) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgProcess') AND name = 'CostRate')
    ALTER TABLE MfgProcess ADD CostRate DECIMAL(18,2) NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgProcess_Site' AND object_id = OBJECT_ID('MfgProcess'))
    CREATE INDEX IX_MfgProcess_Site ON MfgProcess(Site);
GO

-- 3. MfgSubCategory
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgSubCategory') AND name = 'Description')
    ALTER TABLE MfgSubCategory ADD Description NVARCHAR(500) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgSubCategory') AND name = 'Site')
    ALTER TABLE MfgSubCategory ADD Site NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgSubCategory') AND name = 'ToleranceGrade')
    ALTER TABLE MfgSubCategory ADD ToleranceGrade NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgSubCategory_Site' AND object_id = OBJECT_ID('MfgSubCategory'))
    CREATE INDEX IX_MfgSubCategory_Site ON MfgSubCategory(Site);
GO

-- 4. MfgEquipment
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgEquipment') AND name = 'Site')
    ALTER TABLE MfgEquipment ADD Site NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgEquipment') AND name = 'Model')
    ALTER TABLE MfgEquipment ADD Model NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgEquipment') AND name = 'Spec')
    ALTER TABLE MfgEquipment ADD Spec NVARCHAR(200) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgEquipment') AND name = 'Manufacturer')
    ALTER TABLE MfgEquipment ADD Manufacturer NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgEquipment_Site' AND object_id = OBJECT_ID('MfgEquipment'))
    CREATE INDEX IX_MfgEquipment_Site ON MfgEquipment(Site);
GO

PRINT '工艺管理模块扩展字段添加完成。'
