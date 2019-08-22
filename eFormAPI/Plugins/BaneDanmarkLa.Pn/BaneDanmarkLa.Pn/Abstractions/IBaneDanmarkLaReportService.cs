using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Infrastructure.Models.Report;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IBaneDanmarkLaReportService
    {
        Task<OperationDataResult<ReportModel>> GenerateReport(GenerateReportModel model);
        Task<OperationDataResult<FileStreamModel>> GenerateReportFile(GenerateReportModel model);
    }
}