using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomWorkerServiceContract;

namespace TomExecutor
{
	public class WorkerServiceClient : ClientBase<IWorkerService>, IWorkerService
	{
		public WorkerServiceClient()
		{
		}

		#region IWorkerService 成员

		public void Complete(string serviceName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
