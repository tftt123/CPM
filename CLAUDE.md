# CPM Project Context

> Customer Project Management — CRM + RFQ + PM + Approval for Spindex Industries
> Stack: Vue 3 + Element Plus + .NET 8 + EF Core + SQL Server

---

## Architecture Patterns

### Multi-Site + Multi-App Isolation
All tenant-scoped entities have `Site` (NT01/MY01) and `App` (cpm/sp/mes) fields. Backend filters via `ICurrentUser.App` / `ICurrentUser.Site`; frontend sends `X-App-Code` header + `localStorage.current_app`.

### Generalized Code (通用代码)
All dropdown enums live in `SysGeneralizedCode` (Domain + Code + Label/LabelEn + TagType). Use `GcSelect` component or `useGeneralizedCode()` composable. **Always provide `fallback` prop.** Key domains: `OPP_STAGE`, `QUO_STATUS`, `INDUSTRY`, `CURRENCY`, `PM_TRACE_STATUS`, `ACT_STATUS`, `APPROVAL_STEP_TYPE`, `APPROVAL_STEP_MODE`, `REJECT_BEHAVIOR`, `RULE_TYPE`, `SEQUENCE_RESET_RULE`, `LOGIN_DOMAIN`.

### Field Control (字段控制)
Per-page per-module visibility via `SysFieldControl`. Registry: `src/config/field-registry.ts`. Composable: `useFieldControl('Module', 'Page').isVisible('field')`.

### Navigation Config (动态导航)
Sidebar menus via `SysNavigationConfig`. Admin configures in `SystemSettings → Navigation`. Falls back to hardcoded nav if DB empty.

### i18n Translation
Two-layer: static `locales/zh.ts` + `en.ts` → overridden by `SysI18nMessage` DB records. `I18nMessage/active` is `[AllowAnonymous]` for login page.

### Approval Workflow
Template → Steps → Rules → Conditions. Supports SEQUENTIAL, PARALLEL, PARALLEL_ANY, CONDITIONAL, CC. Reject behaviors decoupled via `IBusinessStatusUpdater`.

---

## Naming Conventions

**Backend:** Entities `Sys*`/`Crm*`/`Quo*`/`Pm*`/`Mfg*`; Controllers `[Name]Controller.cs` (`[Authorize]`); Services `I[Name]Service` / `[Name]Service`; DTOs `[Name]Dto`; DbSets plural.

**Frontend:** Views `[Feature][Action].vue`; Components PascalCase; Composables `use[Name].ts`; API `src/api/[feature].ts`; Stores `use[Name]Store`; CSS vars `--cpm-*` (primary), `--slds-*` (legacy).

---

## Critical Rules

1. **Entity change → EF migration.** Run: `cd CpmServer.Core && dotnet ef migrations add [Name] --startup-project ../CpmServer && dotnet ef database update --startup-project ../CpmServer`
2. **Build before dotnet build:** kill `devenv`, `CpmServer`, `dotnet` processes to avoid DLL locks.
3. **Frontend build check:** `npx vite build` ( `vue-tsc --build` may fail on Windows filesystem).
4. **Never `as any` in Vue templates** — causes Vite dynamic import failures.
5. **401 interceptor:** redirect to `/login` only if `pathname !== '/login'` to prevent refresh loops.
6. **New text → i18n.** Use `t('key')` in `locales/zh.ts` + `en.ts`. No hardcoded Chinese/English in templates or C#.
7. **New enum → GeneralizedCode.** Via `GcSelect` with `fallback`. No hardcoded `el-option` lists.
8. **Site/App fields on new entities.** Always add both; filter queries accordingly.
9. **Intranet deployment.** Avoid hard external CDN dependencies; keep system font fallbacks.
10. **Auth:** JWT in `localStorage.token`; default admin `admin`/`123456`.

---

## Key Files

| Purpose | Path |
|---------|------|
| DbContext | `CpmServer.Core/Data/CpmDbContext.cs` |
| DI / Program | `CpmServer/Program.cs` |
| JWT | `CpmServer/Common/JwtHelper.cs` |
| CurrentUser | `CpmServer.SharedKernel/Identity/CurrentUser.cs` |
| Request interceptor | `cpm-web/src/api/request.ts` |
| Router | `cpm-web/src/router/index.ts` |
| Layout | `cpm-web/src/views/layout/MainLayout.vue` |
| Global styles | `cpm-web/src/styles/salesforce-theme.css` |
| i18n | `cpm-web/src/locales/zh.ts`, `en.ts` |
| Field registry | `cpm-web/src/config/field-registry.ts` |
| GcSelect | `cpm-web/src/components/GcSelect.vue` |
| Composables | `cpm-web/src/composables/useGeneralizedCode.ts`, `useFieldControl.ts`, `useI18n.ts` |

---

## Docs

- Detailed architecture docs: `docs/` (create if needed)
- Skill / design system: `.claude/skills/ui-ux-pro-max/SKILL.md`
