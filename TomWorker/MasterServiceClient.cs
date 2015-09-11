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
		public MasterServiceClient(string endpointConfigurationName) : base(endpointConfigurationName)
		{
		}

		#region IMasterService 成员

		public TomMasterServiceContract.Entities.WorkerInfo AcquireWorker(int appId)
		{
			return base.Channel.AcquireWorker(appId);
		}

		public void Heartbeat(int appId)
		{
			base.Channel.Heartbeat(appId);
		}

		#endregion
	}
}
