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
using System.Collections;

namespace TomWorker
{
	public class Worker
	{
		private static log4net.ILog LOG = log4net.LogManager.GetLogger("Server");

		public static string ExecutorFileName { get; private set; }
		public static string WorkerServiceIP { get; private set; }
		public static int MaxTasks { get; private set; }
		public static int MaxExecutors { get; private set; }
		public static Dictionary<int, Worker> s_Workers;
		public static byte[] NotFoundResponseBody{ get; private set; }

		static Worker()
		{
			MaxExecutors = 20;
			ExecutorFileName = System.Configuration.ConfigurationManager.AppSettings["TomExecutorFileName"];
			WorkerServiceIP = System.Configuration.ConfigurationManager.AppSettings["TomWorkerServiceIP"];

			int maxTasks;
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["TomMaxTasks"], out maxTasks);

			s_Workers = new Dictionary<int, Worker>();

			NotFoundResponseBody = CreateNotFoundResonseBody();
		}

		static byte[] CreateNotFoundResonseBody()
		{
			byte[] bytes = null;
			Response response = new Response();
			response.Status = 404;

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream mem = new MemoryStream())
			{
				binaryFormatter.Serialize(mem, response);
				bytes = mem.ToArray();
			}
			return bytes;
		}


		private Thread thread;
		private ConnectionFactory factory;
		private bool isStop;
		private string queueName;
		
		private int appId;
		private int workerId;
		private NetPort workerServiceNetPort = new NetPort(17000);
		
		private BinaryFormatter binaryFormatter = new BinaryFormatter();
		private ServiceConfig config;
		private IList<Executor> executors;
		private object executorSyncRoot;
		private int executorIndex;

		public string WorkerServiceName{get;private set;}
		public int WorkerServicePort { get; private set; }
		public string MQUri { get; private set; }

		public Worker(int appId)
		{
			this.appId = appId;
			this.executors = new List<Executor>(MaxExecutors);
			this.executorSyncRoot = (this.executors as ICollection).SyncRoot;
			this.Init();
		}

		private void Init()
		{
			WorkerInfo workerInfo = null;
			using (MasterServiceClient client = new MasterServiceClient(Consts.TomMasterServiceEndpointConfig))
			{
				workerInfo = client.AcquireWorker(appId);
			}

			this.workerId = workerInfo.WorkerId;
			this.WorkerServiceName = workerInfo.WorkerServiceName;
			this.queueName = workerInfo.QueueName;
			this.MQUri = workerInfo.MQUri;

			factory = new ConnectionFactory();
			factory.Uri = workerInfo.MQUri;

			s_Workers.Add(this.workerId, this);

			this.InitConfig();
			this.OpenWorkerService();
			this.StartThread();
		}

		public void Stop()
		{
			this.StopThread();
			this.KillAllExecutor();
		}

		private void KillAllExecutor()
		{
			Executor[] arr = this.executors.ToArray<Executor>();
			foreach (Executor executor in arr)
			{
				executor.Kill();
			}
		}

		public void OnExecutorExit(Executor executor)
		{
			lock (executorSyncRoot)
			{
				this.executors.Remove(executor);
			}
		}

		public void ReLoadConfig()
		{
			this.InitConfig();
		}

		private void InitConfig()
		{
			this.config = ServiceConfig.GetConfig(appId);
		}

		private Executor AcquriedExecutor(ServiceNode node)
		{
			Executor executor = null;
			if (this.executors.Count >= MaxExecutors)
			{
				while (this.executors.Count == 0)
				{
					int index = executorIndex;
					if (index >= this.executors.Count)
					{
						index = 0;
					}
					else
					{
						index++;
					}
					executorIndex = index;

					lock (executorSyncRoot)
					{
						executor = this.executors[index];
					}

					if (executor.Ping())
					{
						return executor;
					}
					else
					{
						lock (executorSyncRoot)
						{
							this.executors.Remove(executor);
						}
					}
				}
			}

			executor = new Executor(this, node.AppId, node.BinDirectory);
			executor.Start();
			lock (executorSyncRoot)
			{
				executors.Add(executor);
			}
			return executor;
		}

		private void OpenWorkerService()
		{
			NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
			binding.Security.Mode = SecurityMode.None;

			using (ServiceHost host = new ServiceHost(typeof(WorkerServiceImpl)))
			{
				this.WorkerServicePort = this.workerServiceNetPort.AcquriedPort();
				host.AddServiceEndpoint(typeof(IWorkerService), binding,
					new Uri(string.Format("net.tcp://{0}:{1}/{2}", WorkerServiceIP, this.WorkerServicePort, this.WorkerServiceName)));

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
			using(var conn = factory.CreateConnection())
			using(IModel ch = conn.CreateModel())
			{
				ch.QueueDeclare(this.queueName, false, false, false, null);
				ch.BasicQos(0, 1, false);

				var consumer = new QueueingBasicConsumer(ch);
				ch.BasicConsume(this.queueName, false, consumer);

				Executor executor = null;
				while (!this.isStop)
				{
					try
					{
						BasicDeliverEventArgs args = consumer.Queue.Dequeue();

						Request request;
						using (MemoryStream mem = new MemoryStream(args.Body))
						{
							request = (Request)binaryFormatter.Deserialize(mem);
							request.DeliveryTag = args.DeliveryTag;
							request.CorrelationId = args.BasicProperties.CorrelationId;
							request.ReplyTo = args.BasicProperties.ReplyTo;
						}

						ServiceNode node = config.ServiceNodes.Find(request.ServiceName);
						if (node != null)
						{
							executor = AcquriedExecutor(node);
							executor.ExecutorService.PushRequest(request);
						}
						else
						{
							IBasicProperties responseProps = ch.CreateBasicProperties();
							responseProps.CorrelationId = request.CorrelationId;
							ch.BasicPublish(string.Empty, request.ReplyTo, responseProps, NotFoundResponseBody);

							LOG.Warn("未找到请求处理服务：" + request.ServiceName);
						}
						ch.BasicAck(args.DeliveryTag, false);
					}
					catch (Exception ex)
					{
						LOG.Error(ex);
						break;
					}
				}
			}
		}
	}
}
