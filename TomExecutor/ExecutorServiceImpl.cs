using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomExecutorServiceContract;

namespace TomExecutor
{
	public class ExecutorServiceImpl : IExecutorService
	{
		#region IExecutorService 成员

		public void PushRequest(TomComm.Request request)
		{
			RequestQueue.Push(request);
		}

		public void Shutdown()
		{
		}

		#endregion
	}
}
