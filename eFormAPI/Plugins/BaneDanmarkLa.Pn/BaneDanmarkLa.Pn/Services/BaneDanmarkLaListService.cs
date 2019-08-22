using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microting.eFormApi.BasePn.Infrastructure.Extensions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Microting.BaneDanmarkLaBase.Infrastructure.Data.Entities;
using Microting.BaneDanmarkLaBase.Infrastructure.Data;
using Microting.eForm.Infrastructure.Constants;
using Microting.eFormApi.BasePn.Abstractions;

namespace BaneDanmarkLa.Pn.Services
{
    using System.Security.Claims;
    using Infrastructure.Models;
    using Microsoft.AspNetCore.Http;

    public class BaneDanmarkLaListService : IBaneDanmarkLaListService
    {
        private readonly BaneDanmarkLaPnDbContext _dbContext;
        private readonly IBaneDanmarkLaLocalizationService _baneDanmarkLaPlanningLocalizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEFormCoreService _core;

        public BaneDanmarkLaListService(
            BaneDanmarkLaPnDbContext dbContext,
            IBaneDanmarkLaLocalizationService baneDanmarkLaPlanningLocalizationService,
            IHttpContextAccessor httpContextAccessor, IEFormCoreService core)
        {
            _dbContext = dbContext;
            _baneDanmarkLaPlanningLocalizationService = baneDanmarkLaPlanningLocalizationService;
            _httpContextAccessor = httpContextAccessor;
            _core = core;
        }

        public async Task<OperationDataResult<BaneDanmarkLaListsModel>> GetAllLists(BaneDanmarkLaListRequestModel pnRequestModel)
        {
            try
            {
                BaneDanmarkLaListsModel listsModel = new BaneDanmarkLaListsModel();

                IQueryable<ItemList> itemListsQuery = _dbContext.ItemLists.AsQueryable();
                if (!string.IsNullOrEmpty(pnRequestModel.Sort))
                {
                    if (pnRequestModel.IsSortDsc)
                    {
                        itemListsQuery = itemListsQuery
                            .CustomOrderByDescending(pnRequestModel.Sort);
                    }
                    else
                    {
                        itemListsQuery = itemListsQuery
                            .CustomOrderBy(pnRequestModel.Sort);
                    }
                }
                else
                {
                    itemListsQuery = _dbContext.ItemLists
                        .OrderBy(x => x.Id);
                }

                if (!string.IsNullOrEmpty(pnRequestModel.NameFilter))
                {
                    itemListsQuery = itemListsQuery.Where(x => x.Name.Contains(pnRequestModel.NameFilter));
                }

                itemListsQuery
                    = itemListsQuery
                        .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                        .Skip(pnRequestModel.Offset)
                        .Take(pnRequestModel.PageSize);

                List<BaneDanmarkLaListPnModel> lists = await itemListsQuery.Select(x => new BaneDanmarkLaListPnModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    RepeatEvery = x.RepeatEvery,
                    RepeatType = x.RepeatType,
                    RepeatUntil = x.RepeatUntil,
                    DayOfWeek = x.DayOfWeek,
                    DayOfMonth = x.DayOfMonth,
                    RelatedEFormId = x.RelatedEFormId,
                    RelatedEFormName = x.RelatedEFormName,
                }).ToListAsync();

                listsModel.Total = await _dbContext.ItemLists.CountAsync(x =>
                    x.WorkflowState != Constants.WorkflowStates.Removed);
                listsModel.Lists = lists;

                return new OperationDataResult<BaneDanmarkLaListsModel>(true, listsModel);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<BaneDanmarkLaListsModel>(false,
                    _baneDanmarkLaLocalizationService.GetString("ErrorObtainingLists"));
            }
        }

        public async Task<OperationResult> CreateList(BaneDanmarkLaListPnModel model)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var template = _core.GetCore().TemplateItemRead(model.RelatedEFormId);
                    var itemsList = new ItemList
                    {
                        Name = model.Name,
                        Description = model.Description,
                        CreatedByUserId = UserId,
                        CreatedAt = DateTime.UtcNow,
                        RepeatEvery = model.RepeatEvery,
                        RepeatUntil = model.RepeatUntil,
                        RepeatType = model.RepeatType,
                        DayOfWeek = model.DayOfWeek,
                        DayOfMonth = model.DayOfMonth,
                        Enabled = true,
                        Items = new List<Item>(),
                        RelatedEFormId = model.RelatedEFormId,
                        RelatedEFormName = template?.Label
                    };

                    await itemsList.Create(_dbContext);

                    foreach (var itemModel in model.Items)
                    {
                        var item = new Item()
                        {
                            LocationCode = itemModel.LocationCode,
                            ItemNumber = itemModel.ItemNumber,
                            Description = itemModel.Description,
                            Name = itemModel.Name,
                            Version = 1,
                            WorkflowState = Constants.WorkflowStates.Created,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Enabled = true,
                            BuildYear = itemModel.BuildYear,
                            Type = itemModel.Type,
                            ItemListId = itemsList.Id,
                            CreatedByUserId = UserId,
                        };
                        await item.Save(_dbContext);
                    }

                    transaction.Commit();
                    return new OperationResult(
                        true,
                        _baneDanmarkLaPlanningLocalizationService.GetString("ListCreatedSuccessfully"));
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Trace.TraceError(e.Message);
                    return new OperationResult(false,
                        _baneDanmarkLaPlanningLocalizationService.GetString("ErrorWhileCreatingList"));
                }
            }
        }

        public async Task<OperationResult> UpdateList(BaneDanmarkLaListPnModel updateModel)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var template = _core.GetCore().TemplateItemRead(updateModel.RelatedEFormId);
                    var itemsList = new ItemList
                    {
                        Id = updateModel.Id,
                        RepeatUntil = updateModel.RepeatUntil,
                        RepeatEvery = updateModel.RepeatEvery,
                        RepeatType = updateModel.RepeatType,
                        DayOfWeek = updateModel.DayOfWeek,
                        DayOfMonth = updateModel.DayOfMonth,
                        Description = updateModel.Description,
                        Name = updateModel.Name,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedByUserId = UserId,
                        RelatedEFormId = updateModel.RelatedEFormId,
                        RelatedEFormName = template?.Label,
                    };
                    await itemsList.Update(_dbContext);

                    // update current items
                    var items = await _dbContext.Items
                        .Where(x => x.ItemListId == itemsList.Id)
                        .ToListAsync();

                    foreach (var item in items)
                    {
                        var itemModel = updateModel.Items.FirstOrDefault(x => x.Id == item.Id);
                        if (itemModel != null)
                        {
                            item.Description = itemModel.Description;
                            item.ItemNumber = itemModel.ItemNumber;
                            item.LocationCode = itemModel.LocationCode;
                            item.Name = itemModel.Name;
                            item.UpdatedAt = DateTime.UtcNow;
                            item.UpdatedByUserId = UserId;
                            item.BuildYear = itemModel.BuildYear;
                            item.Type = itemModel.Type;
                            await item.Update(_dbContext);
                        }
                    }

                    // Remove old
                    var itemModelIds = updateModel.Items.Select(x => x.Id).ToArray();
                    var itemsForRemove = await _dbContext.Items
                        .Where(x => !itemModelIds.Contains(x.Id) && x.ItemListId == itemsList.Id)
                        .ToListAsync();

                    foreach (var itemForRemove in itemsForRemove)
                    {
                        await itemForRemove.Delete(_dbContext);
                    }

                    // Create new
                    foreach (var itemModel in updateModel.Items)
                    {
                        var item = items.FirstOrDefault(x => x.Id == itemModel.Id);
                        if (item == null)
                        {
                            var newItem = new Item()
                            {
                                LocationCode = itemModel.LocationCode,
                                ItemNumber = itemModel.ItemNumber,
                                Description = itemModel.Description,
                                Name = itemModel.Name,
                                Version = 1,
                                WorkflowState = Constants.WorkflowStates.Created,
                                CreatedAt = DateTime.UtcNow,
                                CreatedByUserId = UserId,
                                UpdatedAt = DateTime.UtcNow,
                                Enabled = true,
                                BuildYear = itemModel.BuildYear,
                                Type = itemModel.Type,
                                ItemListId = itemsList.Id,
                            };
                            await newItem.Save(_dbContext);
                        }
                    }

                    transaction.Commit();
                    return new OperationResult(
                        true,
                        _baneDanmarkLaPlanningLocalizationService.GetString("ListUpdatedSuccessfully"));
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message);
                    transaction.Rollback();
                    return new OperationResult(
                        false,
                        _baneDanmarkLaLocalizationService.GetString("ErrorWhileUpdatingList"));
                }
            }
        }

        public async Task<OperationResult> DeleteList(int id)
        {
            try
            {
                Debugger.Break();
                var itemsList = new ItemList
                {
                    Id = id
                };
                await itemsList.Delete(_dbContext);

                return new OperationResult(
                    true,
                    _baneDanmarkLaPlanningLocalizationService.GetString("ListDeletedSuccessfully"));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationResult(
                    false,
                    _baneDanmarkLaPlanningLocalizationService.GetString("ErrorWhileRemovingList"));
            }
        }

        public async Task<OperationDataResult<BaneDanmarkLaListPnModel>> GetSingleList(int listId)
        {
            try
            {
                var itemList = await _dbContext.ItemLists
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed && x.Id == listId)
                    .Select(x => new BaneDanmarkLaListPnModel()
                    {
                        Id = x.Id,
                        RepeatUntil = x.RepeatUntil,
                        RepeatEvery = x.RepeatEvery,
                        RepeatType = x.RepeatType,
                        DayOfWeek = x.DayOfWeek,
                        DayOfMonth = x.DayOfMonth,
                        Description = x.Description,
                        Name = x.Name,
                        RelatedEFormId = x.RelatedEFormId,
                        RelatedEFormName = x.RelatedEFormName,
                        Items = x.Items.Select(i => new BaneDanmarkLaListPnBaneDanmarkLaModel()
                        {
                            Id = i.Id,
                            Description = i.Description,
                            Name = i.Name,
                            LocationCode = i.LocationCode,
                            ItemNumber = i.ItemNumber,
                            BuildYear = i.BuildYear,
                            Type = i.Type
                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (itemList == null)
                {
                    return new OperationDataResult<BaneDanmarkLaListPnModel>(
                        false,
                        _baneDanmarkLaLocalizationService.GetString("ListNotFound"));
                }


                return new OperationDataResult<BaneDanmarkLaListPnModel>(
                    true,
                    itemList);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<BaneDanmarkLaListPnModel>(
                    false,
                    _baneDanmarkLaLocalizationService.GetString("ErrorWhileObtainingList"));
            }
        }

        public int UserId
        {
            get
            {
                var value = _httpContextAccessor?.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return value == null ? 0 : int.Parse(value);
            }
        }
    }
}