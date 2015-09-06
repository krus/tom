using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomExecutorServiceContract;

namespace TomWorker
{
	public class ExecutorServiceClient : ClientBase<IExecutorService>, IExecutorService
	{
		public ExecutorServiceClient()
		{
		}

		#region IExecutorService 成员

		public void PushRequest(EventArgs request)
		{
			base.Channel.PushRequest(request);
		}

		#endregion
	}
}
