namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

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
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}