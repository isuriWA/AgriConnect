namespace OfficerService.Entities;

public enum VerificationStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected
}

public enum SuitabilityStatus
{
    NotAssessed,
    Suitable,
    ConditionallySuitable,
    NotSuitable
}

public enum CallbackStatus
{
    NotSent,
    Sent,
    Failed
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High
}

public enum ComplianceSubjectType
{
    Farmer,
    Supplier
}

public enum ComplianceStatus
{
    Compliant,
    UnderWatch,
    NonCompliant
}