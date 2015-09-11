using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomMasterServiceContract.Entities;

namespace TomMasterServiceContract
{
	[ServiceContract(Name = "MasterService", Namespace = "http://www.3c.org/")]
	public interface IMasterService
	{
		[OperationContract]
		WorkerInfo AcquireWorker(int appId);

		[OperationContract]
		void Heartbeat(int appId);
	}
}
