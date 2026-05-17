namespace App.BLL.Services.Interfaces;

public interface IActivityExportService
{
    /// <summary>
    /// Export activities for a company to Excel format, optionally filtered by date range, activity type, and user
    /// </summary>
    /// <param name="companyId">Company ID to filter activities</param>
    /// <param name="startDate">Optional start date of the range</param>
    /// <param name="endDate">Optional end date of the range</param>
    /// <param name="activityTypeId">Optional activity type filter</param>
    /// <param name="userId">Optional user filter</param>
    /// <returns>Excel file as byte array</returns>
    Task<byte[]> ExportActivitiesToExcelAsync(
        Guid companyId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? activityTypeId = null,
        Guid? userId = null);
}
