using FluentAssertions;
using RealEstate.Api.Tests.Helpers;
using Xunit;

namespace RealEstate.Api.Tests.HelpersTests
{
    public class TestDbContextFactoryTests
    {
        [Fact]
        public void CreateContextWithData_SeedsListingsAndProperties()
        {
            using var context = TestDbContextFactory.CreateContextWithData();

            context.Properties.Should().NotBeEmpty();
            context.Listings.Should().NotBeEmpty();
        }
    }
}
