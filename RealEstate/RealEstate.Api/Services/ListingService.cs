using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
using RealEstate.Api.Repositories;

namespace RealEstate.Api.Services
{
    public enum OperationErrorType
    {
        None,
        NotFound,
        Forbidden,
        Validation
    }

    public record OperationResult<T>(bool Success, T? Data, string? Error, OperationErrorType ErrorType)
    {
        public static OperationResult<T> Ok(T? data = default) => new(true, data, null, OperationErrorType.None);
        public static OperationResult<T> Fail(OperationErrorType type, string? error) => new(false, default, error, type);
    }

    public interface IListingService
    {
        Task<IEnumerable<ListingResponseDto>> GetAllAsync(string? type, bool includeInactive, bool isAdmin);
        Task<IEnumerable<ListingResponseDto>> GetMyListingsAsync(int userId);
        Task<OperationResult<ListingResponseDto>> GetByIdAsync(int id, int? currentUserId, bool isAdmin);
        Task<OperationResult<ListingResponseDto>> CreateAsync(ListingDtos dto, int userId);
        Task<OperationResult<bool>> UpdateAsync(int id, ListingDtos dto, int currentUserId);
        Task<OperationResult<bool>> DeactivateAsync(int id, int adminUserId, string? reason);
        Task<OperationResult<bool>> ActivateAsync(int id, int adminUserId);
        Task<OperationResult<bool>> DeleteAsync(int id, int currentUserId, bool isAdmin);
    }

    public class ListingService : IListingService
    {
        private readonly IListingRepository _repository;
        private readonly IWebHostEnvironment _environment;

        public ListingService(IListingRepository repository, IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        public async Task<IEnumerable<ListingResponseDto>> GetAllAsync(string? type, bool includeInactive, bool isAdmin)
        {
            var listings = await _repository.GetAllAsync(includeInactive, isAdmin, type);
            return listings.Select(MapToResponse).ToList();
        }

        public async Task<IEnumerable<ListingResponseDto>> GetMyListingsAsync(int userId)
        {
            var listings = await _repository.GetByUserIdAsync(userId);
            return listings.Select(MapToResponse).ToList();
        }

        public async Task<OperationResult<ListingResponseDto>> GetByIdAsync(int id, int? currentUserId, bool isAdmin)
        {
            var listing = await _repository.GetByIdAsync(id, includeOwner: true);
            if (listing == null)
            {
                return OperationResult<ListingResponseDto>.Fail(OperationErrorType.NotFound, "Ilan bulunamadi.");
            }

            if (!listing.IsActive && !isAdmin && listing.UserId != currentUserId)
            {
                return OperationResult<ListingResponseDto>.Fail(OperationErrorType.NotFound, "Ilan bulunamadi.");
            }

            return OperationResult<ListingResponseDto>.Ok(MapToResponse(listing));
        }

        public async Task<OperationResult<ListingResponseDto>> CreateAsync(ListingDtos dto, int userId)
        {
            if (dto == null)
            {
                return OperationResult<ListingResponseDto>.Fail(OperationErrorType.Validation, "Veri gelmedi.");
            }

            var listing = new Listing
            {
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                City = dto.City ?? string.Empty,
                District = dto.District,
                Price = dto.Price,
                Type = dto.Type ?? "Satılık",
                CreatedDate = DateTime.UtcNow,
                UserId = userId,
                IsActive = true,
                RoomCount = dto.RoomCount,
                SquareMeters = dto.SquareMeters
            };

            var uploadedUrl = await SavePhotoAsync(dto.Photo);
            listing.ImageUrl = uploadedUrl ?? dto.ImageUrl;

            await _repository.AddAsync(listing);
            await _repository.SaveChangesAsync();

            return OperationResult<ListingResponseDto>.Ok(MapToResponse(listing));
        }

        public async Task<OperationResult<bool>> UpdateAsync(int id, ListingDtos dto, int currentUserId)
        {
            var listing = await _repository.GetByIdAsync(id);
            if (listing == null)
            {
                return OperationResult<bool>.Fail(OperationErrorType.NotFound, "Ilan bulunamadi.");
            }

            if (listing.UserId != currentUserId)
            {
                return OperationResult<bool>.Fail(OperationErrorType.Forbidden, "Yetkiniz yok.");
            }

            if (!listing.IsActive)
            {
                return OperationResult<bool>.Fail(OperationErrorType.Validation, "Pasif ilanlar duzenlenemez.");
            }

            listing.Title = dto.Title ?? listing.Title;
            listing.Description = dto.Description ?? listing.Description;
            listing.City = dto.City ?? listing.City;
            listing.District = dto.District ?? listing.District;
            listing.Price = dto.Price;
            listing.Type = dto.Type ?? listing.Type;
            listing.RoomCount = dto.RoomCount;
            listing.SquareMeters = dto.SquareMeters;

            var uploadedUrl = await SavePhotoAsync(dto.Photo);
            if (uploadedUrl != null)
            {
                listing.ImageUrl = uploadedUrl;
            }

            await _repository.SaveChangesAsync();
            return OperationResult<bool>.Ok(true);
        }

        public async Task<OperationResult<bool>> DeactivateAsync(int id, int adminUserId, string? reason)
        {
            var listing = await _repository.GetByIdAsync(id);
            if (listing == null)
            {
                return OperationResult<bool>.Fail(OperationErrorType.NotFound, "Ilan bulunamadi.");
            }

            if (!listing.IsActive)
            {
                return OperationResult<bool>.Fail(OperationErrorType.Validation, "Zaten pasif.");
            }

            listing.IsActive = false;
            listing.DeactivationReason = reason ?? "Admin tarafindan pasife alindi.";
            listing.DeactivatedAt = DateTime.UtcNow;
            listing.DeactivatedByUserId = adminUserId;

            if (listing.UserId != 0)
            {
                await _repository.AddNotificationAsync(new Notification
                {
                    UserId = listing.UserId,
                    Title = "Ilaniniz Pasife Alindi",
                    Message = $"'{listing.Title}' pasife alindi.",
                    Type = "warning",
                    ListingId = listing.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }

            await _repository.SaveChangesAsync();
            return OperationResult<bool>.Ok(true);
        }

        public async Task<OperationResult<bool>> ActivateAsync(int id, int adminUserId)
        {
            var listing = await _repository.GetByIdAsync(id);
            if (listing == null)
            {
                return OperationResult<bool>.Fail(OperationErrorType.NotFound, "Ilan bulunamadi.");
            }

            if (listing.IsActive)
            {
                return OperationResult<bool>.Fail(OperationErrorType.Validation, "Zaten aktif.");
            }

            listing.IsActive = true;
            listing.DeactivationReason = null;
            listing.DeactivatedAt = null;
            listing.DeactivatedByUserId = null;

            if (listing.UserId != 0)
            {
                await _repository.AddNotificationAsync(new Notification
                {
                    UserId = listing.UserId,
                    Title = "Ilaniniz Aktif",
                    Message = $"'{listing.Title}' tekrar aktif.",
                    Type = "success",
                    ListingId = listing.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }

            await _repository.SaveChangesAsync();
            return OperationResult<bool>.Ok(true);
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id, int currentUserId, bool isAdmin)
        {
            var listing = await _repository.GetByIdAsync(id);
            if (listing == null)
            {
                return OperationResult<bool>.Fail(OperationErrorType.NotFound, "Ilan bulunamadi.");
            }

            if (!isAdmin && listing.UserId != currentUserId)
            {
                return OperationResult<bool>.Fail(OperationErrorType.Forbidden, "Yetkiniz yok.");
            }

            await _repository.RemoveAsync(listing);
            await _repository.SaveChangesAsync();
            return OperationResult<bool>.Ok(true);
        }

        private async Task<string?> SavePhotoAsync(IFormFile? photo)
        {
            if (photo == null || photo.Length == 0)
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            var root = _environment.WebRootPath ?? _environment.ContentRootPath;
            var uploadPath = Path.Combine(root, "uploads");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            return "/uploads/" + fileName;
        }

        private static ListingResponseDto MapToResponse(Listing listing)
        {
            return new ListingResponseDto
            {
                Id = listing.Id,
                Title = listing.Title,
                Price = listing.Price,
                Type = listing.Type,
                City = listing.City,
                District = listing.District,
                RoomCount = listing.RoomCount,
                Description = listing.Description,
                SquareMeters = listing.SquareMeters,
                ImageUrl = listing.ImageUrl,
                CreatedDate = listing.CreatedDate,
                UserId = listing.UserId,
                IsActive = listing.IsActive,
                OwnerName = listing.Owner != null ? listing.Owner.FirstName + " " + listing.Owner.LastName : null,
                OwnerPhone = listing.Owner?.PhoneNumber,
                DeactivationReason = listing.DeactivationReason,
                DeactivatedAt = listing.DeactivatedAt
            };
        }
    }
}
