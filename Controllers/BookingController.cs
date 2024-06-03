using AngularApi.Context;
using AngularApi.Models.DTO;
using AngularApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using AngularApi.Repository;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingRepo _bookingRepo;
        private readonly IUserRepo _userRepo;
        public BookingController(AppDbContext appdbcontext,IUserRepo userRepo ,IBookingRepo bookingRepo,ILogger<BookingController> logger)
        {
            _appDbContext = appdbcontext;
            _logger = logger;
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddBooking([FromBody] AddBookingDto addBookingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User ID not found in token.");
            }

            var now = DateTime.Now;

            if (addBookingDto.Start_Date.Date <= DateTime.Today)
            {
                return BadRequest(new { Message = "Booking for today or any past date is not allowed." });
            }

            if (addBookingDto.End_Date.Date < addBookingDto.Start_Date.Date)
            {
                return BadRequest(new { Message = "End date cannot be before start date." });
            }

            if (addBookingDto.Start_Date.Date > DateTime.Today.AddMonths(3))
            {
                return BadRequest(new { Message = "Booking cannot be made more than 3 months in advance." });
            }

            if (addBookingDto.Start_Date.Date == DateTime.Today.AddDays(1) && now.Hour >= 20)
            {
                return BadRequest(new { Message = "Bookings for tomorrow can only be made before 8 PM today." });
            }

            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var existingBooking = await _bookingRepo.GetBookingAsync(username, addBookingDto.Start_Date, addBookingDto.End_Date, addBookingDto.Booking_Type);

            if (existingBooking != null)
            {
                return BadRequest(new { Message = $"You already have a booking of type {addBookingDto.Booking_Type} from {existingBooking.Start_Date.ToShortDateString()} to {existingBooking.End_Date.ToShortDateString()}." });
            }

            var bookings = new List<Booking>();
            for (DateTime date = addBookingDto.Start_Date.Date; date <= addBookingDto.End_Date.Date; date = date.AddDays(1))
            {
                var booking = new Booking
                {
                    Booking_Date = now.Date,
                    Booking_Type = addBookingDto.Booking_Type,
                    Start_Date = date,
                    End_Date = date,
                    User = user
                };

                bookings.Add(booking);
            }

            await _bookingRepo.AddBookingAsync(bookings);

            return Ok(new { Message = "Booking added successfully!!" });
        }


        //cancle booking 
        [HttpDelete("{date}")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(DateTime date)
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var booking = await _appDbContext.Booking
                .Where(b => b.User.Username == username &&
                            b.Start_Date <= date &&
                            b.End_Date >= date)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("Booking not found for the selected date.");
            }

            if (DateTime.Now.Date == date.Date && DateTime.Now.Hour >= 20)
            {
                return BadRequest("Cannot cancel booking on the same day after 8 PM.");
            }

            if (DateTime.Now.Date > date.Date)
            {
                return BadRequest("Cannot cancel past bookings.");
            }

            if (booking.Start_Date == booking.End_Date)
            {
                await _bookingRepo.RemoveBookingAsync(booking);
            }

            else if (booking.Start_Date == date)
            {
                booking.Start_Date = date.AddDays(1);
            }
            else if (booking.End_Date == date)
            {
                booking.End_Date = date.AddDays(-1);
            }
            //else
            //{
            //    var newBooking = new Booking
            //    {
            //        Booking_Date = booking.Booking_Date,
            //        Booking_Type = booking.Booking_Type,
            //        Start_Date = date.AddDays(1),
            //        End_Date = booking.End_Date,
            //        User = booking.User
            //    };

            //    booking.End_Date = date.AddDays(-1);

            //    _appDbContext.Booking.Add(newBooking);
            //}

            await _appDbContext.SaveChangesAsync();

            return Ok(new {Message= "Booking cancelled successfully." });
        }

        [HttpGet("user-booking")]
        [Authorize]
        public async Task<IActionResult> GetUserBooking()
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var bookings = await _bookingRepo.GetUserBookingsAsync(username);

            var totalDays = bookings.SelectMany(b => Enumerable.Range(0, 1 + b.End_Date.Subtract(b.Start_Date).Days)
                                            .Select(offset => b.Start_Date.AddDays(offset)))
                                      .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                                      .Count();

            return Ok(new { totalBookings = totalDays, bookings });
        }
    }
}
    










