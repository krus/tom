using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomComm;
using System.Collections;
using System.IO;
using System.Reflection;

namespace TomExecutor
{
	public class Task
	{
		private static Dictionary<string, Task> s_Tasks = new Dictionary<string, Task>();
		private static object s_SyncRoot;
		static Task()
		{
			s_SyncRoot = (s_Tasks as ICollection).SyncRoot;
		}

		public static Task TryGetTask(string serviceName, string serviceDirectory)
		{
			Task executor;
			lock (s_SyncRoot)
			{
				if (s_Tasks.TryGetValue(serviceName, out executor))
				{
					return executor;
				}

				executor = new Task(serviceName, serviceDirectory);
				s_Tasks.Add(serviceName, executor);

				return executor;
			}
		}

		private string serviceName;
		private string serviceDirectory;
		private MyAppDomain appDomain;
		private Task(string serviceName, string serviceDirectory)
		{
			this.serviceName = serviceName;
			this.serviceDirectory = serviceDirectory;
			this.appDomain = MyAppDomain.Acquried(serviceDirectory);
		}

		public object Execute(Request request)
		{
			try
			{
				return appDomain.Invoke(request.ServiceName, new object[] { request });
			}
			catch (Exception ex)
			{
				System.Console.Write(ex.ToString());
				return null;
			}
		}
	}
}
