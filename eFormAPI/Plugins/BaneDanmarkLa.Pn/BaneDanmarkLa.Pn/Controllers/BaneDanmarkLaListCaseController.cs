using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Controllers
{
    [Authorize]
    public class BaneDanmarkLaListCaseController : Controller
    {
        private readonly IBaneDanmarkLaListCaseService _listService;

        public BaneDanmarkLaListCaseController(IBaneDanmarkLaListCaseService listService)
        {
            _listService = listService;
        }


        [HttpGet]
        [Route("api/bane-danmark-la-pn/list-cases/")]
        public async Task<OperationDataResult<BaneDanmarkLaListCasePnModel>> GetSingleList(BaneDanmarkLaListCasesPnRequestModel requestModel)
        {
            return await _listService.GetSingleList(requestModel);
        }

        [HttpGet]
        [Route("api/bane-danmark-pn/list-case-results")]
        public async Task<OperationDataResult<BaneDanmarkLaListPnCaseResultListModel>> GetSingleListResults(BaneDanmarkLaListCasesPnRequestModel requestModel)
        {
            return await _listService.GetSingleListResults(requestModel);
        }
        
    }
}