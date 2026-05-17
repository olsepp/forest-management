using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using ClosedXML.Excel;

namespace App.BLL.Services.Implementations;

public class ActivityExportService : IActivityExportService
{
    private readonly IUnitOfWork _uow;

    public ActivityExportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<byte[]> ExportActivitiesToExcelAsync(
        Guid companyId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? activityTypeId = null,
        Guid? userId = null)
    {
        // Reuse the same repository method that the filtered endpoint uses
        var activities = await _uow.Activities.GetByCompanyFilteredAsync(companyId, startDate, endDate, activityTypeId, userId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Activities");

        // Estonian headers
        var headers = new[]
        {
            "Kuupäev",
            "Töö liik",
            "Kirjeldus",
            "Kogus",
            "Ühik",
            "Kasutaja",
            "Eraldis",
            "Kataster",
            "Taotluse staatus"
        };

        // Add headers
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Add data rows
        int row = 2;
        foreach (var activity in activities)
        {
            worksheet.Cell(row, 1).Value = activity.Date;
            worksheet.Cell(row, 2).Value = activity.ActivityType?.ActivityTypeName ?? string.Empty;
            worksheet.Cell(row, 3).Value = activity.Description ?? string.Empty;
            worksheet.Cell(row, 4).Value = (double)activity.Quantity;
            worksheet.Cell(row, 5).Value = activity.Unit ?? string.Empty;
            worksheet.Cell(row, 6).Value = activity.User?.FirstName + " " + activity.User?.LastName ?? string.Empty;
            worksheet.Cell(row, 7).Value = activity.ForestStand != null ? activity.ForestStand.Number.ToString() : string.Empty;
            worksheet.Cell(row, 8).Value = activity.Cadaster?.CadastralNumber 
                ?? activity.ForestStand?.Cadaster?.CadastralNumber 
                ?? string.Empty;
            worksheet.Cell(row, 9).Value = activity.ApplicationStatus?.ToString() ?? string.Empty;

            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Generate Excel file
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
