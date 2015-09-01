using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using System.ServiceModel;
using TomWorkerServiceContract;
using TomMasterServiceContract.Entities;
using RabbitMQ.Client.Events;
using System.Runtime.Serialization.Formatters.Binary;
using TomComm;
using System.IO;

namespace TomWorker
{
	public partial class WinService : ServiceBase
	{
		private static log4net.ILog LOG = log4net.LogManager.GetLogger("WinService");

		private Thread thread;
		private ConnectionFactory connectFactory;
		private bool isStop;
		private string queueName;
		public WinService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			log4net.Config.DOMConfigurator.Configure();

			connectFactory = new ConnectionFactory();

			int nodeId = 0;
			int workerId = 0;
			int maxTasks = 0;
			int maxExecutors;
			string executorPath = System.Configuration.ConfigurationManager.AppSettings["ExecutorPath"];
			string workerServiceIP = System.Configuration.ConfigurationManager.AppSettings["WorkerServiceIP"];

			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["MaxTasks"], out maxTasks);
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["MaxExecutors"], out maxExecutors);
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["NodeId"], out nodeId);

			if (maxTasks == 0)
			{
				LOG.Error("最大执行任务数配置错误");
				return;
			}

			if (maxExecutors == 0)
			{
				LOG.Error("最大任务分配进程数配置错误");
				return;
			}

			if (nodeId == 0)
			{
				LOG.Error("节点配置错误");
				return;
			}

			WorkerInfo workerInfo = null;
			using (MasterServiceClient client = new MasterServiceClient())
			{
				workerInfo = client.AcquireWorker(nodeId);
			}


			Context.Current.WorkerId = workerId;
			Context.Current.NodeId = nodeId;
			Context.Current.MaxExecutors = maxExecutors;
			Context.Current.MaxTasks = maxTasks;
			Context.Current.WorkerServiceIP = workerServiceIP;
			Context.Current.WorkerServicePort = workerInfo.WorkerServicePort;
			Context.Current.WorkerServieName = workerInfo.WorkerServiceName;
			Context.Current.MQUri = workerInfo.MQUri;

			this.queueName = "tom.node." + nodeId;

			this.OpenWorkerService();
			this.StartThread();
		}

		private void OpenWorkerService()
		{
			NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
			binding.Security.Mode = SecurityMode.None;

			using (ServiceHost host = new ServiceHost(typeof(WorkerServiceImpl)))
			{
				host.AddServiceEndpoint(typeof(IWorkerService), binding,
					new Uri(string.Format("net.tcp://{0}:{1}/{2}", Context.Current.WorkerServiceIP, Context.Current.WorkerServicePort, Context.Current.WorkerServieName)));

				if (host.State != CommunicationState.Opening)
					host.Open();
			}
		}

		private void StartThread()
		{
			this.thread = new Thread(Proc);
			this.thread.IsBackground = true;
			this.thread.Start();
		}

		private void StopThread()
		{

		}

		protected override void OnStop()
		{
			this.StopThread();
		}

		private void Proc()
		{
			ConnectionFactory factory = new ConnectionFactory();
			factory.Uri = Context.Current.MQUri;
			IConnection conn = factory.CreateConnection();
			IModel ch = conn.CreateModel();
			ch.QueueDeclare(this.queueName, false, false, false, null);
			QueueingBasicConsumer consumer = new QueueingBasicConsumer(ch);
			ch.BasicConsume(this.queueName, false, consumer);

			BinaryFormatter binFormat = new BinaryFormatter();

			while (!this.isStop)
			{
				BasicDeliverEventArgs args = consumer.Queue.Dequeue();
				IBasicProperties props = args.BasicProperties;

				byte[] body = args.Body;

				using (MemoryStream mem = new MemoryStream(body))
				{
					Request request = (Request)binFormat.Deserialize(mem);

				}
			}
		}
	}
}
