





namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Reflection;
	using UFSoft.UBF.AopFrame; 	

	/// <summary>
	/// 查询在制MO服务 business operation
	/// 
	/// </summary>
	[Serializable]	
	public partial class QueryMOSV
	{
	    #region Fields
		private System.String bOMQueryInfo;
		
	    #endregion
		
	    #region constructor
		public QueryMOSV()
		{}
		
	    #endregion

	    #region member		
		/// <summary>
		/// BOM查询信息	
		/// 查询在制MO服务.Misc.BOM查询信息
		/// </summary>
		/// <value></value>
		public System.String BOMQueryInfo
		{
			get
			{
				return this.bOMQueryInfo;
			}
			set
			{
				bOMQueryInfo = value;
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
