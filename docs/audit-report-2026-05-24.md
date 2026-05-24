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

### 1. 审批核心模块：4 个实体完全无租户隔离

| 实体 | 缺失字段 | 风险 |
|------|---------|------|
| `SysApprovalCondition` | Site + App | 条件分支跨站点泄漏 |
| `SysApprovalInstanceTask` | Site + App | 用户能看到其他站点的待办任务 |
| `SysApprovalRule` | Site + App | 审批人解析规则跨站点匹配错误 |
| `SysModuleTypeConfig` | Site + App | 模块类型配置全局唯一，站点间冲突 |

**证据：** `ApprovalTaskService.GetPendingTasksAsync()` 只按 `AssigneeId + Status` 查询，完全没有 Site 过滤。

### 2. 唯一索引跨站点冲突

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

### 3. 种子数据硬编码 admin/123456 + "NT01"

```csharp
// Program.cs
var admin = new SysUser { Username = "admin", Password = BCrypt.HashPassword("123456"), Site = "NT01" };
```

**风险：** 生产环境也会创建默认账号；密码在源码中明文可见；Site 硬编码。

### 4. 审批服务角色解析不区分 Site

```csharp
// ApprovalNotificationService.cs / ApprovalTaskService.cs
var roleId = await _db.Roles
    .Where(r => r.RoleName == step.ApproverRole)   // 没过滤 Site！
    .Select(r => (long?)r.Id)
    .FirstOrDefaultAsync();
```

NT01 的 "Manager" 可能匹配到 MY01 的 "Manager"，导致通知发错人。

---

## 二、HIGH（严重影响灵活性和可配置性）

### 5. App 维度几乎未使用

- 38 个实体中，**仅 `SysGeneralizedCode` 同时有 Site + App**
- 其余 31 个实体只有 Site，没有 App
- `ICurrentUser.App` 已存在，但服务层查询几乎从不使用

**后果：** 同一个 Site（如 NT01）下，cpm 和 sp 的数据完全混在同一套表里，无法隔离。

### 6. 硬编码角色名遍布代码

| 位置 | 硬编码值 |
|------|---------|
| 15 个 Controller | `[Authorize(Roles = "ADMIN")]` |
| Program.cs | `RequireRole("ADMIN", "APPROVER_QUOTATION")` |
| MainLayout.vue | `roles.some(r => r.toUpperCase() === 'ADMIN')` |
| AuthController | `RoleCode = "USER"` |

**问题：** 新增角色需要改代码重新编译。商业化系统应支持角色通过 `SysRole` 管理，权限通过 RBAC 配置，Controller 使用 Policy。

### 7. 前端路由完全硬编码

```typescript
// router/index.ts — 所有路由写死
{ path: 'quotation/list', component: () => import('@/views/quotation/QuotationList.vue') }
{ path: 'pm/trace', component: () => import('@/views/pm/ProductTraceList.vue') }
```

**问题：** 新增页面必须改代码。商业化系统应支持动态路由注册。

### 8. 审批业务逻辑硬编码字符串

```csharp
// ApprovalInstanceService.cs
case "SEQUENTIAL": ... case "PARALLEL": ... case "PARALLEL_ANY": ...
case "CC": ...

// ApprovalActionService.cs
if (rejectBehavior == "REJECT_TO_PREV") ...
if (rejectBehavior == "REJECT_TO_START") ...

// 硬编码 action 类型
"APPROVE", "REJECT", "TRANSFER", "SKIP"
```

### 9. FieldControl 模块/页面列表硬编码

```csharp
// FieldControlService.cs
private static readonly Dictionary<string, List<string>> Modules = new()
{
    ["Quotation"] = new() { "QuotationForm", "QuotationList", "QuotationDetail" },
    ["PM"] = new() { "ProductTraceList", "ProductTraceDetail", ... },
};
```

### 10. CORS 只允许 localhost

```csharp
policy.WithOrigins("http://localhost:5173")
```

---

## 三、MEDIUM（影响可维护性）

11. 状态值魔数遍地（QuotationService Status 0/1/3，Approval Status 0/1/2）
12. 邮件模板命名约定硬编码（`"APPROVAL_TIMEOUT"` 等）
13. SiteSettingsController 无 Site 过滤
14. Hangfire Dashboard 无权限校验
15. 服务注册紧耦合（注册具体类而非接口）

---

## 四、商业化路线图

### Phase 1: 数据隔离（CRITICAL）
- [ ] 给 31 个实体加 `App` 字段
- [ ] 给 4 个审批实体加 Site + App
- [ ] 修复 4 个唯一索引（加 Site/App）
- [ ] 所有服务层补 App 过滤

### Phase 2: 权限体系重构（HIGH）
- [ ] 角色权限配置化（RBAC）
- [ ] Controller 改用 Policy
- [ ] 前端权限判断动态化
- [ ] 审批角色解析加 Site 过滤

### Phase 3: 配置化深化（HIGH）
- [ ] FieldControl 模块/页面注册表动态化
- [ ] 前端路由动态化
- [ ] 审批状态/行为常量集中管理
- [ ] CORS / 种子数据 / Hangfire 配置化

### Phase 4: 多 App 架构完善（MEDIUM）
- [ ] 每个 App 独立的 GeneralizedCode 初始化
- [ ] App 间数据共享/隔离策略
- [ ] 前端 App Switcher 动态路由加载

---

## 五、核心结论

> **最大风险模块：审批工作流（Approval）**
> 它是全系统中唯一存在"完全无租户隔离实体"的模块，且角色解析不区分 Site，是唯一可能导致跨站点数据泄漏的功能域。

> **最大配置化缺口：路由 + 权限 + 模块注册**
> 前端路由、FieldControl 注册表、Controller 角色注解，这三个地方每新增一个功能都需要改代码重新编译，完全违背"通过系统设置取代编码"的目标。

> **最优先修复：**
> 1. 4 个审批实体加 Site + App 字段 + 索引
> 2. 审批服务补 Site 过滤
> 3. 唯一索引加 Site（OpportunityNo, QuotationNo, TemplateCode, ModuleType）
> 4. admin/123456 种子数据改为配置驱动
