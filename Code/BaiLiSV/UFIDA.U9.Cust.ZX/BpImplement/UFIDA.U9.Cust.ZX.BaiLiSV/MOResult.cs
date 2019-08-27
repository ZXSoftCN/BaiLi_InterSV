using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    [XmlRoot("MOResult", Namespace = "", IsNullable = true)]
    public class MOResult
    {
        [XmlArrayItem("BOMMasterResult")]
        public List<BOMMasterResult> BomMasters { get; set; }

        public MOResult()
        {
            BomMasters = new List<BOMMasterResult>();
        }
    }
}
