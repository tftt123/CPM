-- ============================================
-- CPM PM Module - Create Product Trace Tables
-- Run this in SQL Server Management Studio (SSMS)
-- against your CpmDb database
-- ============================================

-- 1. PmProjectTrace (产品跟踪主表)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmProjectTrace')
BEGIN
    CREATE TABLE [PmProjectTrace] (
        [Id] bigint NOT NULL IDENTITY,
        [QuotationId] bigint NULL,
        [CustomerId] bigint NOT NULL,
        [CustomerName] nvarchar(450) NOT NULL,
        [ProductId] bigint NULL,
        [ProductCode] nvarchar(450) NOT NULL,
        [ProductName] nvarchar(max) NULL,
        [PlannedQty] int NULL,
        [ProjectStartDate] datetime2 NULL,
        [DisplayWeeks] int NULL,
        [Status] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PmProjectTrace] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PmProjectTrace_CrmCustomer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CrmCustomer] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PmProjectTrace_CrmProduct_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [CrmProduct] ([Id]),
        CONSTRAINT [FK_PmProjectTrace_QuoQuotation_QuotationId] FOREIGN KEY ([QuotationId]) REFERENCES [QuoQuotation] ([Id])
    );

    CREATE INDEX [IX_PmProjectTrace_CustomerId] ON [PmProjectTrace] ([CustomerId]);
    CREATE INDEX [IX_PmProjectTrace_ProductId] ON [PmProjectTrace] ([ProductId]);
    CREATE INDEX [IX_PmProjectTrace_QuotationId] ON [PmProjectTrace] ([QuotationId]);
    CREATE INDEX [IX_PmProjectTrace_CustomerName_ProductCode_Status_CreatedAt] ON [PmProjectTrace] ([CustomerName], [ProductCode], [Status], [CreatedAt]);
END
GO

-- 2. PmProjectTraceStep (产品跟踪工序明细)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmProjectTraceStep')
BEGIN
    CREATE TABLE [PmProjectTraceStep] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectTraceId] bigint NOT NULL,
        [StepOrder] int NOT NULL,
        [ProcessName] nvarchar(max) NOT NULL,
        [PersonInCharge] nvarchar(max) NULL,
        [CycleTime] decimal(18,4) NULL,
        [SettingDays] decimal(18,4) NULL,
        [EstimatedHours] decimal(18,4) NULL,
        [Remarks] nvarchar(max) NULL,
        [PlanDurationDays] int NULL,
        [PlanStartDate] datetime2 NULL,
        [PlanEndDate] datetime2 NULL,
        [ActualStartDate] datetime2 NULL,
        [ActualForecastStartDate] datetime2 NULL,
        [ActualDurationDays] int NULL,
        [ActualPlanDurationDays] int NULL,
        [ActualEndDate] datetime2 NULL,
        CONSTRAINT [PK_PmProjectTraceStep] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PmProjectTraceStep_PmProjectTrace_ProjectTraceId] FOREIGN KEY ([ProjectTraceId]) REFERENCES [PmProjectTrace] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PmProjectTraceStep_ProjectTraceId] ON [PmProjectTraceStep] ([ProjectTraceId]);
END
GO

-- 3. PmProjectTraceStepActualCycleTime (工序实际节拍历史)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmProjectTraceStepActualCycleTime')
BEGIN
    CREATE TABLE [PmProjectTraceStepActualCycleTime] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectTraceStepId] bigint NOT NULL,
        [RecordDate] datetime2 NOT NULL,
        [ActualCycleTime] decimal(18,4) NULL,
        [Remarks] nvarchar(max) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PmProjectTraceStepActualCycleTime] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PmProjectTraceStepActualCycleTime_PmProjectTraceStep_ProjectTraceStepId] FOREIGN KEY ([ProjectTraceStepId]) REFERENCES [PmProjectTraceStep] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PmProjectTraceStepActualCycleTime_ProjectTraceStepId] ON [PmProjectTraceStepActualCycleTime] ([ProjectTraceStepId]);
END
GO

-- 4. PmStepCycleTimeChangeRequest (节拍变更申请)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmStepCycleTimeChangeRequest')
BEGIN
    CREATE TABLE [PmStepCycleTimeChangeRequest] (
        [Id] bigint NOT NULL IDENTITY,
        [StepId] bigint NOT NULL,
        [TraceId] bigint NOT NULL,
        [SubmitterId] nvarchar(max) NULL,
        [SubmitterName] nvarchar(max) NULL,
        [SubmittedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [ApprovalStatus] int NOT NULL DEFAULT 0,
        [ApprovalInstanceId] bigint NULL,
        [Remarks] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PmStepCycleTimeChangeRequest] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PmStepCycleTimeChangeRequest_PmProjectTraceStep_StepId] FOREIGN KEY ([StepId]) REFERENCES [PmProjectTraceStep] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PmStepCycleTimeChangeRequest_StepId] ON [PmStepCycleTimeChangeRequest] ([StepId]);
    CREATE INDEX [IX_PmStepCycleTimeChangeRequest_StepId_ApprovalStatus] ON [PmStepCycleTimeChangeRequest] ([StepId], [ApprovalStatus]);
END
GO

-- 5. PmStepCycleTimeChangeDetail (节拍变更明细)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmStepCycleTimeChangeDetail')
BEGIN
    CREATE TABLE [PmStepCycleTimeChangeDetail] (
        [Id] bigint NOT NULL IDENTITY,
        [RequestId] bigint NOT NULL,
        [ChangeType] int NOT NULL,
        [TargetRecordId] bigint NULL,
        [RecordDate] datetime2 NOT NULL,
        [ActualCycleTime] decimal(18,4) NULL,
        [Remarks] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PmStepCycleTimeChangeDetail] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PmStepCycleTimeChangeDetail_PmStepCycleTimeChangeRequest_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [PmStepCycleTimeChangeRequest] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PmStepCycleTimeChangeDetail_RequestId] ON [PmStepCycleTimeChangeDetail] ([RequestId]);
END
GO

-- ============================================
-- EF Migrations History (optional but recommended)
-- This tells EF Core that these migrations are already applied
-- so future 'dotnet ef' commands won't try to re-run them
-- ============================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260511063815_AddPmProjectTrace')
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260511063815_AddPmProjectTrace', N'8.0.26');

    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260511103341_AddStepActualCycleTime')
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260511103341_AddStepActualCycleTime', N'8.0.26');

    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260512013411_AddCycleTimeStatusAndChangeRequest')
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260512013411_AddCycleTimeStatusAndChangeRequest', N'8.0.26');

    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260512052202_AddPmProjectTraceIndexes')
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260512052202_AddPmProjectTraceIndexes', N'8.0.26');
END
GO

PRINT 'PM tables created successfully.';
