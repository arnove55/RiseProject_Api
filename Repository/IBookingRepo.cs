using AngularApi.Models;

namespace AngularApi.Repository
{
    public interface IBookingRepo
    {
        Task AddBookingAsync(IEnumerable<Booking> bookings);
        Task<Booking> GetBookingAsync(string username, DateTime startDate, DateTime endDate, string bookingType);
        Task<Booking> GetBookingByDateAsync(string username, DateTime date);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string username);
        Task RemoveBookingAsync(Booking booking);
        Task<bool> SaveChangesAsync();
    }
}
