namespace RealEstate.Web.Models;

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string City { get; set; } = null!;
    public string District { get; set; } = null!;
    public decimal Price { get; set; }
    public int RoomCount { get; set; }
    public int Area { get; set; }
    public string ImageUrl { get; set; } = null!;
}