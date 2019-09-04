





namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Reflection;
	using UFSoft.UBF.AopFrame; 	

	/// <summary>
	/// 创建料品服务 business operation
	/// 
	/// </summary>
	[Serializable]	
	public partial class CreateItemMasterSV
	{
	    #region Fields
		private System.String contextInfo;
		private System.String itemModule;
		private System.String itemInfo;
		
	    #endregion
		
	    #region constructor
		public CreateItemMasterSV()
		{}
		
	    #endregion

	    #region member		
		/// <summary>
		/// 上下文	
		/// 创建料品服务.Misc.上下文
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
		/// 模板料号	
		/// 创建料品服务.Misc.模板料号
		/// </summary>
		/// <value></value>
		public System.String ItemModule
		{
			get
			{
				return this.itemModule;
			}
			set
			{
				itemModule = value;
			}
		}
		/// <summary>
		/// 料品信息	
		/// 创建料品服务.Misc.料品信息
		/// </summary>
		/// <value></value>
		public System.String ItemInfo
		{
			get
			{
				return this.itemInfo;
			}
			set
			{
				itemInfo = value;
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
