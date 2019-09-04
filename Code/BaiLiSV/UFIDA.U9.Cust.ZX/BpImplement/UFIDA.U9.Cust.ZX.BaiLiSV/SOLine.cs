using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    public class SOLine
    {
        [XmlAttribute]
        public string ItemMasterCode { get; set; }

        [XmlAttribute]
        public decimal Qty { get; set; }

        [XmlAttribute]
        public decimal TCPrice { get; set; }

        [XmlAttribute]
        public string DeliveryDate { get; set; }//交货日期


    }
}
