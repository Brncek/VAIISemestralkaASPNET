using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAIISemestralkaASPNET.App;
using VAIISemestralkaASPNET.Data;
using VAIISemestralkaASPNET.Models;

namespace VAIISemestralkaASPNET.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Mechanic") || User.IsInRole("Manager"))
            {
				var applicationDbContext = _context.Orders.Include(o => o.Car).Include(o => o.User);
				return View(await applicationDbContext.ToListAsync());
			}
            else
            {
				var user = await _userManager.GetUserAsync(User);
				var userID = await _userManager.GetUserIdAsync(user!);
				var applicationDbContext = _context.Orders.Where(o => o.UserId == userID)
                    .Include(o => o.Car).Include(o => o.User);
				return View(await applicationDbContext.ToListAsync());
			}
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Car)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin, Manager, Mechanic")]
        public IActionResult Create() 
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Mechanic")]
        public async Task<IActionResult> Create([Bind("Id,Date,CarID,VIN,ServiseInfo,State,UserId")] Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            var userID = await _userManager.GetUserIdAsync(user!);
            order.UserId = userID;
            order.State = CONSTANTS.ORDER_STATE_RECEAVED;
            order.CarID = null;

            if (await VINAPI.GetVinDetailsAsync(order.VIN) != null) //CONTROL VIN
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            order.VIN = "";

            return View(order);
        }

        [Authorize(Roles = "Admin, Manager, Mechanic")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Mechanic")]
        public async Task<IActionResult> Edit(int id, DateTime date, string State)
        {
            Order? order = await _context.Orders.FindAsync(id);


            bool isin = false;

            foreach (var acceptedState in CONSTANTS.ORDER_STATES())
            {
                if (State == acceptedState)
                {
                    isin = true;
                }
            }

            if (order == null || !isin || date < DateTime.Now) //validation
            {
                return View(order);
            } 
            else
            {
                order.Date = date;
                order.State = State;
            }

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Car)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
