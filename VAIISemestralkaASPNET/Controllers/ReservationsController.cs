using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAIISemestralkaASPNET.Data;
using VAIISemestralkaASPNET.Models;
using VAIISemestralkaASPNET.App;
using Microsoft.AspNetCore.Authorization;
using VAIISemestralkaASPNET.Models.NonDBDataHolders;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VAIISemestralkaASPNET.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await GetCalendarData());
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ClosedDates()
        {
            await RemoveOldDates();
            return View(_context.ClosedDates.ToList());
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ClosedDatesCreate()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> ReservationCreate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dates = DateGetter.NextDates();
            int hourIndex = (int)(id) % CONSTANTS.CALENDAR_START_HOURS.Length;
            int datesIndex = (int)(id) / CONSTANTS.CALENDAR_START_HOURS.Length;

            try
            {
                OrderForm orderForm = new()
                {
                    Date = new DateTime(dates[datesIndex].Year, dates[datesIndex].Month, dates[datesIndex].Day,
                CONSTANTS.CALENDAR_START_HOURS[hourIndex], 0, 0)
                };

                if (!CheckDateValid(orderForm.Date, true))
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                var userId = await _userManager.GetUserIdAsync(user!);
                var list = new SelectList(_context.Car.Where(c => c.UserId == userId), "Id", "Name");
                ViewBag.CarID = list;

                return View(orderForm);
            }
            catch
            {
                return NotFound();
            }

            
        }

        [HttpPost, ActionName("ReservationCreate")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ReservationCreateConfirm([Bind("Date, CarID, ServiseInfo")] OrderForm orderForm)
        {
            if (!CheckDateValid(orderForm.Date, true))
            {
                return Forbid();
            }

            Car? car = await _context.Car.AsNoTracking().FirstOrDefaultAsync(c => c.Id == orderForm.CarID);
            if (car == null) 
            {
                return NotFound();
            }

            if (car.UserId != _userManager.GetUserId(User)!)
            {
                return Forbid();
            }

            Order order = new()
            {
                Date = orderForm.Date,
                ServiseInfo = orderForm.ServiseInfo,
                CarID = orderForm.CarID,
                UserId = _userManager.GetUserId(User)!,
                State = CONSTANTS.ORDER_STATE_RECEAVED
            };

            _context.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ClosedDatesDelete(int? id)
        {
            if (id == null) 
            {
                    return NotFound();
            }

            var date = await _context.ClosedDates.FirstOrDefaultAsync(d => d.Id == id);

            if (date == null) 
            {
                return NotFound();
            }

            return View(date);
        }

        [HttpPost, ActionName("ClosedDatesDelete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ClosedDatesDeleteConfirmed(int id)
        {
            var date = await _context.ClosedDates.FindAsync(id);

            if (date != null)
            {
                _context.ClosedDates.Remove(date);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ClosedDates));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ClosedDatesCreate([Bind("Closed, Hour")] ClosedDateForm data)
        {

            ClosedDate date = new();
            data.Closed = new DateTime(data.Closed.Year, data.Closed.Month, data.Closed.Day, data.Hour, 0,0);
            
            if(!CheckDateValid(data.Closed, true))
            {
                return View(data);
            }

            bool isin = false;

            foreach (var hour in CONSTANTS.CALENDAR_START_HOURS)
            {
                if (data.Hour == hour)
                {
                    isin = true;
                    break;
                }
            }

            if (!isin)
            {
                return View(data);    
            }

            date.Closed = data.Closed;

            _context.Add(date);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ClosedDates));
        }

        private async Task<List<(string, List<(string, int)>)>> GetCalendarData()
        {
            int difference = CONSTANTS.CALENDAR_START_HOURS[1] - CONSTANTS.CALENDAR_START_HOURS[0];
            int[] hours = CONSTANTS.CALENDAR_START_HOURS;

            List<(string, List<(string, int)>)> calendarData = new();

            await RemoveOldDates();

            List<ClosedDate> closedDates = _context.ClosedDates.Where(d => d.Closed >= DateTime.Today).ToList();

            var NextDates = DateGetter.NextDates();

            int index = 0;

            foreach (var date in NextDates)
            {
                List<(string, int)> blocksLine = new();

                foreach (var hour in hours)
                {
                    bool isClosed = false;
                    bool isFull = false;

                    DateTime dateAddedHours = new DateTime(date.Year, date.Month, date.Day , hour, 0,0);

                    if(dateAddedHours < DateTime.Now)
                    {
                        isClosed = true;
                    }

                    foreach (var closedDate in closedDates)
                    {
                        if (closedDate.Closed == dateAddedHours)
                        {
                            isClosed = true;
                            break;
                        }
                    }

                    if (!isClosed)
                    {
                        var otherOrders = _context.Orders.Where(o => o.Date > DateTime.Now);
                        foreach (var order in otherOrders)
                        {
                            DateTime closest = FindClosestCalendarDate(order.Date);

                            if (dateAddedHours == closest && order.State != CONSTANTS.ORDER_STATE_DONE)
                            {
                                isFull = true;
                            }
                        }
                    }

                    if (isFull)
                    {
                        blocksLine.Add((CONSTANTS.CALENDAR_FULL, index));
                    } 
                    else if (isClosed)
                    {
                        blocksLine.Add((CONSTANTS.CALENDAR_CLOSED, index));
                    } 
                    else
                    {
                        blocksLine.Add((CONSTANTS.CALENDAR_OPEN, index));
                    }

                    index++;
                }

                calendarData.Add((date.ToString("dd.MM.yyyy") ,blocksLine));
            }

            return calendarData;
        }

        private async Task RemoveOldDates()
        {
            List<ClosedDate> oldDates = _context.ClosedDates.Where(d => d.Closed < DateTime.Today).ToList();

            foreach (var date in oldDates)
            {
                _context.ClosedDates.Remove(date);
            }

            await _context.SaveChangesAsync();
        }

        private DateTime FindClosestCalendarDate(DateTime date)
        {
            int difference = CONSTANTS.CALENDAR_START_HOURS[1] - CONSTANTS.CALENDAR_START_HOURS[0];

            foreach (var hour in CONSTANTS.CALENDAR_START_HOURS)
            {
                if (date.Hour >= hour && date.Hour < hour + difference)
                {
                    return new DateTime(date.Year, date.Month, date.Day, hour, 0,0);
                }
            }

            return date;
        }

        private bool CheckDateValid(DateTime date, bool validateWholeHours)
        {
            int difference =  CONSTANTS.CALENDAR_START_HOURS[1] - CONSTANTS.CALENDAR_START_HOURS[0];

            if (date <= DateTime.Now)
            {
                return false;
            }

            if (validateWholeHours)
            {
                bool isin = false;
                foreach (var time in CONSTANTS.CALENDAR_START_HOURS)
                {
                    if (date.Hour == time)
                    {
                        isin = true;
                        break;
                    }
                }

                if (!isin)
                {
                    return false;
                }
            }

            var closedDates = _context.ClosedDates;
            var otherOrders = _context.Orders.Where(o => o.Date > DateTime.Now);


            foreach (var closedDate in closedDates)
            {
                if (date > closedDate.Closed && date < closedDate.Closed.AddHours(difference))
                {
                    return false;
                }
            }

            foreach (var order in otherOrders)
            {
                DateTime closest = FindClosestCalendarDate(order.Date);
                if (date >= closest && date < closest && order.State != CONSTANTS.ORDER_STATE_DONE)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
