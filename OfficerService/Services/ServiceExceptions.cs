namespace OfficerService.Services;

/// <summary>Request clashes with existing data (e.g. duplicate email). Mapped to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>The caller is not allowed to act on this item (e.g. not the assigned officer). Mapped to HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
