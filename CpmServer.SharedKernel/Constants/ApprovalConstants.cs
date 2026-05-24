namespace CpmServer.Constants;

public static class ApprovalConstants
{
    public static class StepType
    {
        public const string Review = "REVIEW";
        public const string Approval = "APPROVAL";
        public const string Notify = "NOTIFY";
    }

    public static class StepMode
    {
        public const string Start = "START";
        public const string Sequential = "SEQUENTIAL";
        public const string Parallel = "PARALLEL";
        public const string ParallelAny = "PARALLEL_ANY";
        public const string Conditional = "CONDITIONAL";
        public const string Cc = "CC";
        public const string End = "END";
    }

    public static class Action
    {
        public const string Approve = "APPROVE";
        public const string Reject = "REJECT";
        public const string Transfer = "TRANSFER";
        public const string Skip = "SKIP";
    }

    public static class RejectBehavior
    {
        public const string RejectAndClose = "REJECT_AND_CLOSE";
        public const string RejectToPrev = "REJECT_TO_PREV";
        public const string RejectToStep = "REJECT_TO_STEP";
        public const string RejectToStart = "REJECT_TO_START";
        public const string RejectToRequestor = "REJECT_TO_REQUESTOR";
    }

    public static class TaskStatus
    {
        public const int Pending = 0;
        public const int Approved = 1;
        public const int Rejected = 2;
        public const int Skipped = 3;
        public const int Timeout = 4;
    }

    public static class InstanceStatus
    {
        public const int Active = 0;
        public const int Completed = 1;
        public const int Rejected = 2;
    }

    public static class RuleType
    {
        public const string FixedRole = "FIXED_ROLE";
        public const string FixedUser = "FIXED_USER";
        public const string OrgTree = "ORG_TREE";
        public const string Submitter = "SUBMITTER";
    }
}
