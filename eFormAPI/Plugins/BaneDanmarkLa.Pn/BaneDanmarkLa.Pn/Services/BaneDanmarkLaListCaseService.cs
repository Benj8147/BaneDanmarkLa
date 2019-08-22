using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure.Constants;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Extensions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Microting.BaneDanmarkLaBase.Infrastructure.Data;
using Microting.BaneDanmarkLaBase.Infrastructure.Data.Entities;

namespace BaneDanmarkLa.Pn.Services
{
    public class BaneDanmarkLaListCaseService :IBaneDanmarkLaListCaseService
    {
        private readonly BaneDanmarkLaPnDbContext _dbContext;
        private readonly IBaneDanmarkLaLocalizationService _baneDanmarkLaPlanningLocalizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEFormCoreService _core;

        public BaneDanmarkLaListCaseService(
            BaneDanmarkLaPnDbContext dbContext,
            IBaneDanmarkLaLocalizationService baneDanmarkLaLocalizationService,
            IHttpContextAccessor httpContextAccessor, IEFormCoreService core)
        {
            _dbContext = dbContext;
            _baneDanmarkLaLocalizationService = baneDanmarkLaLocalizationService;
            _httpContextAccessor = httpContextAccessor;
            _core = core;
        }

        public async Task<OperationDataResult<BaneDanmarkLaListCasePnModel>> GetSingleList(BaneDanmarkLaListCasesPnRequestModel requestModel)
        {
            try
            {

                var newItems = (_dbContext.Items.Where(item => item.ItemListId == requestModel.ListId)
                    .Join(_dbContext.ItemCases, item => item.Id, itemCase => itemCase.ItemId,
                        (item, itemCase) => new
                        {
                            itemCase.Id,
                            item.Name,
                            item.Description,
                            item.Type,
                            item.BuildYear,
                            item.ItemNumber,
                            itemCase.Comment,
                            itemCase.Location,
                            itemCase.FieldStatus,
                            itemCase.NumberOfImages,
                            itemCase.WorkflowState,
                            itemCase.CreatedAt,
                            itemCase.MicrotingSdkCaseId,
                            itemCase.MicrotingSdkeFormId,
                            itemCase.Status
                        }));
                
                if (!string.IsNullOrEmpty(requestModel.Sort))
                {
                    if (requestModel.IsSortDsc)
                    {
                        newItems = newItems
                            .CustomOrderByDescending(requestModel.Sort);
                    }
                    else
                    {
                        newItems = newItems
                            .CustomOrderBy(requestModel.Sort);
                    }
                }
                else
                {
                    newItems = newItems
                        .OrderBy(x => x.Id);
                }


                newItems
                    = newItems
                        .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                        .Skip(requestModel.Offset)
                        .Take(requestModel.PageSize);
                if (newItems.Any())
                {
                    
                    BaneDanmarkLaListCasePnModel baneDanmarkLaListCasePnModel = new BaneDanmarkLaListCasePnModel();
                    baneDanmarkLaListCasePnModel.Items = await newItems.Select(x => new BaneDanmarkLaListPnBaneDanmarkLaCaseModel()
                    {
                        Id = x.Id,
                        Date = x.CreatedAt,
                        Name = x.Name,
                        ItemNumber = x.ItemNumber,
                        BuildYear = x.BuildYear,
                        Description = x.Description,
                        Type = x.Type,
                        Comment = x.Comment,
                        Location = x.Location,
                        FieldStatus = x.FieldStatus,
                        NumberOfImages = x.NumberOfImages,
                        SdkCaseId = x.MicrotingSdkCaseId,
                        SdkeFormId = x.MicrotingSdkeFormId,
                        Status = x.Status
                    }).ToListAsync();
                    
                    baneDanmarkLaListCasePnModel.Total = await (_dbContext.Items.Where(item => item.ItemListId == requestModel.ListId)
                        .Join(_dbContext.ItemCases, item => item.Id, itemCase => itemCase.ItemId,
                            (item, itemCase) => new
                            {
                                itemCase.Id
                            })).CountAsync();

                    return new OperationDataResult<BaneDanmarkLaListCasePnModel>(
                        true,
                        baneDanmarkLaListCasePnModel);
                }
                else
                {
                    return new OperationDataResult<BaneDanmarkLaListCasePnModel>(
                        false, "");
                }
            }
            catch (Exception ex)
            {
                return new OperationDataResult<BaneDanmarkLaListCasePnModel>(
                    false, ex.Message);
            }
        }

        public async Task<OperationDataResult<BaneDanmarkLaListPnCaseResultListModel>> GetSingleListResults(BaneDanmarkLaListCasesPnRequestModel requestModel)
        {
            BaneDanmarkLaListPnCaseResultListModel baneDanmarkLaListPnCaseResultListModel = new BaneDanmarkLaListPnCaseResultListModel();
            baneDanmarkLaListPnCaseResultListModel.Total = 0;
            baneDanmarkLaListPnCaseResultListModel.FieldEnabled1 = true;
            
            return new OperationDataResult<BaneDanmarkLaListPnCaseResultListModel>(true, baneDanmarkLaListPnCaseResultListModel);
        }
    }
}