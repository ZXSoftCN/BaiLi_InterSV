namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

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
			
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}