-- 清空工艺成本相关表（按外键依赖顺序，先子表后父表）
DELETE FROM MfgEquipments;
DELETE FROM MfgSubCategories;
DELETE FROM MfgProcesses;

-- 如需重置自增列编号，取消下面注释：
-- DBCC CHECKIDENT ('MfgEquipments', RESEED, 0);
-- DBCC CHECKIDENT ('MfgSubCategories', RESEED, 0);
-- DBCC CHECKIDENT ('MfgProcesses', RESEED, 0);
