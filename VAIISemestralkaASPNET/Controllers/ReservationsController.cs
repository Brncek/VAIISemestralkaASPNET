using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAIISemestralkaASPNET.Data;
using VAIISemestralkaASPNET.Models;
using VAIISemestralkaASPNET.App;
using Microsoft.AspNetCore.Authorization;
using VAIISemestralkaASPNET.Models.NonDBDataHolders;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Index()
        {
            return View(GetCalendarData());
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ClosedDates()
        {
            RemoveOldDates();
            return View(_context.ClosedDates.ToList());
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ClosedDatesCreate()
        {
            return View();
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
        [Authorize]
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

            //TODO: VALIDATION

            ClosedDate date = new ClosedDate();
            data.Closed = new DateTime(data.Closed.Year, data.Closed.Month, data.Closed.Day, data.Hour, 0,0);
            

            date.Closed = data.Closed;

            _context.Add(date);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ClosedDates));
        }

        private List<(string, List<(string, int)>)> GetCalendarData()
        {
            int[] hours = CONSTANTS.CALENDAR_START_HOURS;

            List<(string, List<(string, int)>)> calendarData =  new List<(string, List<(string, int)>)>();

            RemoveOldDates();

            List<ClosedDate> closedDates = _context.ClosedDates.Where(d => d.Closed >= DateTime.Today).ToList();

            var NextDates = DateGetter.NextDates();

            int index = 0;

            foreach (var date in NextDates)
            {
                List<(string, int)> blocksLine = new List<(string, int)>();

                foreach (var hour in hours)
                {
                    bool isClosed = false;
                    bool isFull = false;

                    foreach (var closedDate in closedDates)
                    {
                        if (closedDate.Closed.Date.ToString("dd.MM.yyyy") == date.ToString("dd.MM.yyyy") && closedDate.Closed.Hour == hour)
                        {
                            isClosed = true;
                            break;
                        }
                    }

                    if(date.ToString("dd.MM.yyyy") == DateTime.Now.ToString("dd.MM.yyyy") && DateTime.Now.Hour >= hour)
                    {
                        isFull = true;
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

        private async void RemoveOldDates()
        {
            List<ClosedDate> oldDates = _context.ClosedDates.Where(d => d.Closed < DateTime.Today).ToList();

            foreach (var date in oldDates)
            {
                _context.ClosedDates.Remove(date);
            }

            await _context.SaveChangesAsync();
        }
    }
}
