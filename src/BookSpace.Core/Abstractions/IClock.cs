namespace BookSpace.Core.Abstractions;

/// <summary>მიმდინარე დრო უნივერსიტეტის დროის სარტყელში. ტესტებში იცვლება ფიქსირებული საათით.</summary>
public interface IClock
{
    DateTime Now { get; }
    DateOnly Today => DateOnly.FromDateTime(Now);
}
