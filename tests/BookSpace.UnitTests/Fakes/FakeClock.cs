using BookSpace.Core.Abstractions;

namespace BookSpace.UnitTests.Fakes;

public class FakeClock : IClock
{
    /// <summary>ტესტების "ახლა": სამშაბათი, 2026-11-10 10:30.</summary>
    public static readonly DateTime DefaultNow = new(2026, 11, 10, 10, 30, 0);

    public FakeClock(DateTime? now = null)
    {
        Now = now ?? DefaultNow;
    }

    public DateTime Now { get; set; }
}
