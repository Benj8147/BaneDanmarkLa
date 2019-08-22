using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IBaneDanmarkLaListCaseService
    {
        Task<OperationDataResult<BaneDanmarkLaListCasePnModel>> GetSingleList(BaneDanmarkLaListCasesPnRequestModel requestModel);

        Task<OperationDataResult<BaneDanmarkLaListPnCaseResultListModel>> GetSingleListResults(
            BaneDanmarkLaListCasesPnRequestModel requestModel);
    }
}