using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;

namespace TomWorker
{
	public sealed class NetPort
	{
		private int startPort;

		public NetPort(int startPort)
		{
			this.startPort = startPort;
		}

		public int AcquriedPort()
		{
			int port;
			do{
				Interlocked.Increment(ref startPort);
				port = startPort;
			} while (PortInUse(port));
			return port;
		}

		public static bool PortInUse(int port)
		{
			bool inUse = false;

			IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
			IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

			foreach (IPEndPoint endPoint in ipEndPoints)
			{
				if (endPoint.Port == port)
				{
					inUse = true;
					break;
				}
			}
			return inUse;
		}
	}
}
