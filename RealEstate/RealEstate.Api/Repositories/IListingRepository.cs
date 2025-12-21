using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Api.Entities;

namespace RealEstate.Api.Repositories
{
    public interface IListingRepository
    {
        Task<List<Listing>> GetAllAsync(bool includeInactive, bool isAdmin, string? type);
        Task<List<Listing>> GetByUserIdAsync(int userId);
        Task<Listing?> GetByIdAsync(int id, bool includeOwner = false);
        Task AddAsync(Listing listing);
        Task RemoveAsync(Listing listing);
        Task AddNotificationAsync(Notification notification);
        Task SaveChangesAsync();
    }
}
