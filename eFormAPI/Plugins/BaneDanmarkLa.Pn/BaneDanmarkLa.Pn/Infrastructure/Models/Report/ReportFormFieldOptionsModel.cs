using System.Collections.Generic;

namespace BaneDanmarkLa.Pn.Infrastructure.Models.Report
{
    public class ReportFormFieldOptionModel
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public List<string> Values { get; set; } = new List<string>();
    }
}