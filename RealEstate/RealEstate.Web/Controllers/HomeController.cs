using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Services;

namespace RealEstate.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PropertyApiClient _propertyApiClient;

        public HomeController(PropertyApiClient propertyApiClient)
        {
            _propertyApiClient = propertyApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var properties = await _propertyApiClient.GetFeaturedAsync();
            return View(properties);
        }
    }
}