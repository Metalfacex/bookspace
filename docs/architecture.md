# BookSpace — არქიტექტურა (პრაქტიკული 2)

> სცენარი: **D. საკუთარი იდეა** — სასწავლო ოთახების დაჯავშნის ვებპლატფორმა უნივერსიტეტისთვის.

## 1. არქიტექტურული სტილი: მოდულური მონოლითი (Modular Monolith) Clean Architecture-ის ფენებით

სისტემა არის **ერთი deploy-ის ASP.NET Core MVC აპლიკაცია** (მონოლითი), რომლის კოდი მკაცრად გამოყოფილ მოდულებად — სამ პროექტად — არის დაყოფილი. ეს ზუსტად ის გზაა, რასაც ლექცია 4 პატარა გუნდს ურჩევს („მოდულური მონოლითი Clean Architecture-ის ფენებით“): ერთი deploy, მაგრამ მოდული საჭიროებისას მარტივად გადადის ცალკე სერვისზე.

| ფენა | პროექტი | პასუხისმგებლობა |
|---|---|---|
| Presentation | `BookSpace.Web` | MVC კონტროლერები, Razor Views, REST API, ავტორიზაცია (cookie) |
| Domain / Application | `BookSpace.Core` | ენტიტები, ბიზნეს-წესები, სერვისები, ინტერფეისები. **არცერთ ფრეიმვორკზე არ არის დამოკიდებული** |
| Infrastructure | `BookSpace.Infrastructure` | EF Core + MS SQL Server, რეპოზიტორიები, ჰეშერი, ელფოსტა, საათი |

დამოკიდებულების მიმართულება: `Web → Core ← Infrastructure` (Dependency Inversion — Core განსაზღვრავს ინტერფეისებს, Infrastructure ახორციელებს).

Clean Architecture-ის ფენებთან შესაბამისობა (**Dependency Rule** — დამოკიდებულება მხოლოდ შიგნით):

| Clean Architecture ფენა | BookSpace |
|---|---|
| Entities | `Core/Domain` (`Room`, `Booking`, `User`) + `Core/Rules` (`DailyLimitPolicy`, `BookingRules`) |
| Use Cases | `Core/Services` (`BookingService`, `AvailabilityService`, …) |
| Interface Adapters | `Web/Controllers` (MVC + API), `Infrastructure/Repositories` |
| Frameworks & Drivers | ASP.NET Core, EF Core, MS SQL Server |

`BookSpace.Core`-ს არცერთი NuGet პაკეტი და ფრეიმვორკი არ სჭირდება — ამიტომ ბიზნეს-წესები unit-ტესტებით ბაზის გარეშე მოწმდება, და SQL Server-ის სხვა ბაზით ჩანაცვლება Core-ს არ ეხება.

**არგუმენტები:**

1. **მასშტაბი და გუნდი.** 1 დეველოპერი, 1 სემესტრი, ~200 ოთახი და 100 ერთდროული მომხმარებელი (NFR-2). Microservices-ის ოპერაციული ხარჯი (ცალკე deploy, ქსელური შეცდომები, განაწილებული ტრანზაქციები) აქ არაფერს გვაძლევს. ერთი deploy ერთეული მარტივია IIS-ზე ან Azure App Service-ზე.
2. **ტესტირებადობა.** ბიზნეს-წესები (გადაფარვა, დღიური ლიმიტი, სამუშაო საათები) `Core`-შია და არ საჭიროებს ბაზას ან HTTP-ს — unit-ტესტები მილიწამებში ეშვება ფეიკ-რეპოზიტორიებით. Integration-ტესტები ცალკე ამოწმებს Infrastructure-ს რეალურ SQL Server-ზე.
3. **ევოლუციის გზა.** თუ მომავალში საჭირო გახდა (მაგ. მობილური აპი ან შეხსენებების ცალკე სერვისი), `Core` უცვლელად გადავა; REST API უკვე არსებობს, ხოლო `ReminderService` შეიძლება ცალკე Worker-ად გამოვიდეს.

## 2. კომპონენტების დიაგრამა

```mermaid
flowchart LR
    subgraph Client["კლიენტი"]
        B["ბრაუზერი<br/>Razor + Bootstrap"]
        M["მომავალი მობილური / SPA"]
    end

    subgraph Web["BookSpace.Web (ASP.NET Core MVC)"]
        MVC["MVC Controllers<br/>Account · Rooms · Bookings · Statistics"]
        API["REST API v1<br/>/api/v1/rooms · /api/v1/bookings<br/>Swagger · Rate limiting"]
        AUTH["Cookie Authentication<br/>+ Role Authorization"]
        BG["ReminderBackgroundService<br/>(ყოველ 5 წთ)"]
    end

    subgraph Core["BookSpace.Core (დომენი)"]
        SVC["Services<br/>Auth · Room · Availability · Booking · Reminder · Statistics"]
        RULES["Rules & Validation<br/>BookingRules · DailyLimitPolicy · Validators"]
        IF["Interfaces<br/>IRoomRepository · IBookingRepository · IUserRepository<br/>IClock · IPasswordHasher · IEmailSender"]
    end

    subgraph Infra["BookSpace.Infrastructure"]
        EF["BookSpaceDbContext<br/>(EF Core 8)"]
        REPO["Repositories"]
        ADP["IdentityPasswordHasher · SystemClock · LoggingEmailSender"]
    end

    DB[("MS SQL Server<br/>Users · Rooms · Bookings")]
    SMTP["SMTP / ელფოსტის სერვისი"]

    B --> MVC
    M --> API
    MVC --> AUTH
    API --> AUTH
    MVC --> SVC
    API --> SVC
    BG --> SVC
    SVC --> RULES
    SVC --> IF
    REPO -. implements .-> IF
    ADP -. implements .-> IF
    REPO --> EF --> DB
    ADP --> SMTP
```

## 3. Design Patterns

### Backend

| Pattern | სად | რატომ |
|---|---|---|
| **Repository** | `IBookingRepository` → `BookingRepository` | სერვისები არ იცნობს EF Core-ს; unit-ტესტებში იცვლება `InMemoryBookingRepository`-ით |
| **Dependency Injection / Inversion of Control** | `DependencyInjection.AddBookSpaceInfrastructure()` | ყველა დამოკიდებულება კონსტრუქტორით შემოდის — ადვილი ჩანაცვლება და ტესტირება |
| **Service Layer** | `BookingService`, `RoomService`… | ბიზნეს-ლოგიკა ერთ ადგილზეა; MVC და REST API ერთსა და იმავე სერვისს იყენებს |
| **Strategy** | `IClock`, `IEmailSender` | დრო და ელფოსტა ცვალებადი ქცევაა: პროდაქშენში `SystemClock`/SMTP, ტესტებში `FakeClock`/`FakeEmailSender` |
| **Adapter** | `IdentityPasswordHasher` | ASP.NET Identity-ის `PasswordHasher<T>`-ს ვახვევთ საკუთარ `IPasswordHasher` ინტერფეისში |
| **Result Object** | `Result<T>` + `ErrorCode` | ბიზნეს-შეცდომები exception-ების გარეშე; კონტროლერი `ErrorCode`-ს HTTP სტატუსზე ასახავს (400/403/404/409/422) |

### Frontend (Razor MVC)

| Pattern | სად | რატომ |
|---|---|---|
| **MVC** | Controllers + Views | View მხოლოდ აჩვენებს, Controller მართავს ნაკადს |
| **ViewModel** | `RoomDetailsViewModel`, `RegisterViewModel` | View იღებს ზუსტად იმ მონაცემებს, რაც სჭირდება; ვალიდაცია DataAnnotations-ით |
| **Composite / Partial View** | `_Layout`, `_Alerts`, `_RoomForm` | ერთი ფორმა გამოიყენება Create-შიც და Edit-შიც |
| **Post/Redirect/Get** | `BookingsController.Create` → `Rooms/Details` | გვერდის განახლება ჯავშანს ხელმეორედ არ აგზავნის |

## 4. REST API (6 endpoint, ვერსია v1)

**რატომ REST და არა GraphQL.** ლექციის წესით: მარტივი CRUD და საჯარო API → REST; ბევრი სხვადასხვა ეკრანი დაკავშირებული მონაცემით → GraphQL. BookSpace-ის მონაცემები მარტივია (ოთახი, სლოტი, ჯავშანი), ეკრანები ცოტაა და თითოეულს ერთი-ორი რესურსი სჭირდება, ამიტომ over-/under-fetching პრობლემა არ გვაქვს. REST-ს ემატება უფასო HTTP ქეშირება და მარტივი სწავლა; GraphQL-ის სქემა და ერთი endpoint აქ მხოლოდ სირთულეს დაამატებდა.

**REST best practices, რომლებიც დაცულია:**

- **ვერსიონირება** — ყველა მისამართი `/api/v1/...`; ახალი, შეუთავსებელი ვერსია იქნება `/api/v2`, ძველი კლიენტები არ გატყდება.
- **არსებითი სახელი მრავლობითში**, ზმნის გარეშე (`/rooms`, `/bookings`); მოქმედებას HTTP მეთოდი განსაზღვრავს.
- **იერარქია კავშირისთვის** — `/rooms/{id}/availability`.
- **სწორი სტატუს-კოდები** (`201` + `Location`, `204`, `400`, `401`, `403`, `404`, `409`, `422`, `429`) და **ერთგვაროვანი შეცდომის ფორმატი** `{ "error", "message" }`.
- **დოკუმენტაცია OpenAPI (Swagger)** სტანდარტით — გაშვებულ აპლიკაციაში `/swagger` (ინტერაქტიული) და `/swagger/v1/swagger.json`.
- **Rate limiting** — ერთ მომხმარებელს (ან IP-ს) წუთში 60 მოთხოვნა (`BookSpace:RateLimit`), შემდეგ `429 Too Many Requests`.
- **უსაფრთხოება** — HTTPS redirect + HSTS, ავთენტიფიკაცია cookie-ით (HttpOnly, SameSite=Strict), არაავტორიზებულზე `401` (არა redirect).

| # | მეთოდი და მისამართი | აღწერა | პასუხები | Story |
|---|---|---|---|---|
| 1 | `GET /api/v1/rooms` | აქტიური ოთახების სია | `200` | US-03 |
| 2 | `GET /api/v1/rooms/{id}/availability?date=2026-11-11` | დღის სლოტები (`Free`/`Booked`/`Past`) | `200`, `404` | US-04 |
| 3 | `POST /api/v1/bookings` | ჯავშნის შექმნა | `201` + `Location`, `400`, `401`, `404`, `409` (დაკავებულია), `422` (დღიური ლიმიტი) | US-05 |
| 4 | `GET /api/v1/bookings/{id}` | ჩემი ერთი ჯავშანი | `200`, `401`, `404` | US-06 |
| 5 | `GET /api/v1/bookings/me` | ჩემი მომავალი ჯავშნები და ისტორია | `200`, `401` | US-06 |
| 6 | `DELETE /api/v1/bookings/{id}` | ჯავშნის გაუქმება | `204`, `401`, `403` (სხვისია), `404`, `409` | US-07 |

ყველა endpoint-ზე შეიძლება `429` (rate limit).

მაგალითი:

```http
POST /api/v1/bookings
Content-Type: application/json

{ "roomId": 1, "date": "2026-11-11", "startHour": 14 }
```

```json
HTTP/1.1 201 Created
Location: /api/v1/bookings/17

{ "id": 17, "roomId": 1, "roomName": "ოთახი 101", "date": "2026-11-11", "startHour": 14, "status": "Active" }
```

შეცდომის ფორმატი: `{ "error": "Conflict", "message": "ეს დრო უკვე დაკავებულია." }`

## 5. მონაცემთა ბაზა: SQL (MS SQL Server)

**რატომ SQL და არა NoSQL:**

- მონაცემები მკაცრად რელაციურია (User 1—N Booking N—1 Room) და საჭიროა foreign key-ები.
- მთავარი ბიზნეს-წესი — **ერთ სლოტზე ერთი ჯავშანი** — ბაზის დონეზე მოგვარებულია **filtered unique index**-ით. NoSQL-ში იგივე გარანტია ბევრად რთული იქნებოდა.
- სტატისტიკა (US-11) არის `GROUP BY` აგრეგაცია, რაც SQL-ის ბუნებრივი ძალაა.
- .NET + EF Core + SQL Server უნივერსიტეტის ინფრასტრუქტურაში (Windows / Azure) სტანდარტული წყვილია.

```mermaid
erDiagram
    USERS ||--o{ BOOKINGS : "აკეთებს"
    ROOMS ||--o{ BOOKINGS : "ჯავშნის"

    USERS {
        int Id PK
        nvarchar Email UK "254, unique"
        nvarchar PasswordHash "PBKDF2"
        int Role "0 Student, 1 Admin, 2 Manager"
        datetime2 CreatedAt
    }
    ROOMS {
        int Id PK
        nvarchar Name "100"
        nvarchar Location "200"
        int Capacity "CHECK 1..100"
        bit IsActive
    }
    BOOKINGS {
        int Id PK
        int RoomId FK
        int UserId FK
        date Date
        int StartHour "8..21"
        int Status "0 Active, 1 Cancelled"
        datetime2 CreatedAt
        datetime2 ReminderSentAt "nullable"
    }
```

ინდექსები:

- `UX_Bookings_ActiveSlot` — `UNIQUE (RoomId, Date, StartHour) WHERE Status = 0` — გადაფარვის დაცვა, მათ შორის ერთდროული მოთხოვნებისას. გაუქმებული ჯავშანი სლოტს ათავისუფლებს.
- `IX_Users_Email` — unique, ერთ ელფოსტაზე ერთი ანგარიში.
- `IX_Bookings_UserId_Date` — "ჩემი ჯავშნები" და დღიური ლიმიტის შემოწმება.

სრული T-SQL: [`database/schema.sql`](../database/schema.sql).

## 6. ტექნოლოგიური სტეკი

| ფენა | ტექნოლოგია |
|---|---|
| ენა / runtime | C# 12, .NET 8 (LTS) |
| ვები | ASP.NET Core MVC 8, Razor Views, საკუთარი CSS (Noto Serif/Sans Georgian) |
| API | ASP.NET Core Web API კონტროლერები (JSON), `/api/v1`, OpenAPI/Swagger (Swashbuckle), ჩაშენებული Rate Limiter |
| ავტორიზაცია | Cookie Authentication, როლები Student / Admin / Manager, PBKDF2 პაროლის ჰეში |
| ბაზა | MS SQL Server 2022 (დეველოპმენტში LocalDB ან Docker), EF Core 8 |
| ფონური ამოცანები | `BackgroundService` + `PeriodicTimer` |
| ტესტები | xUnit (unit, integration), `WebApplicationFactory` (API), Playwright (E2E), k6 (დატვირთვა), coverlet + ReportGenerator |
| CI | GitHub Actions: lint (`dotnet format`), ტესტები SQL Server კონტეინერზე, E2E + დატვირთვის ტესტი გაშვებულ აპლიკაციაზე |
| ამოცანების მართვა | GitHub Issues + GitHub Projects |

## 7. რისკები და შემცირება

| # | რისკი | გავლენა | შემცირება |
|---|---|---|---|
| 1 | **ერთდროული დაჯავშნა (race condition)** — ორი სტუდენტი ერთსა და იმავე წამს აჭერს „დაჯავშნას“ | ორმაგი ჯავშანი, კონფლიქტი ოთახში | სერვისის შემოწმების გარდა, filtered unique index ბაზაში; `TryAddAsync` იჭერს 2601/2627 შეცდომას და აბრუნებს `409 Conflict`. დაფარულია integration-ტესტით `ConcurrentRequests_ForSameSlot_CreateAtMostOneBooking` |
| 2 | **დროის სარტყელი** — სერვერი UTC-ში, უნივერსიტეტი UTC+4 | „წარსული“ სლოტის დაჯავშნა ან სწორი სლოტის დაბლოკვა | `IClock` აბრუნებს დროს `Asia/Tbilisi` სარტყელში (`BookSpace:TimeZone`); ბიზნეს-ლოგიკა არასდროს იძახებს `DateTime.Now`-ს პირდაპირ |
| 3 | **სქემის ცვლილება** — `EnsureCreated` არ აკეთებს მიგრაციას | ახალი სვეტი არსებულ ბაზაში არ დაემატება | MVP-ში მისაღებია; პროდაქშენამდე გადასვლა EF Core Migrations-ზე (`dotnet ef migrations add Initial`) |
| 4 | **ელფოსტის მიწოდება (US-10)** — SMTP მიუწვდომელია | შეხსენება არ მივა | `ReminderSentAt` იწერება მხოლოდ წარმატებული გაგზავნის შემდეგ, ასე რომ შემდეგ ციკლში თავიდან სცდის; შეცდომა ილოგება და სერვისი არ ჩერდება |
| 5 | **API-ს ბოროტად გამოყენება** — ბოტი ან შეცდომით დაწერილი კლიენტი ასობით მოთხოვნას აგზავნის | სერვერის გადატვირთვა, სხვა სტუდენტებისთვის შენელება | Rate limiting (60 მოთხოვნა/წთ მომხმარებელზე, შემდეგ `429`); დაფარულია API-ტესტით `FourthRequestInWindow_Returns429` |
