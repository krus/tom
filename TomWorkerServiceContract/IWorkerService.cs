using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace TomWorkerServiceContract
{
	[ServiceContract(Name = "WorkerService", Namespace = "http://www.3c.org/")]
	public interface IWorkerService
	{
		[OperationContract]
		void Complete(string serviceName);
	}
}
