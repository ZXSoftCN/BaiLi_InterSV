using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    [XmlRoot("BOMInfo", Namespace = "", IsNullable = true)]
    public class BOMInfo
    {
        [XmlArrayItem("BOMMaster")]
        public List<BOMMaster> Masters { get; set; }

        public BOMInfo()
        {
            Masters = new List<BOMMaster>();
        }
    }
}
