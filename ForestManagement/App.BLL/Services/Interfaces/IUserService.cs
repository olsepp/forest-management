using App.DTO.Users;

namespace App.BLL.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserListDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserProfileDto?> GetProfileAsync(Guid userId);
    Task<UserDto> CreateAsync(UserCreateDto dto);
    Task<UserDto?> UpdateAsync(Guid id, UserUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
