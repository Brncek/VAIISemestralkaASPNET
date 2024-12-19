using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using VAIISemestralkaASPNET.App;
using VAIISemestralkaASPNET.Data;
using VAIISemestralkaASPNET.Models;

namespace VAIISemestralkaASPNET.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ServicesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		[Authorize(Roles = "Admin, Mechanic, Manager")]
		public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Services.Include(s => s.Car);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.ServiceId == id);

			var isAuthorized = User.IsInRole("Admin") ||
							   User.IsInRole("Manager") ||
							   User.IsInRole("Mechanic") ||
							   (order != null && order.UserId == userId);

			if (!isAuthorized)
			{
				return Forbid(); 
			}

            List<string> imageUrls = [];

            if (!string.IsNullOrEmpty(service.ServiceImagesLocation))
            {
                var folderPath = service.ServiceImagesLocation;

                if (Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        var imageUrl = Url.Action("ViewImage", "Services", new { folder = Path.GetFileName(folderPath), fileName });
                        imageUrls.Add(imageUrl!);
                    }
                }
            }

            ViewBag.Images = imageUrls;
            return View(service);
        }

        [Authorize(Roles = "Admin, Mechanic, Manager")]
		public async Task<IActionResult> Create(int Id) // OrderID
		{
			var order = await _context.Orders
				.Include(o => o.Car)
				.FirstOrDefaultAsync(o => o.Id == Id);

			if (order == null)
			{
				return NotFound();
			}

            if (order.Service != null)
            {
                return Forbid();
            }

            ViewBag.OrderID = Id;
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Mechanic, Manager")]
		public async Task<IActionResult> Create(int OrderId, [Bind("StartDate,EndtDate,WorkTime,ServisesDone")] Service service, List<IFormFile> uploadedFiles)
		{
            var order = await _context.Orders.FindAsync(OrderId);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Service != null)
            {
                return Forbid();
            }

            TimeSpan difference = service.EndtDate - service.StartDate;
            double minutesDifference = difference.TotalMinutes;

            if (service.EndtDate <= service.StartDate || (int)Math.Ceiling(minutesDifference / 60) <= service.WorkTime 
                || String.IsNullOrEmpty(service.ServisesDone))
            {
                return RedirectToAction("Details", new { OrderId });
            }

            service.CarID = order.CarID;
            service.VIN = order.VIN;

			string folderName = Path.Combine("wwwroot", "uploads", $"service_{Guid.NewGuid()}");
			Directory.CreateDirectory(folderName);

			foreach (var file in uploadedFiles)
			{
				if (file.Length > 0)
				{
					var filePath = Path.Combine(folderName, Path.GetFileName(file.FileName));
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
			}

			service.ServiceImagesLocation = folderName;

			_context.Add(service);
            await _context.SaveChangesAsync();

            order.State = CONSTANTS.ORDER_STATE_DONE;
            order.ServiceId = service.Id;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

		[Authorize(Roles = "Admin, Mechanic, Manager")]
		public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            var images = new List<(string FileName, string ImageUrl)>();

            if (!string.IsNullOrEmpty(service.ServiceImagesLocation))
            {
                var folderPath = service.ServiceImagesLocation;

                if (Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath);
                    foreach (var file in files)
                    {
                        var fileName = System.IO.Path.GetFileName(file);
                        var imageUrl = $"/uploads/{System.IO.Path.GetFileName(folderPath)}/{fileName}";
                        images.Add((fileName, imageUrl));
                    }
                }
            }

            ViewBag.Images = images; 
            ViewBag.FolderName = System.IO.Path.GetFileName(service.ServiceImagesLocation ?? "");

            return View(service);
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Mechanic, Manager")]
        public async Task<IActionResult> UploadImages(string folder, List<IFormFile> files)
        {
            try
            {
                var folderPath = Path.Combine("wwwroot", "uploads", folder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var uploadedFiles = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        uploadedFiles.Add($"/uploads/{folder}/{fileName}");
                    }
                }

                return Json(new { success = true, files = uploadedFiles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Mechanic, Manager")]
        public IActionResult DeleteImage(string folder, string fileName)
        {
            try
            {
                var filePath = Path.Combine("wwwroot", "uploads", folder, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "File not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Mechanic, Manager")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndtDate,CarID,VIN,WorkTime,ServisesDone")] Service service)
		{

			if (id != service.Id)
			{
				return NotFound();
			}

            TimeSpan difference = service.EndtDate - service.StartDate;
            double minutesDifference = difference.TotalMinutes;

            if (service.EndtDate <= service.StartDate || (int)Math.Ceiling(minutesDifference / 60) <= service.WorkTime
                || String.IsNullOrEmpty(service.ServisesDone))
            {
                return RedirectToAction("Edit", new { id });
            }

            try
			{
				var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
				if (existingService == null)
				{
					return NotFound();
				}

				service.ServiceImagesLocation = existingService.ServiceImagesLocation;
                service.VIN = existingService.VIN;
                service.CarID = existingService.CarID;

				_context.Update(service);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ServiceExists(service.Id))
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


		[Authorize(Roles = "Admin, Mechanic, Manager")]
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Mechanic, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            var linkedOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.ServiceId == service.Id);

            if (linkedOrder != null)
            {
                linkedOrder.ServiceId = null; 
                _context.Update(linkedOrder);
            }

            if (!string.IsNullOrEmpty(service.ServiceImagesLocation))
            {
                try
                {
                    if (Directory.Exists(service.ServiceImagesLocation))
                    {
                        Directory.Delete(service.ServiceImagesLocation, true); 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting folder: {ex.Message}");
                }
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult ViewImage(string folder, string fileName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var authorized = User.IsInRole("Admin") || User.IsInRole("Mechanic") || User.IsInRole("Manager") ||
                             _context.Orders.Any(o => o.Service.ServiceImagesLocation.Contains(folder) && o.UserId == userId);

            if (!authorized)
            {
                return Forbid();
            }

            var rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "wwwroot", "uploads", folder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = GetContentType(filePath);

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream"; 
            }
            return contentType;
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
