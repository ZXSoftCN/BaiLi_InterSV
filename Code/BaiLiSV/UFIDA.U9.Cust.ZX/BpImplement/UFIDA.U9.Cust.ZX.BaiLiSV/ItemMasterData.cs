using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    public class ItemMasterData
    {
        [XmlIgnore]
        public bool IsCheck { get; set; }

        [XmlIgnore]
        public string IsValid { get; set; }

        [XmlAttribute]
        public string OrgCode { get; set; }

        [XmlAttribute]
        public string ItemCode { get; set; }
        
        [XmlAttribute]
        public bool ItemUseVersion { get; set; }

        [XmlAttribute]
        public string ItemName { get; set; }

        [XmlAttribute]
        public string VersionCode { get; set; }

        [XmlAttribute]
        public string Specs { get; set; }
        [XmlAttribute]
        public string UOMCode { get; set; }
        [XmlAttribute]
        public bool Effective { get; set; }

        [XmlAttribute]
        public string ItemForm { get; set; }//料品形态
        [XmlAttribute]
        public string ItemFormAttribute { get; set; }//料品形态属性
        [XmlAttribute]
        public string ItemProperty1 { get; set; }
        [XmlAttribute]
        public string ItemProperty2 { get; set; }
        [XmlAttribute]
        public string ItemProperty3 { get; set; }
        [XmlAttribute]
        public string ItemProperty4 { get; set; }
        [XmlAttribute]
        public string ItemProperty5 { get; set; }
        [XmlAttribute]
        public string ItemProperty6 { get; set; }
        [XmlAttribute]
        public string ItemProperty7 { get; set; }
        [XmlAttribute]
        public string ItemProperty8 { get; set; }
        [XmlAttribute]
        public string ItemProperty9 { get; set; }
        [XmlAttribute]
        public string ItemProperty10 { get; set; }
        [XmlAttribute]
        public string ItemProperty11 { get; set; }
        [XmlAttribute]
        public string ItemProperty12 { get; set; }
        [XmlAttribute]
        public string ItemProperty13 { get; set; }
        [XmlAttribute]
        public string ItemProperty14 { get; set; }
        [XmlAttribute]
        public string ItemProperty15 { get; set; }
        [XmlAttribute]
        public string ItemProperty16 { get; set; }

        [XmlAttribute]
        public string ItemDescSeg5 { get; set; }
        [XmlAttribute]
        public string ItemDescSeg6 { get; set; }
        [XmlAttribute]
        public string ItemDescSeg7 { get; set; }
        [XmlAttribute]
        public string ItemDescSeg8 { get; set; }
        [XmlAttribute]
        public string ItemDescSeg9 { get; set; }
        [XmlAttribute]
        public string ItemDescSeg10 { get; set; }
    }
}
