using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomRPCClient;
using TomComm;
using System.Reflection;

namespace TomTest
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		static void Main()
		{
			Request req = new Request();
			req.ServiceName = "test.test";

			
			object s = c.Invoke(req);
			for (int i = 0; i < 100; i++)
			{
			}
		}
	}
}
