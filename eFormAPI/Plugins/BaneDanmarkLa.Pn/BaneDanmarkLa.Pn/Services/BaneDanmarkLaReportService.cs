/*
The MIT License (MIT)

Copyright (c) 2007 - 2019 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using BaneDanmarkLa.Pn.Infrastructure.Models.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microting.eForm.Infrastructure.Models;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Microting.BaneDanmarkLaBase.Infrastructure.Data;
using Microting.BaneDanmarkLaBase.Infrastructure.Data.Entities;

namespace BaneDanmarkLa.Pn.Services
{
    public class BaneDanmarkLaReportService : IBaneDanmarkLaReportService
    {
        private readonly ILogger<BaneDanmarkLaReportService> _logger;
        private readonly IBaneDanmarkLaLocalizationService _itemsPlanningLocalizationService;
        private readonly IExcelService _excelService;
        private readonly BaneDanmarkLaPnDbContext _dbContext;
        private readonly IEFormCoreService _coreHelper;

        // ReSharper disable once SuggestBaseTypeForParameter
        public BaneDanmarkLaReportService(IBaneDanmarkLaLocalizationService baneDanmarkLaLocalizationService, ILogger<BaneDanmarkLaReportService> logger, IExcelService excelService, BaneDanmarkLaPnDbContext dbContext, IEFormCoreService coreHelper)
        {
            _baneDanmarkLaLocalizationService = baneDanmarkLaLocalizationService;
            _logger = logger;
            _excelService = excelService;
            _dbContext = dbContext;
            _coreHelper = coreHelper;
        }

        public async Task<OperationDataResult<ReportModel>> GenerateReport(GenerateReportModel model)
        {
            try
            {
                var core = _coreHelper.GetCore();
                var itemList = await _dbContext.ItemLists.FirstAsync(x => x.Id == model.ItemList);
                var item = await _dbContext.Items.FirstAsync(x => x.Id == model.Item);
                var template = core.TemplateRead(itemList.RelatedEFormId);

                var casesQuery = _dbContext.ItemCases.Where(x => x.ItemId == item.Id);

                if (model.DateFrom != null)
                {
                    casesQuery = casesQuery.Where(x => 
                        x.CreatedAt >= new DateTime(model.DateFrom.Value.Year, model.DateFrom.Value.Month, model.DateFrom.Value.Day, 0, 0, 0));
                }

                if (model.DateTo != null)
                {
                    casesQuery = casesQuery.Where(x => 
                        x.CreatedAt <= new DateTime(model.DateTo.Value.Year, model.DateTo.Value.Month, model.DateTo.Value.Day, 23, 59, 59));
                }
                
                var itemCases = await casesQuery.ToListAsync();

                var reportModel = GetReportData(model, item, itemCases, template);

                return new OperationDataResult<ReportModel>(true, reportModel);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationDataResult<ReportModel>(false,
                    _baneDanmarkLaLocalizationService.GetString("ErrorWhileGeneratingReport"));
            }
        }

        public async Task<OperationDataResult<FileStreamModel>> GenerateReportFile(GenerateReportModel model)
        {
            string excelFile = null;
            try
            {
                OperationDataResult<ReportModel> reportDataResult = await GenerateReport(model);
                if (!reportDataResult.Success)
                {
                    return new OperationDataResult<FileStreamModel>(false, reportDataResult.Message);
                }

                excelFile = _excelService.CopyTemplateForNewAccount("report_template");
                bool writeResult = _excelService.WriteRecordsExportModelsToExcelFile(
                    reportDataResult.Model,
                    model,
                    excelFile);

                if (!writeResult)
                {
                    throw new Exception($"Error while writing excel file {excelFile}");
                }

                FileStreamModel result = new FileStreamModel()
                {
                    FilePath = excelFile,
                    FileStream = new FileStream(excelFile, FileMode.Open),
                };

                return new OperationDataResult<FileStreamModel>(true, result);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(excelFile) && File.Exists(excelFile))
                {
                    File.Delete(excelFile);
                }

                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationDataResult<FileStreamModel>(
                    false,
                    _itemsPlanningLocalizationService.GetString("ErrorWhileGeneratingReportFile"));
            }
        }
        
        private ReportModel GetReportData(
            GenerateReportModel model, 
            Item item, 
            IEnumerable<ItemCase> itemCases,
            CoreElement template)
        {
            var core = _coreHelper.GetCore();

            var finalModel = new ReportModel
            {
                Name = item.Name,
                Description = item.Description,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo
            };

            // Go through template elements and get fields and options labels
            foreach (var element in template.ElementList)
            {
                if (!(element is DataElement dataElement)) 
                    continue;

                var dataItems = dataElement.DataItemList;

                foreach (var dataItem in dataItems)
                {
                    var reportFieldModel = new ReportFormFieldModel()
                    {
                        DataItemId = dataItem.Id,
                        Label = dataItem.Label
                    };

                    switch (dataItem)
                    {
                        case MultiSelect multiSelect:
                            // Add label for each option
                            reportFieldModel.Options = multiSelect.KeyValuePairList.Select(x => new ReportFormFieldOptionModel()
                            {
                                Key = x.Key,
                                Label = x.Value
                            }).ToList();
                            break;
                        case SingleSelect singleSelect:
                        case CheckBox checkBox:
                        case Number number:
                        case Text text:
                            // No option label needed for these types
                            reportFieldModel.Options.Add(new ReportFormFieldOptionModel()
                            {
                                Key = string.Empty,
                                Label = string.Empty
                            });
                            break;
                        default:
                            continue;
                    }

                    finalModel.FormFields.Add(reportFieldModel);
                }
            }
            
            // Get all answered cases
            var casesList = core.CaseReadAll(template.Id, null, null)
                .Where(c => itemCases.Select(ic => ic.MicrotingSdkCaseId).Contains(c.Id))
                .ToList();

            // Go through all itemCases
            foreach (var ic in itemCases)
            {
                finalModel.Dates.Add(ic.CreatedAt);

                var @case = casesList.FirstOrDefault(c => c.Id == ic.MicrotingSdkCaseId);

                // Fill with empty values, if this itemCase was not replied
                if (@case == null)
                {
                    foreach (var fieldModel in finalModel.FormFields)
                    {
                        foreach (var optionModel in fieldModel.Options)
                        {
                            optionModel.Values.Add("");
                        }
                    }

                    finalModel.DatesDoneAt.Add(null);

                    continue;
                }
                else
                {
                    finalModel.DatesDoneAt.Add(@case.DoneAt);
                }

                // Get the reply and work with its ElementList
                foreach (var element in core.CaseRead(@case.MicrotingUId, @case.CheckUIid).ElementList)
                {
                    if (!(element is CheckListValue checkListValue)) 
                        continue;

                    // Get the values for each field from the reply
                    foreach (var fieldModel in finalModel.FormFields)
                    {
                        if (!(checkListValue.DataItemList.First(x => x.Id == fieldModel.DataItemId) is Field field))
                            continue;

                        // Fill values for field options
                        foreach (var optionModel in fieldModel.Options)
                        {
                            if (optionModel.Key.IsNullOrEmpty())
                            {
                                optionModel.Values.Add(field.FieldValues[0].ValueReadable);
                            }
                            else
                            {
                                var selectedKeys = field.FieldValues[0].Value.Split('|');

                                optionModel.Values.Add(
                                    selectedKeys.Contains(optionModel.Key) 
                                        ? _baneDanmarkLaLocalizationService.GetString("Yes") 
                                        : ""
                                );
                            }
                        }
                    }
                }
            }

            return finalModel;
        }
    }
}