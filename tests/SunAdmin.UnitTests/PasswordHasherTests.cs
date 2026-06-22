using SunAdmin.Infrastructure.Security;

namespace SunAdmin.UnitTests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void VerifyPassword_ReturnsTrue_ForMatchingPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.HashPassword("Admin@123456");

        Assert.True(hasher.VerifyPassword("Admin@123456", hash));
        Assert.False(hasher.VerifyPassword("wrong-password", hash));
    }
}
