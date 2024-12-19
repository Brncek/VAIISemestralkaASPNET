using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using VAIISemestralkaASPNET.App;
using VAIISemestralkaASPNET.Data;

namespace VAIISemestralkaASPNET.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<(IdentityUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                if(user.Email != "admin@admin.com")
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRoles.Add((user, roles));
                }
            }

            return View(userRoles);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await DeleteAllUserDataInDBAsync(id);
                await _userManager.DeleteAsync(user);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "User not found." });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            List<string?> rolesFiltered = [];

            foreach (var item in allRoles)
            {
                if (item == "Admin" || item == "role")
                {
                    continue;
                }

                rolesFiltered.Add(item);
            }

            return View((user, userRoles, rolesFiltered));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoles(string userId, string roles)
        {
            if (roles == "Admin")
            {
                return Forbid();
            }

            bool isInRoles = false;

            foreach (var realRole in CONSTANTS.ROLES)
            {
                if (roles == realRole)
                {
                    isInRoles = true;
                    break;
                }
            }

            if (!isInRoles)
            {
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, roles); 

            return RedirectToAction("Index");
        }

        private async Task DeleteAllUserDataInDBAsync(string userId)
        {

            try
            {
                var userCars = _context.Car.Where(c => c.UserId == userId);
                var userOrders = _context.Orders.Where(o => o.UserId == userId);


                if (userOrders.Any())
                {
                    _context.Orders.RemoveRange(userOrders);
                    await _context.SaveChangesAsync();
                }

                if (userCars.Any())
                {
                    foreach (var car in userCars)
                    {
                        foreach(var service in _context.Services.Where(s => s.CarID == car.Id))
                        {
                            service.CarID = null;
                            service.VIN = car.VIN;
                            _context.Update(service);
                        }
                    }
                    await _context.SaveChangesAsync();

                    _context.Car.RemoveRange(userCars);
                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while deleting cars.", ex);
            }
        }
    }
}
