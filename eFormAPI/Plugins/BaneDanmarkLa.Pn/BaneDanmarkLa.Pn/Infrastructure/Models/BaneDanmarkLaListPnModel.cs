using System.Collections.Generic;

namespace BaneDanmarkLa.Pn.Infrastructure.Models
{
    using System;

    public class BaneDanmarkLaListPnModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RepeatEvery { get; set; }
        public DateTime? RepeatUntil { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public int RelatedEFormId { get; set; }
        public string RelatedEFormName { get; set; }

        public List<BaneDanmarkLaListPnBaneDanmarkLaModel> Items { get; set; }
            = new List<BaneDanmarkLaListPnBaneDanmarkLaModel>();
    }
}