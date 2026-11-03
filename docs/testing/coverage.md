# Coverage ანგარიში (პრაქტიკული 3, პუნქტი 6)

ანგარიში გენერირდება ავტომატურად ყოველ push-ზე GitHub Actions-ში (`.github/workflows/ci.yml`):
coverlet (`--collect:"XPlat Code Coverage"`) → ReportGenerator. სრული HTML ანგარიში — CI run-ის **Artifacts → coverage-report**.

ლოკალურად:

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html
```

## შედეგი (main, PR #17-ის შემდეგ, 2026-11-03)

ტესტები: **85 unit + 14 integration = 99, ყველა გადის** (integration — MS SQL Server 2022 კონტეინერზე).

| მეტრიკა | პირველი ანგარიში | ახლა |
|---|---:|---:|
| Line coverage | 78.4% | **84.8%** (421 / 496) |
| Branch coverage | 75% | **79.7%** (134 / 168) |
| Method coverage | 82.8% | **84.6%** (94 / 111) |

| Assembly / კლასი | Line coverage |
|---|---:|
| **BookSpace.Core** (ბიზნეს-ლოგიკა) | **97%** |
| `Rules.DailyLimitPolicy`, `Rules.BookingRules` | 100% |
| `Services.BookingService`, `AvailabilityService`, `ReminderService`, `StatisticsService` | 100% |
| `Services.AuthService` | 94.1% |
| `Validation.RoomValidator` | 91.6% |
| `Services.RoomService` | 88.8% |
| **BookSpace.Infrastructure** | **65.6%** (იყო 48.9%) |
| `Data.BookSpaceDbContext`, `Data.DbInitializer` | 100% |
| `Repositories.BookingRepository` | 96.2% |
| `Repositories.UserRepository` | 92.3% |
| `Data.DbErrors` | 66.6% |
| `Repositories.RoomRepository` | 53.3% |
| `IdentityPasswordHasher`, `SystemClock`, `LoggingEmailSender`, `DependencyInjection` | **0%** |

`BookSpace.Web` (კონტროლერები, Views) ტესტ-პროექტებიდან არ არის რეფერენსირებული, ამიტომ ანგარიშში საერთოდ არ ჩანს — ესეც დაუფარავი ფენაა.

### ისტორია: `DbInitializer` 0% → 100%

პირველ ანგარიშში უტესტო ადგილად `DbInitializer` შევარჩიე. ის დაიფარა [PR #17](https://github.com/Metalfacex/bookspace/pull/17)-ით (5 integration-ტესტი: idempotency, ცარიელი პაროლი, ელფოსტის ნორმალიზაცია, Admin/Manager როლები, არსებული ოთახები). PR-მა code review გაიარა (იხ. `code-review.md`), review-ში აღმოჩენილი ერთდროული სტარტის პრობლემა გადავიდა Issue [#18](https://github.com/Metalfacex/bookspace/issues/18)-ში.

## ერთი უტესტო ადგილი: `IdentityPasswordHasher` (0%)

**ფაილი:** `src/BookSpace.Infrastructure/Services/IdentityPasswordHasher.cs`

**რას აკეთებს:** პაროლს ჰეშავს (ASP.NET Identity, PBKDF2) და შესვლისას ამოწმებს. unit-ტესტები ამას ფეიკ-ჰეშერით (`FakePasswordHasher`) ცვლიან, integration-ტესტები — `PlainTextHasher`-ით, ამიტომ რეალური ჰეშერი არცერთ ტესტში არ ეშვება.

**რატომ არის სარისკო:**

- ეს უსაფრთხოების კოდია: თუ `Verify` შეცდომით `true`-ს დააბრუნებს ნებისმიერ პაროლზე, ნებისმიერ ანგარიშში შესვლა გახდება შესაძლებელი — და ყველა არსებული ტესტი მაინც მწვანე დარჩება.
- `Verify` იჭერს `FormatException`-ს (დაზიანებული ჰეში ბაზაში) და აბრუნებს `false`-ს; ეს ქცევა დოკუმენტირებულია მხოლოდ კოდში.
- `PasswordVerificationResult.SuccessRehashNeeded` (ძველი ფორმატის ჰეში) ახლა `true`-დ ითვლება, მაგრამ ჰეში არ ახლდება.

**როგორ დაიფარება (შემდეგი ნაბიჯი):** unit-ტესტები `IdentityPasswordHasher`-ზე —

1. `Hash("Password1")` → შედეგი არ უდრის ღია პაროლს და ორი ჰეში ერთნაირი პაროლისთვისაც განსხვავდება (salt).
2. `Verify(hash, "Password1")` → `true`; `Verify(hash, "Password2")` → `false`.
3. `Verify("not-a-hash", "x")` და `Verify("", "x")` → `false` (exception-ის გარეშე).

მეორე პრიორიტეტი — `RoomsController`/`BookingsController`-ის HTTP-დონის ტესტები `WebApplicationFactory`-ით (ავტორიზაცია: სტუდენტი `/Rooms/Create`-ზე უნდა მიიღოს 403).
