using App.DTO.Activity;

namespace App.BLL.Services.Interfaces;

public interface IActivityService
{
    Task<IEnumerable<ActivityListDto>> GetAllAsync();
    Task<ActivityDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ActivityListDto>> GetByForestStandIdAsync(Guid forestStandId);
    Task<IEnumerable<ActivityListDto>> GetByCadasterIdAsync(Guid cadasterId);
    Task<IEnumerable<RecentActivityDto>> GetRecentAsync(int count);
    Task<ActivityDto> CreateAsync(ActivityCreateDto dto, Guid userId);
    Task<ActivityDto?> UpdateAsync(Guid id, ActivityUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
