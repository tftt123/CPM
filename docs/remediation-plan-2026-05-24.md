# CPM 系统多租户就绪性修复计划

> 基于审计报告: `audit-report-2026-05-24.md`
> 生成日期: 2026-05-24
> 目标: 将系统从"基础设施就绪"推进到"商业化多租户就绪"

---

## 执行摘要

| 阶段 | 范围 | 预计工作量 | 风险等级 |
|------|------|-----------|---------|
| Phase 1 | 数据隔离补漏 (CRITICAL) | 2-3 小时 | 阻断级 |
| Phase 2 | 权限体系完善 (HIGH) | 3-4 小时 | 高 |
| Phase 3 | 配置化深化 (HIGH) | 4-6 小时 | 中 |
| Phase 4 | 安全加固 + 常量治理 (MEDIUM) | 3-4 小时 | 中 |
| Phase 5 | 服务解耦 + 架构完善 (LOW) | 4-6 小时 | 低 |

**总计: 16-23 小时开发工作量 + 测试验证**

---

## Phase 1: 数据隔离补漏 (CRITICAL) — 优先执行

### 1.1 SiteSettingsController 加 Site 过滤

**风险:** 所有站点共享同一行配置，任何站点修改会影响全局。

**文件:**
- `CpmServer/Controllers/SiteSettingsController.cs`

**当前代码 (Line 25):**
```csharp
var settings = await _db.SiteSettings.FirstOrDefaultAsync();
```

**修复:**
```csharp
var currentSite = _currentUser.Site;
var settings = await _db.SiteSettings
    .FirstOrDefaultAsync(s => s.Site == currentSite);
```

**验证:**
- NT01 用户保存配置后，MY01 用户查看应看不到该配置
- 新站点首次访问应返回默认配置

---

### 1.2 ApprovalInstanceService.GetInstanceAsync 加 Site/App 过滤

**风险:** 通过 BusinessType + BusinessId 可跨站点查看审批实例。

**文件:**
- `CpmServer.Modules.Approval/Services/ApprovalInstanceService.cs`

**当前代码 (Line 71-78):**
```csharp
public async Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId)
{
    return await _db.ApprovalInstances
        .FirstOrDefaultAsync(i => i.BusinessType == businessType && i.BusinessId == businessId);
}
```

**修复:**
```csharp
public async Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId)
{
    var currentApp = _currentUser.App ?? "cpm";
    var currentSite = _currentUser.Site;
    return await _db.ApprovalInstances
        .FirstOrDefaultAsync(i =>
            i.BusinessType == businessType &&
            i.BusinessId == businessId &&
            (i.App == currentApp || string.IsNullOrEmpty(i.App)) &&
            (i.Site == currentSite || string.IsNullOrEmpty(i.Site)));
}
```

---

### 1.3 ApprovalTaskService.GetInstanceTasksAsync 加 Site/App 过滤

**风险:** 通过 InstanceId 可查看任意站点的审批任务。

**文件:**
- `CpmServer.Modules.Approval/Services/ApprovalTaskService.cs`

**当前代码 (Line 104-115):**
```csharp
public async Task<List<SysApprovalInstanceTask>> GetInstanceTasksAsync(long instanceId)
{
    return await _db.ApprovalInstanceTasks
        .Where(t => t.InstanceId == instanceId)
        .OrderBy(t => t.CreatedAt)
        .ToListAsync();
}
```

**修复:** 增加实例所有权验证
```csharp
public async Task<List<SysApprovalInstanceTask>> GetInstanceTasksAsync(long instanceId)
{
    var currentApp = _currentUser.App ?? "cpm";
    var currentSite = _currentUser.Site;

    // 先验证实例归属
    var instance = await _db.ApprovalInstances
        .FirstOrDefaultAsync(i => i.Id == instanceId);
    if (instance == null) return new List<SysApprovalInstanceTask>();
    if (!string.IsNullOrEmpty(instance.App) && instance.App != currentApp) return new List<SysApprovalInstanceTask>();
    if (!string.IsNullOrEmpty(instance.Site) && instance.Site != currentSite) return new List<SysApprovalInstanceTask>();

    return await _db.ApprovalInstanceTasks
        .Where(t => t.InstanceId == instanceId)
        .OrderBy(t => t.CreatedAt)
        .ToListAsync();
}
```

---

### 1.4 ApprovalTaskService.GetApprovalRecordsAsync 加 Site/App 过滤

**风险:** 同 1.3，审批记录可跨站点查看。

**文件:**
- `CpmServer.Modules.Approval/Services/ApprovalTaskService.cs`

**修复:** 与 1.3 相同模式 —— 先验证实例归属，再返回记录。

---

### 1.5 ApprovalTaskService.CanUserApproveAsync 加 Site/App 过滤

**风险:** 审批权限校验不验证实例归属。

**文件:**
- `CpmServer.Modules.Approval/Services/ApprovalTaskService.cs`

**当前代码 (Line 163-218):**
```csharp
// Line 169: instance lookup
var instance = await _db.ApprovalInstances.FindAsync(instanceId);
```

**修复:** 在 FindAsync 后增加 Site/App 校验。

---

### 1.6 ApprovalInstanceService.ScanTimeoutTasksAsync 加 Site/App 过滤

**风险:** 后台定时扫描超时任务时全局扫描，可能跨站点处理。

**文件:**
- `CpmServer.Modules.Approval/Services/ApprovalInstanceService.cs`

**当前代码 (Line 161-182):**
```csharp
.Where(t => t.Status == 0 && t.DueDate != null && t.DueDate < now)
```

**修复:** 扫描应按 App + Site 分批处理，或传入当前上下文过滤。

**注意:** 如果 Hangfire Job 以站点/应用维度调度，则传入相应参数；如果全局调度，则需遍历所有站点分别处理。

---

### 1.7 ApprovalNotificationService / ApprovalTaskService — 角色解析加 App 过滤

**风险:** NT01 的 "Manager" 会匹配到 MY01 的 "Manager"，且跨 App 也会匹配。

**文件:**
- `CpmServer.Modules.Approval/Services/ApprovalNotificationService.cs` (Lines 92, 126, 153)
- `CpmServer.Modules.Approval/Services/ApprovalTaskService.cs` (Lines 33, 190, 210)

**当前代码:**
```csharp
.Where(r => r.RoleName == step.ApproverRole && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
```

**修复:**
```csharp
var currentApp = _currentUser.App ?? "cpm";
.Where(r =>
    r.RoleName == step.ApproverRole &&
    (r.App == currentApp || string.IsNullOrEmpty(r.App)) &&
    (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
```

---

### 1.8 QuotationService / PmProjectTraceService — 查询补 App 过滤

**风险:** 同一 Site 下 cpm/sp/mes 数据混在一起。

**文件:**
- `CpmServer.Modules.Quotation/Services/QuotationService.cs` (Lines 45, 97, 204)
- `CpmServer.Modules.PM/Services/PmProjectTraceService.cs` (Lines 33, 85, 348, 468, 559)

**当前代码:**
```csharp
.Where(o => o.Site == _currentUser.Site)
```

**修复:**
```csharp
var currentApp = _currentUser.App ?? "cpm";
.Where(o =>
    (o.App == currentApp || string.IsNullOrEmpty(o.App)) &&
    o.Site == _currentUser.Site)
```

**注意:** 需同步更新 `CountAsync()`、`OrderBy` 等链式调用位置。

---

### Phase 1 完成检查清单

- [x] SiteSettingsController 所有方法加 Site 过滤
- [x] ApprovalInstanceService.GetInstanceAsync 加 Site/App
- [x] ApprovalTaskService.GetInstanceTasksAsync 加实例归属验证
- [x] ApprovalTaskService.GetApprovalRecordsAsync 加实例归属验证
- [x] ApprovalTaskService.CanUserApproveAsync 加实例归属验证
- [x] ApprovalInstanceService.ScanTimeoutTasksAsync 加 Site/App
- [x] ApprovalNotificationService 角色解析加 App
- [x] ApprovalTaskService 角色解析加 App
- [x] QuotationService 所有查询加 App
- [x] PmProjectTraceService 所有查询加 App
- [x] 运行 `dotnet build` 验证编译
- [x] 核心流程回归测试（创建审批 → 审批 → 完成）

---

## Phase 2: 权限体系完善 (HIGH)

### 2.1 ApprovalController 改用 Policy

**风险:** RBAC 改造遗漏了审批模块的 Controller，仍使用硬编码角色。

**文件:**
- `CpmServer.Modules.Approval/Controllers/ApprovalController.cs`

**当前代码 (Lines 24, 32, 40, 48, 56):**
```csharp
[Authorize(Roles = "ADMIN")]
```

**修复:**
```csharp
[Authorize(Policy = Policies.CanManageApproval)]  // 或合适的策略
```

**前提:** 确认 `Policies.cs` 中已定义审批管理相关策略。如果没有，先添加：
```csharp
public const string CanManageApproval = "CanManageApproval";
```
并在 `Program.cs` 注册：
```csharp
options.AddPolicy(Policies.CanManageApproval, policy =>
    policy.Requirements.Add(new PermissionRequirement(Permissions.Approval.Manage)));
```

**验证:** 非 ADMIN 但拥有 `approval.manage` 权限的用户可以访问模板管理。

---

### 2.2 前端路由守卫移除 ADMIN 硬编码

**风险:** 前端路由守卫中保留 ADMIN 后备判断，与后端 RBAC 不一致。

**文件:**
- `cpm-web/src/router/index.ts` (Line 140)

**当前代码:**
```typescript
const isAdmin = roles.some((r: string) => r.toUpperCase() === 'ADMIN')
if (!isAdmin && !permissions.includes(required)) {
  return '/home'
}
```

**修复:** 移除 ADMIN 特判，统一走权限校验：
```typescript
if (!permissions.includes(required)) {
  return '/home'
}
```

**注意:** 确保所有现有 ADMIN 用户已在 `SysRolePermission` 表中拥有相应权限，否则升级后会无法访问。

---

### 2.3 PermissionAuthorizationHandler 移除 ADMIN 后备

**风险:** ADMIN 角色绕过所有权限检查，存在权限提升隐患。

**文件:**
- `CpmServer/Authorization/PermissionAuthorizationHandler.cs` (Lines 21-28)

**当前代码:**
```csharp
var isAdmin = context.User.Claims
    .Any(c => c.Type == ClaimTypes.Role && c.Value.ToUpperInvariant() == "ADMIN");

if (hasPermission || isAdmin)
{
    context.Succeed(requirement);
}
```

**修复方案 A（渐进式）:** 保留但加日志告警
```csharp
if (hasPermission)
{
    context.Succeed(requirement);
}
else if (isAdmin)
{
    // TODO: 商业化后移除此后备
    _logger.LogWarning("ADMIN fallback used for permission {Permission}", requirement.Permission);
    context.Succeed(requirement);
}
```

**修复方案 B（彻底）:** 直接移除 `isAdmin` 判断，要求 ADMIN 也必须在 `SysRolePermission` 中有对应权限。

**建议:** 先执行方案 A，待确认所有 ADMIN 用户已配置权限后，再执行方案 B。

---

### Phase 2 完成检查清单

- [x] ApprovalController 5 个 endpoint 改用 Policy
- [x] Policies.cs 补充审批管理策略
- [x] Program.cs 注册审批策略
- [x] 前端路由守卫移除 ADMIN 特判
- [x] PermissionAuthorizationHandler 加日志告警
- [x] 为现有 ADMIN 角色分配所有权限
- [x] 非 ADMIN 用户按权限访问测试

---

## Phase 3: 配置化深化 (HIGH)

### 3.1 种子数据改为配置驱动

**风险:** 生产环境硬编码 admin/123456 弱密码 + NT01 站点。

**文件:**
- `CpmServer/Program.cs` (Lines 282-328)

**当前代码:**
```csharp
var admin = new SysUser {
    Username = "admin",
    Password = BCrypt.HashPassword("123456"),
    Site = "NT01",
    // ...
};
```

**修复方案:** 环境变量驱动
```csharp
var defaultAdminUser = builder.Configuration["SeedData:AdminUsername"] ?? "admin";
var defaultAdminPass = builder.Configuration["SeedData:AdminPassword"];
var defaultAdminSite = builder.Configuration["SeedData:AdminSite"] ?? "NT01";

if (string.IsNullOrEmpty(defaultAdminPass))
{
    // 生产环境未配置密码，跳过种子数据创建
    // 或抛出异常要求必须配置
    logger.LogWarning("SeedData:AdminPassword not configured. Skipping default admin creation.");
    return;
}

var admin = new SysUser {
    Username = defaultAdminUser,
    Password = BCrypt.HashPassword(defaultAdminPass),
    Site = defaultAdminSite,
    // ...
};
```

**配套:** `appsettings.Production.json` 示例
```json
{
  "SeedData": {
    "AdminUsername": "admin",
    "AdminPassword": "${ADMIN_PASSWORD}",
    "AdminSite": "NT01"
  }
}
```

**验证:** 不配置密码时生产环境不会创建默认账号。

---

### 3.2 CORS 配置化

**风险:** 只允许 localhost，生产环境无法访问。

**文件:**
- `CpmServer/Program.cs` (Lines 117-126)

**当前代码:**
```csharp
policy.WithOrigins("http://localhost:5173")
```

**修复:**
```csharp
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

policy.WithOrigins(allowedOrigins)
```

**配套:** `appsettings.Production.json`
```json
{
  "Cors": {
    "AllowedOrigins": ["https://cpm.spindex.com", "https://cpm-mes.spindex.com"]
  }
}
```

---

### 3.3 FieldControlService 模块/页面注册表动态化

**风险:** 新增页面必须改后端代码重新编译。

**文件:**
- `CpmServer/Services/FieldControlService.cs` (Lines 21-37)

**当前代码:**
```csharp
private static readonly List<string> Modules = new() { "Quotation", "Opportunity", ... };
private static readonly Dictionary<string, List<string>> Pages = new() { ... };
```

**修复方案:** 基于反射自动发现 + 数据库覆盖

1. **反射扫描** (启动时一次):
```csharp
private static readonly Lazy<Dictionary<string, List<string>>> _discoveredPages = new(() =>
{
    var result = new Dictionary<string, List<string>>();
    var assembly = Assembly.GetExecutingAssembly();
    // 扫描所有 Controller，提取 Module/Page 信息
    // 或扫描前端路由注册表（如果共享）
    return result;
});
```

2. **数据库注册表** (SysFieldControlRegistry 新表):
```csharp
[Table("SysFieldControlRegistry")]
public class SysFieldControlRegistry
{
    public long Id { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string PageCode { get; set; } = string.Empty;
    public string? PageLabel { get; set; }
    public string? PageLabelEn { get; set; }
    public bool IsActive { get; set; } = true;
}
```

3. **Service 查询时合并**:
```csharp
var modules = await _db.FieldControlRegistries
    .Where(r => r.IsActive)
    .Select(r => r.ModuleCode)
    .Distinct()
    .ToListAsync();

var pages = await _db.FieldControlRegistries
    .Where(r => r.IsActive)
    .GroupBy(r => r.ModuleCode)
    .ToDictionaryAsync(g => g.Key, g => g.Select(r => r.PageCode).ToList());
```

**验证:** 新增页面只需在数据库注册，无需重启后端。

---

### 3.4 FieldControlService 加 App 过滤

**风险:** SysFieldControl 实体有 App 字段但查询从未使用。

**文件:**
- `CpmServer/Services/FieldControlService.cs` (Lines 48, 52, 66, 71, 106, 115)

**修复:** 所有查询增加 `&& (f.App == currentApp || string.IsNullOrEmpty(f.App))`。

---

### 3.5 前端路由动态化（长期）

**风险:** 新增页面必须改 `router/index.ts` 重新编译。

**文件:**
- `cpm-web/src/router/index.ts`

**方案:** 保留静态路由作为骨架，支持运行时动态注册。

```typescript
// 基础路由（登录、布局、404）静态定义
// 业务路由从 NavigationConfig API 加载

const router = createRouter({ ... });

// 运行时动态添加路由
async function registerDynamicRoutes() {
  const res = await navigationConfigApi.getList();
  const configs = res.data || [];
  for (const config of configs) {
    if (config.routePath && config.componentPath) {
      router.addRoute('layout', {
        path: config.routePath.replace(/^\//, ''),
        component: () => import(/* @vite-ignore */ config.componentPath),
        meta: { title: config.navLabel }
      });
    }
  }
}
```

**挑战:** Vite 的 `import()` 在编译时需要静态分析路径。动态路径可能导致 chunk 无法预构建。

**替代方案:** 预注册所有可能的组件映射表，动态路由只决定**启用**哪些：
```typescript
const componentMap: Record<string, () => Promise<any>> = {
  'CustomerList': () => import('@/views/customer/CustomerList.vue'),
  'QuotationList': () => import('@/views/quotation/QuotationList.vue'),
  // ... 所有页面预注册
};

// 运行时从配置中读取组件名，映射到实际导入
router.addRoute('layout', {
  path: config.routePath,
  component: componentMap[config.componentName],
  meta: { title: config.navLabel }
});
```

**建议:** Phase 3 只做后端注册表，前端路由动态化放到 Phase 5。

---

### Phase 3 完成检查清单

- [x] Program.cs 种子数据读取环境变量
- [x] appsettings.json 添加 SeedData 和 Cors 配置节
- [x] CORS 读取配置
- [x] FieldControlService 加 App 过滤
- [x] FieldControl 模块/页面注册表方案设计评审
- [ ] （可选）创建 SysFieldControlRegistry 实体和 API
- [x] dotnet build 通过
- [x] 部署配置文档更新

---

## Phase 4: 安全加固 + 常量治理 (MEDIUM)

### 4.1 Hangfire Dashboard 加权限校验

**风险:** 任何已登录用户都能访问后台作业管理。

**文件:**
- `CpmServer/Common/HangfireDashboardAuthFilter.cs` (Line 14)

**当前代码:**
```csharp
return httpContext.User.Identity?.IsAuthenticated == true;
```

**修复:**
```csharp
var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
if (!isAuthenticated) return false;

// 要求 system.manage 权限或 ADMIN 角色
var hasPermission = httpContext.User.Claims
    .Any(c => c.Type == "Permission" && c.Value == Permissions.System.Manage);
var isAdmin = httpContext.User.Claims
    .Any(c => c.Type == ClaimTypes.Role && c.Value.ToUpperInvariant() == "ADMIN");

return hasPermission || isAdmin;
```

---

### 4.2 审批业务逻辑字符串常量集中管理

**风险:** 字符串拼写错误导致审批流程异常，且散落在 6 个文件中。

**新建文件:** `CpmServer/Constants/ApprovalConstants.cs`

```csharp
public static class ApprovalConstants
{
    // 步骤类型
    public static class StepType
    {
        public const string Review = "REVIEW";
        public const string Approval = "APPROVAL";
        public const string Notify = "NOTIFY";
    }

    // 步骤模式
    public static class StepMode
    {
        public const string Start = "START";
        public const string Sequential = "SEQUENTIAL";
        public const string Parallel = "PARALLEL";
        public const string ParallelAny = "PARALLEL_ANY";
        public const string Conditional = "CONDITIONAL";
        public const string Cc = "CC";
        public const string End = "END";
    }

    // 审批动作
    public static class Action
    {
        public const string Approve = "APPROVE";
        public const string Reject = "REJECT";
        public const string Transfer = "TRANSFER";
        public const string Skip = "SKIP";
    }

    // 驳回行为
    public static class RejectBehavior
    {
        public const string RejectAndClose = "REJECT_AND_CLOSE";
        public const string RejectToPrev = "REJECT_TO_PREV";
        public const string RejectToStep = "REJECT_TO_STEP";
        public const string RejectToStart = "REJECT_TO_START";
        public const string RejectToRequestor = "REJECT_TO_REQUESTOR";
    }

    // 任务状态
    public static class TaskStatus
    {
        public const int Pending = 0;
        public const int Approved = 1;
        public const int Rejected = 2;
        public const int Skipped = 3;
        public const int Timeout = 4;
    }

    // 实例状态
    public static class InstanceStatus
    {
        public const int Active = 0;
        public const int Completed = 1;
        public const int Rejected = 2;
    }
}
```

**替换范围:**
- `ApprovalActionService.cs` — 所有硬编码字符串和数字
- `ApprovalInstanceService.cs` — 同上
- `ApprovalTaskService.cs` — 同上
- `ApprovalNotificationService.cs` — 同上

**验证:** 全文替换后 `dotnet build` 通过，审批流程端到端测试通过。

---

### 4.3 报价单状态魔数集中管理

**风险:** 0/1/2/3 等魔数含义不清，维护困难。

**新建文件:** `CpmServer/Constants/QuotationConstants.cs`

```csharp
public static class QuotationConstants
{
    public static class Status
    {
        public const int Draft = 0;
        public const int PendingReview = 1;
        public const int PendingApproval = 2;
        public const int Issued = 3;
        public const int Completed = 9;
    }

    public static class OpportunityStage
    {
        public const string New = "NEW";
        public const string Qualified = "QUALIFIED";
        public const string Proposal = "PROPOSAL";
        public const string Negotiation = "NEGOTIATION";
        public const string Closed = "CLOSED";
        public const string Lost = "LOST";
    }
}
```

**替换范围:** `QuotationService.cs` 所有 Status 比较。

---

### 4.4 邮件模板命名常量

**新建文件:** `CpmServer/Constants/EmailTemplateConstants.cs`

```csharp
public static class EmailTemplateConstants
{
    public const string ApprovalTimeout = "APPROVAL_TIMEOUT";
    public const string CycleTimeExceeded = "CYCLE_TIME_EXCEEDED";

    public static string GetApprovalApprovedTemplate(string businessType) =>
        $"{businessType.ToUpperInvariant()}_APPROVED";

    public static string GetApprovalRejectedTemplate(string businessType) =>
        $"{businessType.ToUpperInvariant()}_REJECTED";
}
```

**替换范围:**
- `ApprovalInstanceService.cs`
- `ApprovalNotificationService.cs`
- `AlertService.cs`

---

### Phase 4 完成检查清单

- [x] ApprovalConstants.cs 创建
- [x] QuotationConstants.cs 创建
- [x] EmailTemplateConstants.cs 创建
- [x] 6 个 Service 文件字符串替换完成
- [x] HangfireDashboardAuthFilter 加权限校验
- [x] dotnet build 通过
- [x] 审批全流程回归测试
- [x] 报价单 CRUD 回归测试

---

## Phase 5: 服务解耦 + 架构完善 (LOW)

### 5.1 Approval 模块服务接口化

**风险:** 直接依赖实现类，无法单元测试、无法替换实现。

**文件:**
- `CpmServer/Program.cs` (Lines 186-190)
- `CpmServer.Modules.Approval/Services/*.cs`

**当前:**
```csharp
builder.Services.AddScoped<ApprovalTemplateService>();
builder.Services.AddScoped<ApprovalInstanceService>();
// ...
```

**目标:**
```csharp
builder.Services.AddScoped<IApprovalTemplateService, ApprovalTemplateService>();
builder.Services.AddScoped<IApprovalInstanceService, ApprovalInstanceService>();
// ...
```

**涉及接口新建:**
- `IApprovalTemplateService`
- `IApprovalInstanceService`
- `IApprovalTaskService`
- `IApprovalActionService`
- `IApprovalNotificationService`

**工作量:** 每个 Service 提取接口约 10-15 分钟，共约 1-1.5 小时。

---

### 5.2 JwtHelper 接口化

**当前:**
```csharp
builder.Services.AddSingleton<JwtHelper>();
```

**目标:**
```csharp
builder.Services.AddSingleton<IJwtHelper, JwtHelper>();
```

---

### 5.3 前端路由守卫动态权限匹配优化

**当前:** 路由守卫中 `permissions` 数组每次从 localStorage JSON.parse。

**优化:** 使用 session 级缓存（已在 `router/index.ts` 中实现 `cachedUserInfo`）。

**现状已满足需求，此条标记为可选。**

---

### Phase 5 完成检查清单

- [x] 6 个 Approval Service 接口提取（含 Notification）
- [x] Program.cs 注册改为接口绑定
- [x] Controller 构造函数改为接口依赖
- [x] JwtHelper 接口化（IJwtHelper）
- [x] dotnet build + 测试编译通过

---

## 附录 A: 快速修复脚本 (P0 问题)

以下是一行命令可以定位的所有 P0 问题：

```bash
# 1. SiteSettingsController 无 Site 过滤
grep -n "FirstOrDefaultAsync()" CpmServer/Controllers/SiteSettingsController.cs

# 2. ApprovalInstanceService.GetInstanceAsync 无过滤
grep -n "FirstOrDefaultAsync(i => i.BusinessType" CpmServer.Modules.Approval/Services/ApprovalInstanceService.cs

# 3. ApprovalTaskService.GetInstanceTasksAsync 无过滤
grep -n "Where(t => t.InstanceId == instanceId)" CpmServer.Modules.Approval/Services/ApprovalTaskService.cs

# 4. ApprovalTaskService 角色解析无 App
grep -n "r.RoleName == step.ApproverRole" CpmServer.Modules.Approval/Services/ApprovalTaskService.cs

# 5. ApprovalNotificationService 角色解析无 App
grep -n "r.RoleName == step.ApproverRole" CpmServer.Modules.Approval/Services/ApprovalNotificationService.cs

# 6. QuotationService 无 App 过滤
grep -n "o.Site == _currentUser.Site" CpmServer.Modules.Quotation/Services/QuotationService.cs

# 7. PmProjectTraceService 无 App 过滤
grep -n "Site == _currentUser.Site" CpmServer.Modules.PM/Services/PmProjectTraceService.cs
```

---

## 附录 B: 测试验证矩阵

| 功能 | 修复后验证项 |
|------|------------|
| SiteSettings | NT01 修改后 MY01 不受影响 |
| 审批实例查看 | 跨 Site/App 的 BusinessId 无法查看 |
| 审批任务列表 | 只能看到本 Site/App 的任务 |
| 角色解析 | 不同 Site 同名角色不互相匹配 |
| 报价单列表 | cpm App 看不到 sp App 的报价单 |
| 权限控制 | 非 ADMIN 用户按分配权限访问 |
| 种子数据 | 不配置密码时不创建默认账号 |
| CORS | 生产域名可正常访问 API |
| Hangfire | 普通用户无法访问 Dashboard |

---

## 附录 C: 文档维护

本修复计划执行后，需同步更新以下文档：

- [x] `docs/audit-report-2026-05-24.md` — 标记已修复项
- [x] `CLAUDE.md` — 更新关键规则（如 SeedData 环境变量要求）
- [ ] 部署文档 — 添加环境变量配置说明
- [ ] API 文档 — 更新权限策略列表
