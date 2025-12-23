using System; // Console.WriteLine için gerekli
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstate.Web.Models;
using RealEstate.Web.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RealEstate.Web.Controllers
{
    public class ListingController : Controller
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private readonly FavoriteService _favoriteService;

        public ListingController(ApiService apiService, AuthService authService, FavoriteService favoriteService)
        {
            _apiService = apiService;
            _authService = authService;
            _favoriteService = favoriteService;
        }

        // --- 1. INDEX (FİLTRELEME) ---
        [HttpGet]
        public async Task<IActionResult> Index(string? type, string? sort, string? city, int? minPrice, int? maxPrice, string? roomCount)
        {
            var values = await _apiService.GetAllListingsAsync(type);
            if (values == null) values = new List<ListingViewModel>();

            if (!string.IsNullOrEmpty(city))
            {
                var searchCity = city.Trim().ToLower();
                values = values.Where(x => x.City != null && x.City.ToLower().Contains(searchCity)).ToList();
            }

            if (minPrice.HasValue) values = values.Where(x => x.Price >= minPrice.Value).ToList();
            if (maxPrice.HasValue) values = values.Where(x => x.Price <= maxPrice.Value).ToList();

            if (!string.IsNullOrEmpty(roomCount))
            {
                values = values.Where(x => x.RoomCount != null && x.RoomCount.Trim() == roomCount.Trim()).ToList();
            }

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "price_asc": values = values.OrderBy(x => x.Price).ToList(); break;
                    case "price_desc": values = values.OrderByDescending(x => x.Price).ToList(); break;
                    case "date_desc": values = values.OrderByDescending(x => x.CreatedDate).ToList(); break;
                    case "date_asc": values = values.OrderBy(x => x.CreatedDate).ToList(); break;
                    default: values = values.OrderByDescending(x => x.CreatedDate).ToList(); break;
                }
            }

            ViewBag.CurrentType = type;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentCity = city;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentRoomCount = roomCount;

            ViewBag.TypeOptions = new List<SelectListItem>
            {
                new() { Text = "Tümü", Value = "", Selected = string.IsNullOrEmpty(type) },
                new() { Text = "Satılık", Value = "Satılık", Selected = type == "Satılık" },
                new() { Text = "Kiralık", Value = "Kiralık", Selected = type == "Kiralık" }
            };

            ViewBag.SortOptions = new List<SelectListItem>
            {
                new() { Text = "En Yeni", Value = "date_desc", Selected = string.IsNullOrEmpty(sort) || sort == "date_desc" },
                new() { Text = "En Eski", Value = "date_asc", Selected = sort == "date_asc" },
                new() { Text = "Fiyat Artan", Value = "price_asc", Selected = sort == "price_asc" },
                new() { Text = "Fiyat Azalan", Value = "price_desc", Selected = sort == "price_desc" }
            };

            ViewBag.RoomOptions = new List<SelectListItem>
            {
                new() { Text = "Tümü", Value = "", Selected = string.IsNullOrEmpty(roomCount) },
                new() { Text = "1+0", Value = "1+0", Selected = roomCount == "1+0" },
                new() { Text = "1+1", Value = "1+1", Selected = roomCount == "1+1" },
                new() { Text = "2+1", Value = "2+1", Selected = roomCount == "2+1" },
                new() { Text = "3+1", Value = "3+1", Selected = roomCount == "3+1" },
                new() { Text = "4+1", Value = "4+1", Selected = roomCount == "4+1" },
                new() { Text = "4+2", Value = "4+2", Selected = roomCount == "4+2" },
                new() { Text = "5+1", Value = "5+1", Selected = roomCount == "5+1" },
                new() { Text = "Villa", Value = "Villa", Selected = roomCount == "Villa" }
            };

            return View(values);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Create", "Listing") });
            return View();
        }

        // --- DEDEKTİF MODU EKLENMİŞ CREATE METODU ---
        [HttpPost]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var currentUser = _authService.GetCurrentUser();
            model.UserId = currentUser?.Id ?? 0;

            // --- DEDEKTİF MODU BAŞLANGIÇ ---
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"[WEB CONTROLLER] Token Kontrol: IsAuthenticated = {_authService.IsAuthenticated()}");
            var token = HttpContext.Request.Cookies["auth_token"];
            Console.WriteLine($"[WEB CONTROLLER] Cookie'den Token: {(string.IsNullOrEmpty(token) ? "BOŞŞŞ!" : token.Substring(0, Math.Min(30, token.Length)) + "...")}");
            Console.WriteLine($"[WEB CONTROLLER] Formdan Gelen Başlık: {model.Title}");
            Console.WriteLine($"[WEB CONTROLLER] Formdan Gelen Oda Sayısı: '{model.RoomCount}'");
            Console.WriteLine($"[WEB CONTROLLER] Formdan Gelen Metrekare: {model.SquareMeters}");
            Console.WriteLine($"[WEB CONTROLLER] Formdan Gelen UserId: {model.UserId}");
            Console.WriteLine("--------------------------------------------------");
            // --- DEDEKTİF MODU BİTİŞ ---

            var (success, errorMsg) = await _apiService.CreateListingAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = "İlan başarıyla oluşturuldu!";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(errorMsg)) ModelState.AddModelError("", $"API Hatası: {errorMsg}");
            else ModelState.AddModelError("", "Bilinmeyen bir hata oluştu.");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var value = await _apiService.GetListingByIdAsync(id);
            if (value == null) return NotFound();

            var model = new CreateListingViewModel
            {
                Title = value.Title ?? "",
                City = value.City ?? "",
                Price = value.Price,
                Description = value.Description,
                Type = value.Type,
                RoomCount = value.RoomCount?.Trim(),
                SquareMeters = value.SquareMeters,
                UserId = value.UserId ?? 0,
                ImageUrl = value.ImageUrl
            };

            ViewBag.Id = id;
            ViewBag.CurrentImage = value.ImageUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateListingViewModel model)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");

            bool result = await _apiService.UpdateListingAsync(id, model);

            if (result) return RedirectToAction("Index");

            ModelState.AddModelError("", "Güncelleme başarısız.");
            ViewBag.Id = id;
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");
            var (success, errorMessage) = await _apiService.DeleteListingAsync(id);
            if (success) TempData["SuccessMessage"] = "Silindi!";
            else TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _apiService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return View(listing);
        }

        [HttpGet]
        public async Task<IActionResult> MyListings()
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");
            var listings = await _apiService.GetMyListingsAsync();
            return View(listings);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddToFavorites(int listingId, string? returnUrl)
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Auth", new { returnUrl = returnUrl ?? Url.Action("Index", "Listing") });

            await _favoriteService.AddToFavoritesAsync(listingId);

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }



    }
}
