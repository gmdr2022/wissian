using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

// One calibrated hash, no database identity. Unknown users incur a normal verification cost.
public sealed class UnknownPasswordCheck
{
    private readonly PasswordHasher<WissianUser> hasher;
    private readonly WissianUser dummy = new();
    private readonly string hash;
    public UnknownPasswordCheck(IOptions<PasswordHasherOptions> options)
    {
        hasher = new(options);
        hash = hasher.HashPassword(dummy, Guid.NewGuid().ToString("N"));
    }
    public void Verify(string password) => hasher.VerifyHashedPassword(dummy, hash, password);
}
