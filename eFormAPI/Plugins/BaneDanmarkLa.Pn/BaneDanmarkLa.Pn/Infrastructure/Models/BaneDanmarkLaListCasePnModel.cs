using System.Collections.Generic;

namespace BaneDanmarkLa.Pn.Infrastructure.Models
{
    public class BaneDanmarkLaListCasePnModel
    {
        public int Total { get; set; }
        public List<BaneDanmarkLaListPnBaneDanmarkLaCaseModel> Items { get; set; }
            = new List<BaneDanmarkLaListPnBaneDanmarkLaCaseModel>();
    }
}