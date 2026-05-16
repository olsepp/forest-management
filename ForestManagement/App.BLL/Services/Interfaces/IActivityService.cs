using App.DTO.Activity;

namespace App.BLL.Services.Interfaces;

public interface IActivityService
{
    Task<IEnumerable<ActivityListDto>> GetAllAsync();
    Task<ActivityDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ActivityListDto>> GetByForestStandIdAsync(Guid forestStandId);
    Task<IEnumerable<ActivityListDto>> GetByCadasterIdAsync(Guid cadasterId);
    Task<IEnumerable<ActivityDto>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<ActivityDto>> GetByCompanyIdAndUserIdAsync(Guid companyId, Guid userId);
    Task<IEnumerable<ActivityDto>> GetByCompanyIdAndDateRangeAsync(Guid companyId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ActivityDto>> GetByLandPropertyIdAsync(Guid landPropertyId);
    Task<IEnumerable<ActivityDto>> GetByLandPropertyIdAndUserIdAsync(Guid landPropertyId, Guid userId);
    Task<IEnumerable<RecentActivityDto>> GetRecentAsync(int count);
    Task<IEnumerable<RecentActivityDto>> GetRecentByUserIdAsync(Guid userId, int count, Guid? companyId = null);
    Task<ActivityDto> CreateAsync(ActivityCreateDto dto, Guid userId);
    Task<ActivityDto?> UpdateAsync(Guid id, ActivityUpdateDto dto, Guid currentUserId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId, bool isAdmin);
    Task<bool> ExistsAsync(Guid id);
}
