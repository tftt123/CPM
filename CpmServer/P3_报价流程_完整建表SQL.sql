-- =============================================
-- CPM P3 报价流程 - 完整建表脚本
-- 含：自定义审批配置 + 邮件通知
-- 在 SSMS 中执行
-- =============================================

USE CpmDb;
GO

-- =============================================
-- 1. 商机表
-- =============================================
IF OBJECT_ID('QuoOpportunity', 'U') IS NOT NULL DROP TABLE QuoOpportunity;

CREATE TABLE QuoOpportunity (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OpportunityNo NVARCHAR(50) NOT NULL UNIQUE,
    CustomerId BIGINT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    ExpectedAmount DECIMAL(18,2) NULL,
    QuoteDeadline DATE NULL,
    Stage NVARCHAR(20) DEFAULT 'NEW',
    Status INT DEFAULT 0,
    OwnerId BIGINT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

ALTER TABLE QuoOpportunity ADD CONSTRAINT FK_QuoOpportunity_Customer
    FOREIGN KEY (CustomerId) REFERENCES CrmCustomer(Id);
ALTER TABLE QuoOpportunity ADD CONSTRAINT FK_QuoOpportunity_Owner
    FOREIGN KEY (OwnerId) REFERENCES SysUser(Id);

CREATE INDEX IX_QuoOpportunity_Customer ON QuoOpportunity(CustomerId);
CREATE INDEX IX_QuoOpportunity_Owner ON QuoOpportunity(OwnerId);
CREATE INDEX IX_QuoOpportunity_Status ON QuoOpportunity(Status);
CREATE INDEX IX_QuoOpportunity_Stage ON QuoOpportunity(Stage);
CREATE INDEX IX_QuoOpportunity_Deadline ON QuoOpportunity(QuoteDeadline);
GO

-- =============================================
-- 2. 报价单表
-- =============================================
IF OBJECT_ID('QuoQuotation', 'U') IS NOT NULL DROP TABLE QuoQuotation;

CREATE TABLE QuoQuotation (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    QuotationNo NVARCHAR(50) NOT NULL UNIQUE,
    OpportunityId BIGINT NOT NULL,
    CustomerId BIGINT NOT NULL,
    TotalAmount DECIMAL(18,2) NULL,
    CurrentStepId BIGINT NULL,
    Status INT DEFAULT 0,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_Opportunity
    FOREIGN KEY (OpportunityId) REFERENCES QuoOpportunity(Id) ON DELETE CASCADE;
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_Customer
    FOREIGN KEY (CustomerId) REFERENCES CrmCustomer(Id);
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_Creator
    FOREIGN KEY (CreatedBy) REFERENCES SysUser(Id);

CREATE INDEX IX_QuoQuotation_Opportunity ON QuoQuotation(OpportunityId);
CREATE INDEX IX_QuoQuotation_Customer ON QuoQuotation(CustomerId);
CREATE INDEX IX_QuoQuotation_Status ON QuoQuotation(Status);
GO

-- =============================================
-- 3. 报价明细表
-- =============================================
IF OBJECT_ID('QuoQuotationItem', 'U') IS NOT NULL DROP TABLE QuoQuotationItem;

CREATE TABLE QuoQuotationItem (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    QuotationId BIGINT NOT NULL,
    ProductId BIGINT NULL,
    Qty INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,4) NULL,
    LineAmount DECIMAL(18,2) NULL,
    CONSTRAINT FK_QuoQuotationItem_Quotation FOREIGN KEY (QuotationId)
        REFERENCES QuoQuotation(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuoQuotationItem_Product FOREIGN KEY (ProductId)
        REFERENCES CrmProduct(Id)
);

CREATE INDEX IX_QuoQuotationItem_Quotation ON QuoQuotationItem(QuotationId);
CREATE INDEX IX_QuoQuotationItem_Product ON QuoQuotationItem(ProductId);
GO

-- =============================================
-- 4. 审批流程模板表（自定义审批核心）
-- =============================================
IF OBJECT_ID('SysApprovalTemplate', 'U') IS NOT NULL DROP TABLE SysApprovalTemplate;

CREATE TABLE SysApprovalTemplate (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TemplateCode NVARCHAR(50) NOT NULL UNIQUE,
    TemplateName NVARCHAR(100) NOT NULL,
    ModuleType NVARCHAR(50) NOT NULL, -- 'Quotation' / 'Sample' / 'Procurement'
    Description NVARCHAR(500) NULL,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedBy BIGINT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- =============================================
-- 5. 审批环节表（模板下的具体环节）
-- =============================================
IF OBJECT_ID('SysApprovalStep', 'U') IS NOT NULL DROP TABLE SysApprovalStep;

CREATE TABLE SysApprovalStep (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TemplateId BIGINT NOT NULL,
    StepName NVARCHAR(50) NOT NULL,
    StepOrder INT NOT NULL DEFAULT 1,
    StepType NVARCHAR(20) DEFAULT 'REVIEW', -- REVIEW / APPROVAL / NOTIFY
    ApproverRole NVARCHAR(50) NULL, -- 'TECH' / 'MANAGER' / 'PURCHASE' / NULL
    ApproverUserId BIGINT NULL,
    CanReject BIT DEFAULT 1,
    CanTransfer BIT DEFAULT 0,
    NotifyEmailTemplate NVARCHAR(50) NULL,
    NextStepId BIGINT NULL, -- 为空表示最后环节
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_SysApprovalStep_Template FOREIGN KEY (TemplateId)
        REFERENCES SysApprovalTemplate(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SysApprovalStep_ApproverUser FOREIGN KEY (ApproverUserId)
        REFERENCES SysUser(Id)
);

CREATE INDEX IX_SysApprovalStep_Template ON SysApprovalStep(TemplateId);
GO

-- =============================================
-- 6. 审批实例表（每个业务单据的审批实例）
-- =============================================
IF OBJECT_ID('SysApprovalInstance', 'U') IS NOT NULL DROP TABLE SysApprovalInstance;

CREATE TABLE SysApprovalInstance (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TemplateId BIGINT NOT NULL,
    BusinessType NVARCHAR(50) NOT NULL,
    BusinessId BIGINT NOT NULL,
    CurrentStepId BIGINT NULL,
    CurrentStepOrder INT DEFAULT 0,
    Status INT DEFAULT 0, -- 0=进行中, 1=完成, 2=驳回, 3=撤回
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL,
    CONSTRAINT FK_SysApprovalInstance_Template FOREIGN KEY (TemplateId)
        REFERENCES SysApprovalTemplate(Id)
);

CREATE INDEX IX_SysApprovalInstance_Business ON SysApprovalInstance(BusinessType, BusinessId);
GO

-- =============================================
-- 7. 审批记录表（每次审批操作的历史）
-- =============================================
IF OBJECT_ID('SysApprovalRecord', 'U') IS NOT NULL DROP TABLE SysApprovalRecord;

CREATE TABLE SysApprovalRecord (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    InstanceId BIGINT NOT NULL,
    StepId BIGINT NULL,
    StepName NVARCHAR(50) NOT NULL,
    ApproverId BIGINT NULL,
    ApproverName NVARCHAR(50) NULL,
    Action NVARCHAR(20) NOT NULL, -- 'APPROVE' / 'REJECT' / 'TRANSFER'
    Comment NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_SysApprovalRecord_Instance FOREIGN KEY (InstanceId)
        REFERENCES SysApprovalInstance(Id) ON DELETE CASCADE
);

CREATE INDEX IX_SysApprovalRecord_Instance ON SysApprovalRecord(InstanceId);
CREATE INDEX IX_SysApprovalRecord_Approver ON SysApprovalRecord(ApproverId);
GO

-- =============================================
-- 8. 邮件模板表
-- =============================================
IF OBJECT_ID('SysEmailTemplate', 'U') IS NOT NULL DROP TABLE SysEmailTemplate;

CREATE TABLE SysEmailTemplate (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TemplateCode NVARCHAR(50) NOT NULL UNIQUE,
    TemplateName NVARCHAR(100) NOT NULL,
    Subject NVARCHAR(200) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    IsSystem BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- =============================================
-- 9. 邮件发送记录表
-- =============================================
IF OBJECT_ID('SysEmailLog', 'U') IS NOT NULL DROP TABLE SysEmailLog;

CREATE TABLE SysEmailLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ToAddress NVARCHAR(200) NOT NULL,
    Subject NVARCHAR(200) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    Status INT DEFAULT 0, -- 0=待发送, 1=成功, 2=失败
    ErrorMessage NVARCHAR(MAX) NULL,
    RetryCount INT DEFAULT 0,
    SentAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX IX_SysEmailLog_Status ON SysEmailLog(Status);
GO

-- =============================================
-- 10. 邮件配置表（SMTP设置）
-- =============================================
IF OBJECT_ID('SysEmailConfig', 'U') IS NOT NULL DROP TABLE SysEmailConfig;

CREATE TABLE SysEmailConfig (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    SmtpServer NVARCHAR(200) NOT NULL,
    SmtpPort INT NOT NULL DEFAULT 587,
    SmtpUsername NVARCHAR(200) NOT NULL,
    SmtpPassword NVARCHAR(200) NOT NULL,
    FromName NVARCHAR(100) NULL,
    FromAddress NVARCHAR(200) NOT NULL,
    EnableSsl BIT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- =============================================
-- 11. 插入默认审批模板（报价流程）
-- =============================================
INSERT INTO SysApprovalTemplate (TemplateCode, TemplateName, ModuleType, Description, IsDefault, IsActive)
VALUES ('QUOTATION_DEFAULT', '报价单默认审批流程', 'Quotation', '技术评审 -> 定价审批', 1, 1);

DECLARE @TemplateId BIGINT = SCOPE_IDENTITY();

-- 环节1：技术评审
INSERT INTO SysApprovalStep (TemplateId, StepName, StepOrder, StepType, ApproverRole, CanReject, CanTransfer, NotifyEmailTemplate)
VALUES (@TemplateId, '技术评审', 1, 'REVIEW', 'TECH', 1, 1, 'QUOTATION_TECH_REVIEW');

DECLARE @Step1Id BIGINT = SCOPE_IDENTITY();

-- 环节2：定价审批
INSERT INTO SysApprovalStep (TemplateId, StepName, StepOrder, StepType, ApproverRole, CanReject, CanTransfer, NotifyEmailTemplate)
VALUES (@TemplateId, '定价审批', 2, 'APPROVAL', 'MANAGER', 1, 0, 'QUOTATION_PRICING_APPROVE');

DECLARE @Step2Id BIGINT = SCOPE_IDENTITY();

-- 更新环节关系
UPDATE SysApprovalStep SET NextStepId = @Step2Id WHERE Id = @Step1Id;
GO

-- =============================================
-- 12. 插入默认邮件模板
-- =============================================
INSERT INTO SysEmailTemplate (TemplateCode, TemplateName, Subject, Body, IsSystem, IsActive)
VALUES
('QUOTATION_TECH_REVIEW', '技术评审通知',
 '【CPM系统】您有新的报价单待技术评审',
 '您好 {{ApproverName}}，<br><br>
 您收到一个新的报价单需要技术评审。<br>
 报价单号：{{QuotationNo}}<br>
 商机标题：{{OpportunityTitle}}<br>
 客户：{{CustomerName}}<br>
 预计金额：{{ExpectedAmount}}<br>
 报价截止：{{QuoteDeadline}}<br><br>
 <a href="{{Link}}">点击处理</a><br><br>
 此邮件由CPM系统自动发送，请勿回复。',
 1, 1),

('QUOTATION_PRICING_APPROVE', '定价审批通知',
 '【CPM系统】报价单技术评审通过，待定价审批',
 '您好 {{ApproverName}}，<br><br>
 报价单已通过技术评审，现需要您进行定价审批。<br>
 报价单号：{{QuotationNo}}<br>
 商机标题：{{OpportunityTitle}}<br>
 客户：{{CustomerName}}<br>
 技术评估成本：{{TechReviewCost}}<br>
 评审意见：{{TechReviewNote}}<br><br>
 <a href="{{Link}}">点击处理</a><br><br>
 此邮件由CPM系统自动发送，请勿回复。',
 1, 1),

('QUOTATION_REJECTED', '报价单被驳回通知',
 '【CPM系统】您的报价单已被驳回',
 '您好 {{CreatorName}}，<br><br>
 您提交的报价单已被驳回。<br>
 报价单号：{{QuotationNo}}<br>
 驳回环节：{{StepName}}<br>
 驳回原因：{{RejectReason}}<br><br>
 <a href="{{Link}}">点击查看详情</a><br><br>
 此邮件由CPM系统自动发送，请勿回复。',
 1, 1),

('QUOTATION_APPROVED', '报价单审批完成通知',
 '【CPM系统】报价单已全部审批通过',
 '您好 {{CreatorName}}，<br><br>
 恭喜！您的报价单已全部审批通过，可以发出报价。<br>
 报价单号：{{QuotationNo}}<br>
 商机标题：{{OpportunityTitle}}<br>
 客户：{{CustomerName}}<br><br>
 <a href="{{Link}}">点击查看详情</a><br><br>
 此邮件由CPM系统自动发送，请勿回复。',
 1, 1);
GO

-- =============================================
-- 13. 插入测试邮件配置（请修改为您的SMTP信息）
-- =============================================
INSERT INTO SysEmailConfig (SmtpServer, SmtpPort, SmtpUsername, SmtpPassword, FromName, FromAddress, EnableSsl, IsActive)
VALUES ('smtp.qq.com', 587, 'your-email@qq.com', 'your-auth-code', 'CPM系统', 'your-email@qq.com', 1, 1);
GO

PRINT 'P3 报价流程完整表创建完成（含自定义审批配置 + 邮件通知）！';
GO
