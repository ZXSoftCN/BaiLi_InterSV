








namespace UFIDA.U9.Cust.ZX.BaiLiSV.Proxy
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using System.ServiceModel;
	using System.Runtime.Serialization;
	using UFSoft.UBF;
	using UFSoft.UBF.Exceptions;
	using UFSoft.UBF.Util.Context;
	using UFSoft.UBF.Service;
	using UFSoft.UBF.Service.Base ;

    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.UFIDA.org", Name="UFIDA.U9.Cust.ZX.BaiLiSV.ICreateItemMasterSV")]
    public interface ICreateItemMasterSV
    {
		[ServiceKnownType(typeof(ApplicationContext))]
		[ServiceKnownType(typeof(PlatformContext))]
		[ServiceKnownType(typeof(ThreadContext))]
		[ServiceKnownType(typeof( UFSoft.UBF.Business.BusinessException))]
		[ServiceKnownType(typeof( UFSoft.UBF.Business.EntityNotExistException))]
		[ServiceKnownType(typeof( UFSoft.UBF.Business.AttributeInValidException))]
		[ServiceKnownType(typeof(UFSoft.UBF.Business.AttrsContainerException))]
		[ServiceKnownType(typeof(UFSoft.UBF.Exceptions.MessageBase))]
			[FaultContract(typeof(UFSoft.UBF.Service.ServiceLostException))]
		[FaultContract(typeof(UFSoft.UBF.Service.ServiceException))]
		[FaultContract(typeof(UFSoft.UBF.Service.ServiceExceptionDetail))]
		[FaultContract(typeof(ExceptionBase))]
		[FaultContract(typeof(Exception))]
		[OperationContract()]
		System.String Do(IContext context, out IList<MessageBase> outMessages ,System.String contextInfo, System.String itemModule, System.String itemInfo);
    }
	[Serializable]    
    public class CreateItemMasterSVProxy : ServiceProxyBase//, UFIDA.U9.Cust.ZX.BaiLiSV.Proxy.ICreateItemMasterSV
    {
	#region Fields	
				private System.String contextInfo ;
						private System.String itemModule ;
						private System.String itemInfo ;
			
	#endregion	
		
	#region Properties
	
				

		/// <summary>
		/// 上下文 (该属性可为空,且无默认值)
		/// 创建料品服务.Misc.上下文
		/// </summary>
		/// <value>System.String</value>
		public System.String ContextInfo
		{
			get	
			{	
				return this.contextInfo;
			}

			set	
			{	
				this.contextInfo = value;	
			}
		}		
						

		/// <summary>
		/// 模板料号 (该属性可为空,且无默认值)
		/// 创建料品服务.Misc.模板料号
		/// </summary>
		/// <value>System.String</value>
		public System.String ItemModule
		{
			get	
			{	
				return this.itemModule;
			}

			set	
			{	
				this.itemModule = value;	
			}
		}		
						

		/// <summary>
		/// 料品信息 (该属性可为空,且无默认值)
		/// 创建料品服务.Misc.料品信息
		/// </summary>
		/// <value>System.String</value>
		public System.String ItemInfo
		{
			get	
			{	
				return this.itemInfo;
			}

			set	
			{	
				this.itemInfo = value;	
			}
		}		
			
	#endregion	


	#region Constructors
        public CreateItemMasterSVProxy()
        {
        }
        #endregion
        
        #region 跨site调用
        public System.String Do(string targetSite)
        {
  			InitKeyList() ;
			System.String result = (System.String)InvokeBySite<UFIDA.U9.Cust.ZX.BaiLiSV.Proxy.ICreateItemMasterSV>(targetSite);
			return GetRealResult(result);
        }
        #endregion end跨site调用

		#region 跨组织调用
        public System.String Do(long targetOrgId)
        {
  			InitKeyList() ;
			System.String result = (System.String)InvokeByOrg<UFIDA.U9.Cust.ZX.BaiLiSV.Proxy.ICreateItemMasterSV>(targetOrgId);
			return GetRealResult(result);
        }
		#endregion end跨组织调用

		#region Public Method
		
        public System.String Do()
        {
  			InitKeyList() ;
 			System.String result = (System.String)InvokeAgent<UFIDA.U9.Cust.ZX.BaiLiSV.Proxy.ICreateItemMasterSV>();
			return GetRealResult(result);
        }
        
		protected override object InvokeImplement<T>(T oChannel)
        {
			IContext context = ContextManager.Context;			

            ICreateItemMasterSV channel = oChannel as ICreateItemMasterSV;
            if (channel != null)
            {
				return channel.Do(context, out returnMsgs, contextInfo, itemModule, itemInfo);
	    }
            return  null;
        }
		#endregion
		
		//处理由于序列化导致的返回值接口变化，而进行返回值的实际类型转换处理．
		private System.String GetRealResult(System.String result)
		{

				return result ;
		}
		#region  Init KeyList 
		//初始化SKey集合--由于接口不一样.BP.SV都要处理
		private void InitKeyList()
		{
			System.Collections.Hashtable dict = new System.Collections.Hashtable() ;
															
		}
		#endregion 

    }
}



