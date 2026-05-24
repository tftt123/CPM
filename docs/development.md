# CPM Development Guide

## Environment Setup

### Frontend
```bash
cd CpmServer/cpm-web
npm install
npm run dev          # http://localhost:5173
```

### Backend
```bash
cd CpmServer
dotnet run           # http://localhost:5001
```

### Database
- SQL Server (LocalDB or instance)
- Connection string in `appsettings.Development.json`

## Naming Conventions

### Backend
| Type | Pattern | Example |
|------|---------|---------|
| System entities | `Sys*` | `SysUser`, `SysRole` |
| CRM entities | `Crm*` | `CrmCustomer`, `CrmProduct` |
| Quotation entities | `Quo*` | `QuoQuotation`, `QuoOpportunity` |
| PM entities | `Pm*` | `PmProjectTrace` |
| Mfg entities | `Mfg*` | `MfgProcess`, `MfgEquipment` |
| Controllers | `[Name]Controller.cs` | `GeneralizedCodeController.cs` |
| Services | `I[Name]Service` / `[Name]Service` | `IQuotationService` / `QuotationService` |
| DTOs | `[Name]Dto` | `GeneralizedCodeDto` |
| DbSets | Plural | `Users`, `Quotations` |

### Frontend
| Type | Pattern | Example |
|------|---------|---------|
| Views | `[Feature][Action].vue` | `QuotationList.vue`, `CustomerForm.vue` |
| Components | PascalCase | `GcSelect.vue` |
| Composables | `use[Name].ts` | `useFieldControl.ts` |
| API modules | `src/api/[feature].ts` | `generalizedCode.ts` |
| Stores | `use[Name]Store` | `useUserStore` |
| CSS vars | `--cpm-*` (primary), `--slds-*` (legacy) | `--cpm-font-family` |

## Critical Development Rules

### 1. Entity Changes → EF Migration
**Always run migration after modifying any entity.**
```bash
cd CpmServer.Core
dotnet ef migrations add [MigrationName] --startup-project ../CpmServer
dotnet ef database update --startup-project ../CpmServer
```

### 2. Kill Processes Before Build
DLL locking is common on Windows. Kill backend processes first:
```powershell
Stop-Process -Name devenv -Force -ErrorAction SilentlyContinue
Stop-Process -Name CpmServer -Force -ErrorAction SilentlyContinue
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue
```

### 3. Frontend Build Verification
`vue-tsc --build` may fail with filesystem errors on Windows. Use `npx vite build` as the reliable fallback.

### 4. Vue Template Type Safety
Never use `as any` in Vue template expressions. It produces invalid JS during Vite compilation and causes dynamic module load failures.

### 5. No Hardcoded Text
All user-facing text must go through i18n:
- Vue templates: `t('key')`
- Static files: `src/locales/zh.ts`, `en.ts`
- Labels for enums: `SysGeneralizedCode.Label` / `LabelEn`

### 6. No Hardcoded Enums
All dropdown options must use `GcSelect` with a `SysGeneralizedCode` domain. Always provide the `fallback` prop with original hardcoded values for zero-downtime fallback.

### 7. Site + App on New Entities
Every new entity that needs tenant isolation must include:
```csharp
public string? Site { get; set; }
public string? App { get; set; }
```
And queries must apply `ApplyAppSiteFilter()` or equivalent.

### 8. Intranet Deployment Constraints
System runs on an internal network without internet access:
- Avoid external CDN-only dependencies
- Fonts (Google Fonts) must keep system fallbacks: `PingFang SC`, `Microsoft YaHei`

### 9. Soft Delete Pattern
Use `IsActive` flag rather than physical deletion where applicable. Batch update endpoints (e.g., `GeneralizedCode/batch`) soft-delete items not in the incoming list.

### 10. Pre-Commit Checklist
Before committing or creating a PR, verify:
1. New entity has `Site` + `App` fields
2. New text is in i18n files
3. New enum uses `GeneralizedCode` + `GcSelect`
4. EF migration has been run
5. Both frontend (`npx vite build`) and backend (`dotnet build`) compile cleanly
