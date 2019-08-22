using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IBaneDanmarkLaListService
    {
        Task<OperationResult> CreateList(BaneDanmarkLaListPnModel model);
        Task<OperationResult> DeleteList(int id);
        Task<OperationResult> UpdateList(BaneDanmarkLaListPnModel updateModel);
        Task<OperationDataResult<BaneDanmarkLaListsModel>> GetAllLists(BaneDanmarkLaListRequestModel requestModel);
        Task<OperationDataResult<BaneDanmarkLaListPnModel>> GetSingleList(int baneDanmarkLaListId);
        
    }
}