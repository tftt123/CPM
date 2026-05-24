# CPM/SIP 多租户就绪性审计报告

> 审计日期：2026-05-24
> 审计范围：Backend (.NET 8) + Frontend (Vue 3) 全代码库
> 目标：评估现有模块是否满足 "多 App (cpm/sp/mes) + 多 Site (NT01/MY01) + 全面可配置 + 商业化" 要求

---

## 执行摘要

| 维度 | 健康度 | 说明 |
|------|--------|------|
| **数据库实体隔离** | 🟡 60% | 31/38 实体有 Site，仅 1 个有 App，4 个完全无隔离 |
| **服务层过滤** | 🟡 55% | 多数服务只过滤 Site，不过滤 App；审批核心模块多处漏过滤 |
| **配置化程度** | 🔴 35% | 大量硬编码：角色、状态、站点、URL、种子数据、业务规则 |
| **前端动态化** | 🟡 50% | 导航/字段控制已动态化，但路由、权限判断、菜单 fallback 仍硬编码 |
| **安全/商业化** | 🔴 40% | admin/123456 硬编码种子、CORS 硬编码、Hangfire 无认证 |

**整体结论：** 系统已完成从"硬编码"到"配置化"的 **第一阶段基础设施**（GeneralizedCode、FieldControl、NavigationConfig、i18n），但 **第二阶段深度改造**（审批核心、实体 App 维度、服务全面过滤、角色/权限配置化）尚未完成。距离商业化多租户系统还有显著差距。

---

## 一、CRITICAL（阻断商业化）

### 1. 审批核心模块：4 个实体完全无租户隔离 ✅ FIXED

| 实体 | 缺失字段 | 风险 |
|------|---------|------|
| `SysApprovalCondition` | ~~Site + App~~ | 条件分支跨站点泄漏 |
| `SysApprovalInstanceTask` | ~~Site + App~~ | 用户能看到其他站点的待办任务 |
| `SysApprovalRule` | ~~Site + App~~ | 审批人解析规则跨站点匹配错误 |
| `SysModuleTypeConfig` | ~~Site + App~~ | 模块类型配置全局唯一，站点间冲突 |

**修复：** 4 个实体已添加 `Site` + `App` 字段，所有服务层查询已补过滤。

### 2. 唯一索引跨站点冲突 ✅ FIXED

```csharp
// QuoOpportunity
.HasIndex(e => e.OpportunityNo).IsUnique();   // 无 Site！NT01 和 MY01 不能同号

// QuoQuotation
.HasIndex(e => e.QuotationNo).IsUnique();     // 无 Site！

// SysApprovalTemplate
.HasIndex(e => e.TemplateCode).IsUnique();    // 无 Site！

// SysModuleTypeConfig
.HasIndex(e => e.ModuleType).IsUnique();      // 无 Site/App！
```

**修复：** 所有业务编号类唯一索引必须改为复合索引 `{Code, Site}` 或 `{Code, App, Site}`。

### 3. 种子数据硬编码 admin/123456 + "NT01" ✅ FIXED

```csharp
// Program.cs — 修复后
var defaultAdminPassword = builder.Configuration["SeedData:AdminPassword"];
if (!string.IsNullOrEmpty(defaultAdminPassword)) { ... }
```

**修复：** 种子数据改为配置驱动，未配置 `SeedData:AdminPassword` 时跳过创建。Site 从配置读取。

### 4. 审批服务角色解析不区分 Site ✅ FIXED

```csharp
// ApprovalNotificationService.cs / ApprovalTaskService.cs — 修复后
var roleId = await _db.Roles
    .Where(r => r.RoleName == step.ApproverRole &&
        (r.App == currentApp || string.IsNullOrEmpty(r.App)) &&
        (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
    .Select(r => (long?)r.Id)
    .FirstOrDefaultAsync();
```

**修复：** 所有角色解析查询已添加 App + Site 过滤。

---

## 二、HIGH（严重影响灵活性和可配置性）

### 5. App 维度几乎未使用 ✅ FIXED

- ~~38 个实体中，仅 `SysGeneralizedCode` 同时有 Site + App~~
- 所有审批实体、报价实体、PM 实体、配置实体已添加 `App` 字段
- 服务层查询已全部补 `App` 过滤（`(x.App == currentApp || string.IsNullOrEmpty(x.App))`）

**修复：** 关键实体均已添加 App 字段，查询已补过滤。

### 6. 硬编码角色名遍布代码 ✅ FIXED

| 位置 | 修复前 | 修复后 |
|------|--------|--------|
| ApprovalController | `[Authorize(Roles = "ADMIN")]` | `[Authorize(Policy = CanManageApprovalTemplates)]` |
| Program.cs | `RequireRole("ADMIN", ...)` | 基于 PermissionRequirement 的策略授权 |
| MainLayout.vue | `roles.some(r => r === 'ADMIN')` | 纯 `permissions.includes(required)` |
| 前端路由守卫 | `isAdmin` 特判 | 已移除 |

**修复：** 后端全面改用 Policy + Permission claim；前端移除 ADMIN 特判。

### 7. 前端路由完全硬编码 ⚠️ PENDING（长期规划）

```typescript
// router/index.ts — 所有路由写死
{ path: 'quotation/list', component: () => import('@/views/quotation/QuotationList.vue') }
{ path: 'pm/trace', component: () => import('@/views/pm/ProductTraceList.vue') }
```

**现状：** 导航菜单（NavigationConfig）已动态化，但 Vue Router 路由表仍为静态定义。新增页面需修改 `router/index.ts` 并重新编译。
**计划：** Phase 5 后期通过预注册组件映射表 + `router.addRoute()` 实现动态路由，需解决 Vite 静态分析限制。

### 8. 审批业务逻辑硬编码字符串 ✅ FIXED

```csharp
// ApprovalInstanceService.cs — 修复后
switch (step.StepMode)
{
    case ApprovalConstants.StepMode.Sequential: ...
    case ApprovalConstants.StepMode.Parallel: ...
    case ApprovalConstants.StepMode.ParallelAny: ...
    case ApprovalConstants.StepMode.Cc: ...
}

// ApprovalActionService.cs
if (rejectBehavior == ApprovalConstants.RejectBehavior.RejectToPrev) ...

// 动作类型
ApprovalConstants.Action.Approve
ApprovalConstants.Action.Reject
ApprovalConstants.Action.Transfer
ApprovalConstants.Action.Skip
```

**修复：** 所有硬编码字符串已迁移至 `CpmServer.SharedKernel/Constants/ApprovalConstants.cs`，魔数替换为命名常量。

### 9. FieldControl 模块/页面列表硬编码 ⚠️ PARTIALLY FIXED

```csharp
// FieldControlService.cs — 仍保留硬编码注册表
private static readonly Dictionary<string, List<string>> Modules = new()
{
    ["Quotation"] = new() { "QuotationForm", "QuotationList", "QuotationDetail" },
    ["PM"] = new() { "ProductTraceList", "ProductTraceDetail", ... },
};
```

**现状：** FieldControl 查询已支持 App + Site 过滤（数据隔离已修复），但模块/页面注册表仍为后端硬编码。新增页面需改代码并重新编译。
**计划：** 未来引入 `SysFieldControlRegistry` 数据库注册表，实现运行时动态扩展。

### 10. CORS 只允许 localhost ✅ FIXED

```csharp
// Program.cs — 修复后
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

policy.WithOrigins(allowedOrigins)
```

**修复：** CORS 允许域名从配置读取，生产环境通过 `appsettings.Production.json` 配置。

---

## 三、MEDIUM（影响可维护性）

11. 状态值魔数遍地 ✅ FIXED — 所有魔数已替换为 `ApprovalConstants` / `QuotationConstants` 命名常量
12. 邮件模板命名约定硬编码 ✅ FIXED — 迁移至 `EmailTemplateConstants`
13. SiteSettingsController 无 Site 过滤 ✅ FIXED — 所有查询已加 `Site == currentSite`
14. Hangfire Dashboard 无权限校验 ✅ FIXED — 需 `system.manage` 权限或 ADMIN 角色
15. 服务注册紧耦合 ✅ FIXED — Approval 模块 6 个服务 + JwtHelper 已全部接口化，DI 注册改为接口绑定

---

## 四、商业化路线图

### Phase 1: 数据隔离（CRITICAL）✅ COMPLETE
- [x] 给关键实体加 `App` 字段
- [x] 给 4 个审批实体加 Site + App
- [x] 修复 4 个唯一索引（加 Site/App）
- [x] 所有服务层补 App 过滤

### Phase 2: 权限体系重构（HIGH）✅ COMPLETE
- [x] 角色权限配置化（RBAC）
- [x] Controller 改用 Policy
- [x] 前端权限判断动态化
- [x] 审批角色解析加 Site 过滤

### Phase 3: 配置化深化（HIGH）✅ COMPLETE
- [x] 审批状态/行为常量集中管理
- [x] CORS / 种子数据 / Hangfire 配置化
- [ ] FieldControl 模块/页面注册表动态化（可选）
- [ ] 前端路由动态化（长期）

### Phase 4: 多 App 架构完善（MEDIUM）⚠️ PENDING
- [ ] 每个 App 独立的 GeneralizedCode 初始化
- [ ] App 间数据共享/隔离策略
- [ ] 前端 App Switcher 动态路由加载

---

## 五、核心结论

> ~~**最大风险模块：审批工作流（Approval）**~~
> ~~它是全系统中唯一存在"完全无租户隔离实体"的模块，且角色解析不区分 Site，是唯一可能导致跨站点数据泄漏的功能域。~~
> **状态：** ✅ 已修复。所有审批实体已加 Site + App，角色解析已加过滤，常量已集中管理。

> **最大配置化缺口：路由 + 模块注册**
> ~~前端路由、~~ FieldControl 注册表 ~~、Controller 角色注解~~，这三个地方每新增一个功能都需要改代码重新编译。
> **现状：**
> - Controller 角色注解 ✅ 已全面改用 Policy
> - 前端路由 ⚠️ 仍为静态定义（长期规划）
> - FieldControl 注册表 ⚠️ 仍为后端硬编码（可选）

> **最优先修复：**
> 1. 4 个审批实体加 Site + App 字段 + 索引
> 2. 审批服务补 Site 过滤
> 3. 唯一索引加 Site（OpportunityNo, QuotationNo, TemplateCode, ModuleType）
> 4. admin/123456 种子数据改为配置驱动
