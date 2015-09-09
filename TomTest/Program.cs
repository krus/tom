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

			RPCClient c = new RPCClient("amqp://test:test@115.29.236.46:5672");

			for (int i = 0; i < 100; i++)
			{
				c.Request(req);
			}
		}
	}
}
