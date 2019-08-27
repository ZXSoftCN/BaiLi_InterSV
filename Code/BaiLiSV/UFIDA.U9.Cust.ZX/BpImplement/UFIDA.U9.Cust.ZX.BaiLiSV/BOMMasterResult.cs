using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    public class BOMMasterResult
    {
        [XmlAttribute]
        public string ItemMasterCode { get; set; }

        [XmlAttribute]
        public string BOMVersionCode { get; set; }

        [XmlAttribute]
        public int BOMType { get; set; }

        [XmlArrayItem("MOInfo")]
        public List<MOInfo> Infos { get; set; }

        public BOMMasterResult()
        {
            Infos = new List<MOInfo>();
        }
    }
}
