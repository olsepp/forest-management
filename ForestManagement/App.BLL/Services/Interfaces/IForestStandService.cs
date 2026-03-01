using App.DTO.ForestStand;

namespace App.BLL.Services.Interfaces;

public interface IForestStandService
{
    Task<IEnumerable<ForestStandListDto>> GetAllAsync();
    Task<ForestStandDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ForestStandListDto>> GetByCadasterIdAsync(Guid cadasterId);
    Task<IEnumerable<ForestStandListDto>> GetActiveAsync();
    Task<ForestStandDto> CreateAsync(ForestStandCreateDto dto);
    Task<ForestStandDto?> UpdateAsync(Guid id, ForestStandUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
