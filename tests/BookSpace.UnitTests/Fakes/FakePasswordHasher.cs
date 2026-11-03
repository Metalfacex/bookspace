using BookSpace.Core.Abstractions;

namespace BookSpace.UnitTests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "hashed:" + password;
    public bool Verify(string hash, string password) => hash == "hashed:" + password;
}
