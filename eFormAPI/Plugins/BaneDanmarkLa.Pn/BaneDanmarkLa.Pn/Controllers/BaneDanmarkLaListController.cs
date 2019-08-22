using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Controllers
{
    [Authorize]
    public class BaneDanmarkLaListController : Controller
    {        
        private readonly IBaneDanmarkLaListService _listService;

        public BaneDanmarkLaListController(IBaneDanmarkLaListService listService)
        {
            _listService = listService;
        }

        [HttpGet]
        [Route("api/bane-danmark-la-pn/lists")]
        public async Task<OperationDataResult<BaneDanmarkLaListsModel>> GetAllLists(BaneDanmarkLaListRequestModel requestModel)
        {
            return await _listService.GetAllLists(requestModel);
        }

        [HttpGet]
        [Route("api/bane-danmark-la-pn/lists/{id}")]
        public async Task<OperationDataResult<BaneDanmarkLaListPnModel>> GetSingleList(int id)
        {
            return await _listService.GetSingleList(id);
        }

        [HttpPost]
        [Route("api/bane-danmark-la-pn/lists")]
        public async Task<OperationResult> CreateList([FromBody] BaneDanmarkLaListPnModel createModel)
        {
            return await _listService.CreateList(createModel);
        }

        [HttpPut]
        [Route("api/bane-danmark-la-pn/lists")]
        public async Task<OperationResult> UpdateList([FromBody] BaneDanmarkLaListPnModel updateModel)
        {
            return await _listService.UpdateList(updateModel);
        }

        [HttpDelete]
        [Route("api/bane-danmark-la-pn/lists/{id}")]
        public async Task<OperationResult> DeleteList(int id)
        {
            return await _listService.DeleteList(id);
        }
    }
}