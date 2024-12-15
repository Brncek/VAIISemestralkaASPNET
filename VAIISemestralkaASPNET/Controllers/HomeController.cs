using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VAIISemestralkaASPNET.Models;
using VAIISemestralkaASPNET.App;
using System;

namespace VAIISemestralkaASPNET.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }


        public IActionResult Reservations()
        {
            return View();
        }

        public IActionResult Tester()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public String Dates() //TODO REMOVE
        {
            
            String dates = "";

            foreach (var date in DateGetter.NextDates())
            {
                dates += date.ToString("dd.MM.yyyy") + " ";
            }

            return dates;
        }
    }
}
