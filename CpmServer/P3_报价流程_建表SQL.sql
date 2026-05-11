-- =============================================
-- CPM P3 报价流程 - 数据库建表脚本
-- 在 SSMS 中执行（确保 CpmDb 已创建）
-- =============================================

USE CpmDb;
GO

-- =============================================
-- 1. 商机表 (Opportunity)
-- =============================================
CREATE TABLE QuoOpportunity (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OpportunityNo NVARCHAR(50) NOT NULL,
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

-- 商机表约束
ALTER TABLE QuoOpportunity ADD CONSTRAINT UQ_QuoOpportunity_No UNIQUE (OpportunityNo);
ALTER TABLE QuoOpportunity ADD CONSTRAINT FK_QuoOpportunity_Customer
    FOREIGN KEY (CustomerId) REFERENCES CrmCustomer(Id);
ALTER TABLE QuoOpportunity ADD CONSTRAINT FK_QuoOpportunity_Owner
    FOREIGN KEY (OwnerId) REFERENCES SysUser(Id);

-- 商机表索引
CREATE INDEX IX_QuoOpportunity_Customer ON QuoOpportunity(CustomerId);
CREATE INDEX IX_QuoOpportunity_Owner ON QuoOpportunity(OwnerId);
CREATE INDEX IX_QuoOpportunity_Status ON QuoOpportunity(Status);
CREATE INDEX IX_QuoOpportunity_Stage ON QuoOpportunity(Stage);
CREATE INDEX IX_QuoOpportunity_Deadline ON QuoOpportunity(QuoteDeadline);
GO

-- =============================================
-- 2. 报价单表 (Quotation)
-- =============================================
CREATE TABLE QuoQuotation (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    QuotationNo NVARCHAR(50) NOT NULL,
    OpportunityId BIGINT NOT NULL,
    CustomerId BIGINT NOT NULL,
    TotalAmount DECIMAL(18,2) NULL,
    TechReviewBy BIGINT NULL,
    TechReviewDate DATETIME2 NULL,
    TechReviewCost DECIMAL(18,2) NULL,
    TechReviewNote NVARCHAR(MAX) NULL,
    PricingApprovedBy BIGINT NULL,
    PricingApprovedDate DATETIME2 NULL,
    PricingNote NVARCHAR(MAX) NULL,
    Status INT DEFAULT 0,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- 报价单表约束
ALTER TABLE QuoQuotation ADD CONSTRAINT UQ_QuoQuotation_No UNIQUE (QuotationNo);
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_Opportunity
    FOREIGN KEY (OpportunityId) REFERENCES QuoOpportunity(Id) ON DELETE CASCADE;
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_Customer
    FOREIGN KEY (CustomerId) REFERENCES CrmCustomer(Id);
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_TechReviewer
    FOREIGN KEY (TechReviewBy) REFERENCES SysUser(Id);
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_PricingApprover
    FOREIGN KEY (PricingApprovedBy) REFERENCES SysUser(Id);
ALTER TABLE QuoQuotation ADD CONSTRAINT FK_QuoQuotation_Creator
    FOREIGN KEY (CreatedBy) REFERENCES SysUser(Id);

-- 报价单表索引
CREATE INDEX IX_QuoQuotation_Opportunity ON QuoQuotation(OpportunityId);
CREATE INDEX IX_QuoQuotation_Customer ON QuoQuotation(CustomerId);
CREATE INDEX IX_QuoQuotation_Status ON QuoQuotation(Status);
CREATE INDEX IX_QuoQuotation_TechReviewBy ON QuoQuotation(TechReviewBy);
CREATE INDEX IX_QuoQuotation_PricingApprovedBy ON QuoQuotation(PricingApprovedBy);
GO

-- =============================================
-- 3. 报价明细表 (Quotation Item)
-- =============================================
CREATE TABLE QuoQuotationItem (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    QuotationId BIGINT NOT NULL,
    ProductId BIGINT NULL,
    Qty INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,4) NULL,
    LineAmount DECIMAL(18,2) NULL
);

-- 报价明细表约束
ALTER TABLE QuoQuotationItem ADD CONSTRAINT FK_QuoQuotationItem_Quotation
    FOREIGN KEY (QuotationId) REFERENCES QuoQuotation(Id) ON DELETE CASCADE;
ALTER TABLE QuoQuotationItem ADD CONSTRAINT FK_QuoQuotationItem_Product
    FOREIGN KEY (ProductId) REFERENCES CrmProduct(Id);

-- 报价明细表索引
CREATE INDEX IX_QuoQuotationItem_Quotation ON QuoQuotationItem(QuotationId);
CREATE INDEX IX_QuoQuotationItem_Product ON QuoQuotationItem(ProductId);
GO

-- =============================================
-- 4. 状态说明注释（方便查看）
-- =============================================

/*
QuoOpportunity.Status:
  0 = 进行中（Open）
  9 = 关闭（Closed）

QuoOpportunity.Stage:
  NEW       = 新建
  QUOTING   = 报价中
  EVALUATING= 客户评估
  WON       = 赢单
  LOST      = 输单

QuoQuotation.Status:
  0 = 草稿（Draft）
  1 = 待技术评审（Pending Tech Review）
  2 = 待定价审批（Pending Pricing Approval）
  3 = 已发出（Issued）
  9 = 完成（Complete）
*/

-- =============================================
-- 5. 测试数据（可选：开发阶段使用）
-- =============================================

-- 确保有测试客户和产品（如果没有请先在P2阶段添加）
-- INSERT INTO CrmCustomer (CustomerCode, CustomerName, Industry, ContactName) VALUES ('C001', '测试客户A', '汽车', '张三');
-- INSERT INTO CrmProduct (ProductCode, ProductName, Material, SurfaceTreatment) VALUES ('P001', '精密轴套', 'SUS304', '抛光');

-- 测试商机数据
-- INSERT INTO QuoOpportunity (OpportunityNo, CustomerId, Title, ExpectedAmount, QuoteDeadline, Stage, Status, OwnerId, CreatedAt, UpdatedAt)
-- VALUES ('OPP202601010001', 1, '汽车配件精密加工项目', 50000.00, '2026-02-15', 'NEW', 0, 1, GETDATE(), GETDATE());

PRINT 'P3 报价流程表创建完成！';
GO
