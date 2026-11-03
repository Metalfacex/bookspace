using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using BookSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookSpaceDbContext _db;

    public BookingRepository(BookSpaceDbContext db)
    {
        _db = db;
    }

    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(int roomId, DateOnly date,
        CancellationToken ct = default) =>
        await _db.Bookings.AsNoTracking()
            .Where(b => b.RoomId == roomId && b.Date == date && b.Status == BookingStatus.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetForUserAsync(int userId, CancellationToken ct = default) =>
        await _db.Bookings.AsNoTracking()
            .Include(b => b.Room)
            .Where(b => b.UserId == userId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetActiveInRangeAsync(DateOnly from, DateOnly to,
        CancellationToken ct = default) =>
        await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.Date >= from && b.Date <= to && b.Status == BookingStatus.Active)
            .ToListAsync(ct);

    public async Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default)
    {
        _db.Bookings.Add(booking);
        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
        {
            _db.Entry(booking).State = EntityState.Detached;
            return false;
        }
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        if (_db.Entry(booking).State == EntityState.Detached)
        {
            _db.Bookings.Update(booking);
        }

        await _db.SaveChangesAsync(ct);
    }
}
