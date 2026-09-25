using Microsoft.AspNetCore.Identity;

namespace Wissian.Persistence;

public sealed class WissianUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
