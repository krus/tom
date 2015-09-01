using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomComm;

namespace TomExecutorServiceContract
{
	[ServiceContract(Name = "ExecutorService", Namespace = "http://www.3c.org/")]
	public interface IExecutorService
	{
		[OperationContract]
		void PushRequest(Request request);

		[OperationContract]
		void Shutdown();
	}
}
