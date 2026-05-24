using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Data;

public class CpmDbContext : DbContext
{
    public CpmDbContext(DbContextOptions<CpmDbContext> options) : base(options) { }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeKinds();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeKinds();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimeKinds()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Utc)
                {
                    property.CurrentValue = dt.Kind == DateTimeKind.Local
                        ? dt.ToUniversalTime()
                        : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }

    // P1 - System
    public DbSet<SysUser> Users => Set<SysUser>();
    public DbSet<SysRole> Roles => Set<SysRole>();
    public DbSet<SysUserRole> UserRoles => Set<SysUserRole>();
    public DbSet<SysUserSite> UserSites => Set<SysUserSite>();
    public DbSet<SysDept> Depts => Set<SysDept>();

    // P2 - CRM
    public DbSet<CrmCustomer> Customers => Set<CrmCustomer>();
    public DbSet<CrmProduct> Products => Set<CrmProduct>();

    // P3 - Quotation
    public DbSet<QuoOpportunity> Opportunities => Set<QuoOpportunity>();
    public DbSet<QuoQuotation> Quotations => Set<QuoQuotation>();
    public DbSet<QuoQuotationItem> QuotationItems => Set<QuoQuotationItem>();

    // P3 - Approval Workflow
    public DbSet<SysApprovalTemplate> ApprovalTemplates => Set<SysApprovalTemplate>();
    public DbSet<SysApprovalStep> ApprovalSteps => Set<SysApprovalStep>();
    public DbSet<SysApprovalRule> ApprovalRules => Set<SysApprovalRule>();
    public DbSet<SysApprovalCondition> ApprovalConditions => Set<SysApprovalCondition>();
    public DbSet<SysApprovalInstance> ApprovalInstances => Set<SysApprovalInstance>();
    public DbSet<SysApprovalRecord> ApprovalRecords => Set<SysApprovalRecord>();
    public DbSet<SysApprovalInstanceTask> ApprovalInstanceTasks => Set<SysApprovalInstanceTask>();
    public DbSet<SysModuleTypeConfig> ModuleTypeConfigs => Set<SysModuleTypeConfig>();
    public DbSet<SysSiteSettings> SiteSettings => Set<SysSiteSettings>();

    // P3 - Email
    public DbSet<SysEmailTemplate> EmailTemplates => Set<SysEmailTemplate>();
    public DbSet<SysEmailLog> EmailLogs => Set<SysEmailLog>();
    public DbSet<SysEmailConfig> EmailConfigs => Set<SysEmailConfig>();

    // Alert
    public DbSet<SysAlertRecipient> AlertRecipients => Set<SysAlertRecipient>();

    // Manufacturing Process
    public DbSet<MfgProcess> MfgProcesses => Set<MfgProcess>();
    public DbSet<MfgSubCategory> MfgSubCategories => Set<MfgSubCategory>();
    public DbSet<MfgEquipment> MfgEquipments => Set<MfgEquipment>();

    // File Upload
    public DbSet<SysFileRecord> FileRecords => Set<SysFileRecord>();

    // Auth
    public DbSet<SysRefreshToken> RefreshTokens => Set<SysRefreshToken>();

    // PM - Product Trace
    public DbSet<PmProjectTrace> ProjectTraces => Set<PmProjectTrace>();
    public DbSet<PmProjectTraceStep> ProjectTraceSteps => Set<PmProjectTraceStep>();
    public DbSet<PmProjectTraceStepActualCycleTime> StepActualCycleTimes => Set<PmProjectTraceStepActualCycleTime>();
    public DbSet<PmStepCycleTimeChangeRequest> StepCycleTimeChangeRequests => Set<PmStepCycleTimeChangeRequest>();
    public DbSet<PmStepCycleTimeChangeDetail> StepCycleTimeChangeDetails => Set<PmStepCycleTimeChangeDetail>();

    // Sequence Rule
    public DbSet<SysSequenceRule> SequenceRules => Set<SysSequenceRule>();

    // Field Control
    public DbSet<SysFieldControl> FieldControls => Set<SysFieldControl>();

    // i18n Translation
    public DbSet<SysI18nMessage> I18nMessages => Set<SysI18nMessage>();

    // Navigation Config
    public DbSet<SysNavigationConfig> NavigationConfigs => Set<SysNavigationConfig>();

    // Generalized Code
    public DbSet<SysGeneralizedCode> GeneralizedCodes => Set<SysGeneralizedCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysUserRole>()
            .HasKey(e => new { e.UserId, e.RoleId });

        modelBuilder.Entity<SysUserSite>()
            .HasKey(e => new { e.UserId, e.Site });

        // SysDept - Manager 鍏崇郴
        modelBuilder.Entity<SysDept>()
            .HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        // SysUser - Dept 鍏崇郴
        modelBuilder.Entity<SysUser>()
            .HasOne(u => u.Dept)
            .WithMany()
            .HasForeignKey(u => u.DeptId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<QuoOpportunity>()
            .HasIndex(e => e.OpportunityNo)
            .IsUnique();

        modelBuilder.Entity<QuoQuotation>()
            .HasIndex(e => e.QuotationNo)
            .IsUnique();

        modelBuilder.Entity<SysApprovalTemplate>()
            .HasIndex(e => e.TemplateCode)
            .IsUnique();

        modelBuilder.Entity<SysModuleTypeConfig>()
            .HasIndex(e => e.ModuleType)
            .IsUnique();

        modelBuilder.Entity<SysEmailTemplate>()
            .HasIndex(e => new { e.TemplateCode, e.Site })
            .IsUnique();

        // Approval Workflow - indexes
        modelBuilder.Entity<SysApprovalRule>()
            .HasIndex(e => e.StepId);

        modelBuilder.Entity<SysApprovalCondition>()
            .HasIndex(e => e.StepId);

        modelBuilder.Entity<SysApprovalInstanceTask>()
            .HasIndex(e => e.InstanceId);

        modelBuilder.Entity<SysApprovalInstanceTask>()
            .HasIndex(e => new { e.AssigneeId, e.Status });

        modelBuilder.Entity<SysApprovalInstanceTask>()
            .HasIndex(e => new { e.AssigneeRole, e.Status });

        // Manufacturing Process - Site indexes
        modelBuilder.Entity<MfgProcess>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<MfgSubCategory>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<MfgEquipment>()
            .HasIndex(e => e.Site);

        // PM - Product Trace indexes
        modelBuilder.Entity<PmProjectTrace>()
            .HasIndex(e => new { e.CustomerName, e.ProductCode, e.Status, e.CreatedAt });

        modelBuilder.Entity<PmProjectTrace>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<PmProjectTraceStep>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<PmProjectTraceStepActualCycleTime>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<PmStepCycleTimeChangeRequest>()
            .HasIndex(e => new { e.StepId, e.ApprovalStatus });

        modelBuilder.Entity<PmStepCycleTimeChangeRequest>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<PmStepCycleTimeChangeDetail>()
            .HasIndex(e => e.Site);

        // Email - Site indexes
        modelBuilder.Entity<SysEmailTemplate>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<SysEmailConfig>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<SysEmailLog>()
            .HasIndex(e => e.Site);

        // Alert - Site index
        modelBuilder.Entity<SysAlertRecipient>()
            .HasIndex(e => e.Site);

        // Sequence Rule - unique index on ModuleType + Site
        modelBuilder.Entity<SysSequenceRule>()
            .HasIndex(e => new { e.ModuleType, e.Site })
            .IsUnique();

        // Field Control - unique index on ModuleCode + PageCode + FieldCode + Site
        modelBuilder.Entity<SysFieldControl>()
            .HasIndex(e => new { e.ModuleCode, e.PageCode, e.FieldCode, e.Site })
            .IsUnique();

        // i18n - unique index on MessageKey + Site
        modelBuilder.Entity<SysI18nMessage>()
            .HasIndex(e => new { e.MessageKey, e.Site })
            .IsUnique();

        // Navigation Config - unique index on NavCode + Site
        modelBuilder.Entity<SysNavigationConfig>()
            .HasIndex(e => new { e.NavCode, e.Site })
            .IsUnique();

        modelBuilder.Entity<SysNavigationConfig>()
            .HasIndex(e => e.Site);

        // Generalized Code - unique index on Domain + Code + App + Site
        modelBuilder.Entity<SysGeneralizedCode>()
            .HasIndex(e => new { e.Domain, e.Code, e.App, e.Site })
            .IsUnique();

        modelBuilder.Entity<SysGeneralizedCode>()
            .HasIndex(e => new { e.Domain, e.App, e.Site });

        modelBuilder.Entity<SysGeneralizedCode>()
            .HasIndex(e => e.Site);

        modelBuilder.Entity<SysGeneralizedCode>()
            .HasIndex(e => e.App);
    }
}
