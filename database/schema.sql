-- BookSpace — MS SQL Server სქემა
-- აპლიკაცია სქემას ავტომატურად ქმნის (EF Core EnsureCreated, იხ. DbInitializer.cs).
-- ეს სკრიპტი დოკუმენტაციისთვის / ხელით შესაქმნელად.

CREATE TABLE dbo.Users (
    Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Email         NVARCHAR(254)     NOT NULL,
    PasswordHash  NVARCHAR(500)     NOT NULL,
    Role          INT               NOT NULL,          -- 0 = Student, 1 = Admin, 2 = Manager
    CreatedAt     DATETIME2         NOT NULL
);
CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);

CREATE TABLE dbo.Rooms (
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rooms PRIMARY KEY,
    Name      NVARCHAR(100)     NOT NULL,
    Location  NVARCHAR(200)     NOT NULL,
    Capacity  INT               NOT NULL,
    IsActive  BIT               NOT NULL,
    CONSTRAINT CK_Rooms_Capacity CHECK ([Capacity] >= 1 AND [Capacity] <= 100)
);

CREATE TABLE dbo.Bookings (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
    RoomId          INT       NOT NULL CONSTRAINT FK_Bookings_Rooms_RoomId REFERENCES dbo.Rooms (Id),
    UserId          INT       NOT NULL CONSTRAINT FK_Bookings_Users_UserId REFERENCES dbo.Users (Id),
    [Date]          DATE      NOT NULL,
    StartHour       INT       NOT NULL,                -- 14 = 14:00–15:00
    Status          INT       NOT NULL,                -- 0 = Active, 1 = Cancelled
    CreatedAt       DATETIME2 NOT NULL,
    ReminderSentAt  DATETIME2 NULL,
    CONSTRAINT CK_Bookings_StartHour CHECK ([StartHour] >= 0 AND [StartHour] <= 23)
);

-- გადაფარვის დაცვა: ერთ ოთახში ერთ სლოტზე მხოლოდ ერთი აქტიური ჯავშანი
-- (ერთდროული მოთხოვნების დროსაც — მეორე INSERT 2601 შეცდომით ვარდება).
CREATE UNIQUE INDEX UX_Bookings_ActiveSlot
    ON dbo.Bookings (RoomId, [Date], StartHour)
    WHERE [Status] = 0;

CREATE INDEX IX_Bookings_UserId_Date ON dbo.Bookings (UserId, [Date]);
