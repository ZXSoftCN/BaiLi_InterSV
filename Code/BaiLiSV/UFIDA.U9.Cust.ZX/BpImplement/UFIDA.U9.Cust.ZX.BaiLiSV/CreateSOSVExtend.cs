namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;
    using MyMVC;
    using UFSoft.UBF.PL;
    using UFIDA.U9.Cust.ZX.BaiLiSV;
    using UFIDA.U9.ISV.SM;
    using UFIDA.U9.Base.Organization;
    using UFIDA.U9.Base.Profile;
    using UFIDA.U9.Base;
    using UFSoft.UBF.Util.DataAccess;
    using UFIDA.U9.CBO.Pub.Controller;
    using UFIDA.U9.CBO.SCM.Customer;
    using UFIDA.U9.CBO.Enums;
    using UFIDA.U9.CBO.SCM.Item;
    using UFIDA.U9.Base.Currency;
    using UFIDA.U9.CBO.MFG.Enums;
    using UFIDA.U9.SM.SO;
    using UFIDA.U9.CBO.SCM.Enums;
    using UFIDA.U9.SM.Enums;

	/// <summary>
	/// CreateSOSV partial 
	/// </summary>	
	public partial class CreateSOSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateSOSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class CreateSOSVImpementStrategy : BaseStrategy
	{
		public CreateSOSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateSOSV bpObj = (CreateSOSV)obj;

            //get business operation context is as follows
            //IContext context = ContextManager.Context	

            //auto generating code end,underside is user custom code
            //and if you Implement replace this Exception Code...

            CreateSOSV svCreateSO = new CreateSOSV();
            StringBuilder strbResult = new StringBuilder();
            string strSOInfo = "<SOInfo DocNo=\"{0}\" />";

            if (string.IsNullOrEmpty(bpObj.DocInfo))
            {
                //logger.Error(string.Format("创建料品失败：传入参数BOMItemInfo为空。"));
                strbResult.AppendFormat(string.Format("<ResultInfo Error=\"{0}\" />", "创建销售订单失败：传入参数DocInfo为空。"));
                return strbResult.ToString();
            }
            SOInfo soInfo = new SOInfo();
            ContextInfo cxtInfo = new ContextInfo();
            #region 校验数据有效性
            try
            {
                soInfo = XmlSerializerHelper.XmlDeserialize<SOInfo>(bpObj.DocInfo, Encoding.Unicode);
                cxtInfo = XmlSerializerHelper.XmlDeserialize<ContextInfo>(bpObj.ContextInfo, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                strbResult.AppendFormat(string.Format("<ResultInfo Error=\"{0}\" />", string.Format("反序列化销售订单DocInfo失败：{0}", bpObj.DocInfo)));
                return strbResult.ToString();
            }
            if (soInfo.Lines.Count <= 0)
            {
                strbResult.AppendLine(string.Format("<ResultInfo Error=\"{0}\" />", "传入的DocInfo中没有订货信息"));
                return strbResult.ToString();
            }

            Organization beOrgContext = Organization.FindByCode(cxtInfo.OrgCode);

            //单据类型
            string soDocTypeCode = string.Empty;
            if (!string.IsNullOrEmpty(bpObj.DocTypeCode))
            {
                soDocTypeCode = bpObj.DocTypeCode;
            }
            else
            {
                soDocTypeCode = cxtInfo.SoDocTypeCode;
            }
            if (string.IsNullOrEmpty(soDocTypeCode))
            {
                strbResult.AppendLine(string.Format("<ResultInfo Error=\"{0}\" />", "不能建立生成销售订单单据类型"));
                return strbResult.ToString();
            }

            //客户
            if (string.IsNullOrEmpty(soInfo.CustCode))
            {
                strbResult.AppendLine(string.Format("<ResultInfo Error=\"{0}\" />", "来源客户不能为空"));
                return strbResult.ToString();
            }

            Customer itemCust = Customer.FindByCode(beOrgContext, soInfo.CustCode);
            if (itemCust == null)
            {
                strbResult.AppendLine(string.Format("<ResultInfo Error=\"{0}\" />",string.Format("客户编号{0}在U9中不存在",soInfo.CustCode)));
                return strbResult.ToString();
            }

            //订单日期
            if (string.IsNullOrEmpty(soInfo.BusinessDate))
            {
                strbResult.AppendLine(string.Format("<ResultInfo Error=\"{0}\" />", "订单日期不能为空"));
                return strbResult.ToString();
            }

            #endregion

            CommonCreateSOSRV sv = new CommonCreateSOSRV();
            List<SaleOrderDTO> lstSODTO = new List<SaleOrderDTO>();
            
            try
            {
                SaleOrderDTO dtoSO = CreateSOHead(beOrgContext, soDocTypeCode, itemCust,soInfo);
                CreateSOLines(beOrgContext, soInfo, dtoSO, itemCust);

                lstSODTO.Add(dtoSO);
                sv.SOs = lstSODTO;
                List<CommonArchiveDataDTO> lstSORlt = new List<CommonArchiveDataDTO>();
                lstSORlt = sv.Do();

                strbResult.AppendLine("<ResultInfo>");
                if (lstSORlt.Count > 0)
                {
                    strbResult.AppendLine(string.Format(strSOInfo, lstSODTO[0].DocNo));
                }
                else
                {
                    strbResult.AppendLine("<Error Desc=\"未能生成U9销售订单\" ></Error>");
                }
                strbResult.AppendLine("</ResultInfo>");

                return strbResult.ToString();
            }
            catch (Exception ex)
            {
                strbResult.AppendFormat(string.Format("<ResultInfo error={0} />", string.Format("创建U9销售订单失败：{0}", bpObj.DocInfo)));
                return strbResult.ToString();
            }
            
        }

        private SaleOrderDTO CreateSOHead(Organization org, string soDocTypeCode, Customer itemCust,SOInfo soInfo)
        {
            SaleOrderDTO dtoSOHead = new SaleOrderDTO();
            //单据类型
            long doctypeID = GetProfileValue(soDocTypeCode);//销售订单单据类型
            CommonArchiveDataDTO dtoDocType = new CommonArchiveDataDTO();
            dtoDocType.ID = doctypeID;
            dtoSOHead.DocumentType = dtoDocType;

            //客户
            CustomerMISCInfo customer = new CustomerMISCInfo();
            customer.Code = itemCust.Code;
            dtoSOHead.OrderBy = customer;

            //订单日期
            dtoSOHead.BusinessDate = DateTime.Parse(soInfo.BusinessDate);

            Profile profDept = Base.Profile.Profile.Finder.Find("Application.Code = @appCode and Code = @code ", new OqlParam[2] { new OqlParam("0503"), new OqlParam("SD051") });//销售管理-参数设置-默认销售部门
            Profile profEmployee = Base.Profile.Profile.Finder.Find("Application.Code = @appCode and Code = @code ", new OqlParam[2] { new OqlParam("0503"), new OqlParam("SD052") });//销售管理-参数设置-默认销售员
            //部门
            dtoSOHead.SaleDepartment = new CommonArchiveDataDTO();
            dtoSOHead.SaleDepartment.ID = Convert.ToInt64(profDept.GetProfileValue().ToString());
            //业务员
            dtoSOHead.Seller = new CommonArchiveDataDTO();
            dtoSOHead.Seller.ID = Convert.ToInt64(profEmployee.GetProfileValue().ToString());

            //立账位置
            dtoSOHead.IsPriceIncludeTax = true;
            CustomerSite cs = CustomerSite.Finder.Find("Customer  ='" + itemCust.ID + "'");
            CustomerSiteMISCInfo cdd = new CustomerSiteMISCInfo();
            cdd.CustomerSite = cs;
            //收货位置
            dtoSOHead.ShipToSite = cdd;
            dtoSOHead.ShipToSite.Code = cdd.Code;
            dtoSOHead.ShipToSite.Name = cdd.Name;
            //立账位置
            dtoSOHead.BillToSite = cdd;
            dtoSOHead.BillToSite.Code = cdd.Code;
            dtoSOHead.BillToSite.Name = cdd.Name;
            //付款位置
            dtoSOHead.PayerSite = cdd;
            dtoSOHead.PayerSite.Code = cdd.Code;
            dtoSOHead.PayerSite.Name = cdd.Name;

            //币种
            if (itemCust.TradeCurrency != null)
            {
                dtoSOHead.TC = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                dtoSOHead.TC.Code = itemCust.TradeCurrency.Code;
            }

            //优先级
            dtoSOHead.SOPriority =SOPRIEnum.GetFromValue(99);
            //核算组织
            dtoSOHead.AccountOrg = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
            dtoSOHead.AccountOrg.Code = org.Code;
            //开票组织
            dtoSOHead.InvoiceOrg = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
            dtoSOHead.InvoiceOrg.Code = org.Code;
            //出货原则
            if (itemCust.ShippmentRule != null)
            {
                dtoSOHead.ShipRule = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                dtoSOHead.ShipRule.Code = itemCust.ShippmentRule.Code;
            }
            //立账条件
            if (itemCust.ARConfirmTerm != null)
            {
                dtoSOHead.ConfirmTerm = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                dtoSOHead.ConfirmTerm.Code = itemCust.ARConfirmTerm.Code;
            }
            //成交方式
            dtoSOHead.BargainMode = BargainEnum.GetFromValue(-1);
            //运费支付
            dtoSOHead.TransPayMode = UFIDA.U9.CBO.SCM.Enums.FreightPaymentEnum.GetFromValue(-1);
            //扩展段
            dtoSOHead.DescFlexField = new UFIDA.U9.Base.FlexField.DescFlexField.DescFlexSegments();
            return dtoSOHead;
        }


        private void CreateSOLines(Organization org, SOInfo soInfo, SaleOrderDTO dtoSO, Customer customer)
        {
            CustomerSite cs = CustomerSite.Finder.Find("Customer  ='" + customer.ID + "'");
            CustomerSiteMISCInfo customerSite = new CustomerSiteMISCInfo();
            customerSite.CustomerSite = cs;
            customerSite.Code = cs.Code;
            customerSite.Name = cs.Name;

            foreach (var itemLine in soInfo.Lines)
            {
                UFIDA.U9.ISV.SM.SOLineDTO soLine = new UFIDA.U9.ISV.SM.SOLineDTO();
                //料品
                soLine.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfo();

                ItemMaster beItemMaster = ItemMaster.Finder.Find("Code = @code and Org = @orgID",
                    new OqlParam[2] { new OqlParam(itemLine.ItemMasterCode), new OqlParam(org.ID) });
                if (beItemMaster == null)
                {
                    throw new Exception(string.Format("物料编码{0}在U9中不存在", itemLine.ItemMasterCode));
                }

                soLine.ItemInfo.ItemID = beItemMaster;

                //soLine.BomOwner = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                //soLine.BomOwner.ID = org.ID;
                //soLine.BomOwner.Code = org.Code;
                //soLine.BomOwner.Name = org.Name;

                //数量
                soLine.ChoiceResult = 0;
                soLine.OrderByQtyTU = itemLine.Qty;
                //扩展字段
                soLine.DescFlexField = new UFIDA.U9.Base.FlexField.DescFlexField.DescFlexSegments();
                //收款条件
                if (customer.RecervalTerm != null)
                {
                    soLine.RecTerm = new CommonArchiveDataDTO();
                    soLine.RecTerm.Code = customer.RecervalTerm.Code;
                }
                //立账位置
                if (customer.CustomerSites != null)
                {
                    if (customer.CustomerSites.Count > 0)
                    {
                        soLine.BillToSite = customerSite;
                        soLine.BillToSite.Code = customerSite.Code;
                        soLine.BillToSite.CustomerSiteKey = customerSite.CustomerSiteKey;
                    }
                }

                //定价方式
                soLine.CooperatePriceStyle = -1;
                //免费品类型
                soLine.FreeType = FreeTypeEnum.GetFromValue(-1);
                //免费品原因
                soLine.FreeReason = DonationReasonEnum.GetFromValue(-1);
                //台阶划分依据
                soLine.StepBy = -1;
                //预收环节 
                soLine.PreRecObject = -1;
                //原币-额币
                soLine.TCToCCExchRateType = ExchangeRateTypesEnum.GetFromValue(0);
                //来源单据类别
                soLine.SrcDocType = SOSourceTypeEnum.GetFromValue(0);
                //成套收发货标志
                soLine.ShipTogetherFlag = KITShipModeEnum.GetFromValue(-1);
                //数量类型  
                soLine.QuantityType = UsageQuantityTypeEnum.GetFromValue(-1);
                //资源成本计费基础 
                soLine.ChargeBasis = ChargeBasisEnum.GetFromValue(-1);
                //价格来源
                soLine.PriceSource = PriceSourceEnum.GetFromValue(1);
                //是否消耗信用额度
                soLine.IsEngrossCreditLimit = true;

                //添加销售订单上的计划行
                soLine.SOShiplines = new List<SOShipLineDTO>();
                SOShipLineDTO soship = new SOShipLineDTO();
                soship.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfo();
                soship.ItemInfo.ItemID = beItemMaster;
                soship.ItemInfo.ItemCode = beItemMaster.Code;
                soship.ItemInfo.ItemName = beItemMaster.Name;
                soship.RequireDate = DateTime.Parse(itemLine.DeliveryDate);
                soship.DescFlexField = new UFIDA.U9.Base.FlexField.DescFlexField.DescFlexSegments();

                //区域位置
                soship.ShipToSite = new CustomerSiteMISCInfo();
                soship.ShipToSite.CustomerSite = customerSite.CustomerSite;
                soship.ShipToSite.Code = customerSite.Code;
                soship.ShipToSite.Name = customerSite.Name;

                //单价
                soLine.OrderPriceTC = itemLine.TCPrice;
                soLine.SOShiplines.Add(soship);

                dtoSO.SOLines.Add(soLine);
            }
        
        }
        /// <summary>
        /// 获取参数设置
        /// </summary>
        /// <param name="orgID"></param>
        /// <param name="profileCode"></param>
        /// <returns></returns>
        public long GetProfileValue(string prDocType)
        {
            long org = Context.LoginOrg.ID;
            object value = "";

            string sql = "select top 1 id from  SM_SODocType where code= '" + prDocType + "' and org= " + org;
            DataAccessor.RunSQL(UFSoft.UBF.Util.DataAccess.DataAccessor.GetConn(), sql.ToString(), null, out value);
            return Convert.ToInt64(value);
        }
	}

	#endregion
	
	
}