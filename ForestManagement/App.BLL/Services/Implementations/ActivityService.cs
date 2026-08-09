using App.BLL.Services.Interfaces;
using App.Contracts.Enums;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO;
using App.DTO.Activity;

namespace App.BLL.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserService _userService;

    public ActivityService(IUnitOfWork uow, IUserService userService)
    {
        _uow = uow;
        _userService = userService;
    }

    public async Task<IEnumerable<ActivityListDto>> GetAllAsync()
    {
        var activities = await _uow.Activities.GetAllWithDetailsAsync();
        return activities.Select(MapToListDto);
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id)
    {
        var activity = await _uow.Activities.GetWithDetailsAsync(id);
        return activity == null ? null : MapToDto(activity);
    }

    public async Task<IEnumerable<ActivityListDto>> GetByForestStandIdAsync(Guid forestStandId)
    {
        var activities = await _uow.Activities.GetByForestStandIdAsync(forestStandId);
        return activities.Select(MapToListDto);
    }

    public async Task<IEnumerable<ActivityListDto>> GetByCadasterIdAsync(Guid cadasterId)
    {
        var activities = await _uow.Activities.GetByCadasterIdAsync(cadasterId);
        return activities.Select(MapToListDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCompanyIdAsync(Guid companyId)
    {
        var activities = await _uow.Activities.GetByCompanyIdAsync(companyId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCompanyIdAndUserIdAsync(Guid companyId, Guid userId)
    {
        var activities = await _uow.Activities.GetByCompanyIdAndUserIdAsync(companyId, userId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCompanyFilteredAsync(
        Guid companyId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? activityTypeId = null,
        Guid? userId = null)
    {
        var activities = await _uow.Activities.GetByCompanyFilteredAsync(
            companyId, startDate, endDate, activityTypeId, userId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLandPropertyIdAsync(Guid landPropertyId)
    {
        var activities = await _uow.Activities.GetByLandPropertyIdAsync(landPropertyId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLandPropertyIdAndUserIdAsync(Guid landPropertyId, Guid userId)
    {
        var activities = await _uow.Activities.GetByLandPropertyIdAndUserIdAsync(landPropertyId, userId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentAsync(int count)
    {
        var activities = await _uow.Activities.GetRecentAsync(count);
        return activities.Select(MapToRecentDto);
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentByUserIdAsync(Guid userId, int count, Guid? companyId = null)
    {
        var activities = await _uow.Activities.GetRecentByUserIdAsync(userId, count, companyId);
        return activities.Select(MapToRecentDto);
    }

    public async Task<ActivityDto?> CreateAsync(ActivityCreateDto dto, Guid userId, bool isAdmin)
    {
        // Validate ApplicationStatus if provided
        if (dto.ApplicationStatus.HasValue)
        {
            if (!Enum.TryParse<EApplicationStatus>(dto.ApplicationStatus.Value.ToString(), out var status))
            {
                throw new ArgumentException($"Invalid application status '{dto.ApplicationStatus}'. Valid values: {string.Join(", ", Enum.GetNames<EApplicationStatus>())}");
            }

            if (!Enum.IsDefined(typeof(EApplicationStatus), status))
            {
                throw new ArgumentException($"Invalid application status '{dto.ApplicationStatus}'. Valid values: {string.Join(", ", Enum.GetNames<EApplicationStatus>())}");
            }
        }

        // Admins may set UserId to log the activity on behalf of another user.
        // Non-admins have any supplied UserId ignored (falls back to the JWT user).
        var targetUserId = isAdmin && dto.UserId.HasValue ? dto.UserId.Value : userId;
        if (targetUserId != userId)
        {
            var targetUser = await _userService.GetByIdAsync(targetUserId);
            if (targetUser == null) return null;
        }

        var entity = new Activity
        {
            Description = dto.Description,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Notes = dto.Notes,
            Date = dto.Date,
            ActivityTypeId = dto.ActivityTypeId,
            UserId = targetUserId,
            ForestStandId = dto.ForestStandId,
            CadasterId = dto.CadasterId,
            ApplicationStatus = dto.ApplicationStatus
        };
        await _uow.Activities.AddAsync(entity);
        await _uow.SaveChangesAsync();
        // Reload with details for response
        var created = await _uow.Activities.GetWithDetailsAsync(entity.Id);
        return MapToDto(created!);
    }

    public async Task<ActivityDto?> UpdateAsync(Guid id, ActivityUpdateDto dto, Guid currentUserId, bool isAdmin)
    {
        var entity = await _uow.Activities.GetByIdAsync(id);
        if (entity == null) return null;

        if (!isAdmin && entity.UserId != currentUserId)
            return null;

        // Admins may reassign the activity to another user. Non-admins have any
        // supplied UserId ignored (the current assignment is kept).
        if (isAdmin && dto.UserId.HasValue && dto.UserId.Value != entity.UserId)
        {
            var targetUser = await _userService.GetByIdAsync(dto.UserId.Value);
            if (targetUser == null) return null;
            entity.UserId = dto.UserId.Value;
        }

        // Validate ApplicationStatus if provided
        if (dto.ApplicationStatus.HasValue)
        {
            if (!Enum.TryParse<EApplicationStatus>(dto.ApplicationStatus.Value.ToString(), out var status))
            {
                throw new ArgumentException($"Invalid application status '{dto.ApplicationStatus}'. Valid values: {string.Join(", ", Enum.GetNames<EApplicationStatus>())}");
            }

            if (!Enum.IsDefined(typeof(EApplicationStatus), status))
            {
                throw new ArgumentException($"Invalid application status '{dto.ApplicationStatus}'. Valid values: {string.Join(", ", Enum.GetNames<EApplicationStatus>())}");
            }
        }

        entity.Description = dto.Description;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.Notes = dto.Notes;
        entity.Date = dto.Date;
        entity.ActivityTypeId = dto.ActivityTypeId;
        entity.ForestStandId = dto.ForestStandId;
        entity.CadasterId = dto.CadasterId;
        entity.ApplicationStatus = dto.ApplicationStatus;

        await _uow.Activities.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        // Reload with details for response
        var updated = await _uow.Activities.GetWithDetailsAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var entity = await _uow.Activities.GetByIdAsync(id);
        if (entity == null) return false;

        if (!isAdmin && entity.UserId != currentUserId)
            return false;

        await _uow.Activities.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.Activities.ExistsAsync(id);

    public async Task<PagedResult<ActivityDto>> GetByCompanyPagedAsync(Guid companyId, int skip, int take)
    {
        var (items, total) = await _uow.Activities.GetByCompanyIdPagedAsync(companyId, skip, take);
        return new PagedResult<ActivityDto>
        {
            Items = items.Select(MapToDto),
            Total = total
        };
    }

    public async Task<PagedResult<ActivityDto>> GetByCompanyAndUserPagedAsync(Guid companyId, Guid userId, int skip, int take)
    {
        var (items, total) = await _uow.Activities.GetByCompanyIdAndUserIdPagedAsync(companyId, userId, skip, take);
        return new PagedResult<ActivityDto>
        {
            Items = items.Select(MapToDto),
            Total = total
        };
    }

    public async Task<PagedResult<ActivityDto>> GetByCompanyFilteredPagedAsync(
        Guid companyId,
        int skip,
        int take,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? activityTypeId = null,
        Guid? userId = null)
    {
        var (items, total) = await _uow.Activities.GetByCompanyFilteredPagedAsync(
            companyId, skip, take, startDate, endDate, activityTypeId, userId);
        return new PagedResult<ActivityDto>
        {
            Items = items.Select(MapToDto),
            Total = total
        };
    }

    // --- Mapping ---

    private static ActivityDto MapToDto(Activity a) => new()
    {
        Id = a.Id,
        Description = a.Description,
        Quantity = a.Quantity,
        Unit = a.Unit,
        Notes = a.Notes,
        Date = a.Date,
        ActivityTypeId = a.ActivityTypeId,
        UserId = a.UserId,
        UserName = a.User?.UserName ?? string.Empty,
        UserFirstName = a.User?.FirstName,
        UserLastName = a.User?.LastName,
        ForestStandId = a.ForestStandId,
        ForestStandNumber = a.ForestStand?.Number,
        CadasterId = a.CadasterId,
        CadasterCadastralNumber = a.Cadaster?.CadastralNumber ?? a.ForestStand?.Cadaster?.CadastralNumber,
        LandPropertyId = a.Cadaster?.LandPropertyId ?? a.ForestStand?.Cadaster?.LandPropertyId,
        LandPropertyName = a.Cadaster?.LandProperty?.Name ?? a.ForestStand?.Cadaster?.LandProperty?.Name,
        ApplicationStatus = a.ApplicationStatus,
        ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty
    };

    private static ActivityListDto MapToListDto(Activity a) => new()
    {
        Id = a.Id,
        Description = a.Description,
        Quantity = a.Quantity,
        Unit = a.Unit,
        Date = a.Date,
        ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
        UserName = a.User?.UserName ?? string.Empty,
        UserFirstName = a.User?.FirstName,
        UserLastName = a.User?.LastName,
        ForestStandNumber = a.ForestStand?.Number,
        CadasterCadastralNumber = a.Cadaster?.CadastralNumber ?? a.ForestStand?.Cadaster?.CadastralNumber,
        ApplicationStatus = a.ApplicationStatus
    };

    private static RecentActivityDto MapToRecentDto(Activity a) => new()
    {
        Id = a.Id,
        Description = a.Description,
        Quantity = a.Quantity,
        Unit = a.Unit,
        Date = a.Date,
        ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
        UserName = a.User?.UserName ?? string.Empty,
        UserFirstName = a.User?.FirstName,
        UserLastName = a.User?.LastName,

        // IDs
        CadasterId = a.CadasterId ?? a.ForestStand?.CadasterId,
        ForestStandId = a.ForestStandId,

        // If activity on ForestStand → return ForestStandNumber + CadasterCadastralNumber (from ForestStand.Cadaster)
        // If activity on Cadaster directly → return CadasterCadastralNumber (from Cadaster)
        ForestStandNumber = a.ForestStandId.HasValue ? a.ForestStand?.Number : null,
        CadasterCadastralNumber = a.CadasterId.HasValue
            ? a.Cadaster?.CadastralNumber
            : a.ForestStand?.Cadaster?.CadastralNumber
    };
}
