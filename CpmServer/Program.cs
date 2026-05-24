using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Services;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.Services;
using CpmServer.Modules.Approval.Services.Approvers;
using CpmServer.Modules.Quotation.Contracts;
using CpmServer.Modules.Quotation.Services;
using CpmServer.Modules.PM.Contracts;
using CpmServer.Modules.PM.Services;
using CpmServer.Modules.SequenceRule.Contracts;
using CpmServer.Modules.SequenceRule.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using CpmServer.Authorization;
using CpmServer.Core.Repositories;
using CpmServer.Middleware;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OfficeOpenXml;

// EPPlus license for non-commercial use
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .WriteTo.File("logs/cpm-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonDateTimeUtcConverter());
    });
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    });

// 基于策略的授权
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CanApproveQuotation, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanApproveQuotation)));
    options.AddPolicy(Policies.CanApproveCycleTime, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanApproveCycleTime)));
    options.AddPolicy(Policies.CanManageSystem, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageSystem)));
    options.AddPolicy(Policies.CanManageUsers, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageUsers)));
    options.AddPolicy(Policies.CanManageRoles, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageRoles)));
    options.AddPolicy(Policies.CanManageCustomers, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageCustomers)));
    options.AddPolicy(Policies.CanManageProducts, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageProducts)));
    options.AddPolicy(Policies.CanManageMfgProcesses, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageMfgProcesses)));
    options.AddPolicy(Policies.CanManageQuotations, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageQuotations)));
    options.AddPolicy(Policies.CanManageProjectTrace, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageProjectTrace)));
    options.AddPolicy(Policies.CanManageApprovalTemplates, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageApprovalTemplates)));
    options.AddPolicy(Policies.CanViewApprovalCenter, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanViewApprovalCenter)));
    options.AddPolicy(Policies.CanManageGeneralizedCode, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageGeneralizedCode)));
    options.AddPolicy(Policies.CanManageNavigation, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageNavigation)));
    options.AddPolicy(Policies.CanManageFieldControl, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageFieldControl)));
    options.AddPolicy(Policies.CanManageTranslations, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageTranslations)));
    options.AddPolicy(Policies.CanManageEmailTemplates, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageEmailTemplates)));
    options.AddPolicy(Policies.CanManageAlertRecipients, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageAlertRecipients)));
    options.AddPolicy(Policies.CanViewSettings, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CanViewSettings)));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks().AddDbContextCheck<CpmDbContext>();
builder.Services.AddSwaggerGen(options =>
{
    // 解决相同类名在不同命名空间下的冲突（模块拆分后可能出现）
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "_") ?? type.Name);

    // 添加 JWT 认证支持
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<CpmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// CORS - 允许 Vue 前端访问
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT 配置
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtHelper, JwtHelper>();

// JWT 认证
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var roleClaims = identity.FindAll(ClaimTypes.Role).ToList();
                    foreach (var claim in roleClaims)
                    {
                        identity.RemoveClaim(claim);
                        identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value.ToUpperInvariant()));
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// HTTP 上下文访问
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Repository + UnitOfWork
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 注册业务服务
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();

// P3 - 报价流程服务
builder.Services.AddScoped<IEmailService, EmailService>();

// Approval 模块 - 接口注册
builder.Services.AddScoped<IApprovalTemplateService, ApprovalTemplateService>();
builder.Services.AddScoped<IApprovalInstanceService, ApprovalInstanceService>();
builder.Services.AddScoped<IApprovalTaskService, ApprovalTaskService>();
builder.Services.AddScoped<IApprovalActionService, ApprovalActionService>();
builder.Services.AddScoped<IApprovalNotificationService, ApprovalNotificationService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IModuleTypeConfigService, ModuleTypeConfigService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<IPmProjectTraceService, PmProjectTraceService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// 审批引擎 - 业务变量提供者和状态更新器（解耦 Approval 与业务模块）
builder.Services.AddScoped<IBusinessVariableProvider, QuotationBusinessVariableProvider>();
builder.Services.AddScoped<IBusinessStatusUpdater, QuotationBusinessStatusUpdater>();
builder.Services.AddScoped<IBusinessStatusUpdater, PmStepCycleTimeBusinessStatusUpdater>();
builder.Services.AddScoped<IAlertService, AlertService>();

// Phase 1 - 审批人解析策略
builder.Services.AddScoped<IApproverResolver, FixedRoleResolver>();
builder.Services.AddScoped<IApproverResolver, FixedUserResolver>();
builder.Services.AddScoped<IApproverResolver, OrgTreeResolver>();
builder.Services.AddScoped<IApproverResolver, SubmitterResolver>();

// 流水号规则
builder.Services.AddScoped<ISequenceRuleService, SequenceRuleService>();

// 工艺维护
builder.Services.AddScoped<IMfgProcessService, MfgProcessService>();

// 字段控制
builder.Services.AddScoped<IFieldControlService, FieldControlService>();

// i18n 翻译管理
builder.Services.AddScoped<II18nMessageService, I18nMessageService>();

// 通用代码验证服务
builder.Services.AddScoped<IGeneralizedCodeService, GeneralizedCodeService>();

// QAD (Progress OpenEdge) 认证服务
builder.Services.AddSingleton<IQadAuthService, QadAuthService>();

// Hangfire 后台任务
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Hangfire 仪表盘（开发环境开放，生产环境建议加认证）
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthFilter() }
});

// 测试接口
app.MapGet("/api/test/hello", () => ApiResult.Success("Hello CPM"))
   .RequireCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// app.UseHttpsRedirection();

// 中间件顺序很重要
app.UseCors("AllowVueApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

// 注册 Hangfire 定时任务：每 5 分钟扫描一次超时审批任务
using (var scope = app.Services.CreateScope())
{
    var approvalService = scope.ServiceProvider.GetRequiredService<IApprovalService>();
    RecurringJob.AddOrUpdate("ScanApprovalTimeouts",
        () => approvalService.ScanTimeoutTasksAsync(),
        Cron.MinuteInterval(5));
}

// 种子数据：自动创建 admin 账号（密码从配置读取，未配置则跳过）
var defaultAdminPassword = builder.Configuration["SeedData:AdminPassword"];
if (!string.IsNullOrEmpty(defaultAdminPassword))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CpmDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var seedSite = builder.Configuration["SeedData:AdminSite"] ?? "NT01";

    if (!db.Users.Any(u => u.Username == "admin"))
    {
        var admin = new SysUser
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword),
            RealName = "管理员",
            Site = seedSite,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Users.Add(admin);
        db.SaveChanges();

        var adminRole = db.Roles.FirstOrDefault(r => r.RoleCode == "ADMIN");
        if (adminRole == null)
        {
            adminRole = new SysRole { RoleCode = "ADMIN", RoleName = "系统管理员", Site = seedSite };
            db.Roles.Add(adminRole);
            db.SaveChanges();
        }

        db.UserRoles.Add(new SysUserRole { UserId = admin.Id, RoleId = adminRole.Id });
        db.SaveChanges();

        db.UserSites.Add(new SysUserSite { UserId = admin.Id, Site = seedSite });
        db.SaveChanges();

        logger.LogInformation("Seed admin user created for site {Site}", seedSite);
    }
    else
    {
        var admin = db.Users.First(u => u.Username == "admin");
        if (!db.UserSites.Any(us => us.UserId == admin.Id))
        {
            db.UserSites.Add(new SysUserSite { UserId = admin.Id, Site = admin.Site ?? seedSite });
            db.SaveChanges();
        }
    }
}

app.Run();
