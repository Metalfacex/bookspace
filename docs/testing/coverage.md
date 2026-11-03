# Coverage ანგარიში (პრაქტიკული 3, პუნქტი 6)

ანგარიში გენერირდება ავტომატურად ყოველ push-ზე GitHub Actions-ში (`.github/workflows/ci.yml`):
coverlet (`--collect:"XPlat Code Coverage"`) → ReportGenerator. სრული HTML ანგარიში — CI run-ის **Artifacts → coverage-report**.

ლოკალურად:

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html
```

## შედეგი (PR #19, 2026-11-03)

ტესტები: **85 unit + 14 integration + 19 API (HTTP) = 118, ყველა გადის** (integration და API — MS SQL Server 2022 კონტეინერზე). ამას ემატება 3 E2E სცენარი Playwright-ით და დატვირთვის ტესტი k6-ით (იხ. ქვემოთ) — ისინი გაშვებულ აპლიკაციაზე ეშვება და coverage-ში არ ითვლება.

| Assembly | Line coverage | შენიშვნა |
|---|---:|---|
| **BookSpace.Core** (ბიზნეს-ლოგიკა) | **97.3%** | unit-ტესტები |
| **BookSpace.Infrastructure** | **86.9%** (იყო 48.9%) | integration + API ტესტები |
| **BookSpace.Web** | **41.6%** (პირველად იზომება) | API ტესტებმა დაამატა; Razor გვერდების კოდიც ითვლება |
| **სულ** | **65%** (713 / 1096 ხაზი) | ჯამი დაეცა, რადგან ახლა Web-იც იზომება — ეს უფრო პატიოსანი რიცხვია |

| კლასი | Line coverage |
|---|---:|
| `Core.Services.*` (Booking, Availability, Reminder, Statistics) | 100% |
| `Web.Controllers.Api.BookingsApiController`, `RoomsApiController` | 100% |
| `Infrastructure.Data.DbInitializer`, `BookSpaceDbContext` | 100% |
| `Infrastructure.Repositories.BookingRepository` | 96.2% |
| `Infrastructure.DependencyInjection` | 90.3% |
| `Web.Controllers.AccountController` | 66.6% |
| `Infrastructure.Services.IdentityPasswordHasher` | 66.6% (იყო 0%) |
| `Web.Controllers.RoomsController` | 9.7% |
| `Web.Controllers.BookingsController`, `HomeController`, `StatisticsController` | 0% (მხოლოდ E2E ფარავს) |
| `Web.Background.ReminderBackgroundService`, `LoggingEmailSender` | **0%** |

### ისტორია

| ანგარიში | სულ | Core | Infrastructure | Web | რა შეიცვალა |
|---|---:|---:|---:|---:|---|
| CI #1 | 78.4% | 97% | 48.9% | — | unit + integration |
| PR #17 | 84.8% | 97% | 65.6% | — | `DbInitializer`-ის ტესტები (0% → 100%) |
| PR #19 | 65% | 97.3% | 86.9% | 41.6% | API ტესტები; Web პირველად იზომება |

## დატვირთვის ტესტი (k6, CI-ში)

| მოთხოვნა | ზღვარი | შედეგი |
|---|---|---|
| NFR-1: `/Rooms`, 50 ერთდროული მომხმარებელი | p95 ≤ 2000 ms | **p95 = 10.35 ms** |
| NFR-2: 100 ერთდროული მომხმარებელი (მთავარი, ფილტრი, განრიგი) | შეცდომები < 1% | **0%** (0 / 9000 მოთხოვნა) |

CI-ში ტესტი მოკლეა (30 წამი თითო სცენარზე) და მცირე მონაცემებით (4 ოთახი); NFR-ის სრული შემოწმება (200 ოთახი, 10 წუთი) — `k6 run -e DURATION=10m load/rooms-load.js` წინასწარ შევსებულ ბაზაზე.

## ერთი უტესტო ადგილი: `ReminderBackgroundService` (0%)

**ფაილი:** `src/BookSpace.Web/Background/ReminderBackgroundService.cs`

**რას აკეთებს:** ყოველ 5 წუთში ერთხელ ქმნის DI scope-ს და იძახებს `ReminderService.SendDueRemindersAsync`-ს (US-10). შეცდომას ლოგავს და აგრძელებს.

**რატომ არის სარისკო:**

- `ReminderService`-ის ლოგიკა 100%-ით დაფარულია, მაგრამ მისი **გამშვები** — არა. თუ ციკლი ჩავარდება ან გაჩერდება (მაგ. exception `OperationCanceledException`-ის გარდა, scope-ის შეცდომა), შეხსენებები უბრალოდ აღარ წავა, და ამას ვერცერთი ტესტი ვერ შეამჩნევს.
- ინტერვალი (5 წუთი) კოდშია ჩაწერილი; 60-წუთიანი წესის შემთხვევაში ყველაზე გვიანი შეხსენება 55 წუთით ადრე მოვა — ეს ქცევა არსად არის შემოწმებული.

**როგორ დაიფარება (შემდეგი ნაბიჯი):** ინტერვალი გავიტანოთ კონფიგურაციაში და დავწეროთ ტესტი, რომელიც სერვისს რამდენიმე წამიანი ინტერვალით უშვებს ფეიკ `IEmailSender`-ით: ერთი ციკლის შემდეგ ზუსტად 1 წერილი, `StopAsync`-ის შემდეგ — ახალი აღარ.

მეორე პრიორიტეტი — MVC კონტროლერები (`BookingsController`, `RoomsController`): ისინი მხოლოდ E2E ტესტებით მოწმდება, რაც coverage-ში არ ჩანს. API ტესტების მსგავსად, HTTP-დონის ტესტები გვერდებზეც დაემატება.
