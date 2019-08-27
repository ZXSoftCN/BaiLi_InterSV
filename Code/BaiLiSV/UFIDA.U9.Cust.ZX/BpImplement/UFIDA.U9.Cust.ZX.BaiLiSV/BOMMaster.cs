using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    public class BOMMaster
    {
        [XmlIgnore]
        public bool IsCheck { get; set; }

        [XmlIgnore]
        public string IsValid { get; set; }
        

        [XmlAttribute]
        public string ItemMasterCode { get; set; }

        [XmlAttribute]
        public string ItemVersionCode { get; set; }

        [XmlAttribute]
        public string BOMVersionCode { get; set; }

        [XmlAttribute]
        public int BOMType { get; set; }

        [XmlAttribute]
        public string Explain { get; set; }

        [XmlArrayItem("Component")]
        public List<BOMComponent> Components { get; set; }

        public BOMMaster()
        {
            IsCheck = false;
            IsValid = "Y";

            Components = new List<BOMComponent>();
        }
    }
}
