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
using RabbitMQ.Client.Impl;
using RabbitMQ.Client.Content;
using TomWorker.Configuration;
using LiveTK.Data;

namespace TomWorker
{
	public class Worker
	{
		private static log4net.ILog LOG = log4net.LogManager.GetLogger("Server");

		public static string ExecutorFileName { get; private set; }
		public static string WorkerServiceIP { get; private set; }
		public static int WorkerServicePort { get; private set; }
		public static int MaxTasks { get; private set; }

		static Worker()
		{
			ExecutorFileName = System.Configuration.ConfigurationManager.AppSettings["TomExecutorFileName"];
			WorkerServiceIP = System.Configuration.ConfigurationManager.AppSettings["TomWorkerServiceIP"];

			int maxTasks;
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["TomMaxTasks"], out maxTasks);

			int workerServicePort;
			if(int.TryParse(System.Configuration.ConfigurationManager.AppSettings["TomWorkerServicePort"], out workerServicePort))
			{
				WorkerServicePort = workerServicePort;
			}
		}


		private Thread thread;
		private ConnectionFactory connectFactory;
		private bool isStop;
		private string queueName;
		private string workerServiceName;
		private int appId;
		private int workerId;
		public string MQUri { get; private set; }
		private BinaryFormatter binaryFormatter = new BinaryFormatter();

		public Worker(int appId)
		{
			this.appId = appId;
		}

		public void Start()
		{
			connectFactory = new ConnectionFactory();

			int workerId = 0;
			int maxTasks = 0;
			int maxExecutors;

			if (appId == 0)
			{
				LOG.Error("应用ID配置错误");
				return;
			}

			WorkerInfo workerInfo = null;
			using (MasterServiceClient client = new MasterServiceClient(Consts.TomMasterServiceEndpointConfig))
			{
				workerInfo = client.AcquireWorker(appId);
			}

			this.workerId = workerId;
			this.workerServiceName = "WorkerServiceName_" + this.workerId;
			this.MQUri = workerInfo.MQUri;
			this.queueName = "tom.host." + appId;

			this.InitConfig();
			this.OpenWorkerService();
			this.StartThread();
		}

		public void Stop()
		{
			this.StopThread();
		}

		private void InitConfig()
		{
			this.config = ServiceConfig.GetConfig(appId);
		}

		private void OpenWorkerService()
		{
			NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
			binding.Security.Mode = SecurityMode.None;

			using (ServiceHost host = new ServiceHost(typeof(WorkerServiceImpl)))
			{
				host.AddServiceEndpoint(typeof(IWorkerService), binding,
					new Uri(string.Format("net.tcp://{0}:{1}/{2}", WorkerServiceIP, WorkerServicePort, this.workerServiceName)));

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

		private void Proc()
		{
			ConnectionFactory factory = new ConnectionFactory();
			factory.Uri = this.MQUri;
			IConnection conn = factory.CreateConnection();
			IModel ch = conn.CreateModel();
			ch.QueueDeclare(this.queueName, false, false, false, null);
			QueueingBasicConsumer consumer = new QueueingBasicConsumer(ch);
			ch.BasicConsume(this.queueName, false, consumer);

			Executor executor = null;
			while (!this.isStop)
			{
				BasicDeliverEventArgs args = consumer.Queue.Dequeue();

				Request request;
				using(MemoryStream mem = new MemoryStream(args.Body))
				{
					request = (Request)binaryFormatter.Deserialize(mem);
					request.DeliveryTag = args.DeliveryTag;
				}

				string serviceName = request.ServiceName;


				ServiceNode node = config.ServiceNodes.Find(serviceName);
				if (node != null)
				{
					executor = Executor.Run(this, node);
				}

				if (executor != null)
				{
					executor.PushRequest(request);
				}
				else
				{
					byte[] bytes = null;
					Response response = new Response();

					BinaryFormatter binaryFormatter = new BinaryFormatter();
					using (MemoryStream mem = new MemoryStream())
					{
						binaryFormatter.Serialize(mem, response);
						bytes = mem.ToArray();
					}

					IBasicProperties responseProps = ch.CreateBasicProperties();
					responseProps.CorrelationId = request.CorrelationId;
					ch.BasicPublish(string.Empty, request.ReplyTo, responseProps, bytes);

					LOG.Warn("未找到请求处理服务：" + serviceName);
				}

				ch.BasicAck(args.DeliveryTag, false);
			}

			conn.Close();
			ch.Close();
		}

		private ServiceConfig config;
	}
}
