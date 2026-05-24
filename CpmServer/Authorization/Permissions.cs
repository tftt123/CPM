namespace CpmServer.Authorization;

public static class Permissions
{
    // System management
    public const string CanManageSystem = "system.manage";
    public const string CanManageUsers = "users.manage";
    public const string CanManageRoles = "roles.manage";
    public const string CanViewSettings = "settings.view";

    // Master data
    public const string CanManageCustomers = "customers.manage";
    public const string CanManageProducts = "products.manage";
    public const string CanManageMfgProcesses = "mfg.manage";

    // Quotation / RFQ
    public const string CanManageQuotations = "quotations.manage";
    public const string CanApproveQuotation = "quotations.approve";

    // PM
    public const string CanManageProjectTrace = "pm.manage";
    public const string CanApproveCycleTime = "pm.cycletime.approve";

    // Approval workflow
    public const string CanManageApprovalTemplates = "approval.templates.manage";
    public const string CanViewApprovalCenter = "approval.center.view";

    // Generalized code / config
    public const string CanManageGeneralizedCode = "gc.manage";
    public const string CanManageNavigation = "nav.manage";
    public const string CanManageFieldControl = "fieldcontrol.manage";
    public const string CanManageTranslations = "i18n.manage";

    // Email / Alerts
    public const string CanManageEmailTemplates = "email.templates.manage";
    public const string CanManageAlertRecipients = "alerts.manage";

    public static readonly string[] All =
    [
        CanManageSystem,
        CanManageUsers,
        CanManageRoles,
        CanViewSettings,
        CanManageCustomers,
        CanManageProducts,
        CanManageMfgProcesses,
        CanManageQuotations,
        CanApproveQuotation,
        CanManageProjectTrace,
        CanApproveCycleTime,
        CanManageApprovalTemplates,
        CanViewApprovalCenter,
        CanManageGeneralizedCode,
        CanManageNavigation,
        CanManageFieldControl,
        CanManageTranslations,
        CanManageEmailTemplates,
        CanManageAlertRecipients
    ];
}
