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
                SquareMeters = dto.SquareMeters,
                Images = new List<Image>(),
                ImageUrl = "" // Artık kullanılmıyor ama null hatası vermesin
            };

            // Resmi Base64 olarak kaydet
            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await dto.Photo.CopyToAsync(memoryStream);
                var base64Data = Convert.ToBase64String(memoryStream.ToArray());
                
                listing.Images.Add(new Image
                {
                    Base64Data = base64Data,
                    FileName = dto.Photo.FileName
                });
            }

            // ImageUrl yedeği (eğer dışarıdan URL gelirse)
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.Photo == null)
            {
                listing.ImageUrl = dto.ImageUrl;
            }

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

            // Fotoğraf güncelleme: Yeni fotoğraf varsa veritabanına ekle
            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await dto.Photo.CopyToAsync(memoryStream);
                var base64Data = Convert.ToBase64String(memoryStream.ToArray());

                // Listing'in Images koleksiyonunu yüklememiz lazım (Repository GetById'de include yoksa null olabilir)
                // Bu yüzden ??= ile initialize ediyoruz ama EF Core tracking için Include edilmesi şart.
                // ListingRepository.GetByIdAsync metodunun 'Include(i => i.Images)' yaptığından emin olmalıyız.
                // Eğer yapmıyorsa, burada explicit loading gerekebilir veya service'de Repository methodu güncellenmeli.
                
                listing.Images ??= new List<Image>();
                listing.Images.Add(new Image
                {
                    Base64Data = base64Data,
                    FileName = dto.Photo.FileName
                });
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

        // SavePhotoAsync metodu artık kullanılmıyor (Base64 veritabanı kaydı kullanılıyor)

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
                DeactivatedAt = listing.DeactivatedAt,
                Images = listing.Images?.Select(i => new ListingImageDto
                {
                    Id = i.Id,
                    Base64Data = i.Base64Data,
                    FileName = i.FileName
                }).ToList()
            };
        }
    }
}
