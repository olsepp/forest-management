using App.DTO.Cadaster;

namespace App.BLL.Services.Interfaces;

public interface ICadasterService
{
    Task<IEnumerable<CadasterListDto>> GetAllAsync();
    Task<CadasterDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CadasterListDto>> GetByLandPropertyIdAsync(Guid landPropertyId);
    Task<CadasterDto> CreateAsync(CadasterCreateDto dto);
    Task<CadasterDto?> UpdateAsync(Guid id, CadasterUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
