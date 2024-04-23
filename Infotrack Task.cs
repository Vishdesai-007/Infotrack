using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPI.Controllers
{
    [ApiController]
    [Route("api/Settlement")]
    public class BookingController : ControllerBase
    {
        private static List<Booking> bookings = new List<Booking>();

        //Allow up to 4 settlements
        private const int MaxBookings = 4;

        //Business Start Time
        private static readonly TimeSpan StartTime = new TimeSpan(9, 0, 0);

        //Business End Tiem
        private static readonly TimeSpan EndTime = new TimeSpan(16, 0, 0);

        public class BookingRequest
        {
            public string BookingTime { get; set; }
            public string Name { get; set; }
        }

        public class Booking
        {
            public string BookingId { get; set; }
            public string Name { get; set; }
            public string BookingTime { get; set; }
        }

        [HttpPost]
        [Route("MakeBooking")]
        public IActionResult MakeBooking([FromBody] BookingRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data format.");
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name must be provided.");
            }

            // Validate Booking Time
            if (!TimeSpan.TryParseExact(request.BookingTime, "hh\\:mm", null, out var bookingTime))
            {
                return BadRequest("Invalid booking time format.");
            }

            // Validate booking time
            if (!IsBusinessHour(bookingTime))
            {
                return BadRequest("Booking time must be between 9:00 and 16:00.");
            }

            // Check if booking slot is available
            if (!IsBookingAvailable(request.BookingTime, bookingTime))
            {
                return Conflict("Booking slot is not available.");
            }

            // Create booking
            var bookingId = Guid.NewGuid().ToString();
            var booking = new Booking
            {
                BookingId = bookingId,
                Name = request.Name,
                BookingTime = request.BookingTime
            };

            bookings.Add(booking);

            return Ok(new { BookingId = bookingId });
        }

        private bool IsBusinessHour(TimeSpan bookingTime)
        {
            return bookingTime >= StartTime && bookingTime <= EndTime;
        }

        private bool IsBookingAvailable(string bookingTimeStr, TimeSpan bookingTime)
        {
            var existingBookings = bookings.Where(b => b.BookingTime == bookingTimeStr).ToList();
            return existingBookings.Count < MaxBookings;
        }
    }
}
