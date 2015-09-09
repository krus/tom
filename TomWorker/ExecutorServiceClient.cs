using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomExecutorServiceContract;
using System.ServiceModel.Channels;
using TomComm;

namespace TomWorker
{
	public class ExecutorServiceClient : ClientBase<IExecutorService>, IExecutorService
	{
		public ExecutorServiceClient(Binding binding, EndpointAddress  remoteAddress)
			: base(binding, remoteAddress)
		{
			
		}

		#region IExecutorService 成员

		public void PushRequest(Request request)
		{
			base.Channel.PushRequest(request);
		}

		#endregion

		#region IExecutorService 成员


		public void Shutdown()
		{
			base.Channel.Shutdown();
		}

		#endregion
	}
}
