using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    public class BOMMasterQuery
    {
        [XmlAttribute]
        public string ItemMasterCode { get; set; }
        
        [XmlAttribute]
        public string BOMVersionCode { get; set; }

        [XmlAttribute]
        public int BOMType { get; set; }
        
        public BOMMasterQuery()
        {
        }
    }
}
