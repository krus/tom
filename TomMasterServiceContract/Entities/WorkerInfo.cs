using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomMasterServiceContract.Entities
{
	[Serializable]
	public class WorkerInfo
	{
		public int WorkerId { get; set; }
		public string MQUri { get; set; }
		public string WorkerServiceName { get; set; }
		public int WorkerServicePort { get; set; }
	}
}
