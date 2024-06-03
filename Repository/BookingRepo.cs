using AngularApi.Context;
using AngularApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Repository
{
    public class BookingRepo : IBookingRepo
    {
        private readonly AppDbContext _appDbContext;
        public BookingRepo(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task AddBookingAsync(IEnumerable<Booking> bookings)
        {
            await _appDbContext.Booking.AddRangeAsync(bookings);
            await SaveChangesAsync();
        }

        public async Task<Booking> GetBookingAsync(string username, DateTime startDate, DateTime endDate, string bookingType)
        {
            return await _appDbContext.Booking.Where(b => b.User.Username == username &&
            b.Booking_Type == bookingType && b.End_Date >= startDate && b.Start_Date <= endDate
            ).FirstOrDefaultAsync();
        }

        public async Task<Booking> GetBookingByDateAsync(string username, DateTime date)
        {
            return await _appDbContext.Booking
                .Where(b => b.User.Username == username &&
                            b.Start_Date <= date &&
                            b.End_Date >= date)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string username)
        {
            return await _appDbContext.Booking
                .Where(b => b.User.Username == username && b.Start_Date >= DateTime.Today)
                .OrderBy(b => b.Start_Date)
                .ToListAsync();
        }

        public async Task RemoveBookingAsync(Booking booking)
        {
            _appDbContext.Booking.Remove(booking);
            await SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _appDbContext.SaveChangesAsync() > 0;
        }

    }
}
