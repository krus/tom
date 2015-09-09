using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomComm;

namespace TomTestService
{
	public class Test
	{
		[TomService("test.test")]
		public static object test(Request req)
		{
			return "test";
		}
	}
}
