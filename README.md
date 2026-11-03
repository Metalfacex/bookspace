# BookSpace

[![CI](https://github.com/Metalfacex/bookspace/actions/workflows/ci.yml/badge.svg)](https://github.com/Metalfacex/bookspace/actions/workflows/ci.yml)

სასწავლო ოთახების დაჯავშნის ვებპლატფორმა უნივერსიტეტისთვის — **ASP.NET Core MVC 8 + MS SQL Server**.

| | |
|---|---|
| 📋 Product Backlog (GitHub Projects) | [github.com/users/Metalfacex/projects/5](https://github.com/users/Metalfacex/projects/5) |
| 🏛️ არქიტექტურა (პრაქტიკული 2) | [docs/architecture.md](docs/architecture.md) |
| 🧪 ტესტ-სცენარები (პრაქტიკული 3) | [docs/testing/test-scenarios.md](docs/testing/test-scenarios.md) |
| 🔁 TDD ციკლი | [docs/testing/tdd-cycle.md](docs/testing/tdd-cycle.md) |
| 📊 Coverage ანგარიში | [docs/testing/coverage.md](docs/testing/coverage.md) |
| 👀 Code Review | [docs/testing/code-review.md](docs/testing/code-review.md) |

## გაშვება

**საჭიროა:** .NET 8 SDK და MS SQL Server (Visual Studio-ს LocalDB, ან Docker).

```bash
# ვარიანტი 1 — LocalDB (Windows / Visual Studio): appsettings.Development.json უკვე მზადაა
dotnet run --project src/BookSpace.Web

# ვარიანტი 2 — SQL Server Docker-ში
docker compose up -d
dotnet run --project src/BookSpace.Web --ConnectionStrings:DefaultConnection="Server=localhost,1433;Database=BookSpace;User Id=sa;Password=BookSpace_Dev_2026!;TrustServerCertificate=True"
```

პირველი გაშვებისას ბაზა და საწყისი ოთახები ავტომატურად იქმნება. Development რეჟიმში იქმნება სატესტო ანგარიშები:

| როლი | ელფოსტა | პაროლი |
|---|---|---|
| ადმინისტრატორი | `admin@bookspace.edu.ge` | `Admin12345` |
| მენეჯერი | `manager@bookspace.edu.ge` | `Manager12345` |

სტუდენტი თავად რეგისტრირდება `@...edu.ge` ელფოსტით.

## ტესტები

```bash
dotnet test                                   # unit + integration (ლოკალურად SQLite-ზე)
dotnet test --collect:"XPlat Code Coverage"   # coverage-ით
```

Integration-ტესტები MS SQL Server-ზე რომ გაეშვას, მიუთითეთ გარემოს ცვლადი:
`BOOKSPACE_TEST_SQLSERVER="Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=True"`.
GitHub Actions-ში (`.github/workflows/ci.yml`) ეს ავტომატურად ხდება SQL Server 2022 კონტეინერით.

## პროექტის სტრუქტურა

```
src/
  BookSpace.Core/            დომენი: ენტიტები, წესები, სერვისები, ინტერფეისები (ფრეიმვორკის გარეშე)
  BookSpace.Infrastructure/  EF Core + SQL Server, რეპოზიტორიები, ჰეშერი, ელფოსტა
  BookSpace.Web/             ASP.NET Core MVC: კონტროლერები, Razor Views, REST API
tests/
  BookSpace.UnitTests/         xUnit, AAA, ფეიკ-რეპოზიტორიები
  BookSpace.IntegrationTests/  xUnit + რეალური ბაზა (SQL Server / SQLite)
database/schema.sql          T-SQL სქემა
docs/                        არქიტექტურა და ტესტირების დოკუმენტაცია
```

## რეალიზებული Story-ები

| Story | სად |
|---|---|
| US-01 რეგისტრაცია, US-02 შესვლა | `AuthService`, `AccountController` |
| US-03 ოთახების სია, US-08 დამატება, US-09 რედაქტირება | `RoomService`, `RoomsController` |
| US-04 თავისუფალი დროები | `AvailabilityService`, `Rooms/Details` |
| US-05 დაჯავშნა, US-06 ჩემი ჯავშნები, US-07 გაუქმება | `BookingService`, `BookingsController`, `/api/bookings` |
| US-10 შეხსენება ელფოსტით | `ReminderService` + `ReminderBackgroundService` (დეველოპმენტში წერილი ლოგში იწერება) |
| US-11 სტატისტიკა | `StatisticsService`, `StatisticsController` |
| US-12 განმეორებადი ჯავშნები | Won't have (this time) — არ არის რეალიზებული |

---

# Product Backlog (პრაქტიკული 1)

## პროექტის აღწერა
BookSpace არის ვებპლატფორმა, რომელიც სტუდენტებს საშუალებას აძლევს უნივერსიტეტში თავისუფალი სასწავლო ოთახები მოიძიონ და წინასწარ დაჯავშნონ. სისტემა აჩვენებს ოთახების ხელმისაწვდომობას კონკრეტულ თარიღსა და დროს და ხელს უშლის ჯავშნების გადაფარვას. ადმინისტრატორი მართავს ოთახებსა და ჯავშნებს, ხოლო უნივერსიტეტის მენეჯერი აკვირდება სივრცეების გამოყენებას. პროექტის მიზანია სასწავლო სივრცეების ორგანიზებულად და ეფექტიანად განაწილება.

## Stakeholders
| როლი | ინტერესი |
|---|---|
| სტუდენტი | თავისუფალი ოთახის მოძიება და სწრაფი დაჯავშნა |
| ადმინისტრატორი | ოთახების მონაცემებისა და ჯავშნების მართვა |
| უნივერსიტეტის მენეჯერი | სივრცეების დატვირთვის შეფასება და მოთხოვნების დამტკიცება |
| უნივერსიტეტი (დამკვეთი/გადამხდელი) | პროექტის დაფინანსება და სასწავლო სივრცეების ეფექტიანი გამოყენება |

**ვეტოს უფლება:** უნივერსიტეტის უფლებამოსილ მენეჯერს აქვს მოთხოვნების და საბოლოო რელიზის დამტკიცების ან უარყოფის უფლება.

## არაფუნქციური მოთხოვნები
1. ოთახების სია 50 ერთდროული მომხმარებლის პირობებში მოთხოვნების მინიმუმ 95%-ში ჩაიტვირთოს მაქსიმუმ 2 წამში (ტესტური მონაცემები: 200 ოთახი).
2. სისტემა გაუმკლავდეს 100 ერთდროულ აქტიურ მომხმარებელს, შეცდომების წილი არ აღემატებოდეს 1%-ს 10-წუთიანი დატვირთვის ტესტის დროს.
3. ჯავშნის შექმნის მოთხოვნების 95% დასრულდეს მაქსიმუმ 3 წამში 50 ერთდროული მომხმარებლის პირობებში.

## Definition of Done
- ფუნქცია მუშაობს User Story-ის აღწერის შესაბამისად და Given/When/Then კრიტერიუმები შესრულებულია.
- კოდი ატვირთულია რეპოზიტორიაში, Pull Request გადახედილია და გაერთიანებულია მთავარ ბრენჩში.
- შესაბამისი ავტომატური ტესტები წარმატებით სრულდება, მათ შორის შეცდომის სცენარები.
- არ არის ღია კრიტიკული ან მაღალი სიმძიმის ხარვეზი ამ Story-ში.
- საჭიროების შემთხვევაში განახლებულია README ან ტექნიკური დოკუმენტაცია.
- Story-ს სტატუსი Done ენიჭება მხოლოდ ზემოთ ჩამოთვლილის შემოწმების შემდეგ.

## MoSCoW და MVP
**Must have (MVP):** US-01–US-08 = **30 SP**.
**Should have:** US-09–US-10 = 8 SP.
**Could have:** US-11 = 8 SP.
**Won't have (this time):** US-12 = 13 SP.
**მთლიანი შეფასება:** 59 SP; მიმდინარე რელიზის კანდიდატები (Won't-ის გარეშე): 46 SP.

MVP-ის დასრულებისას სტუდენტს შეუძლია რეგისტრაცია, შესვლა, ოთახების და ხელმისაწვდომი დროების ნახვა, ოთახის დაჯავშნა, საკუთარი ჯავშნების ნახვა და გაუქმება; ადმინისტრატორს შეუძლია ოთახის დამატება. ჯავშნის გადაფარვა უნდა იბლოკებოდეს სერვერზე, მათ შორის ერთდროული მოთხოვნების დროს.

## შეფასების წესი
Story Points გამოყენებულია Fibonacci-ის სკალით (1, 2, 3, 5, 8, 13). შეფასებები არის ინდივიდუალური საწყისი ვარაუდები და არა რეალური გუნდური Planning Poker-ის შედეგები. US-12 (13 SP) გამოტოვებულია ამ რელიზიდან; მისი შემდგომი განხორციელებისას უნდა დაიშალოს უფრო პატარა Story-ებად.

## GitHub Projects
- დაფა: [github.com/users/Metalfacex/projects/5](https://github.com/users/Metalfacex/projects/5)
- სვეტები: `Backlog`, `Todo`, `In Progress`, `In Review`, `Done`.
- ველები: `Priority` (Must have / Should have / Could have / Won't have) და `Story Points` (Number).
- 12 Issue (US-01…US-12) შექმნილია `.github/workflows/import-bookspace-backlog.yml`-ით; თითოეულს აქვს User Story, Given/When/Then კრიტერიუმები, MoSCoW და Story Points (ასევე label-ებად: `MoSCoW: …`, `SP: …`, `MVP`).

## Backlog
| ID | სათაური | MoSCoW | SP |
|---|---|---|---:|
| US-01 | სტუდენტის რეგისტრაცია | Must have | 3 |
| US-02 | სისტემაში შესვლა | Must have | 3 |
| US-03 | ოთახების სიის ნახვა | Must have | 3 |
| US-04 | თავისუფალი დროების ნახვა | Must have | 5 |
| US-05 | ოთახის დაჯავშნა | Must have | 5 |
| US-06 | ჩემი ჯავშნების ნახვა | Must have | 3 |
| US-07 | ჯავშნის გაუქმება | Must have | 3 |
| US-08 | ოთახის დამატება | Must have | 5 |
| US-09 | ოთახის მონაცემების რედაქტირება | Should have | 3 |
| US-10 | ჯავშნის შეხსენება ელფოსტით | Should have | 5 |
| US-11 | ოთახების გამოყენების სტატისტიკა | Could have | 8 |
| US-12 | განმეორებადი ჯავშნები | Won’t have (this time) | 13 |
