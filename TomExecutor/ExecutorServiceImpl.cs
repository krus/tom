using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomExecutorServiceContract;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TomComm;

namespace TomExecutor
{
	public class ExecutorServiceImpl : IExecutorService
	{
		#region IExecutorService 成员

		public void PushRequest(Request request)
		{
			if (!Program.Server.IsStop)
			{
				RequestQueue.Push(request);
				Program.Server.Set();
			}
		}

		public void Shutdown()
		{
			Program.Server.Stop();
		}

		#endregion
	}
}
