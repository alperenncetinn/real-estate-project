using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Entities;

namespace RealEstate.Api.Repositories
{
    public class ListingRepository : IListingRepository
    {
        private readonly ApplicationDbContext _context;

        public ListingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Listing>> GetAllAsync(bool includeInactive, bool isAdmin, string? type)
        {
            var query = _context.Listings.Include(l => l.Owner).Include(l => l.Images).AsQueryable();

            if (!isAdmin || !includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(x => x.Type == type);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Listing>> GetByUserIdAsync(int userId)
        {
            return await _context.Listings
                .Include(l => l.Images)
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<Listing?> GetByIdAsync(int id, bool includeOwner = false)
        {
            var query = _context.Listings.Include(l => l.Images).AsQueryable();

            if (includeOwner)
            {
                query = query.Include(l => l.Owner);
            }

            return await query.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddAsync(Listing listing)
        {
            await _context.Listings.AddAsync(listing);
        }

        public Task RemoveAsync(Listing listing)
        {
            _context.Listings.Remove(listing);
            return Task.CompletedTask;
        }

        public Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
