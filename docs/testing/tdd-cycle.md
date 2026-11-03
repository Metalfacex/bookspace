# TDD ციკლი: `DailyLimitPolicy` (პრაქტიკული 3, პუნქტი 3)

**მოთხოვნა:** სტუდენტს ერთ დღეში მაქსიმუმ 3 აქტიური ჯავშნის გაკეთება შეუძლია. გაუქმებული ჯავშნები და სხვა დღის ჯავშნები არ ითვლება.

ფუნქცია სრული **Red → Green → Refactor** ციკლით დაიწერა. თითოეული ეტაპი ცალკე commit-ია რეპოზიტორიის ისტორიაში (`git log --oneline -- src/BookSpace.Core/Rules tests/BookSpace.UnitTests/Rules`).

## 🔴 1. Red — ჯერ ტესტი

Commit: `test(red): DailyLimitPolicy — დღიური ლიმიტის ტესტები`

დაიწერა `tests/BookSpace.UnitTests/Rules/DailyLimitPolicyTests.cs`, სანამ `DailyLimitPolicy` კლასი არსებობდა:

```csharp
[Theory]
[InlineData(2, true)]   // ლიმიტამდე ერთი
[InlineData(3, false)]  // ზუსტად ლიმიტზე
[InlineData(4, false)]
public void CanBook_AtBoundaryOfLimit_ReturnsExpected(int existing, bool expected)
{
    // Arrange
    var policy = new DailyLimitPolicy(maxPerDay: 3);
    var bookings = ActiveBookings(existing, Day);

    // Act
    var result = policy.CanBook(bookings, Day);

    // Assert
    Assert.Equal(expected, result);
}
```

ამ ეტაპზე იმავე commit-ში დაემატა ცარიელი ჩონჩხი (`CanBook` აბრუნებს `true`-ს, `RemainingFor` → `0`), რომ ტესტები კომპილირდეს და **ჩავარდეს assertion-ზე**:

```
Failed  CanBook_AtBoundaryOfLimit_ReturnsExpected(existing: 3, expected: False)
Failed  CanBook_AtBoundaryOfLimit_ReturnsExpected(existing: 4, expected: False)
Failed  RemainingFor_TwoUsed_ReturnsOne
Failed  Constructor_LimitBelowOne_Throws(invalidLimit: 0)
Failed  Constructor_LimitBelowOne_Throws(invalidLimit: -1)
```

## 🟢 2. Green — მინიმალური კოდი

Commit: `feat(green): DailyLimitPolicy — მინიმალური იმპლემენტაცია`

```csharp
public bool CanBook(IEnumerable<Booking> userBookings, DateOnly date)
{
    var count = 0;
    foreach (var b in userBookings)
    {
        if (b.Status == BookingStatus.Active && b.Date == date) count++;
    }
    return count < MaxPerDay;
}

public int RemainingFor(IEnumerable<Booking> userBookings, DateOnly date)
{
    var count = 0;
    foreach (var b in userBookings)
    {
        if (b.Status == BookingStatus.Active && b.Date == date) count++;
    }
    return MaxPerDay - count < 0 ? 0 : MaxPerDay - count;
}
```

ყველა ტესტი მწვანეა — მაგრამ დათვლის ლოგიკა გამეორებულია.

## 🔵 3. Refactor — გასუფთავება ტესტების შეცვლის გარეშე

Commit: `refactor: DailyLimitPolicy — დუბლირების მოშორება და BookingService-ში ჩართვა`

- დათვლა ერთ ადგილზე გადავიდა (`RemainingFor`), `CanBook` მას იყენებს.
- `foreach` შეიცვალა LINQ-ით და `Booking.IsActive` თვისებით.
- `BookingService` იყენებს `DailyLimitPolicy`-ს და ლიმიტი კონფიგურაციიდან მოდის (`BookingRules.MaxBookingsPerDay`).

```csharp
public int RemainingFor(IEnumerable<Booking> userBookings, DateOnly date)
{
    var used = userBookings.Count(b => b.IsActive && b.Date == date);
    return Math.Max(0, MaxPerDay - used);
}

public bool CanBook(IEnumerable<Booking> userBookings, DateOnly date) => RemainingFor(userBookings, date) > 0;
```

ტესტები არ შეცვლილა და ისევ მწვანეა — ეს ადასტურებს, რომ რეფაქტორინგმა ქცევა არ შეცვალა.
