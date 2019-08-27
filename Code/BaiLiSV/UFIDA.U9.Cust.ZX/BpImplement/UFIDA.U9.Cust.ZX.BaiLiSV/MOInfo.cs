using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    public class MOInfo
    {
        [XmlAttribute]
        public string OrgCode { get; set; }

        [XmlAttribute]
        public string MONo { get; set; }

        [XmlAttribute]
        public int StatusCode { get; set; }

        [XmlAttribute]
        public string StatusName { get; set; }

        [XmlAttribute]
        public decimal MOQty { get; set; }

        [XmlAttribute]
        public string ItemCode { get; set; }

        [XmlAttribute]
        public string BOMVersion { get; set; }
    }
}
