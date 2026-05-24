# CPM Architecture

## Multi-Site + Multi-App Isolation

Every tenant-scoped entity has:
- `Site` (e.g., "NT01", "MY01") — physical factory
- `App` (e.g., "cpm", "sp", "mes") — application subsystem

Backend filtering pattern:
```csharp
query = query.Where(g => g.App == app || string.IsNullOrEmpty(g.App));
query = query.Where(g => g.Site == site || string.IsNullOrEmpty(g.Site));
```

Frontend sends `X-App-Code` header via `request.ts` interceptor. `current_app` is persisted in `localStorage` and switchable in the global header dropdown.

## Generalized Code (通用代码)

All dropdown enums live in `SysGeneralizedCode`:
- `Domain` + `Code` + `Label`/`LabelEn` + `TagType` + `SortOrder` + `IsActive`
- Two-layer label resolution: DB override → i18n fallback (`gc.{domain}.{code}`)
- Consumed via `GcSelect.vue` or `useGeneralizedCode()` composable
- **Always provide `fallback` prop** for zero-downtime adoption

Key domains:
`OPP_STAGE`, `QUO_STATUS`, `INDUSTRY`, `CURRENCY`, `PM_TRACE_STATUS`, `ACT_STATUS`, `APPROVAL_STEP_TYPE`, `APPROVAL_STEP_MODE`, `REJECT_BEHAVIOR`, `RULE_TYPE`, `SEQUENCE_RESET_RULE`, `LOGIN_DOMAIN`

Management page: `SystemSettings → Generalized Code`

## Field Control (字段控制)

Per-page per-module field visibility via `SysFieldControl`:
- Registry: `src/config/field-registry.ts` — declares all pages and their fields
- Composable: `useFieldControl('ModuleCode', 'PageCode')`
- Method: `isVisible('fieldCode')` controls `v-if` on table columns and form items
- Admin configures visibility in `SystemSettings → Field Control`

## Navigation Config (动态导航)

Sidebar menus stored in `SysNavigationConfig`:
- Fields: `NavCode`, `ModuleCode`, `RoutePath`, `IconName`, `NavLabel`/`NavLabelEn`, `SortOrder`, `IsVisible`
- Admin configures in `SystemSettings → Navigation`
- Falls back to hardcoded navigation in `MainLayout.vue` if DB returns empty
- Two-layer label resolution: DB override → i18n fallback

## i18n Translation

Two-layer resolution:
1. **Static files**: `src/locales/zh.ts`, `en.ts` — base translations
2. **DB override**: `SysI18nMessage` — admin can override without code change

`I18nMessage/active` endpoint is `[AllowAnonymous]` so the login page can load translations without a token.

Locale switching: `setLocale(lang)` → `window.location.reload()`

## Approval Workflow

Data model:
- `SysApprovalTemplate` → `SysApprovalStep` → `SysApprovalRule` + `SysApprovalCondition`
- `SysApprovalInstance` → `SysApprovalRecord` + `SysApprovalInstanceTask`

Step modes: SEQUENTIAL, PARALLEL, PARALLEL_ANY, CONDITIONAL, CC

Reject behaviors: REJECT_AND_CLOSE, REJECT_TO_PREV, REJECT_TO_STEP, REJECT_TO_START, REJECT_TO_REQUESTOR

Business decoupling:
- `IBusinessVariableProvider` — feeds context variables into rule conditions
- `IBusinessStatusUpdater` — decouples approval engine from business modules (Quotation, PM Cycle Time)

Approver resolvers (registered as strategies):
- `FixedRoleResolver`, `FixedUserResolver`, `OrgTreeResolver`, `SubmitterResolver`

## Authentication

- JWT Bearer token stored in `localStorage` as `token`
- `request.ts` interceptor adds `Authorization: Bearer <token>` + `X-App-Code: <app>`
- 401 response → clear token/userInfo → redirect to `/login` (but NOT if already on `/login`)
- Default admin: `admin` / `123456`
- Supports both local auth and QAD (Progress OpenEdge) auth via `IQadAuthService`
