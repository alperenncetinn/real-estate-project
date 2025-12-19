using Xunit;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Controllers;
using RealEstate.Api.Dtos;
using System.Threading.Tasks; // async işlemleri için gerekli

namespace RealEstate.Api.Tests
{
    public class ListingsControllerTests
    {
        // SENARYO 1: Geçerli veri geldiğinde 200 OK dönmeli
        [Fact]
        public async Task Create_ReturnsOk_WhenDtoIsValid()
        {
            // 1. Arrange
            // Controller artık IWebHostEnvironment istiyor.
            // Bu testte dosya yüklemediğimiz için "null" gönderiyoruz.
            var controller = new ListingsController(null!); 
            
            var validDto = new ListingDtos
            {
                Title = "Deneme İlanı",
                Price = 5000,
                City = "İstanbul"
            };

            // 2. Act
            // Metot async olduğu için başına "await" koyduk
            var result = await controller.Create(validDto);

            // 3. Assert
            Assert.IsType<OkObjectResult>(result);
        }

        // SENARYO 2: Veri NULL gelirse 400 Bad Request dönmeli
        [Fact]
        public async Task Create_ReturnsBadRequest_WhenDtoIsNull()
        {
            // 1. Arrange
            var controller = new ListingsController(null!);
            ListingDtos? nullDto = null;

            // 2. Act
            // nullDto! ile "biliyorum null, devam et" diyoruz
            var result = await controller.Create(nullDto!);

            // 3. Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // SENARYO 3: Dönen cevabın içinde gönderdiğimiz Başlık (Title) doğru mu?
        [Fact]
        public async Task Create_ReturnsCorrectTitle_InResponse()
        {
            // 1. Arrange
            var controller = new ListingsController(null!);
            var testTitle = "Manzaralı Villa";
            var dto = new ListingDtos { Title = testTitle };

            // 2. Act
            var result = await controller.Create(dto) as OkObjectResult;

            // 3. Assert
            Assert.NotNull(result);
            
            if (result?.Value != null)
            {
                // Reflection ile kontrol
                var returnTitle = result.Value.GetType().GetProperty("title")?.GetValue(result.Value, null);
                Assert.Equal(testTitle, returnTitle);
            }
        }

        // SENARYO 4: Sadece Fiyat girilse bile çalışmalı
        [Fact]
        public async Task Create_ReturnsOk_WhenOnlyPriceIsProvided()
        {
            // 1. Arrange
            var controller = new ListingsController(null!);
            var partialDto = new ListingDtos
            {
                Price = 150000
            };

            // 2. Act
            var result = await controller.Create(partialDto);

            // 3. Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}