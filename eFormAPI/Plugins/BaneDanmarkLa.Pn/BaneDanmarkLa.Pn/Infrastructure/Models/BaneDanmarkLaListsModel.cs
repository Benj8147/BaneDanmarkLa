namespace BaneDanmarkLa.Pn.Infrastructure.Models
{
    using System.Collections.Generic;

    public class BaneDanmarkLaListsModel
    {
        public int Total { get; set; }
        public List<BaneDanmarkLaListPnModel> Lists { get; set; }

        public BaneDanmarkLaListsModel()
        {
            Lists = new List<BaneDanmarkLaListPnModel>();
        }
    }
}