using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    [XmlRoot("MOQuery", Namespace = "", IsNullable = true)]
    public class MOQuery
    {
        [XmlAttribute]
        public string OrgCode { get; set; }

        [XmlArrayItem("BOMMasterQuery")]
        public List<BOMMasterQuery> BomMasters { get; set; }
    }
}
