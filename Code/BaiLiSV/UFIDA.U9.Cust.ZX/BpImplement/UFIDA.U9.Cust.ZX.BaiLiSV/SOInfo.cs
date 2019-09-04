using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    [XmlRoot("SOInfo", Namespace = "", IsNullable = true)]
    public class SOInfo
    {
        [XmlAttribute]
        public string CustCode { get; set; }
        
        [XmlAttribute]
        public string BusinessDate { get; set; }

        [XmlArrayItem("SOLine")]
        public List<SOLine> Lines { get; set; }

        public SOInfo()
        {
            Lines = new List<SOLine>();
        }
    }
}
