using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.ComponentsTypes
{
    public class ThirukkuralRoot
    {
        public List<ThirukkuralItem> kural { get; set; }
    }
    public class ThirukkuralItem
    {
        public int Number { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string mk { get; set; } // Tamil Explanation
        public string paul_name { get; set; }
        public string iyal_name { get; set; }
        public string adikaram_name { get; set; }
    }
    public class ThirukkuralType
    {
        public int KurralNo { get; set; } = 0;
        public string Pall { get; set; } = "";
        public string athikaram { get; set; } = "";
        public string iyal { get; set; } = "";
        public string[] kural { get; set; } = {"", "" };
        public string vilakam { get; set; } = "";
    }
}
