using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TomWorker.Configuration;

namespace TomWorker
{
	public class Context
	{
		private string workerServieName;

		public int WorkerId { get; set; }
		public int NodeId { get; set; }
		public string MQUri { get; set; }
		public string ExecutorPath { get; set; }
		public string WorkerServieName { get; set; }
		public string WorkerServiceIP { get; set; }
		public int WorkerServicePort { get; set; }
		public int MaxTasks { get; set; }
		public int MaxExecutors { get; set; }
		public int Executors { get; set; }
		public TomWorkerSection ServiceConfig { get; private set; }


		private Context()
		{
			this.ServiceConfig = (TomWorkerSection)System.Configuration.ConfigurationManager.GetSection("tomWorker");
		}

		private static Context s_CurrentContext;
		public static Context Current
		{
			get
			{
				if(s_CurrentContext != null)
				{
					return s_CurrentContext;
				}

				Context	context = new Context();
				Interlocked.CompareExchange<Context>(ref s_CurrentContext, context, null);
				return s_CurrentContext;
			}
		}
	}
}
