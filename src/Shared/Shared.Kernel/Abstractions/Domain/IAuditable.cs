namespace Shared.Kernel.Abstractions.Domain;

public interface IAuditable
{
    public DateTimeOffset InsertedAt { get; set; }

    public string? InsertedBy { get; set; }
    public string? InsertedByDisplayName { get; set; }

    public DateTimeOffset LastModifiedAt { get; set; }

    public string? LastModifiedBy { get; set; }
    public string? LastModifiedByDisplayName { get; set; }
}
