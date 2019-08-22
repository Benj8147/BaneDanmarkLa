using System;
using System.Collections.Generic;
using BaneDanmarkLa.Pn.Infrastructure.Models.Report;

namespace BaneDanmarkLa.Pn.Infrastructure.Models.Report
{
    public class ReportModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<DateTime?> Dates { get; set; } = new List<DateTime?>();
        public List<DateTime?> DatesDoneAt { get; set; } = new List<DateTime?>();
        public List<ReportFormFieldModel> FormFields { get; set; } = new List<ReportFormFieldModel>();
    }
}