namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using UFSoft.UBF.Util.Log;
    using MyMVC;
    using UFIDA.U9.Base.Organization;
    using UFIDA.U9.CBO.SCM.Item;
    using UFSoft.UBF.PL;
    using ISVItem = UFIDA.U9.ISV.Item;
    using UFIDA.U9.CBO.Pub.Controller;
    using UFIDA.U9.Base.UserRole;
    using UFIDA.U9.Base.UOM;
    using UFIDA.U9.Base.PropertyTypes;
    using UFIDA.U9.Base.FlexField.DescFlexField;

    /// <summary>
    /// CreateItemMasterSV partial 
    /// </summary>	
    public partial class CreateItemMasterSV
    {
        internal BaseStrategy Select()
        {
            return new CreateItemMasterSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateItemMasterSVImpementStrategy : BaseStrategy
    {
        public CreateItemMasterSVImpementStrategy() { }
        private static readonly ILogger logger = LoggerManager.GetLogger(LoggerType.Transaction_Scope);
        StringBuilder strbError = new StringBuilder();
        StringBuilder strbSuccess = new StringBuilder();

        public override object Do(object obj)
        {
            CreateItemMasterSV bpObj = (CreateItemMasterSV)obj;

            StringBuilder strbResult = new StringBuilder();
            #region 基础校验&前提检查
            if (string.IsNullOrEmpty(bpObj.ItemInfo))
            {
                logger.Error(string.Format("创建料品失败：传入参数ItemInfo为空。"));
                strbResult.AppendFormat(string.Format("<ResultInfo Error={0} />", "创建料品失败：传入参数ItemInfo为空。"));
                return strbResult.ToString();
            }
            ItemInfo iteminfoAll = new ItemInfo();
            ContextInfo cxtInfo = new ContextInfo();
            try
            {
                iteminfoAll = XmlSerializerHelper.XmlDeserialize<ItemInfo>(bpObj.ItemInfo, Encoding.Unicode);
                cxtInfo = XmlSerializerHelper.XmlDeserialize<ContextInfo>(bpObj.ContextInfo, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("反序列化ItemInfo失败：{0}", bpObj.ItemInfo));
                strbResult.AppendFormat(string.Format("<ResultInfo Error={0} />", string.Format("反序列化ItemInfo失败：{0}", bpObj.ItemInfo)));
                return strbResult.ToString();
            }
            if (iteminfoAll.ItemMasters.Count <= 0)
            {
                logger.Error(string.Format("传入的ItemInfo中没有料品信息"));
                strbResult.AppendLine(string.Format("<ResultInfo Error={0} />", "传入的ItemInfo中没有料品信息"));
                return strbResult.ToString();
            }
            if (string.IsNullOrEmpty(bpObj.ItemModule))
            {
                logger.Error(string.Format("创建料品失败：传入模板料品编号ItemModule为空。"));
                strbResult.AppendLine(string.Format("<ResultInfo Error={0} />", "创建料品失败：传入模板料品编号ItemModule为空。"));
                return strbResult.ToString();
            }
            Organization beOrgContext = Organization.FindByCode(cxtInfo.OrgCode);

            ItemMaster beItemMaster = ItemMaster.Finder.Find("Code=@code and Org=@org",
                        new OqlParam[] { new OqlParam(bpObj.ItemModule), new OqlParam(beOrgContext.ID) });
            if (beItemMaster == null)
            {
                logger.Error(string.Format("模板料品ItemModule编号{0},组织【{1}】下无法找到!", bpObj.ItemModule, beOrgContext.Name));
                strbError.AppendLine(string.Format("<ResultInfo Error={0} />",
                    string.Format("模板料品ItemModule编号{0},组织【{1}】下无法找到!", bpObj.ItemModule, beOrgContext.Name)));
                return strbResult.ToString();
            }
            ISVItem.ItemMasterDTO dtoItemModule = null;
            try
            {
                ISVItem.BatchQueryItemByDTOSRV srvQueryItemDTO = new ISVItem.BatchQueryItemByDTOSRV();
                List<ISVItem.QueryItemDTO> lstQueryItem = new List<ISVItem.QueryItemDTO>();
                ISVItem.QueryItemDTO itemQueryModuleDTO = new ISVItem.QueryItemDTO();
                itemQueryModuleDTO.ItemMaster = new CommonArchiveDataDTO();
                itemQueryModuleDTO.ItemMaster.ID = beItemMaster.ID;
                itemQueryModuleDTO.ItemMaster.Name = beItemMaster.Name;
                itemQueryModuleDTO.ItemMaster.Code = beItemMaster.Code;

                itemQueryModuleDTO.Org = new CommonArchiveDataDTO();
                itemQueryModuleDTO.Org.ID = beOrgContext.ID;
                itemQueryModuleDTO.Org.Name = beOrgContext.Name;
                itemQueryModuleDTO.Org.Code = beOrgContext.Code;

                lstQueryItem.Add(itemQueryModuleDTO);
                srvQueryItemDTO.QueryItemDTOs = lstQueryItem;
                List<ISVItem.ItemMasterDTO> lstQueryItemModule = srvQueryItemDTO.Do();
                if (lstQueryItemModule != null && lstQueryItemModule.Count > 0)
                {
                    dtoItemModule = lstQueryItemModule[0];
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("获取模板料品DTO处理异常：{0}.", ex.Message));
                strbError.AppendLine(string.Format("<ResultInfo Error={0} />",
                    string.Format("获取模板料品DTO处理异常：{0}.", ex.Message)));
                return strbResult.ToString();
            }
            #endregion

            ISVItem.BatchModifyItemByDTOSRV svModify = new ISVItem.BatchModifyItemByDTOSRV();
            List<ISVItem.ItemMasterDTO> lstItemModifyDTO = new List<ISVItem.ItemMasterDTO>();
            ISVItem.BatchCreateItemByDTOSRV svCreate = new ISVItem.BatchCreateItemByDTOSRV();
            List<ISVItem.ItemMasterDTO> lstItemCreateDTO = new List<ISVItem.ItemMasterDTO>();

            User beUser = User.Finder.FindByID(cxtInfo.UserID);
            ContextDTO dtoContext = new ContextDTO();
            dtoContext.CultureName = cxtInfo.CultureName;// "zh-CN";
            dtoContext.UserCode = beUser.Code;//默认：系统管理员admin
            dtoContext.EntCode = cxtInfo.EnterpriseCode;// "";//测试默认公司：.正式使用请根据需要指定。
            dtoContext.OrgCode = cxtInfo.OrgCode;//
            svModify.ContextDTO = dtoContext;
            svCreate.ContextDTO = dtoContext;
            Int64 l = 1;

            string strErrorItem = "<errorItem code=\"{0}\" errorDescription=\"{1}\" />";
            string strInfoItem = "<infoItem code=\"{0}\" />";
            try
            {
                StringBuilder strbPLMLog = new StringBuilder();
                strbPLMLog.AppendLine("<PLMLog>");
                foreach (var item in iteminfoAll.ItemMasters)
                {
                    //strbPLMLog.AppendLine(String.Format("<ItemMaster code=\"{0}\" itemProperty14=\"{1}\" />", item.ItemCode,
                    //    String.IsNullOrEmpty(item.ItemProperty14) ? "0" : item.ItemProperty14));//记录传入的物料和图号ItemProperty14
                    ItemMaster beItemExists = null;
                    if (!string.IsNullOrEmpty(item.ItemCode))
                    {
                        strbPLMLog.AppendLine(String.Format("<ItemMaster code=\"{0}\" />", item.ItemCode));//记录传入的料号
                        beItemExists = ItemMaster.Finder.Find("Org=@org and Code=@code ",
                                new OqlParam[] { new OqlParam(beOrgContext.ID), new OqlParam(item.ItemCode) });
                    }
                    if (beItemExists == null)
                    {
                        #region 生成ItemMaster编码
                        strbPLMLog.AppendLine(String.Format("<ItemMaster code=\"{0}\" />", item.ItemCode));//记录传入的料号
                        #endregion
                        //只在新增时对单位进行检查。修改时可以不提供单位，沿用原单位。
                        UOM beUOM = UOM.FindByCode(item.UOMCode);
                        if (beUOM == null)
                        {
                            strbError.AppendLine(string.Format(strErrorItem,
                                    item.ItemCode, string.Format("单位{0}在U9中不存在。", item.UOMCode)));
                            continue;
                        }

                        #region 校验传入的料品是否存在.如果不存在，则转入到料品创建服务CreateItemSv中。
                        ISVItem.ItemMasterDTO dtoItemCreate = CreateItemMasterDTO(dtoItemModule, item, cxtInfo);
                        lstItemCreateDTO.Add(dtoItemCreate);
                        #endregion
                    }
                    else
                    {
                        strbPLMLog.AppendLine(String.Format("<ItemMaster code=\"{0}\" />", item.ItemCode));//记录传入的料号
                        #region 若存在则转入到修改服务ModifyItemMasterDTO中,无料品版本管理。
                        ISVItem.ItemMasterDTO dtoItemCreate = ModifyItemMasterDTO(beItemExists, item, cxtInfo, false);
                        lstItemModifyDTO.Add(dtoItemCreate);
                        #endregion
                    }
                    l++;
                }
                strbPLMLog.AppendLine("</PLMLog>");
                logger.Error(strbPLMLog);//传入物料号和图号日志记录。
                if (lstItemCreateDTO.Count > 0)
                {
                    svCreate.ItemMasterDTOs = lstItemCreateDTO;
                    ItemMaster.EntityList lstCreateItem = svCreate.Do();
                    foreach (var item in lstCreateItem)
                    {
                        strbSuccess.AppendLine(string.Format(strInfoItem, item.Code));
                    }
                }
                if (lstItemModifyDTO.Count > 0)
                {
                    svModify.ItemMasterDTOs = lstItemModifyDTO;
                    ItemMaster.EntityList lstModifyItem = svModify.Do();
                    foreach (var item in lstModifyItem)
                    {
                        strbSuccess.AppendLine(string.Format(strInfoItem, item.Code));
                    }
                }
                strbResult.AppendLine("<ResultInfo>");
                strbResult.AppendLine(strbError.ToString());
                strbResult.AppendLine(strbSuccess.ToString());
                strbResult.AppendLine("</ResultInfo>");
                logger.Info(strbResult.ToString());//日志记录
                return strbResult.ToString();

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("PLM调用料品接口服务失败：{0}。", ex.Message));
                strbResult.AppendFormat(string.Format("<ResultInfo Error={0} />", string.Format("PLM调用料品接口服务失败：{0}。", ex.Message)));
                return strbResult.ToString();
            }
        }

        /// <summary>
        /// 创建新增料品服务的DTO
        /// </summary>
        /// <param name="_itemModule"></param>
        /// <returns></returns>
        private ISVItem.ItemMasterDTO CreateItemMasterDTO(ISVItem.ItemMasterDTO _dtoItemModule, ItemMasterData _itemData, ContextInfo _cxtInfo)
        {
            ISVItem.ItemMasterDTO dtoItemNew = new ISVItem.ItemMasterDTO();
            dtoItemNew.Org = new CommonArchiveDataDTO();//组织
            Organization beOrg = Organization.FindByCode(_cxtInfo.OrgCode);
            dtoItemNew.Org.ID = beOrg.ID;
            dtoItemNew.Org.Code = beOrg.Code;
            dtoItemNew.Org.Name = beOrg.Name;
            dtoItemNew.Code = _itemData.ItemCode;//料号
            dtoItemNew.SPECS = _itemData.Specs;//规格
            dtoItemNew.ItemFormAttribute = ItemTypeAttributeEnum.GetFromName(_itemData.ItemFormAttribute);//料品形态属性
            //if (ItemTypeAttributeEnum.GetFromName(_itemData.ItemFormAttribute) == ItemTypeAttributeEnum.PurchasePart)
            //{
            //    dtoItemNew.IsBOMEnable = false;//可BOM----采购件，不支持（委外）可BOM母件设定。
            //}
            //else
            //{
            dtoItemNew.IsBOMEnable = _dtoItemModule.IsBOMEnable;//可BOM
            //}
            dtoItemNew.ItemForm = ItemTypeEnum.GetFromValue(_itemData.ItemForm);//料品形态

            //不进行料品版本的业务处理
            dtoItemNew.IsVersionQtyControl = false;
            dtoItemNew.ItemMasterVersions = null;//默认不提供物料版本
            dtoItemNew.Version = string.Empty;
            dtoItemNew.VersionID = 0L;

            StringBuilder strbCombName = new StringBuilder();
            strbCombName.AppendFormat("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}",
                string.IsNullOrEmpty(_itemData.ItemName) ? "0" : _itemData.ItemName, string.IsNullOrEmpty(_itemData.Specs) ? "0" : _itemData.Specs,
                string.IsNullOrEmpty(_itemData.ItemProperty1) ? "0" : _itemData.ItemProperty1, string.IsNullOrEmpty(_itemData.ItemProperty2) ? "0" : _itemData.ItemProperty2,
                string.IsNullOrEmpty(_itemData.ItemProperty3) ? "0" : _itemData.ItemProperty3, string.IsNullOrEmpty(_itemData.ItemProperty4) ? "0" : _itemData.ItemProperty4,
                string.IsNullOrEmpty(_itemData.ItemProperty5) ? "0" : _itemData.ItemProperty5, string.IsNullOrEmpty(_itemData.ItemProperty6) ? "0" : _itemData.ItemProperty6,
                string.IsNullOrEmpty(_itemData.ItemProperty7) ? "0" : _itemData.ItemProperty7, string.IsNullOrEmpty(_itemData.ItemProperty8) ? "0" : _itemData.ItemProperty8,
                string.IsNullOrEmpty(_itemData.ItemProperty9) ? "0" : _itemData.ItemProperty9, string.IsNullOrEmpty(_itemData.ItemProperty10) ? "0" : _itemData.ItemProperty10,
                string.IsNullOrEmpty(_itemData.ItemProperty11) ? "0" : _itemData.ItemProperty11, string.IsNullOrEmpty(_itemData.ItemProperty12) ? "0" : _itemData.ItemProperty12,
                string.IsNullOrEmpty(_itemData.ItemProperty13) ? "0" : _itemData.ItemProperty13, string.IsNullOrEmpty(_itemData.ItemProperty14) ? "0" : _itemData.ItemProperty14);
            dtoItemNew.Name = strbCombName.ToString();
            //dtoItemNew.Name = _itemData.ItemName;//测试

            User beUser = User.Finder.FindByID(_cxtInfo.UserID);

            dtoItemNew.AliasName = string.Empty;//别名
            dtoItemNew.CreatedBy = beUser.Name;//创建人
            dtoItemNew.CreatedOn = DateTime.Now.Date;
            dtoItemNew.ModifiedBy = beUser.Name;//修改人
            dtoItemNew.ModifiedOn = DateTime.Now.Date;

            dtoItemNew.IsMultyUOM = _dtoItemModule.IsMultyUOM;//多单位 
            dtoItemNew.IsDualUOM = _dtoItemModule.IsDualUOM;//双单位
            UOM beUOM = UOM.FindByCode(_itemData.UOMCode);
            CommonArchiveDataDTO dtoUOM = new CommonArchiveDataDTO();//单位
            dtoUOM.Code = beUOM.Code;
            dtoUOM.ID = beUOM.ID;
            dtoUOM.Name = beUOM.Name;

            dtoItemNew.InventoryUOM = dtoUOM;//库存主单位
            dtoItemNew.InventorySecondUOM = dtoUOM;//库存单位
            dtoItemNew.BulkUom = dtoUOM;//体积单位
            dtoItemNew.CostUOM = dtoUOM;//成本单位
            dtoItemNew.ManufactureUOM = dtoUOM;//生产单位
            dtoItemNew.MaterialOutUOM = dtoUOM;//领料单位
            dtoItemNew.PurchaseUOM = dtoUOM;//采购单位
            dtoItemNew.PriceUOM = dtoUOM;//计价单位
            dtoItemNew.SalesUOM = dtoUOM;//销售单位
            dtoItemNew.WeightUom = dtoUOM;//重量单位

            Effective beEffective = new Effective();
            beEffective.IsEffective = _itemData.Effective;
            beEffective.EffectiveDate = DateTime.Now.Date;// _dtoItemModule.Effective.EffectiveDate;
            beEffective.DisableDate = DateTime.Parse("9999-12-31");// _dtoItemModule.Effective.DisableDate;
            dtoItemNew.Effective = beEffective;//生效性

            #region dtoItemNew.DescFlexField;扩展字段
            DescFlexSegments segments = new DescFlexSegments();
            segments.SetValue("PrivateDescSeg5", _itemData.ItemDescSeg5);
            segments.SetValue("PrivateDescSeg6", _itemData.ItemDescSeg6);
            segments.SetValue("PrivateDescSeg7", _itemData.ItemDescSeg7);
            segments.SetValue("PrivateDescSeg8", _itemData.ItemDescSeg8);
            segments.SetValue("PrivateDescSeg9", _itemData.ItemDescSeg9);
            segments.SetValue("PrivateDescSeg10", _itemData.ItemDescSeg10);
            dtoItemNew.DescFlexField = segments;
            #endregion

            #region 模板物料赋值
            dtoItemNew.AssetCategory = _dtoItemModule.AssetCategory;//财务分类
            dtoItemNew.BoundedCategory = _dtoItemModule.BoundedCategory;//保税品类别
            dtoItemNew.BoundedCountTaxRate = _dtoItemModule.BoundedCountTaxRate;//保税应补税率
            dtoItemNew.BoundedCountToLerance = _dtoItemModule.BoundedCountToLerance;//保税盘差率
            dtoItemNew.BoundedTaxNO = _dtoItemModule.BoundedTaxNO;//料品税则号

            dtoItemNew.CatalogNO = _dtoItemModule.CatalogNO;//目录编号
            dtoItemNew.ConverRatioRule = _dtoItemModule.ConverRatioRule;//转换率策略
            dtoItemNew.CostCategory = _dtoItemModule.CostCategory;//成本分类
            dtoItemNew.CostCurrency = _dtoItemModule.CostCurrency;//成本币种
            dtoItemNew.CreditCategory = _dtoItemModule.CreditCategory;//信用分类
            dtoItemNew.CustomNumber = _dtoItemModule.CustomNumber;//海关编码
            dtoItemNew.CustomTaxRate = _dtoItemModule.CustomTaxRate;//海关增税率
            dtoItemNew.DrawbackRate = _dtoItemModule.DrawbackRate;//退税率
            dtoItemNew.EndGrade = _dtoItemModule.EndGrade;//结束等级
            dtoItemNew.EndPotency = _dtoItemModule.EndPotency;//结束成分
            dtoItemNew.EntranceInfo = _dtoItemModule.EntranceInfo;//进出口信息
            dtoItemNew.InspectionInfo = _dtoItemModule.InspectionInfo;//料品质量相关信息
            dtoItemNew.InternalTransCost = _dtoItemModule.InternalTransCost;//内部转移成本
            dtoItemNew.InventoryInfo = _dtoItemModule.InventoryInfo;//料品库存相关信息
            dtoItemNew.InventoryUOMGroup = _dtoItemModule.InventoryUOMGroup;//库存主单位计量单位组
            dtoItemNew.IsBounded = _dtoItemModule.IsBounded;//保税品
            dtoItemNew.IsBuildEnable = _dtoItemModule.IsBuildEnable;//可生产
            dtoItemNew.IsCanFlowStat = _dtoItemModule.IsCanFlowStat;//可流向统计
            dtoItemNew.IsDualQuantity = _dtoItemModule.IsDualQuantity;//双数量
            dtoItemNew.IsGradeControl = _dtoItemModule.IsGradeControl;//等级控制
            dtoItemNew.IsIncludedCostCa = _dtoItemModule.IsIncludedCostCa;//成本卷算
            dtoItemNew.IsIncludedStockAsset = _dtoItemModule.IsIncludedStockAsset;//存货资产
            dtoItemNew.IsInventoryEnable = _dtoItemModule.IsInventoryEnable;//可库存交易
            dtoItemNew.IsMRPEnable = _dtoItemModule.IsMRPEnable;//可MRP
            dtoItemNew.IsNeedLicence = _dtoItemModule.IsNeedLicence;//需许可证
            dtoItemNew.IsOutsideOperationEnable = _dtoItemModule.IsOutsideOperationEnable;//可委外
            dtoItemNew.IsPotencyControl = _dtoItemModule.IsPotencyControl;//成分控制
            dtoItemNew.IsPurchaseEnable = _dtoItemModule.IsPurchaseEnable;//可采购
            dtoItemNew.IsSalesEnable = _dtoItemModule.IsSalesEnable;//可销售
            dtoItemNew.IsSpecialItem = _dtoItemModule.IsSpecialItem;//专用料
            dtoItemNew.IsTrademark = _dtoItemModule.IsTrademark;//厂牌管理
            dtoItemNew.IsVarRatio = _dtoItemModule.IsVarRatio;//固定转换率
            //dtoItemNew.IsVersionQtyControl = _dtoItemModule.IsVersionQtyControl;//版本数量控制
            dtoItemNew.IsVMIEnable = _dtoItemModule.IsVMIEnable;//VMI标志
            dtoItemNew.ItemBulk = _dtoItemModule.ItemBulk;//库存单位体积
            //dtoItemNew.ItemForm = _dtoItemModule.ItemForm;//料品形态
            dtoItemNew.ItemTradeMarkInfos = _dtoItemModule.ItemTradeMarkInfos;//料品厂牌信息
            dtoItemNew.MainItemCategory = _dtoItemModule.MainItemCategory;//主分类
            dtoItemNew.MfgInfo = _dtoItemModule.MfgInfo;//料品生产相关信息
            //MRPPlanningType  计划方法  
            dtoItemNew.MrpInfo = _dtoItemModule.MrpInfo;//料品MRP相关信息
            dtoItemNew.MrpInfo.MRPPlanningType = _dtoItemModule.MrpInfo.MRPPlanningType;//计划方法
            dtoItemNew.MRPCategory = _dtoItemModule.MRPCategory;//MRP分类
            dtoItemNew.NeedInspect = _dtoItemModule.NeedInspect;//需商检
            dtoItemNew.PlanCost = _dtoItemModule.PlanCost;//计划价
            dtoItemNew.PriceCategory = _dtoItemModule.PriceCategory;//价格分类

            dtoItemNew.ProductionCategory = _dtoItemModule.ProductionCategory;//生产分类
            dtoItemNew.PurchaseCategory = _dtoItemModule.PurchaseCategory;//采购分类
            dtoItemNew.PurchaseInfo = _dtoItemModule.PurchaseInfo;//料品采购相关信息
            dtoItemNew.RecentlyCost = _dtoItemModule.RecentlyCost;//最新成本
            dtoItemNew.RefrenceCost = _dtoItemModule.RefrenceCost;//参考成本
            dtoItemNew.SaleCategory = _dtoItemModule.SaleCategory;//销售分类
            dtoItemNew.SaleInfo = _dtoItemModule.SaleInfo;//料品销售相关信息
            dtoItemNew.SaleInfo.ItemForInvoice = new CommonArchiveDataDTO();
            dtoItemNew.SaleInfo.NameForInvoice = strbCombName.ToString();

            dtoItemNew.StandardBatchQty = _dtoItemModule.StandardBatchQty;//标准批量
            dtoItemNew.StandardCost = _dtoItemModule.StandardCost;//标准成本
            dtoItemNew.StandardGrade = _dtoItemModule.StandardGrade;//标准等级
            dtoItemNew.StandardPotency = _dtoItemModule.StandardPotency;//标准成分
            dtoItemNew.StartGrade = _dtoItemModule.StartGrade;//起始等级
            dtoItemNew.StartPotency = _dtoItemModule.StartPotency;//起始成分
            dtoItemNew.State = _dtoItemModule.State;//料品状态
            dtoItemNew.StateTime = _dtoItemModule.StateTime;//状态提交日期
            dtoItemNew.StateUser = _dtoItemModule.StateUser;//提交人
            dtoItemNew.Status = _dtoItemModule.Status;//状态码
            dtoItemNew.StatusLastModify = DateTime.Now.Date;//状态日期
            dtoItemNew.StockCategory = _dtoItemModule.StockCategory;//库存分类
            dtoItemNew.TradeMark = _dtoItemModule.TradeMark;//厂牌
            dtoItemNew.Weight = _dtoItemModule.Weight;//库存单位重量
            #endregion

            return dtoItemNew;
        }

        /// <summary>
        /// 创建修改料品服务的DTO
        /// </summary>
        /// <param name="_itemModule"></param>
        /// <returns></returns>
        private ISVItem.ItemMasterDTO ModifyItemMasterDTO(ItemMaster _itemExists, ItemMasterData _itemData, ContextInfo _cxtInfo, bool _isNewVersion)
        {
            ISVItem.BatchQueryItemByDTOSRV srvQueryItemDTO = new ISVItem.BatchQueryItemByDTOSRV();
            List<ISVItem.QueryItemDTO> lstQueryDTO = new List<ISVItem.QueryItemDTO>();
            ISVItem.QueryItemDTO dtoExists = new ISVItem.QueryItemDTO();
            dtoExists.ItemMaster = new CommonArchiveDataDTO();
            dtoExists.ItemMaster.ID = _itemExists.ID;
            lstQueryDTO.Add(dtoExists);

            srvQueryItemDTO.QueryItemDTOs = lstQueryDTO;
            List<ISVItem.ItemMasterDTO> lstItemMasterDTO = srvQueryItemDTO.Do();
            ISVItem.ItemMasterDTO dtoItemModify = null;
            if (lstItemMasterDTO != null && lstItemMasterDTO.Count > 0)
            {
                dtoItemModify = lstItemMasterDTO[0];
                dtoItemModify.SPECS = _itemData.Specs;//规格
                if (!string.IsNullOrEmpty(_itemData.ItemForm))
                {
                    dtoItemModify.ItemForm = ItemTypeEnum.GetFromValue(_itemData.ItemForm);//料品形态
                }
                dtoItemModify.ItemFormAttribute = ItemTypeAttributeEnum.GetFromName(_itemData.ItemFormAttribute);//料品形态属性
                User beUser = User.Finder.FindByID(_cxtInfo.UserID);

                dtoItemModify.ModifiedBy = beUser.Name;//修改人
                dtoItemModify.ModifiedOn = DateTime.Now.Date;
                dtoItemModify.Effective.IsEffective = _itemData.Effective;

                //若单位为提供，则沿用原单位
                CommonArchiveDataDTO dtoUOM = new CommonArchiveDataDTO();
                if (String.IsNullOrEmpty(_itemData.UOMCode))
                {
                    dtoUOM.Code = _itemExists.InventoryUOM.Code;
                    dtoUOM.ID = _itemExists.InventoryUOM.ID;
                    dtoUOM.Name = _itemExists.InventoryUOM.Name;
                }
                else
                {
                    UOM beUOM = UOM.FindByCode(_itemData.UOMCode);
                    dtoUOM.Code = beUOM.Code;
                    dtoUOM.ID = beUOM.ID;
                    dtoUOM.Name = beUOM.Name;
                }

                dtoItemModify.InventoryUOM = dtoUOM;//库存主单位
                dtoItemModify.InventorySecondUOM = dtoUOM;//库存单位
                dtoItemModify.BulkUom = dtoUOM;//体积单位
                dtoItemModify.CostUOM = dtoUOM;//成本单位
                dtoItemModify.ManufactureUOM = dtoUOM;//生产单位
                dtoItemModify.MaterialOutUOM = dtoUOM;//领料单位
                dtoItemModify.PurchaseUOM = dtoUOM;//采购单位
                dtoItemModify.PriceUOM = dtoUOM;//计价单位
                dtoItemModify.SalesUOM = dtoUOM;//销售单位
                dtoItemModify.WeightUom = dtoUOM;//重量单位

                StringBuilder strbCombName = new StringBuilder();
                strbCombName.AppendFormat("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}",
                 string.IsNullOrEmpty(_itemData.ItemName) ? "0" : _itemData.ItemName, string.IsNullOrEmpty(_itemData.Specs) ? "0" : _itemData.Specs,
                 string.IsNullOrEmpty(_itemData.ItemProperty1) ? "0" : _itemData.ItemProperty1, string.IsNullOrEmpty(_itemData.ItemProperty2) ? "0" : _itemData.ItemProperty2,
                 string.IsNullOrEmpty(_itemData.ItemProperty3) ? "0" : _itemData.ItemProperty3, string.IsNullOrEmpty(_itemData.ItemProperty4) ? "0" : _itemData.ItemProperty4,
                 string.IsNullOrEmpty(_itemData.ItemProperty5) ? "0" : _itemData.ItemProperty5, string.IsNullOrEmpty(_itemData.ItemProperty6) ? "0" : _itemData.ItemProperty6,
                 string.IsNullOrEmpty(_itemData.ItemProperty7) ? "0" : _itemData.ItemProperty7, string.IsNullOrEmpty(_itemData.ItemProperty8) ? "0" : _itemData.ItemProperty8,
                 string.IsNullOrEmpty(_itemData.ItemProperty9) ? "0" : _itemData.ItemProperty9, string.IsNullOrEmpty(_itemData.ItemProperty10) ? "0" : _itemData.ItemProperty10,
                 string.IsNullOrEmpty(_itemData.ItemProperty11) ? "0" : _itemData.ItemProperty11, string.IsNullOrEmpty(_itemData.ItemProperty12) ? "0" : _itemData.ItemProperty12,
                 string.IsNullOrEmpty(_itemData.ItemProperty13) ? "0" : _itemData.ItemProperty13, string.IsNullOrEmpty(_itemData.ItemProperty14) ? "0" : _itemData.ItemProperty14);
                dtoItemModify.Name = strbCombName.ToString();
                //dtoItemModify.Name = _itemData.ItemName;//测试

                #region DescFlexField;扩展字段
                DescFlexSegments segments = dtoItemModify.DescFlexField;
                segments.SetValue("PrivateDescSeg5", _itemData.ItemDescSeg5);
                segments.SetValue("PrivateDescSeg6", _itemData.ItemDescSeg6);
                segments.SetValue("PrivateDescSeg7", _itemData.ItemDescSeg7);
                segments.SetValue("PrivateDescSeg8", _itemData.ItemDescSeg8);
                segments.SetValue("PrivateDescSeg9", _itemData.ItemDescSeg9);
                segments.SetValue("PrivateDescSeg10", _itemData.ItemDescSeg10);
                #endregion

                //无料品版本处理
            }

            return dtoItemModify;
        }		
    }

    #endregion


}