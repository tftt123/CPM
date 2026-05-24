# CPM Project Structure

```
CPM_SrcCode/
├── CLAUDE.md                    # Project context (concise cheat sheet)
├── docs/                        # Detailed documentation
│   ├── architecture.md
│   ├── development.md
│   └── project-structure.md
│
└── CpmServer/                   # Backend root
    ├── CpmServer.csproj         # Main API project
    ├── CpmServer.Core/          # Core library
    │   ├── Data/
    │   │   └── CpmDbContext.cs  # EF DbContext with all DbSets
    │   ├── Models/              # All EF entities
    │   └── Repositories/        # IRepository<T>, IUnitOfWork
    │
    ├── Controllers/             # API controllers
    ├── Services/                # Business services
    │   └── Approvers/           # Approval resolver strategies
    ├── DTOs/                    # Request/response DTOs
    │   ├── Auth/
    │   ├── Customer/
    │   ├── Product/
    │   ├── Quotation/
    │   ├── Role/
    │   └── User/
    ├── Authorization/           # Policies + permission attributes
    ├── Middleware/              # GlobalExceptionHandling, CorrelationId
    ├── Common/                  # ApiResult, JwtHelper, helpers
    ├── Hubs/                    # SignalR
    ├── Protos/                  # gRPC proto definitions
    ├── appsettings.json         # Configuration
    ├── appsettings.Development.json
    ├── Program.cs               # DI container + service registration
    │
    └── cpm-web/                 # Frontend (Vite)
        ├── index.html           # Entry HTML (Montserrat + Noto Sans SC fonts)
        ├── src/
        │   ├── api/
        │   │   ├── auth.ts
        │   │   ├── request.ts     # Axios interceptor (token + X-App-Code)
        │   │   ├── generalizedCode.ts
        │   │   ├── fieldControl.ts
        │   │   ├── i18nMessage.ts
        │   │   └── ...
        │   ├── components/
        │   │   └── GcSelect.vue   # GeneralizedCode dropdown component
        │   ├── composables/
        │   │   ├── useI18n.ts
        │   │   ├── useFieldControl.ts
        │   │   ├── useGeneralizedCode.ts
        │   │   ├── useNavigationConfig.ts
        │   │   └── useTheme.ts
        │   ├── config/
        │   │   └── field-registry.ts
        │   ├── locales/
        │   │   ├── zh.ts
        │   │   └── en.ts
        │   ├── router/
        │   │   └── index.ts
        │   ├── stores/
        │   │   ├── user.ts
        │   │   ├── locale.ts
        │   │   └── uiControl.ts
        │   ├── styles/
        │   │   ├── salesforce-theme.css   # Primary theme + --cpm-* vars
        │   │   └── design-system.css      # Alternative design tokens
        │   └── views/
        │       ├── layout/
        │       │   └── MainLayout.vue     # Global header + sidebar
        │       ├── login/
        │       │   └── LoginView.vue
        │       ├── customer/
        │       ├── product/
        │       ├── quotation/
        │       │   ├── OpportunityList.vue
        │       │   ├── OpportunityForm.vue
        │       │   ├── QuotationList.vue
        │       │   ├── QuotationDetail.vue
        │       │   └── QuotationForm.vue
        │       ├── mfg/
        │       │   └── MfgProcessManage.vue
        │       ├── pm/
        │       │   ├── ProductTraceList.vue
        │       │   ├── ProductTraceDetail.vue
        │       │   └── ActualCycleTimeManage.vue
        │       ├── approval/
        │       │   ├── ApprovalCenter.vue
        │       │   └── ApprovalConfig.vue
        │       ├── system/
        │       │   ├── SystemSettings.vue       # Tabs container
        │       │   ├── GeneralizedCodeManage.vue
        │       │   ├── FieldControlConfig.vue
        │       │   ├── NavigationConfig.vue
        │       │   ├── TranslationManage.vue
        │       │   ├── EmailTemplateManage.vue
        │       │   └── AlertRecipientManage.vue
        │       └── profile/
        │           └── ProfileView.vue
        └── public/
            └── spxlogo/           # Brand logos
```

## Key Files Quick Reference

| Purpose | Path |
|---------|------|
| DbContext | `CpmServer.Core/Data/CpmDbContext.cs` |
| DI / Program | `CpmServer/Program.cs` |
| JWT Helper | `CpmServer/Common/JwtHelper.cs` |
| CurrentUser | `CpmServer.SharedKernel/Identity/CurrentUser.cs` |
| Request interceptor | `cpm-web/src/api/request.ts` |
| Router | `cpm-web/src/router/index.ts` |
| Layout | `cpm-web/src/views/layout/MainLayout.vue` |
| Global styles | `cpm-web/src/styles/salesforce-theme.css` |
| i18n messages | `cpm-web/src/locales/zh.ts`, `en.ts` |
| Field registry | `cpm-web/src/config/field-registry.ts` |
| GcSelect component | `cpm-web/src/components/GcSelect.vue` |
