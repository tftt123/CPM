-- ============================================================
-- Phase 1 审批流升级数据库迁移脚本
-- 执行条件：已有 baseline Migration 的数据库
-- 目标：新增字段 + 新表，向后兼容现有数据
-- ============================================================

USE CpmDb;
GO

-- ============================================================
-- 1. 扩展 SysApprovalStep 表（加新字段）
-- ============================================================

-- 步骤模式
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'StepMode' AND Object_ID = Object_ID(N'SysApprovalStep'))
BEGIN
    ALTER TABLE SysApprovalStep ADD StepMode NVARCHAR(50) DEFAULT 'SEQUENTIAL';
    PRINT 'Added StepMode to SysApprovalStep';
END

-- 驳回行为
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'RejectBehavior' AND Object_ID = Object_ID(N'SysApprovalStep'))
BEGIN
    ALTER TABLE SysApprovalStep ADD RejectBehavior NVARCHAR(50) DEFAULT 'REJECT_AND_CLOSE';
    PRINT 'Added RejectBehavior to SysApprovalStep';
END

-- 驳回目标步骤
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'RejectTargetStepId' AND Object_ID = Object_ID(N'SysApprovalStep'))
BEGIN
    ALTER TABLE SysApprovalStep ADD RejectTargetStepId BIGINT NULL;
    PRINT 'Added RejectTargetStepId to SysApprovalStep';
END

-- 超时设置
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'TimeoutHours' AND Object_ID = Object_ID(N'SysApprovalStep'))
BEGIN
    ALTER TABLE SysApprovalStep ADD TimeoutHours INT NULL;
    PRINT 'Added TimeoutHours to SysApprovalStep';
END

-- ============================================================
-- 2. 扩展 SysApprovalInstance 表（加新字段）
-- ============================================================

-- 流程变量 JSON
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'Variables' AND Object_ID = Object_ID(N'SysApprovalInstance'))
BEGIN
    ALTER TABLE SysApprovalInstance ADD Variables NVARCHAR(MAX);
    PRINT 'Added Variables to SysApprovalInstance';
END

-- 发起人Id
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'SubmitterId' AND Object_ID = Object_ID(N'SysApprovalInstance'))
BEGIN
    ALTER TABLE SysApprovalInstance ADD SubmitterId BIGINT NULL;
    PRINT 'Added SubmitterId to SysApprovalInstance';
END

-- ============================================================
-- 3. 新建 SysApprovalRule 表
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SysApprovalRule')
BEGIN
    CREATE TABLE SysApprovalRule (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        StepId BIGINT NOT NULL,
        RuleType NVARCHAR(50) NOT NULL,  -- FIXED_ROLE / FIXED_USER / ORG_TREE / EXPRESSION
        RuleValue NVARCHAR(500),
        Fallback NVARCHAR(500),
        Priority INT DEFAULT 0,
        IsActive BIT DEFAULT 1
    );

    -- 外键关联
    ALTER TABLE SysApprovalRule ADD CONSTRAINT FK_SysApprovalRule_SysApprovalStep
        FOREIGN KEY (StepId) REFERENCES SysApprovalStep(Id) ON DELETE CASCADE;

    -- 索引
    CREATE INDEX IX_SysApprovalRule_StepId ON SysApprovalRule(StepId);

    PRINT 'Created SysApprovalRule table';
END

-- ============================================================
-- 4. 新建 SysApprovalInstanceTask 表
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SysApprovalInstanceTask')
BEGIN
    CREATE TABLE SysApprovalInstanceTask (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        InstanceId BIGINT NOT NULL,
        StepId BIGINT NOT NULL,
        AssigneeId BIGINT NULL,
        AssigneeRole NVARCHAR(50),
        Status INT DEFAULT 0,  -- 0=待办 1=已处理 2=转交 3=超时
        Action NVARCHAR(20),   -- APPROVE / REJECT / TRANSFER
        Comment NVARCHAR(500),
        DueDate DATETIME2,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        CompletedAt DATETIME2
    );

    -- 外键关联
    ALTER TABLE SysApprovalInstanceTask ADD CONSTRAINT FK_SysApprovalInstanceTask_SysApprovalInstance
        FOREIGN KEY (InstanceId) REFERENCES SysApprovalInstance(Id) ON DELETE CASCADE;

    -- 索引
    CREATE INDEX IX_SysApprovalInstanceTask_InstanceId ON SysApprovalInstanceTask(InstanceId);
    CREATE INDEX IX_SysApprovalInstanceTask_Assignee_Status ON SysApprovalInstanceTask(AssigneeId, Status);
    CREATE INDEX IX_SysApprovalInstanceTask_Role_Status ON SysApprovalInstanceTask(AssigneeRole, Status);

    PRINT 'Created SysApprovalInstanceTask table';
END

-- ============================================================
-- 5. 扩展 SysDept 表（添加 ManagerId 用于 ORG_TREE 解析器）
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE Name = N'ManagerId' AND Object_ID = Object_ID(N'SysDept'))
BEGIN
    ALTER TABLE SysDept ADD ManagerId BIGINT NULL;
    PRINT 'Added ManagerId to SysDept';
END

GO

PRINT 'Phase 1 Approval Upgrade completed successfully.';
