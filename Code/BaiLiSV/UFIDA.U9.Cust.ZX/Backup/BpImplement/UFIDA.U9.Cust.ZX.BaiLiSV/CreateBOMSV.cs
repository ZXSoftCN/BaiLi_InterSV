





namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Reflection;
	using UFSoft.UBF.AopFrame; 	

	/// <summary>
	/// 创建BOM服务 business operation
	/// 
	/// </summary>
	[Serializable]	
	public partial class CreateBOMSV
	{
	    #region Fields
		private System.String contextInfo;
		private System.String bOMInfo;
		
	    #endregion
		
	    #region constructor
		public CreateBOMSV()
		{}
		
	    #endregion

	    #region member		
		/// <summary>
		/// 上下文	
		/// 创建BOM服务.Misc.上下文
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
		/// BOM信息	
		/// 创建BOM服务.Misc.BOM信息
		/// </summary>
		/// <value></value>
		public System.String BOMInfo
		{
			get
			{
				return this.bOMInfo;
			}
			set
			{
				bOMInfo = value;
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
