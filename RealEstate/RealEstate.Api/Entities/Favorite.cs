using System.ComponentModel.DataAnnotations;
using RealEstate.Api.Models;

namespace RealEstate.Api.Entities;


public class Favorite
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ListingId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Listing? Listing { get; set; }
}

