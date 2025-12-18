using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Models;
using RealEstate.Web.Services;

namespace RealEstate.Web.Controllers
{
    public class ListingController : Controller
    {
        private readonly ApiService _apiService;

        // Dependency Injection: We ask for the ApiService here
        public ListingController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Send data to API via Service
            bool result = await _apiService.CreateListingAsync(model);

            if (result)
            {
                // If successful, go to Home Page
                return RedirectToAction("Index", "Home");
            }

            // If failed, reload the form (maybe add an error message here later)
            return View(model);
        }
    }
}