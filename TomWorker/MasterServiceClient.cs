using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomMasterServiceContract;

namespace TomWorker
{
	public class MasterServiceClient : ClientBase<IMasterService>,IMasterService
	{
		public MasterServiceClient()
		{
		}

		#region IMasterService 成员

		public TomMasterServiceContract.Entities.WorkerInfo AcquireWorker(int nodeId)
		{
			return base.Channel.AcquireWorker(nodeId);
		}

		#endregion
	}
}
