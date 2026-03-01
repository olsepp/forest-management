using App.DTO.LandProperty;

namespace App.BLL.Services.Interfaces;

public interface ILandPropertyService
{
    Task<IEnumerable<LandPropertyListDto>> GetAllAsync();
    Task<LandPropertyDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LandPropertyListDto>> SearchAsync(LandPropertySearchParams searchParams);
    Task<LandPropertyDto> CreateAsync(LandPropertyCreateDto dto);
    Task<LandPropertyDto?> UpdateAsync(Guid id, LandPropertyUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
