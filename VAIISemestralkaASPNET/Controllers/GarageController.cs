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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if(User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Mechanic")) {
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

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,VIN,UserId")] Car car)
        {
            VINDetails? carDetails = await VINAPI.GetVinDetailsAsync(car.VIN.ToUpper());

            string name = "";

            try
            {
                string make = carDetails!.Make;
                string model = carDetails!.Model;
                int year = carDetails!.ModelYear;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,VIN,UserId")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = await _userManager.GetUserIdAsync(user!);

            if (await VINAPI.GetVinDetailsAsync(car.VIN) == null || String.IsNullOrEmpty(car.Name))
            {
                car.VIN = "";
                return View(car);
            }

            try
            {
                if (User.IsInRole("Admin") || User.IsInRole("Mechanic") || User.IsInRole("Manager") || car.UserId == userId)
                {
                    _context.Update(car);
                }
                
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

            var user = await _userManager.GetUserAsync(User);
            var userId = await _userManager.GetUserIdAsync(user!);

            if (!(User.IsInRole("Admin") || User.IsInRole("Mechanic") || User.IsInRole("Manager") || car.UserId == userId))
            {
                return Forbid();
            }

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Car.FindAsync(id);

            if (car != null)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = await _userManager.GetUserIdAsync(user!);
                var ordersWithCar = _context.Orders.Where(o => o.CarID == id);
                var servicesWithCar = _context.Services.Where(o => o.CarID == id);


                if (User.IsInRole("Admin") || User.IsInRole("Mechanic") || User.IsInRole("Manager") || car.UserId == userId)
                {
                    _context.RemoveRange(ordersWithCar);

                    foreach (var service in servicesWithCar)
                    {
                        service.VIN = car.VIN;
                        service.CarID = null;
                        _context.Update(service);
                    }

                    await _context.SaveChangesAsync();

                    _context.Car.Remove(car);

                } else
                {
                    return Forbid();
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Mechanic")] 
        public IActionResult VinDetails()
        {
            return View();
        }

        [Authorize(Roles = "Mechanic")]
        public async Task<IActionResult> GetVinDetails(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                return Json(new { success = false, message = "VIN is required." });
            }

            try
            {
                VINDetails details = (await VINAPI.GetVinDetailsAsync(vin))!; 

                var detailsDictionary = new Dictionary<string, object>
                {
                    { "VIN", details.VIN },
                    { "Make", details.Make },
                    { "Model", details.Model },
                    { "Model Year", details.ModelYear },
                    { "Body", details.Body },
                    { "Drive", details.Drive },
                    { "Engine Displacement", details.EngineDisplacement },
                    { "Engine Power (KW)", details.EnginePowerKW },
                    { "Engine Power (HP)", details.EnginePowerHP },
                    { "Fuel Type", details.FuelType },
                    { "Transmission", details.Transmission },
                    { "Number of Gears", details.NumberOfGears },
                    { "Emission Standard", details.EmissionStandard },
                    { "Max Speed", details.MaxSpeed },
                    { "Color", details.Color },
                    { "Number of Doors", details.NumberOfDoors },
                    { "Number of Seats", details.NumberOfSeats }
                };

                return Json(new { success = true, details = detailsDictionary });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "VIN is not recognized" });
            }
        }

        private bool CarExists(int id)
        {
            return _context.Car.Any(e => e.Id == id);
        }
    }
}
