using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await DeleteAllCarsByUserIdAsync(id);
                await _userManager.DeleteAsync(user);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "User not found." });
        }

        public async Task<IActionResult> AssignRoles(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles); 
                await _userManager.AddToRolesAsync(user, roles); 
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            List<string?> rolesFiltered = new List<string?>();

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
        public async Task<IActionResult> UpdateRoles(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            foreach (var item in roles)
            {
                if (item == "Admin")
                {
                    return NotFound();
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, roles); 

            return RedirectToAction("Index");
        }

        public async Task DeleteAllCarsByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            try
            {
                var userCars = _context.Car.Where(c => c.UserId == userId);

                if (!userCars.Any())
                {
                    return;
                }

                _context.Car.RemoveRange(userCars);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while deleting cars.", ex);
            }
        }



    }
}
