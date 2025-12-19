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

        // Dependency Injection: ApiService'i buradan alıyoruz
        public ListingController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // --- 1. LİSTELEME (INDEX) ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // API'den tüm ilanları çekip ekrana gönderiyoruz
            var values = await _apiService.GetAllListingsAsync();
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
            // Zorunlu alanlar doldurulmuş mu?
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Veriyi servise gönder
            bool result = await _apiService.CreateListingAsync(model);

            if (result)
            {
                // Başarılıysa listeye dön
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // --- 4. DÜZENLEME SAYFASI (EDIT GET) ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Düzenlenecek ilanın mevcut bilgilerini getir
            var value = await _apiService.GetListingByIdAsync(id);

            // Gelen veriyi formun anlayacağı modele çeviriyoruz
            var model = new CreateListingViewModel
            {
                Title = value.Title,
                City = value.City,
                Price = value.Price,
                Description = value.Description
            };

            // ID'yi ve mevcut resmi sayfada kullanmak için ViewBag'e atıyoruz
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
                return View(model);
            }

            // Güncelleme isteğini servise gönder
            bool result = await _apiService.UpdateListingAsync(id, model);

            if (result)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // --- 6. SİLME İŞLEMİ (DELETE) ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteListingAsync(id);
            return RedirectToAction("Index");
        }
    }
}