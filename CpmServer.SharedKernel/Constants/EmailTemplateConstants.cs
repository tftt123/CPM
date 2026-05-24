namespace CpmServer.Constants;

public static class EmailTemplateConstants
{
    public const string ApprovalTimeout = "APPROVAL_TIMEOUT";
    public const string CycleTimeExceeded = "CYCLE_TIME_EXCEEDED";

    public static string GetApprovalApprovedTemplate(string businessType) =>
        $"{businessType.ToUpperInvariant()}_APPROVED";

    public static string GetApprovalRejectedTemplate(string businessType) =>
        $"{businessType.ToUpperInvariant()}_REJECTED";
}
