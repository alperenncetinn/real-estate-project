using Xunit;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Controllers;
using RealEstate.Api.Dtos;

namespace RealEstate.Api.Tests
{
    public class ListingsControllerTests
    {
        // SENARYO 1: Geçerli veri geldiğinde 200 OK dönmeli
        [Fact]
        public void Create_ReturnsOk_WhenDtoIsValid()
        {
            // 1. Arrange (Hazırlık)
            var controller = new ListingsController();
            var validDto = new ListingDto
            {
                Title = "Deneme İlanı",
                Price = 5000,
                City = "İstanbul"
            };

            // 2. Act (Eylem)
            var result = controller.Create(validDto);

            // 3. Assert (Kontrol)
            Assert.IsType<OkObjectResult>(result);
        }

        // SENARYO 2: Veri NULL gelirse 400 Bad Request dönmeli
        [Fact]
        public void Create_ReturnsBadRequest_WhenDtoIsNull()
        {
            // 1. Arrange
            var controller = new ListingsController();
            ListingDto nullDto = null; // Bilerek null yapıyoruz

            // 2. Act
            var result = controller.Create(nullDto);

            // 3. Assert
            // Controller kodunda: if (dto == null) return BadRequest(...)
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // SENARYO 3: Dönen cevabın içinde gönderdiğimiz Başlık (Title) doğru mu?
        [Fact]
        public void Create_ReturnsCorrectTitle_InResponse()
        {
            // 1. Arrange
            var controller = new ListingsController();
            var testTitle = "Manzaralı Villa";
            var dto = new ListingDto { Title = testTitle };

            // 2. Act
            var result = controller.Create(dto) as OkObjectResult;

            // 3. Assert
            Assert.NotNull(result); // Sonuç boş olmamalı
            
            // Dönen anonim nesneyi (new { message=..., title=... }) okumak için reflection kullanıyoruz
            // Veya basitçe string'e çevirip içinde aratıyoruz (daha pratik):
            var responseBody = result.Value.ToString();
            
            // Not: Anonim tipler testlerde 'dynamic' ile de okunabilir ama ToString genelde yetiyor.
            // Burada Value property'sine erişip Title'ı kontrol ediyoruz.
            // C# Reflection ile özelliğe erişim:
            var returnTitle = result.Value.GetType().GetProperty("title")?.GetValue(result.Value, null);
            
            Assert.Equal(testTitle, returnTitle);
        }

        // SENARYO 4: Sadece Fiyat girilse, diğerleri boş olsa bile çalışmalı
        // (Çünkü Dto'daki alanların hepsi 'string?' yani nullable, zorunlu değil)
        [Fact]
        public void Create_ReturnsOk_WhenOnlyPriceIsProvided()
        {
            // 1. Arrange
            var controller = new ListingsController();
            // Başlık veya Şehir yok, sadece fiyat var.
            var partialDto = new ListingDto
            {
                Price = 150000
            };

            // 2. Act
            var result = controller.Create(partialDto);

            // 3. Assert
            // Kodunda "Title zorunludur" gibi bir if kontrolü olmadığı için 
            // bunun da OK dönmesi gerekiyor.
            Assert.IsType<OkObjectResult>(result);
        }
    }
}