namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;
    using MyMVC;
    using UFSoft.UBF.PL;

	/// <summary>
	/// QueryMOSV partial 
	/// </summary>	
	public partial class QueryMOSV 
	{	
		internal BaseStrategy Select()
		{
			return new QueryMOSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class QueryMOSVImpementStrategy : BaseStrategy
	{
		public QueryMOSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			QueryMOSV bpObj = (QueryMOSV)obj;

            StringBuilder strbResult = new StringBuilder();

            if (string.IsNullOrEmpty(bpObj.BOMQueryInfo))
            {
                //logger.Error(string.Format("创建料品失败：传入参数BOMItemInfo为空。"));
                strbResult.AppendFormat(string.Format("<ResultInfo Error=\"{0}\" />", "查询生产订单失败：传入参数BOMItemInfo为空。"));
                return strbResult.ToString();
            }
            MOQuery moQuery = new MOQuery();

            try
            {
                moQuery = XmlSerializerHelper.XmlDeserialize<MOQuery>(bpObj.BOMQueryInfo, Encoding.Unicode);
                //cxtInfo = XmlSerializerHelper.XmlDeserialize<ContextInfo>(bpObj.ContextInfo, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                //logger.Error(string.Format("反序列化ItemInfo失败：{0}", bpObj.ItemInfo));
                strbResult.AppendFormat(string.Format("<ResultInfo Error=\"{0}\" />", string.Format("反序列化BOMItemInfo失败：{0}", bpObj.BOMQueryInfo)));
                return strbResult.ToString();
            }
            if (moQuery.BomMasters.Count <= 0)
            {
                //logger.Error(string.Format("传入的ItemInfo中没有料品信息"));
                strbResult.AppendLine(string.Format("<ResultInfo Error=\"{0}\" />", "传入的BOMItemInfo中没有BOM信息"));
                return strbResult.ToString();
            }
            else
            {
                MOResult result = new MOResult();

                foreach (BOMMasterQuery bomMaster in moQuery.BomMasters)
                {
                    BOMMasterResult rltItem = new BOMMasterResult();
                    rltItem.ItemMasterCode = bomMaster.ItemMasterCode;
                    rltItem.BOMType = bomMaster.BOMType;
                    rltItem.BOMVersionCode = bomMaster.BOMVersionCode;
                    result.BomMasters.Add(rltItem);
                    
                    //MO.MO.MO.EntityList lstMO = MO.MO.MO.Finder.FindAll("ItemMaster.Code = @MOItemMaster and DocState !=3 ", new OqlParam[1] { new OqlParam(bomMaster.ItemMasterCode) });
                    MO.MO.MO.EntityList lstMO = MO.MO.MO.Finder.FindAll("ItemMaster.Code = @MOItemMaster and BOMVersion.VersionCode = @BOMVersionCode and DocState !=3 ",
                        new OqlParam[2] { new OqlParam(bomMaster.ItemMasterCode), new OqlParam(bomMaster.BOMVersionCode) });
                    foreach (var moItem in lstMO)
                    {
                        //排除终止状态
                        if (moItem.Cancel.Canceled)
                        {
                            continue;
                        }
                        if (moItem.ProductQty > 0)
                        {
                            MOInfo moInfo = new MOInfo();
                            moInfo.OrgCode = moItem.Org.Code;
                            moInfo.MONo = moItem.DocNo;
                            moInfo.StatusCode = moItem.DocState.Value;
                            if (moItem.BOMVersionKey != null)
                            {
                                moInfo.BOMVersion = moItem.BOMVersion.VersionCode;
                            }
                            else
                            {
                                moInfo.BOMVersion = string.Empty;
                            }
                            switch (moItem.DocState.Value)
                            {
                                case 0:
                                    moInfo.StatusName = "开立";
                                    break;
                                case 1:
                                    moInfo.StatusName = "已核准";
                                    break;
                                case 2:
                                    moInfo.StatusName = "开工";
                                    break;
                                case 3:
                                    moInfo.StatusName = "完工";
                                    break;
                                case 4:
                                    moInfo.StatusName = "核准中";
                                    break;
                                default:
                                    moInfo.StatusName = moItem.DocState.Name;
                                    break;
                            }
                            //MOStateEnum.GetFromValue(moItem.DocState.Value).
                            moInfo.MOQty = moItem.ProductQty;

                            rltItem.Infos.Add(moInfo);
                        }
                    }
                }
                strbResult.Append(XmlSerializerHelper.XmlSerialize<MOResult>(result, Encoding.Unicode));
                return strbResult.ToString();
            }
		}		
	}

	#endregion
	
	
}