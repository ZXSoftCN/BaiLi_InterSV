﻿

namespace UFIDA.U9.Cust.ZX.BaiLiSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using NUnit.Framework;
	
	/// <summary>
	/// Business operation test
	/// </summary> 
	[TestFixture]		
	public class CreateItemMasterSVTest
	{
		private Proxy.CreateItemMasterSVProxy obj = new Proxy.CreateItemMasterSVProxy();

		public CreateItemMasterSVTest()
		{
		}
		#region AutoTestCode ...
		[Test]
		public void TestDo()
		{
			obj.Do() ;  
		
		}
		#endregion 				
	}
	
}