using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomMasterServiceContract;
using TomMasterServiceContract.Entities;

namespace TomMaster
{
	public class MasterServiceImpl : IMasterService
	{
		#region IMasterService 成员

		public WorkerInfo AcquireWorker(int hostId)
		{
			WorkerInfo info = new WorkerInfo();
			info.MQUri = "amqp://test:test@115.29.236.46:5672";
			info.WorkerId = 1;
			info.WorkerServiceName = "WorkerServiceName_" + hostId;
			info.WorkerServicePort = 1188;
			return info;
		}

		#endregion

		#region IMasterService 成员


		public void Heartbeat(int workerId)
		{
		}

		#endregion
	}
}
