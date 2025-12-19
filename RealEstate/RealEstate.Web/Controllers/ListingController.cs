using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Models;
using RealEstate.Web.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RealEstate.Web.Controllers
{
    public class ListingController : Controller
    {
        private readonly ApiService _apiService;

        public ListingController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // --- 1. LİSTELEME (INDEX) ---
        [HttpGet]
        public async Task<IActionResult> Index(string? type) // <-- DEĞİŞİKLİK: Parametre eklendi
        {
            // Gelen type bilgisini (Kiralık/Satılık) servise gönderiyoruz
            var values = await _apiService.GetAllListingsAsync(type);
            
            // Eğer filtre varsa sayfada göstermek için ViewBag'e atabiliriz (Opsiyonel)
            ViewBag.CurrentType = type;

            return View(values);
        }

        // --- 2. EKLEME SAYFASI (CREATE GET) ---
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // --- 3. EKLEME İŞLEMİ (CREATE POST) ---
        [HttpPost]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool result = await _apiService.CreateListingAsync(model);

            if (result)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // --- 4. DÜZENLEME SAYFASI (EDIT GET) ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var value = await _apiService.GetListingByIdAsync(id);

            // Gelen veriyi forma dolduruyoruz
            var model = new CreateListingViewModel
            {
                Title = value.Title,
                City = value.City,
                Price = value.Price,
                Description = value.Description,

                // --- EKLENEN KISIMLAR ---
                Type = value.Type,             // <-- ÖNEMLİ: Radyo butonu doğru seçilsin
                RoomCount = value.RoomCount,   // <-- Diğer eksik bilgiler
                SquareMeters = value.SquareMeters
                // -------------------------
            };

            ViewBag.Id = id;
            ViewBag.CurrentImage = value.ImageUrl;

            return View(model);
        }

        // --- 5. DÜZENLEME KAYDETME (EDIT POST) ---
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hata durumunda resmi tekrar göstermek için
                ViewBag.Id = id;
                return View(model);
            }

            bool result = await _apiService.UpdateListingAsync(id, model);

            if (result)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // --- 6. SİLME İŞLEMİ (DELETE) ---
        // Not: Index sayfasında <a> etiketi kullandığımız için HttpPost'u kaldırdık veya HttpGet yaptık.
        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteListingAsync(id);
            return RedirectToAction("Index");
        }
    }
}