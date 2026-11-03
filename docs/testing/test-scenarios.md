# ტესტ-სცენარები (პრაქტიკული 3, პუნქტი 1)

> სცენარი: **D. პრაქტიკული 2-ის საკუთარი პროექტის მოდული** — BookSpace-ის „დაჯავშნის“ მოდული.
> ყველა სცენარი ავტომატიზებულია; სვეტი „ტესტი“ მიუთითებს შესაბამის მეთოდზე.

კონფიგურაცია, რომელზეც ზღვრებია გათვლილი (`appsettings.json → BookSpace:Booking`):
სამუშაო საათები **08:00–22:00** (ბოლო სლოტი 21:00), დღეში მაქსიმუმ **3** ჯავშანი, მაქსიმუმ **14** დღით წინ, ტევადობა **1–100**, პაროლი **8–100** სიმბოლო.
ტესტებში „ახლა“ = **2026-11-10, 10:30**.

| # | სცენარი | შემავალი მონაცემები | მოსალოდნელი შედეგი | ზღვარი? | ტესტი |
|---|---|---|---|---|---|
| TS-01 | თავისუფალი სლოტის დაჯავშნა | ხვალ, 10:00, თავისუფალი | ჯავშანი ინახება, სტატუსი `Active` | — | `BookingServiceTests.Book_FreeSlot_SavesBooking` |
| TS-02 | სამუშაო საათების ზღვრები | 07, 08, 21, 22 | 07 ✗, **08 ✓**, **21 ✓**, 22 ✗ | ✅ | `Book_OpeningHoursBoundaries` |
| TS-03 | დღევანდელი გასული სლოტი | დღეს 10:00 (ახლა 10:30) / 11:00 | 10:00 ✗ (წარსული), 11:00 ✓ | ✅ | `Book_Today_PastSlotIsRejected` |
| TS-04 | რამდენი დღით წინ | დღეს + 14 / დღეს + 15 | **14 ✓**, 15 ✗ | ✅ | `Book_MaxDaysAheadBoundary` |
| TS-05 | დღიური ლიმიტი | უკვე აქვს 2 / 3 / 4 ჯავშანი იმ დღეს | 2 → შეიძლება, **3 → აღარ**, 4 → აღარ | ✅ | `DailyLimitPolicyTests.CanBook_AtBoundaryOfLimit_ReturnsExpected`, `Book_FourthBookingSameDay_ExceedsDailyLimit` |
| TS-06 | გაუქმებული ჯავშანი ლიმიტში არ ითვლება | 2 აქტიური + 2 გაუქმებული | შეიძლება | ✅ | `CanBook_CancelledBookingsAreNotCounted` |
| TS-07 | დაკავებული სლოტი | სხვა სტუდენტს უკვე აქვს 10:00 | `409 Conflict`, ახალი ჩანაწერი არ იქმნება | — | `Book_SlotAlreadyTaken_ReturnsConflict` |
| TS-08 | ერთდროული მოთხოვნა ერთ სლოტზე | 2 პარალელური მოთხოვნა, 15:00 | მაქსიმუმ **1** ჯავშანი, მეორე → `Conflict` | ✅ (race) | `BookingPersistenceTests.ConcurrentRequests_ForSameSlot_CreateAtMostOneBooking` (SQL Server) |
| TS-09 | გაუქმება ათავისუფლებს სლოტს | გააუქმე 10:00, სხვა ჯავშნის 10:00 | მეორე ჯავშანი წარმატებულია | — | `Cancel_OwnFutureBooking_FreesTheSlot`, `CancelledSlot_CanBeBookedAgain` |
| TS-10 | სხვისი ჯავშნის გაუქმება | სტუდენტი A აუქმებს B-ს ჯავშანს | `403 Forbidden`, სტატუსი უცვლელი | — | `Cancel_SomeoneElsesBooking_IsForbidden` |
| TS-11 | უკვე დაწყებული ჯავშნის გაუქმება | დღეს 10:00 (ახლა 10:30) | უარყოფილია | ✅ | `Cancel_AlreadyStartedBooking_IsRejected` |
| TS-12 | ოთახის ტევადობა | 0 / 1 / 100 / 101 | 0 ✗, **1 ✓**, **100 ✓**, 101 ✗ | ✅ | `RoomValidatorTests.Validate_CapacityBoundaries` |
| TS-13 | პაროლის სიგრძე | 7 / 8 / 100 / 101 სიმბოლო | 7 ✗, **8 ✓**, **100 ✓**, 101 ✗ | ✅ | `PasswordPolicyTests` |
| TS-14 | უნივერსიტეტის ელფოსტა | `a@uni.edu.ge`, `a@edu.ge`, `a@gmail.com`, `a@fakeedu.ge` | ✓, ✓, ✗, ✗ | ✅ | `EmailValidatorTests` |
| TS-15 | დუბლირებული ელფოსტა | იგივე ელფოსტა სხვა რეგისტრით (`ANA@…`) | `409 Conflict` (ნორმალიზაციის შემდეგ) | — | `Register_EmailAlreadyUsed_ReturnsConflict`, `UniqueEmailIndex_BlocksDuplicateEvenWithoutServiceCheck` |
| TS-16 | შეხსენება ზუსტად 1 საათით ადრე | ახლა 13:00, ჯავშანი 14:00 / 15:00 | 14:00 → 1 წერილი, 15:00 → ჯერ არა | ✅ | `ReminderServiceTests` |
| TS-17 | სტატისტიკის პერიოდი | 366 / 367 დღე, from > to | 366 ✓, 367 ✗, from > to ✗ | ✅ | `StatisticsServiceTests.GetRoomUsage_PeriodLengthBoundary` |

**სულ: 17 სცენარი, მათგან 12 — საზღვრის შემთხვევა.**
