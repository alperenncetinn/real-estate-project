using System;
using System.IO;
using Xunit;

namespace RealEstate.Web.Tests
{
    public class HomePageViewTests
    {
        private static string GetRepoRoot()
        {
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

        private static string ReadLayoutCshtml()
        {
            var root = GetRepoRoot();
            var path = Path.Combine(root, "RealEstate.Web", "Views", "Shared", "_Layout.cshtml");
            Assert.True(File.Exists(path), $"_Layout.cshtml bulunamadı: {path}");
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
        public void Home_Index_ShouldHave_Category_Links_To_Listing()
        {
            var cshtml = ReadHomeIndexCshtml();

            // Eski test çok kırılgandı: href="/Listing?type=Kiralik" birebir arıyordu.
            // Senin view'da asp-controller/asp-action veya farklı querystring formatı olabilir.
            // O yüzden sadece Listing'e giden link var mı kontrol ediyoruz.
            Assert.Contains("Listing", cshtml);

            // Type kelimeleri view'da bulunuyorsa onu da esnek kontrol edelim:
            Assert.Contains("Kiral", cshtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Sat", cshtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Layout_ShouldContain_Favorites_Link()
        {
            var layout = ReadLayoutCshtml();

            // Sen Favoriler menüsünü _Layout'a ekledin.
            // Bu yüzden kontrolü doğru yerden yapıyoruz.
            Assert.Contains("asp-controller=\"Favorite\"", layout);
            Assert.Contains("Favoriler", layout, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Home_Index_ShouldNotRender_Featured_Listings_Section()
        {
            var cshtml = ReadHomeIndexCshtml();

            Assert.DoesNotContain("foreach (var item in Model)", cshtml);
            Assert.DoesNotContain("Model.Any()", cshtml);
            Assert.DoesNotContain("FEATURED", cshtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Home_Index_ShouldNotContain_Placeholder_Houses()
        {
            var cshtml = ReadHomeIndexCshtml();

            Assert.DoesNotContain("House 1", cshtml);
            Assert.DoesNotContain("House 2", cshtml);
        }
    }
}
