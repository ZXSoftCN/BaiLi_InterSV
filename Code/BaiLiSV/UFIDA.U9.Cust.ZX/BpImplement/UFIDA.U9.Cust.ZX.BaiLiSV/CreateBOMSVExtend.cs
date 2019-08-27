namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;
    using UFSoft.UBF.Util.Log;
    using MyMVC;
    using UFSoft.UBF.PL;
    using UFSoft.UBF.Util.DataAccess;
    using UFSoft.UBF.Sys.Database;
    using UFIDA.U9.Base.Organization;
    using UFIDA.U9.ISV.MFG.BOM;
    using UFIDA.U9.Base.UserRole;
    using UFIDA.U9.CBO.Pub.Controller;
    using UFIDA.U9.CBO.SCM.Item;
    using UFIDA.U9.CBO.MFG.Enums;
    using UFIDA.U9.CBO.SCM.ProjectTask;
    using UFIDA.U9.CBO.MFG.CostElement;
    using UFIDA.U9.CBO.Enums;

	/// <summary>
	/// CreateBOMSV partial 
	/// </summary>	
	public partial class CreateBOMSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateBOMSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class CreateBOMSVImpementStrategy : BaseStrategy
	{
		public CreateBOMSVImpementStrategy() { }
        private static readonly ILogger logger = LoggerManager.GetLogger(LoggerType.Transaction_Scope);
        StringBuilder strbError = new StringBuilder();
        StringBuilder strbSuccess = new StringBuilder();
        string strErrorItem = "<errorItem bomMasterCode=\"{0}\" bomCompCode=\"{1}\" errorDescription=\"{2}\" />";
        string strInfoItem = "<infoItem code=\"{0}\" />";

        public override object Do(object obj)
        {
            CreateBOMSV bpObj = (CreateBOMSV)obj;

            StringBuilder strbResult = new StringBuilder();
            #region 基础校验&提前检查
            if (string.IsNullOrEmpty(bpObj.BOMInfo))
            {
                logger.Error(string.Format("创建BOM失败：传入参数BOMInfo为空。"));
                strbResult.AppendFormat(string.Format("<ResultInfo error={0} />", "创建BOM失败：传入参数BOMInfo为空。"));
                return strbResult.ToString();
            }
            BOMInfo bominfoAll = new BOMInfo();
            ContextInfo cxtInfo = new ContextInfo();
            try
            {
                bominfoAll = XmlSerializerHelper.XmlDeserialize<BOMInfo>(bpObj.BOMInfo, Encoding.Unicode);
                cxtInfo = XmlSerializerHelper.XmlDeserialize<ContextInfo>(bpObj.ContextInfo, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("反序列化BOMInfo失败：{0}", bpObj.BOMInfo));
                strbResult.AppendFormat(string.Format("<ResultInfo error={0} />", string.Format("反序列化BOMInfo失败：{0}", bpObj.BOMInfo)));
                return strbResult.ToString();
            }


            if (bominfoAll.Masters.Count <= 0)
            {
                logger.Error(string.Format("传入的BOMInfo中没有BOM母件信息"));
                strbResult.AppendFormat(string.Format("<ResultInfo error={0} />", "传入的BOMInfo中没有BOM母件信息"));
                return strbResult.ToString();
            }

            Organization beOrgContext = Organization.FindByCode(cxtInfo.OrgCode);//原历史版本
            #endregion

            ImportBOMSv svModify = new ImportBOMSv();
            List<BOMMasterDTO4CreateSv> lstBOMMasterModifyDTO = new List<BOMMasterDTO4CreateSv>();
            svModify.IsThrowEx = true;
            CreateBOMSv svCreate = new CreateBOMSv();
            List<BOMMasterDTO4CreateSv> lstBOMMasterCreateDTO = new List<BOMMasterDTO4CreateSv>();
            svCreate.IsThrowEx = true;
            DeletePartBOMSv svDelPart = new DeletePartBOMSv();
            List<BOMMasterDTO4CreateSv> lstBOMMasterDelPartDTO = new List<BOMMasterDTO4CreateSv>();
            svDelPart.IsThrowEx = true;

            User beUser = User.Finder.FindByID(cxtInfo.UserID);
            ContextDTO dtoContext = new ContextDTO();
            dtoContext.CultureName = cxtInfo.CultureName;// "zh-CN";
            dtoContext.UserCode = beUser.Code;//默认：系统管理员admin
            dtoContext.EntCode = cxtInfo.EnterpriseCode;// "van0323";//测试默认公司：van0323.正式使用请根据需要指定。
            dtoContext.OrgCode = cxtInfo.OrgCode;//

            svCreate.ContextDTO = dtoContext;
            svModify.ContextDTO = dtoContext;
            svDelPart.ContextDTO = dtoContext;

            Int64 l = 1;
            try
            {
                foreach (var bomMaster in bominfoAll.Masters)
                {
                    #region 校验从PLM传过来的组织、物料是否有效
                    if (ValidateNullValue(bomMaster, beOrgContext))
                    {
                        continue;
                    }
                    ItemMaster beItemMaster = ItemMaster.Finder.Find("Code=@code and Org=@org  and Effective.IsEffective=1 and Effective.DisableDate >= @nowdate1 and Effective.EffectiveDate <= @nowdate2",
                        new OqlParam[] { new OqlParam(bomMaster.ItemMasterCode), new OqlParam(beOrgContext.ID), new OqlParam(DateTime.Now.Date.ToShortDateString()),
                        new OqlParam(DateTime.Now.Date.ToShortDateString()),new OqlParam(DateTime.Now.Date.ToShortDateString())});

                    if (beItemMaster == null)
                    {
                        strbError.AppendLine(string.Format(strErrorItem, bomMaster.ItemMasterCode, "", string.Format("BOMInfo中第{0}项物料编码{1}在组织{2}无法找到!",
                            l.ToString(), bomMaster.ItemMasterCode, beOrgContext.Code)));
                        continue;
                    }
                    if (!beItemMaster.IsBOMEnable)
                    {
                        strbError.AppendLine(string.Format(strErrorItem, bomMaster.ItemMasterCode, "", "母件不允许建BOM。"));
                        continue;
                    }
                    //加入料品状态必须为“已核准”的条件:State=2
                    if (beItemMaster.State != ItemStateEnum.Verified)
                    {
                        strbError.AppendLine(string.Format(strErrorItem, bomMaster.ItemMasterCode, "", string.Format("BOMInfo中第{0}项物料编码{1}料品状态不是“已核准状态”!",
                            l.ToString(), bomMaster.ItemMasterCode)));
                        continue;
                    }
                    bool bFor = false;
                    foreach (var bomComp in bomMaster.Components)
                    {
                        ItemMaster beItemComponent = ItemMaster.Finder.Find("Code=@code and Org=@org and Effective.IsEffective=1 and Effective.DisableDate >= @nowdate1 and Effective.EffectiveDate <= @nowdate2",
                        new OqlParam[] { new OqlParam(bomComp.ItemCode), new OqlParam(beOrgContext.ID),
                        new OqlParam(DateTime.Now.Date.ToShortDateString()),new OqlParam(DateTime.Now.Date.ToShortDateString())});
                        if (beItemComponent == null)
                        {
                            strbError.AppendLine(string.Format(strErrorItem, bomMaster.ItemMasterCode, bomComp.ItemCode, string.Format("第{0}项子件物料编码ItemCode{1}在组织{2}无法找到!",
                            l.ToString(), bomComp.ItemCode, beOrgContext.Code)));
                            bFor = true;//当子项物料无法找到异常时，上级的for循环跳转到下一条记录。
                        }
                        if (beItemComponent.State != ItemStateEnum.Verified)
                        {
                            strbError.AppendLine(string.Format(strErrorItem, bomMaster.ItemMasterCode, "", string.Format("第{0}项子件物料编码{1}料品状态不是“已核准状态”!",
                                l.ToString(), bomComp.ItemCode)));
                            bFor = true;
                        }
                    }
                    //当子项物料无法找到异常时，上级的for循环跳转到下一条记录。
                    if (bFor)
                    {
                        continue;
                    }
                    #endregion

                    #region 校验传入的BOM是否存在.如果不存在，则转入到BOM创建服务CreateBOMSv中。
                    //检查上下文组织下料号、BOM种类(自制/委外)、版本号是否存在。一旦存在，则使用修改BOM服务。
                    UFIDA.U9.CBO.MFG.BOM.BOMMaster beBOMMaster = UFIDA.U9.CBO.MFG.BOM.BOMMaster.Finder.Find("Org=@org and ItemMaster=@item and BomType = @bomtype and BOMVersionCode = @bomVersion ",
                        new OqlParam[] { new OqlParam(beOrgContext.ID), new OqlParam(beItemMaster.ID), new OqlParam(bomMaster.BOMType), new OqlParam(bomMaster.BOMVersionCode) });
                    if (beBOMMaster == null)
                    {
                        BOMMasterDTO4CreateSv dtoMasterCreate = CreateBOMMasterDTO(bomMaster, beOrgContext);
                        if (dtoMasterCreate != null)
                        {
                            lstBOMMasterCreateDTO.Add(dtoMasterCreate);
                        }
                        continue;
                    }
                    else
                    {
                        #region 若存在，则转入到BOM修改服务ImportBOMSv中。
                        BOMMasterDTO4CreateSv dtoMasterModify = ModifyBOMMasterDTO(bomMaster, beBOMMaster, beOrgContext);
                        if (dtoMasterModify != null)
                        {
                            lstBOMMasterModifyDTO.Add(dtoMasterModify);
                        }
                        #endregion
                    }
                    #endregion
                    #region 若存在，检查是否存在需要删除的子件加入到DeleteBOMPartSv中。
                    BOMMasterDTO4CreateSv dtoMasterDelPart = DelBOMPartDTO(bomMaster, beBOMMaster);
                    if (dtoMasterDelPart != null)
                    {
                        lstBOMMasterDelPartDTO.Add(dtoMasterDelPart);
                    }
                    #endregion

                    l++;
                }
                //}

                if (lstBOMMasterCreateDTO.Count > 0)
                {
                    svCreate.BOMMasterDTOList = lstBOMMasterCreateDTO;
                    svCreate.ContextDTO = dtoContext;
                    List<LogDTO4CreateSv> lstBOMLog = new List<LogDTO4CreateSv>();
                    lstBOMLog = svCreate.Do();

                    foreach (var itemLog in lstBOMLog)
                    {
                        if (itemLog.IsOperationSuccess)
                        {
                            #region 标准BOMAPI目前不提供成本卷积和子件收货审核属性赋值.这里补充处理
                            string strUpdateIsCostRoll = string.Format("update vv set vv.IsCostRoll = 1 from CBO_BOMMaster vv where vv.ID = {0} and vv.BOMType = 0",
                                itemLog.BOMMasterDTO.BOMMasterID.ToString());
                            string strUpdateRCVApproved = string.Format("update uu set uu.RCVApproved = 1 from CBO_BOMComponent uu "
                            + "join CBO_BOMMaster vv on vv.ID = uu.BOMMaster where vv.ID = {0} and vv.BOMType = 1",
                                itemLog.BOMMasterDTO.BOMMasterID.ToString());
                            //DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), strUpdateIsCostRoll, null);
                            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), strUpdateRCVApproved, null);
                            #endregion
                            strbSuccess.AppendLine(string.Format(strInfoItem, itemLog.BOMMasterDTO.ItemMaster.Code));
                        }
                        else
                        {
                            strbError.AppendLine(string.Format(strErrorItem, itemLog.BOMMasterDTO.ItemMaster.Code, "",
                                string.Format("U9系统创建BOM母件{0}失败:{1}.", itemLog.BOMMasterDTO.ItemMaster.Code, itemLog.ErrorMsg)));
                        }
                    }
                }
                if (lstBOMMasterModifyDTO.Count > 0)
                {
                    svModify.BOMMasterDTOList = lstBOMMasterModifyDTO;
                    svModify.ContextDTO = dtoContext;
                    List<LogDTO4CreateSv> lstBOMLog = new List<LogDTO4CreateSv>();
                    lstBOMLog = svModify.Do();

                    foreach (var itemLog in lstBOMLog)
                    {
                        if (itemLog.IsOperationSuccess)
                        {
                            strbSuccess.AppendLine(string.Format(strInfoItem, itemLog.BOMMasterDTO.ItemMaster.Code));
                        }
                        else
                        {
                            strbError.AppendLine(string.Format(strErrorItem, itemLog.BOMMasterDTO.ItemMaster.Code, "",
                                string.Format("U9系统修改BOM母件{0}失败:{1}.", itemLog.BOMMasterDTO.ItemMaster.Code, itemLog.ErrorMsg)));
                        }
                    }
                }
                if (lstBOMMasterDelPartDTO.Count > 0)
                {
                    svDelPart.BOMMasterDTOList = lstBOMMasterDelPartDTO;
                    svDelPart.ContextDTO = dtoContext;
                    List<LogDTO4CreateSv> lstBOMLog = new List<LogDTO4CreateSv>();
                    svDelPart.Do();

                    //string strBOM = "<BOM ItemMasterCode=\"{0}\" Description=\"删除子件导入成功!\"/>";
                    //foreach (var item in lstBOMMasterDelPartDTO)
                    //{
                    //    strbSuccess.AppendLine(string.Format(strBOM, item.ItemMaster.Code));
                    //}
                }
                strbResult.AppendLine("<ResultInfo>");
                strbResult.AppendLine(strbError.ToString());
                strbResult.AppendLine(strbSuccess.ToString());
                strbResult.AppendLine("</ResultInfo>");
            }
            catch (Exception ex)
            {
                strbResult.AppendLine(string.Format("<ResultInfo error=\"{0}\">", ex.Message));
                strbResult.AppendLine(strbError.ToString());
                strbResult.AppendLine("</ResultInfo>");
            }
            return strbResult.ToString();
        }
        private bool ValidateNullValue(BOMMaster _bom, Organization _beOrgContext)
        {
            bool bRlt = false;
            string strErrorItem = "<errorItem bomMasterCode=\"{0}\" bomCompCode=\"{1}\" errorDescription=\"{2}\" />";

            string strError = "<Error Description=\"{0}\" />";

            if (string.IsNullOrEmpty(_bom.ItemMasterCode))
            {
                strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, "", "母件物料编码ItemMasterCode为空"));
                bRlt = true;
            }
            if (_bom.BOMType != 0 && _bom.BOMType != 1)
            {
                strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, "", "BOM类型BOMType指定错误"));
                bRlt = true;
            }
            ItemMaster beItemMaster = ItemMaster.Finder.Find("Code=@code and Org=@org",
                        new OqlParam[] { new OqlParam(_bom.ItemMasterCode), new OqlParam(_beOrgContext.ID) });
            if (beItemMaster == null)
            {
                strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, "", string.Format("母件料号{0}不存在。", _bom.ItemMasterCode)));
                bRlt = true;
            }

            int i = 1;
            foreach (var bomComp in _bom.Components)
            {
                if (string.IsNullOrEmpty(bomComp.ItemCode))
                {
                    strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, "", string.Format("明细子项第{0}项中ItemCode为空。", i.ToString())));
                    bRlt = true;
                }

                ItemMaster beItemComponent = ItemMaster.Finder.Find("Code=@code and Org=@org",
                        new OqlParam[] { new OqlParam(bomComp.ItemCode), new OqlParam(_beOrgContext.ID) });
                if (beItemComponent == null)
                {
                    strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, bomComp.ItemCode, string.Format("子件料号{0}不存在。", bomComp.ItemCode)));
                    bRlt = true;
                }

                if (beItemComponent != null)
                {
                    if (beItemComponent.Effective.EffectiveDate.CompareTo(DateTime.Now.Date) > 0)
                    {
                        strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, bomComp.ItemCode,
                            string.Format("子件料号{0}生效日期{1}晚于当前日期。", bomComp.ItemCode, beItemComponent.Effective.EffectiveDate.ToShortDateString())));
                        bRlt = true;
                    }
                }
                i++;
            }
            return bRlt;
        }

        /// <summary>
        /// 创建新增BOM服务的DTO
        /// </summary>
        /// <param name="_bom"></param>
        /// <returns></returns>
        private BOMMasterDTO4CreateSv CreateBOMMasterDTO(BOMMaster _bom, Organization _beOrgContext)
        {
            BOMMasterDTO4CreateSv dtoMaster = new BOMMasterDTO4CreateSv();
            try
            {
                if (string.IsNullOrEmpty(_bom.ItemMasterCode))
                {
                    throw new Exception("母件料号编码传入为空字符串。");
                }
                ItemMaster beItemMaster = ItemMaster.Finder.Find("Code=@code and Org=@org",
                    new OqlParam[] { new OqlParam(_bom.ItemMasterCode), new OqlParam(_beOrgContext.ID) });
                if (beItemMaster == null)
                {
                    throw new Exception(string.Format("母件料号{0}在组织{1}下不存在！", _bom.ItemMasterCode, _beOrgContext.Code));
                }
                #region 检查历史BOM上版本的生效、失效日期。不允许同一天内两次版本升级。
                OqlParamList lstParam = new OqlParamList();
                string opath = " Org=@org and ItemMaster=@item and BOMType=@bomType ";
                lstParam.Add(new OqlParam(_beOrgContext.ID));
                lstParam.Add(new OqlParam(beItemMaster.ID));
                lstParam.Add(new OqlParam(_bom.BOMType));

                UFIDA.U9.CBO.MFG.BOM.BOMMaster.EntityList lstBomOld = new UFIDA.U9.CBO.MFG.BOM.BOMMaster.EntityList();

                lstBomOld = UFIDA.U9.CBO.MFG.BOM.BOMMaster.Finder.FindAll(opath, lstParam.ToArray());
                foreach (var itemOldBom in lstBomOld)
                {
                    if (itemOldBom.EffectiveDate.Date == DateTime.Now.Date)
                    {
                        throw new Exception(string.Format("BOM母件{0}在{1}有版本{2}不允许同一天叠加版本升级。",
                            _bom.ItemMasterCode, DateTime.Today.ToShortDateString(), itemOldBom.BOMVersionCode));
                    }
                }
                #endregion


                #region BOM表头
                dtoMaster.Org = new CommonArchiveDataDTO();
                dtoMaster.Org.ID = _beOrgContext.ID;
                dtoMaster.Org.Code = _beOrgContext.Code;
                dtoMaster.Org.Name = _beOrgContext.Name;

                //不启用货主组织
                //dtoMaster.OwnerOrg = new CommonArchiveDataDTO();
                //dtoMaster.OwnerOrg.ID = beOrg.ID;
                //dtoMaster.OwnerOrg.Code = beOrg.Code;
                //dtoMaster.OwnerOrg.Name = beOrg.Name;

                dtoMaster.ItemMaster = new CommonArchiveDataDTO();
                dtoMaster.ItemMaster.ID = beItemMaster.ID;
                dtoMaster.ItemMaster.Code = beItemMaster.Code;
                dtoMaster.ItemMaster.Name = beItemMaster.Name;
                AlternateTypesEnum enumAlter = AlternateTypesEnum.GetFromValue(_bom.BOMType);//BOM类型
                if (enumAlter == null)
                {
                    throw new Exception(string.Format("传入的BOM类型值{0}在U9中无法找到对应值。", _bom.BOMType.ToString()));
                }

                //CBOBOM.BOMVersion beMasterBOMVersion = CBOBOM.BOMVersion.CreateBOMVersion(beItemMaster.Key, enumAlter, _bom.BOMVersionCode, _beOrgContext.Key);

                dtoMaster.BOMVersionCode = _bom.BOMVersionCode;//默认
                dtoMaster.AlternateType = enumAlter;
                dtoMaster.Lot = 1;
                dtoMaster.ProductUOM = new CommonArchiveDataDTO();
                dtoMaster.ProductUOM.ID = beItemMaster.ManufactureUOM.ID;
                dtoMaster.ProductUOM.Code = beItemMaster.ManufactureUOM.Code;
                dtoMaster.ProductUOM.Name = beItemMaster.ManufactureUOM.Name;
                dtoMaster.EffectiveDate = DateTime.Now.Date;
                dtoMaster.DisableDate = DateTime.MaxValue;
                dtoMaster.FromQty = 0;
                dtoMaster.ToQty = 0;
                dtoMaster.IsPrimaryLot = true;
                dtoMaster.Explain = _bom.Explain;
                dtoMaster.BOMType = BOMTypeEnum.GetFromValue(_bom.BOMType);
                //成本卷积IsCostRoll 在接口DTO中未提供
                dtoMaster.Status = MFGDocStatusEnum.Approved;//已核准

                dtoMaster.BOMComponents = new List<BOMComponentDTO4CreateSv>();
                #endregion

                #region BOM表体
                int iSeq = 10;
                bool isAllBomCompOK = true;
                foreach (var bomComponent in _bom.Components)
                {
                    BOMComponentDTO4CreateSv dtoComponent = CreateBOMComponentDTO(bomComponent, iSeq, _bom, _beOrgContext);
                    if (dtoComponent == null)
                    {
                        isAllBomCompOK = false;
                        continue;
                    }
                    dtoMaster.BOMComponents.Add(dtoComponent);
                    iSeq += 10;
                }
                #endregion

                if (!isAllBomCompOK)
                {
                    dtoMaster = null;
                }
            }
            catch (Exception ex)
            {
                strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, "", ex.Message));
                dtoMaster = null;
            }
            return dtoMaster;
        }

        /// <summary>
        /// 创建修改BOM服务的DTO
        /// </summary>
        /// <param name="_bom"></param>
        /// <param name="_beBOM"></param>
        /// <returns></returns>
        private BOMMasterDTO4CreateSv ModifyBOMMasterDTO(BOMMaster _bom, UFIDA.U9.CBO.MFG.BOM.BOMMaster _beBOM, Organization _beOrgContext)
        {
            BOMMasterDTO4CreateSv dtoMasterModify = PubMethod.GetDataOfBOMMaster(_beBOM);
            try
            {
                //清除系统原有子件清单
                dtoMasterModify.BOMComponents.Clear();
                int iSeq = PubMethod.GetNextMaxSequence(_beBOM);
                //加入待修改的子件清单
                bool isAllBomCompOK = true;
                foreach (var bomComp in _bom.Components)
                {
                    UFIDA.U9.CBO.MFG.BOM.BOMComponent.EntityList elstBOMComponent = UFIDA.U9.CBO.MFG.BOM.BOMComponent.Finder.FindAll("BOMMaster=@BOMID and ItemMaster.Code=@ItemCode ",
                    new OqlParam[] { new OqlParam(_beBOM.ID), new OqlParam(bomComp.ItemCode) });
                    //在系统后台根据BOM主ID和子件的物料编码检索，如果有此子件则由GetDataOfBOMComponent直接创建子件DTO添加dtoMasterModify。
                    if (elstBOMComponent != null && elstBOMComponent.Count > 0)
                    {
                        //处理母件下有相同的子件物料的明细行
                        for (int i = 0; i < elstBOMComponent.Count; i++)
                        {
                            UFIDA.U9.CBO.MFG.BOM.BOMComponent beBOMComponent = elstBOMComponent[i];
                            BOMComponentDTO4CreateSv dtoComponent = PubMethod.GetDataOfBOMComponent(beBOMComponent);

                            if (dtoMasterModify.BOMComponents.Contains(dtoComponent))
                            {
                                continue;
                            }
                            else
                            {
                                //修改内容
                                dtoComponent.UsageQty = bomComp.UsageQty;//用量
                                dtoComponent.Scrap = bomComp.Scrap;//损耗率
                                dtoComponent.ParentQty = bomComp.ParentQty;//母件底数
                                dtoComponent.Remark = bomComp.Remark;//备注

                                if (!String.IsNullOrEmpty(bomComp.CompProject))
                                {
                                    Project itemProject = Project.FindByCode(bomComp.CompProject);
                                    if (itemProject != null)
                                    {
                                        dtoComponent.CompProject = new CommonArchiveDataDTO();
                                        dtoComponent.CompProject.ID = itemProject.ID;
                                        dtoComponent.CompProject.Code = itemProject.Code;
                                        dtoComponent.CompProject.Name = itemProject.Name;
                                    }
                                }
                                else
                                {
                                    //原接口未对项目置空的处理(UFIDA.U9.ISV.MFG.BOM.PubMethod.UpdateBOMComponent)
                                    string strUpdateVersion = string.Format("update uu set uu.CompProject = null from CBO_BOMComponent uu "
                                    + "where uu.ID = {0}", beBOMComponent.ID.ToString());
                                    DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), strUpdateVersion, null);

                                    //dtoComponent.CompProject.ID = 1L;//
                                    //dtoComponent.CompProject.Code = string.Empty;
                                    //dtoComponent.CompProject.Name = string.Empty;
                                    //dtoComponent.CompProject = new CommonArchiveDataDTO();
                                }

                                dtoMasterModify.BOMComponents.Add(dtoComponent);
                                break;
                            }
                        }
                    }
                    else
                    {
                        #region 新增子件
                        BOMComponentDTO4CreateSv dtoComponent = CreateBOMComponentDTO(bomComp, iSeq, _bom, _beBOM.Org);
                        if (dtoComponent == null)
                        {
                            isAllBomCompOK = false;
                        }
                        dtoMasterModify.BOMComponents.Add(dtoComponent);
                        iSeq += 10;
                        #endregion
                    }
                }
                if (!isAllBomCompOK)
                {
                    dtoMasterModify = null;
                }
            }
            catch (Exception ex)
            {
                strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, "", ex.Message));
                dtoMasterModify = null;
            }
            return dtoMasterModify;
        }

        /// <summary>
        /// 创建删除子件BOM服务的DTO
        /// </summary>
        /// <param name="_bom"></param>
        /// <param name="_beBOM"></param>
        /// <returns></returns>
        private BOMMasterDTO4CreateSv DelBOMPartDTO(BOMMaster _bom, UFIDA.U9.CBO.MFG.BOM.BOMMaster _beBOM)
        {
            BOMMasterDTO4CreateSv dtoMasterDelPart = null;

            foreach (var beBOMPart in _beBOM.BOMComponents)
            {
                bool bCompIsExists = false;
                foreach (var bompart in _bom.Components)
                {
                    //当传入的BOM子件中没有该物料编码，则从BOM中删除此子件
                    if (bompart.ItemCode == beBOMPart.ItemMaster.Code)
                    {
                        bCompIsExists = true;
                        continue;
                    }
                }
                
                if (!bCompIsExists)
                {
                    if (dtoMasterDelPart == null)
                    {
                        dtoMasterDelPart = PubMethod.GetDataOfBOMMaster(_beBOM);
                        dtoMasterDelPart.BOMComponents.Clear();
                    }

                    //检索出原始BOM中存在的子件加入到需删除子件服务的dtoMasterDelPart中
                    UFIDA.U9.CBO.MFG.BOM.BOMComponent.EntityList elstBOMComponent = UFIDA.U9.CBO.MFG.BOM.BOMComponent.Finder.FindAll("BOMMaster=@BOMID and ItemMaster.Code=@ItemCode ",
               new OqlParam[] { new OqlParam(_beBOM.ID), new OqlParam(beBOMPart.ItemMaster.Code) });
                    if (elstBOMComponent != null && elstBOMComponent.Count > 0)
                    {
                        for (int i = 0; i < elstBOMComponent.Count; i++)
                        {
                            UFIDA.U9.CBO.MFG.BOM.BOMComponent beBOMComponent = elstBOMComponent[i];
                            BOMComponentDTO4CreateSv dtoComponent = PubMethod.GetDataOfBOMComponent(beBOMComponent);
                            if (dtoMasterDelPart.BOMComponents.Contains(dtoComponent))
                            {
                                continue;
                            }
                            else
                            {
                                dtoMasterDelPart.BOMComponents.Add(dtoComponent);
                            }
                        }
                    }
                }
            }

            return dtoMasterDelPart;
        }

        private UFIDA.U9.ISV.MFG.BOM.BOMComponentDTO4CreateSv CreateBOMComponentDTO(BOMComponent _bomComponent, int _iSeq, BOMMaster _bom, Organization _beOrg)
        {
            try
            {
                UFIDA.U9.ISV.MFG.BOM.BOMComponentDTO4CreateSv dtoComponent = new UFIDA.U9.ISV.MFG.BOM.BOMComponentDTO4CreateSv();

                dtoComponent.Sequence = _iSeq;
                dtoComponent.OperationNum = "10";//默认工序号：空
                ItemMaster beItemComponent = ItemMaster.Finder.Find("Code=@code and Org=@org",
                new OqlParam[] { new OqlParam(_bomComponent.ItemCode), new OqlParam(_beOrg.ID) });
                if (beItemComponent == null)
                {
                    throw new Exception(string.Format("子件料号{0}在组织{1}下不存在！", _bomComponent.ItemCode, _beOrg.Code));
                }

                dtoComponent.ItemMaster = new CommonArchiveDataDTO();
                dtoComponent.ItemMaster.ID = beItemComponent.ID;
                dtoComponent.ItemMaster.Code = beItemComponent.Code;
                dtoComponent.ItemMaster.Name = beItemComponent.Name;
                //子项物料启用了版本管理
                if (beItemComponent.ItemMasterVersions != null && beItemComponent.ItemMasterVersions.Count > 0)
                {
                    dtoComponent.ItemVersionCode = _bomComponent.ItemVersionCode;
                }

                dtoComponent.ComponentType = ComponentTypeEnum.StandardComp;//标准

                dtoComponent.BOMCompSubstituteDTO4CreateSv = new List<BOMComponentDTO4CreateSv>();
                dtoComponent.UsageQtyType = UsageQuantityTypeEnum.Variable;
                dtoComponent.UsageQty = _bomComponent.UsageQty;
                dtoComponent.IssueUOM = new CommonArchiveDataDTO();
                dtoComponent.IssueUOM.ID = beItemComponent.MaterialOutUOM.ID;
                dtoComponent.IssueUOM.Code = beItemComponent.MaterialOutUOM.Code;
                dtoComponent.IssueUOM.Name = beItemComponent.MaterialOutUOM.Name;
                dtoComponent.PlanPercent = 0;
                dtoComponent.IsCharge = true;
                dtoComponent.FromQty = 0;
                dtoComponent.ToQty = 0;
                dtoComponent.IsEffective = true;
                dtoComponent.SubstituteStyle = SubstituteStyleEnum.None;
                dtoComponent.ScrapType = ScrapTypeEnum.SingleScrap;
                dtoComponent.FixedScrap = beItemComponent.MfgInfo.ImmovableWaste; //固定损耗率
                dtoComponent.Scrap = _bomComponent.Scrap;//损耗率
                dtoComponent.ParentQty = _bomComponent.ParentQty;


                dtoComponent.SupplyStyle = SupplyStyleEnum.Org;
                dtoComponent.IssueOrg = new CommonArchiveDataDTO();
                dtoComponent.IssueOrg.ID = _beOrg.ID;
                dtoComponent.IssueOrg.Code = _beOrg.Code;
                dtoComponent.IssueOrg.Name = _beOrg.Name;

                if (beItemComponent.InventoryInfo.Warehouse != null)
                {
                    dtoComponent.SupplyWareHouse = new CommonArchiveDataDTO();//供应地点
                    dtoComponent.SupplyWareHouse.ID = beItemComponent.InventoryInfo.Warehouse.ID;
                    dtoComponent.SupplyWareHouse.Code = beItemComponent.InventoryInfo.Warehouse.Code;
                    dtoComponent.SupplyWareHouse.Name = beItemComponent.InventoryInfo.Warehouse.Name;
                }
                dtoComponent.SetChkAtComplete = false;
                dtoComponent.SetChkAtOptComplete = false;
                dtoComponent.SetChkAtOptStart = false;
                if (_bom.BOMType == 1)
                {
                    dtoComponent.IsWholeSetIssue = true;
                }
                else
                {
                    dtoComponent.IsWholeSetIssue = false;
                }
                dtoComponent.StandardMaterialScale = 0;
                dtoComponent.IsOverIssue = false;
                dtoComponent.IsATP = false;
                dtoComponent.IsCTP = false;
                dtoComponent.LeadTimeOffSet = 0;
                dtoComponent.IsMandatory = false;
                dtoComponent.IsExclude = false;
                dtoComponent.IsDefault = false;
                dtoComponent.IsOptionDependent = false;
                dtoComponent.IsCalcPrice = false;
                dtoComponent.MinSelectedQty = 1;
                dtoComponent.MaxSelectedQty = 1;
                dtoComponent.CostElement = new CommonArchiveDataDTO();
                CostElement beCost = CostElement.Finder.Find("Name='材料费'", new OqlParam[] { });
                if (beCost != null)
                {
                    dtoComponent.CostElement.ID = beCost.ID;
                    dtoComponent.CostElement.Code = beCost.Code;
                    dtoComponent.CostElement.Name = beCost.Name;
                }
                dtoComponent.CostPercent = 0;//成本百分比
                dtoComponent.Remark = _bomComponent.Remark;
                //直接读取料品属性，属于虚拟
                if (beItemComponent.ItemFormAttribute == ItemTypeAttributeEnum.Phantom)
                {
                    //如果子项勾选‘虚拟’，此处选择‘不发料
                    dtoComponent.IsPhantomPart = true;
                    dtoComponent.IssueStyle = IssueStyleEnum.Phantom;
                }
                else
                {
                    dtoComponent.IsPhantomPart = false;
                    dtoComponent.IssueStyle = IssueStyleEnum.Push;
                }
                //收货审核属性RCVApproved，标准接口未提供。

                dtoComponent.IsCeiling = false;//是否取整
                dtoComponent.IsSpecialUseItem = false;//是否专项控制：默认不启用
                if (!String.IsNullOrEmpty(_bomComponent.CompProject))
                {
                    Project itemProject = Project.Finder.Find("Name=@name and Org=@org",
                new OqlParam[] { new OqlParam(_bomComponent.CompProject), new OqlParam(_beOrg.ID) });

                    if (itemProject != null)
                    {
                        dtoComponent.CompProject = new CommonArchiveDataDTO();
                        dtoComponent.CompProject.ID = itemProject.ID;
                        dtoComponent.CompProject.Code = itemProject.Code;
                        dtoComponent.CompProject.Name = itemProject.Name;

                        dtoComponent.IsSpecialUseItem = true;//是否专项控制：指定项目后则设定为启用。
                    }
                    else
                    {
                        throw new Exception(string.Format("在{0}组织下没有找到{1}的项目号.", _beOrg.Name, _bomComponent.CompProject));
                    }
                }

                return dtoComponent;
            }
            catch (Exception ex)
            {
                strbError.AppendLine(string.Format(strErrorItem, _bom.ItemMasterCode, _bomComponent.ItemCode, ex.Message));
                return null;
            }
        }
	}

	#endregion
	
	
}