namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

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

		public override object Do(object obj)
		{						
			CreateItemMasterSV bpObj = (CreateItemMasterSV)obj;
			
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}