-- =============================================
-- 迁移脚本：废除 MfgCategory 表
-- 将 CategoryId 外键改为 Category 字符串
-- =============================================

-- 1. 在 MfgProcess 表添加 Category 字符串列
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgProcess') AND name = 'Category')
    ALTER TABLE MfgProcess ADD Category NVARCHAR(50) NULL;
GO

-- 2. 将现有 CategoryId 数据映射到 Category 字符串
UPDATE p
SET p.Category = c.CategoryName
FROM MfgProcess p
INNER JOIN MfgCategory c ON p.CategoryId = c.Id;
GO

-- 3. 删除外键约束
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MfgProcess_Category')
    ALTER TABLE MfgProcess DROP CONSTRAINT FK_MfgProcess_Category;
GO

-- 4. 删除 CategoryId 列
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MfgProcess') AND name = 'CategoryId')
    ALTER TABLE MfgProcess DROP COLUMN CategoryId;
GO

-- 5. Category 列设为 NOT NULL（有数据后）
-- ALTER TABLE MfgProcess ALTER COLUMN Category NVARCHAR(50) NOT NULL;

-- 6. 删除 MfgCategory 表
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MfgCategory')
    DROP TABLE MfgCategory;
GO

-- 7. 删除 MfgCategory 相关索引
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MfgCategory_Site' AND object_id = OBJECT_ID('MfgCategory'))
    DROP INDEX IX_MfgCategory_Site ON MfgCategory;
GO

PRINT 'MfgCategory 表废除完成，Category 已迁移为字符串。'
