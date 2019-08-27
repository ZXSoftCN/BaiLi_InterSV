





namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Reflection;
	using UFSoft.UBF.AopFrame; 	

	/// <summary>
	/// 创建SO服务 business operation
	/// 
	/// </summary>
	[Serializable]	
	public partial class CreateSOSV
	{
	    #region Fields
		private System.String contextInfo;
		private System.String docTypeCode;
		private System.String docInfo;
		
	    #endregion
		
	    #region constructor
		public CreateSOSV()
		{}
		
	    #endregion

	    #region member		
		/// <summary>
		/// 上下文	
		/// 创建SO服务.Misc.上下文
		/// </summary>
		/// <value></value>
		public System.String ContextInfo
		{
			get
			{
				return this.contextInfo;
			}
			set
			{
				contextInfo = value;
			}
		}
		/// <summary>
		/// 预设单据类型	
		/// 创建SO服务.Misc.预设单据类型
		/// </summary>
		/// <value></value>
		public System.String DocTypeCode
		{
			get
			{
				return this.docTypeCode;
			}
			set
			{
				docTypeCode = value;
			}
		}
		/// <summary>
		/// 单据信息	
		/// 创建SO服务.Misc.单据信息
		/// </summary>
		/// <value></value>
		public System.String DocInfo
		{
			get
			{
				return this.docInfo;
			}
			set
			{
				docInfo = value;
			}
		}
	    #endregion
		
	    #region do method 
		[Transaction(UFSoft.UBF.Transactions.TransactionOption.Supported)]
		[Logger]
		[Authorize]
		public System.String Do()
		{	
		    BaseStrategy selector = Select();	
				System.String result =  (System.String)selector.Execute(this);	
		    
			return result ; 
		}			
	    #endregion 					
	} 		
}
