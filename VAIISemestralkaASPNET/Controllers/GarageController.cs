using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAIISemestralkaASPNET.Data;
using VAIISemestralkaASPNET.Models;
using VAIISemestralkaASPNET.App;
using System.Text.Json;

namespace VAIISemestralkaASPNET.Controllers
{
    public class GarageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GarageController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Garage
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if(User.IsInRole("Admin")) {
                var cars = _context.Car.Include(c => c.User).ToListAsync();
                return View(await cars);
            }
            else
            {
                var userID = _userManager.GetUserId(User)!;
                var applicationDbContext = _context.Car.Include(c => c.User).Where(c => c.UserId == userID);
                return View(await applicationDbContext.ToListAsync());
            }
            
        }

        // GET: Garage/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Garage/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Garage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,VIN,UserId")] Car car)
        {
            VINDetails? carDetails = await VINAPI.GetVinDetailsAsync(car.VIN.ToUpper());

            string name = "";

            try
            {
                string make = carDetails.Make;
                string model = carDetails.Model;
                int year = carDetails.ModelYear;
                name = make + " " + model + " " + year;
            }
            catch 
            {
                return View(car);
            }

            car.UserId = _userManager.GetUserId(User)!;
            car.Name = name;
            _context.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
          
        }

        // GET: Garage/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", car.UserId);
            return View(car);
        }

        // POST: Garage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,VIN,UserId")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            car.UserId = _userManager.GetUserId(User)!;
            try
            {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                    if (!CarExists(car.Id))
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

        // GET: Garage/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Garage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Car.FindAsync(id);
            if (car != null)
            {
                _context.Car.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Car.Any(e => e.Id == id);
        }
    }
}
