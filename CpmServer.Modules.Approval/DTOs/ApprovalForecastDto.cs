namespace CpmServer.Modules.Approval.DTOs;

/// <summary>
/// 审批预测 —— 单步预测结果
/// </summary>
public class ApprovalForecastStepDto
{
    public long StepId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public string StepMode { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public bool CanReject { get; set; }
    public bool CanTransfer { get; set; }
    public int? TimeoutHours { get; set; }

    /// <summary>
    /// 该步骤预测的审批人列表（已解析到具体人或角色）
    /// </summary>
    public List<ApprovalForecastApproverDto> Approvers { get; set; } = new();
}

/// <summary>
/// 审批预测 —— 单个审批人预测结果
/// </summary>
public class ApprovalForecastApproverDto
{
    /// <summary>USER / ROLE</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称：用户为 RealName，角色为 RoleName
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 当 Type=USER 时，具体的用户Id
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 当 Type=ROLE 时，角色代码
    /// </summary>
    public string? RoleCode { get; set; }

    /// <summary>
    /// 用户头像或角色图标（前端展示用）
    /// </summary>
    public string? AvatarUrl { get; set; }
}
