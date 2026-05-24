namespace CpmServer.Constants;

public static class QuotationConstants
{
    public static class Status
    {
        public const int Draft = 0;
        public const int PendingReview = 1;
        public const int PendingApproval = 2;
        public const int Issued = 3;
        public const int Completed = 9;
    }

    public static class OpportunityStage
    {
        public const string New = "NEW";
        public const string Qualified = "QUALIFIED";
        public const string Proposal = "PROPOSAL";
        public const string Negotiation = "NEGOTIATION";
        public const string Closed = "CLOSED";
        public const string Lost = "LOST";
    }
}
