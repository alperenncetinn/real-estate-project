using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstate.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixFavoritesToUseListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Properties_PropertyId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Favorites",
                newName: "ListingId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_UserId_PropertyId",
                table: "Favorites",
                newName: "IX_Favorites_UserId_ListingId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_PropertyId",
                table: "Favorites",
                newName: "IX_Favorites_ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Listings_ListingId",
                table: "Favorites",
                column: "ListingId",
                principalTable: "Listings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Listings_ListingId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "ListingId",
                table: "Favorites",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_UserId_ListingId",
                table: "Favorites",
                newName: "IX_Favorites_UserId_PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_ListingId",
                table: "Favorites",
                newName: "IX_Favorites_PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Properties_PropertyId",
                table: "Favorites",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
