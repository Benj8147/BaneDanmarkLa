using BaneDanmarkLa.Pn.Infrastructure.Models.Report;
namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IExcelService
    {
        bool WriteRecordsExportModelsToExcelFile(
            ReportModel reportModel,
            GenerateReportModel generateReportModel,
            string destFile);

        string CopyTemplateForNewAccount(string templateName);
    }
}