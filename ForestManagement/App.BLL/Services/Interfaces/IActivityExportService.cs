namespace App.BLL.Services.Interfaces;

public interface IActivityExportService
{
    /// <summary>
    /// Export activities for a company within a date range to Excel format
    /// </summary>
    /// <param name="companyId">Company ID to filter activities</param>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <returns>Excel file as byte array</returns>
    Task<byte[]> ExportActivitiesToExcelAsync(Guid companyId, DateTime startDate, DateTime endDate);
}
