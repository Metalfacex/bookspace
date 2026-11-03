# Coverage ანგარიში (პრაქტიკული 3, პუნქტი 6)

ანგარიში გენერირდება ავტომატურად ყოველ push-ზე GitHub Actions-ში (`.github/workflows/ci.yml`):
coverlet (`--collect:"XPlat Code Coverage"`) → ReportGenerator. სრული HTML ანგარიში — CI run-ის **Artifacts → coverage-report**.

ლოკალურად:

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html
```

## შედეგი (CI run #1, 2026-11-03)

ტესტები: **85 unit + 9 integration = 94, ყველა გადის** (integration — MS SQL Server 2022 კონტეინერზე).

| მეტრიკა | მნიშვნელობა |
|---|---|
| Line coverage | **78.4%** (389 / 496) |
| Branch coverage | **75%** (126 / 168) |
| Method coverage | **82.8%** (92 / 111) |

| Assembly / კლასი | Line coverage |
|---|---:|
| **BookSpace.Core** (ბიზნეს-ლოგიკა) | **97%** |
| `Rules.DailyLimitPolicy`, `Rules.BookingRules` | 100% |
| `Services.BookingService`, `AvailabilityService`, `ReminderService`, `StatisticsService` | 100% |
| `Services.AuthService` | 94.1% |
| `Validation.RoomValidator` | 91.6% |
| `Services.RoomService` | 88.8% |
| **BookSpace.Infrastructure** | **48.9%** |
| `Data.BookSpaceDbContext` | 100% |
| `Repositories.BookingRepository` | 96.2% |
| `Repositories.UserRepository` | 92.3% |
| `Repositories.RoomRepository` | 53.3% |
| `Data.DbErrors` | 66.6% |
| `Data.DbInitializer`, `DependencyInjection`, `SystemClock`, `IdentityPasswordHasher`, `LoggingEmailSender` | **0%** |

`BookSpace.Web` (კონტროლერები, Views) ტესტ-პროექტებიდან არ არის რეფერენსირებული, ამიტომ ანგარიშში საერთოდ არ ჩანს — ესეც დაუფარავი ფენაა.

## ერთი უტესტო ადგილი: `DbInitializer` (0%)

**ფაილი:** `src/BookSpace.Infrastructure/Data/DbInitializer.cs`

**რას აკეთებს:** აპლიკაციის გაშვებისას ქმნის სქემას (`EnsureCreated`), ამატებს ადმინისტრატორისა და მენეჯერის ანგარიშებს კონფიგურაციიდან და საწყის ოთახებს, თუ ცხრილი ცარიელია.

**რატომ არის სარისკო, რომ უტესტოა:**

- თუ `EnsureUserAsync` ელფოსტას არ ანორმალიზებს ან ხელმეორე გაშვებაზე იმავე ადმინს ისევ დაამატებს, unique index-ის გამო **აპლიკაცია საერთოდ ვერ ჩაირთვება** — და ამას მხოლოდ deploy-ის დროს დავინახავდით.
- ცარიელი `AdminPassword` (პროდაქშენის `appsettings.json`) უნდა ნიშნავდეს „არ შექმნა“, და არა „შექმენი ცარიელი პაროლით“ — ეს ქცევა დოკუმენტირებულია მხოლოდ კოდში.

**როგორ დაიფარება (შემდეგი ნაბიჯი):** integration-ტესტი `TestDatabase`-ზე —

1. `InitializeBookSpaceDatabaseAsync` ორჯერ ზედიზედ → `Users`-ში ზუსტად 1 ადმინი, `Rooms`-ში 4 ოთახი (idempotency).
2. ცარიელი `AdminPassword` → ადმინი არ იქმნება.
3. `Admin@X.edu.ge` კონფიგურაციაში → ბაზაში `admin@x.edu.ge` (ნორმალიზაცია).

მეორე პრიორიტეტი — `RoomsController`/`BookingsController`-ის HTTP-დონის ტესტები `WebApplicationFactory`-ით (ავტორიზაცია: სტუდენტი `/Rooms/Create`-ზე უნდა მიიღოს 403).
