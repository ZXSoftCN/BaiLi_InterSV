using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    [XmlRoot("ItemInfo", Namespace = "", IsNullable = true)]
    public class ItemInfo
    {
        [XmlArrayItem("ItemMaster")]
        public List<ItemMasterData> ItemMasters { get; set; }
    }
}