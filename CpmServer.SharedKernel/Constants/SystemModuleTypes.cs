namespace CpmServer.Constants;

/// <summary>
/// 系统预定义业务模块类型
/// </summary>
public static class SystemModuleTypes
{
    public const string Quotation = "Quotation";
    public const string PmStepCycleTime = "PmStepCycleTime";
    public const string Procurement = "Procurement";
    public const string Sample = "Sample";
    public const string Opportunity = "Opportunity";

    public static readonly string[] All = new[]
    {
        Quotation, PmStepCycleTime, Procurement, Sample, Opportunity
    };
}
