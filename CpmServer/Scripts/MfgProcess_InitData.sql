-- =============================================
-- 工艺管理模块 数据库建表 + 初始化数据
-- 表名: MfgCategory, MfgProcess, MfgSubCategory, MfgEquipment
-- =============================================

-- 1. 大类表 (MfgCategory)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MfgCategory')
CREATE TABLE MfgCategory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    CategoryCode NVARCHAR(50) NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Site NVARCHAR(50) NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 2. 工序表 (MfgProcess)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MfgProcess')
CREATE TABLE MfgProcess (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    CategoryId BIGINT NOT NULL,
    ProcessCode NVARCHAR(50) NULL,
    ProcessName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Site NVARCHAR(50) NULL,
    StdTimeMin DECIMAL(18,2) NULL,
    CostRate DECIMAL(18,2) NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MfgProcess_Category FOREIGN KEY (CategoryId) REFERENCES MfgCategory(Id) ON DELETE CASCADE
);

-- 3. 子类表 (MfgSubCategory)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MfgSubCategory')
CREATE TABLE MfgSubCategory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProcessId BIGINT NOT NULL,
    SubCategoryCode NVARCHAR(50) NULL,
    SubCategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Site NVARCHAR(50) NULL,
    ToleranceGrade NVARCHAR(50) NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MfgSubCategory_Process FOREIGN KEY (ProcessId) REFERENCES MfgProcess(Id) ON DELETE CASCADE
);

-- 4. 设备表 (MfgEquipment)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MfgEquipment')
CREATE TABLE MfgEquipment (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    SubCategoryId BIGINT NOT NULL,
    EquipmentCode NVARCHAR(50) NULL,
    EquipmentName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Site NVARCHAR(50) NULL,
    Model NVARCHAR(100) NULL,
    Spec NVARCHAR(200) NULL,
    Manufacturer NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MfgEquipment_SubCategory FOREIGN KEY (SubCategoryId) REFERENCES MfgSubCategory(Id) ON DELETE CASCADE
);
GO

-- Site 索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgCategory_Site')
CREATE INDEX IX_MfgCategory_Site ON MfgCategory(Site);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgProcess_Site')
CREATE INDEX IX_MfgProcess_Site ON MfgProcess(Site);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgSubCategory_Site')
CREATE INDEX IX_MfgSubCategory_Site ON MfgSubCategory(Site);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgEquipment_Site')
CREATE INDEX IX_MfgEquipment_Site ON MfgEquipment(Site);
GO

-- =============================================
-- 初始化数据 - 系统维护工艺示例
-- =============================================

-- 大类: 系统维护
SET IDENTITY_INSERT MfgCategory ON;
INSERT INTO MfgCategory (Id, CategoryCode, CategoryName, Description, Site, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 1, N'SYS', N'系统维护', N'设备与系统维护工艺', N'NT01', 1, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgCategory WHERE CategoryCode = N'SYS');
SET IDENTITY_INSERT MfgCategory OFF;
GO

-- 工序: 系统清洗, 系统调试, 系统检查
SET IDENTITY_INSERT MfgProcess ON;
INSERT INTO MfgProcess (Id, CategoryId, ProcessCode, ProcessName, Description, Site, StdTimeMin, CostRate, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 1, 1, N'CLEAN', N'系统清洗', N'清洗与去污工序', N'NT01', 30, 120.00, 1, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgProcess WHERE ProcessCode = N'CLEAN');
INSERT INTO MfgProcess (Id, CategoryId, ProcessCode, ProcessName, Description, Site, StdTimeMin, CostRate, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 2, 1, N'DEBUG', N'系统调试', N'参数调试与优化', N'NT01', 45, 150.00, 2, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgProcess WHERE ProcessCode = N'DEBUG');
INSERT INTO MfgProcess (Id, CategoryId, ProcessCode, ProcessName, Description, Site, StdTimeMin, CostRate, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 3, 1, N'INSPECT', N'系统检查', N'质量与功能检查', N'NT01', 20, 100.00, 3, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgProcess WHERE ProcessCode = N'INSPECT');
SET IDENTITY_INSERT MfgProcess OFF;
GO

-- 子类 - 系统清洗
SET IDENTITY_INSERT MfgSubCategory ON;
INSERT INTO MfgSubCategory (Id, ProcessId, SubCategoryCode, SubCategoryName, Description, Site, ToleranceGrade, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 1, 1, N'PRE_CLEAN', N'预清洗', N'初步去除表面污垢', N'NT01', N'IT8', 1, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgSubCategory WHERE SubCategoryCode = N'PRE_CLEAN');
INSERT INTO MfgSubCategory (Id, ProcessId, SubCategoryCode, SubCategoryName, Description, Site, ToleranceGrade, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 2, 1, N'DEEP_CLEAN', N'深度清洗', N'深度去污与干燥', N'NT01', N'IT7', 2, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgSubCategory WHERE SubCategoryCode = N'DEEP_CLEAN');
-- 子类 - 系统调试
INSERT INTO MfgSubCategory (Id, ProcessId, SubCategoryCode, SubCategoryName, Description, Site, ToleranceGrade, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 3, 2, N'PARAM_TUNE', N'参数调谐', N'调整系统运行参数', N'NT01', N'IT6', 1, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgSubCategory WHERE SubCategoryCode = N'PARAM_TUNE');
INSERT INTO MfgSubCategory (Id, ProcessId, SubCategoryCode, SubCategoryName, Description, Site, ToleranceGrade, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 4, 2, N'CALIBRATE', N'校准标定', N'精度校准与标定', N'NT01', N'IT5', 2, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgSubCategory WHERE SubCategoryCode = N'CALIBRATE');
-- 子类 - 系统检查
INSERT INTO MfgSubCategory (Id, ProcessId, SubCategoryCode, SubCategoryName, Description, Site, ToleranceGrade, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 5, 3, N'VISUAL_CHK', N'目视检查', N'外观与表面检查', N'NT01', N'IT8', 1, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgSubCategory WHERE SubCategoryCode = N'VISUAL_CHK');
INSERT INTO MfgSubCategory (Id, ProcessId, SubCategoryCode, SubCategoryName, Description, Site, ToleranceGrade, SortOrder, IsActive, CreatedAt, UpdatedAt)
SELECT 6, 3, N'FUNC_TEST', N'功能测试', N'功能与性能测试', N'NT01', N'IT7', 2, 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgSubCategory WHERE SubCategoryCode = N'FUNC_TEST');
SET IDENTITY_INSERT MfgSubCategory OFF;
GO

-- 设备 - 预清洗
SET IDENTITY_INSERT MfgEquipment ON;
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 1, 1, N'PUMP-A01', N'高压清洗泵', N'型号: HP-3000, 压力: 30MPa, 流量: 50L/min', N'NT01', N'HP-3000', N'30MPa/50L/min', N'清洗科技', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'PUMP-A01');
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 2, 1, N'SOLV-T01', N'溶剂清洗槽', N'型号: ST-200L, 容量: 200L, 温控: 20-80°C', N'NT01', N'ST-200L', N'200L/20-80°C', N'清洗科技', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'SOLV-T01');
-- 设备 - 深度清洗
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 3, 2, N'ULTRA-C01', N'超声波清洗机', N'型号: UC-500W, 功率: 500W, 频率: 40kHz', N'NT01', N'UC-500W', N'500W/40kHz', N'超声设备厂', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'ULTRA-C01');
-- 设备 - 参数调谐
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 4, 3, N'SCOPE-D01', N'数字示波器', N'型号: DSO-X3054T, 带宽: 500MHz, 采样率: 4GSa/s', N'NT01', N'DSO-X3054T', N'500MHz/4GSa/s', N'Keysight', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'SCOPE-D01');
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 5, 3, N'SIGGEN-A01', N'信号发生器', N'型号: SG-33600A, 频率: 80MHz, 通道: 2CH', N'NT01', N'SG-33600A', N'80MHz/2CH', N'Keysight', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'SIGGEN-A01');
-- 设备 - 校准标定
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 6, 4, N'MULTI-C01', N'六位半万用表', N'型号: 34465A, 分辨率: 6.5位, 精度: 0.0024%', N'NT01', N'34465A', N'6.5位/0.0024%', N'Keysight', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'MULTI-C01');
-- 设备 - 目视检查
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 7, 5, N'BORE-V01', N'工业内窥镜', N'型号: BVS-360, 探头: 6mm, 像素: 1080P', N'NT01', N'BVS-360', N'6mm/1080P', N'视觉检测', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'BORE-V01');
-- 设备 - 功能测试
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 8, 6, N'PWR-A01', N'可编程电源', N'型号: PPS-30V10A, 电压: 0-30V, 电流: 0-10A', N'NT01', N'PPS-30V10A', N'0-30V/0-10A', N'电源科技', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'PWR-A01');
INSERT INTO MfgEquipment (Id, SubCategoryId, EquipmentCode, EquipmentName, Description, Site, Model, Spec, Manufacturer, IsActive, CreatedAt, UpdatedAt)
SELECT 9, 6, N'LOAD-T01', N'电子负载', N'型号: EL-150W, 功率: 150W, 电压: 0-150V', N'NT01', N'EL-150W', N'150W/0-150V', N'电源科技', 1, GETDATE(), GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM MfgEquipment WHERE EquipmentCode = N'LOAD-T01');
SET IDENTITY_INSERT MfgEquipment OFF;
GO

PRINT '工艺管理模块 数据库表创建及初始化数据完成。'
