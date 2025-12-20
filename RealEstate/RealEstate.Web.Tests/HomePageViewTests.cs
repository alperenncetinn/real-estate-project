using System;
using System.IO;
using Xunit;

namespace RealEstate.Web.Tests
{
    public class HomePageViewTests
    {
        private static string GetRepoRoot()
        {
            // Test çıktısından yukarı doğru çıkarak solution/root'u bulmaya çalışıyoruz.
            // Hedef: "RealEstate.Web" klasörünü içeren kökü bulmak.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null)
            {
                var webDir = Path.Combine(dir.FullName, "RealEstate.Web");
                if (Directory.Exists(webDir))
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException(
                "Repo root bulunamadı. 'RealEstate.Web' klasörü bulunamıyor. " +
                "Test projesi solution içinde mi çalışıyor kontrol et."
            );
        }

        private static string ReadHomeIndexCshtml()
        {
            var root = GetRepoRoot();
            var path = Path.Combine(root, "RealEstate.Web", "Views", "Home", "Index.cshtml");
            Assert.True(File.Exists(path), $"Index.cshtml bulunamadı: {path}");
            return File.ReadAllText(path);
        }

        [Fact]
        public void Home_Index_ShouldContain_Rent_And_Sale_Category_Cards()
        {
            var cshtml = ReadHomeIndexCshtml();

            Assert.Contains("Kiralık Evler", cshtml);
            Assert.Contains("Satılık Evler", cshtml);
        }

        [Fact]
        public void Home_Index_ShouldHave_Correct_Category_Links()
        {
            var cshtml = ReadHomeIndexCshtml();

            // Senin istediğin davranış: anasayfada sadece kategori kartları,
            // tıklayınca Listing sayfasına gitsin.
            Assert.Contains("href=\"/Listing?type=Kiralik\"", cshtml);
            Assert.Contains("href=\"/Listing?type=Satilik\"", cshtml);
        }

        [Fact]
        public void Home_Index_ShouldNotRender_Featured_Listings_Section()
        {
            var cshtml = ReadHomeIndexCshtml();

            // Anasayfada aşağıda ilan listelenmesini istemiyordun.
            // O yüzden view içinde "Model.Any()" / "foreach (var item in Model)" gibi bloklar olmamalı.
            Assert.DoesNotContain("foreach (var item in Model)", cshtml);
            Assert.DoesNotContain("Model.Any()", cshtml);
            Assert.DoesNotContain("FEATURED", cshtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Home_Index_ShouldNotContain_Placeholder_Houses()
        {
            var cshtml = ReadHomeIndexCshtml();

            // Senin ekranda görünen “House 1 / House 2” gibi placeholderlar anasayfada görünmesin istiyordun.
            Assert.DoesNotContain("House 1", cshtml);
            Assert.DoesNotContain("House 2", cshtml);
        }
    }
}
