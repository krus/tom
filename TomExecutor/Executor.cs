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
	public class Executor
	{
		private static Dictionary<string, Executor> s_Executors = new Dictionary<string, Executor>();
		private static object s_SyncRoot;
		static Executor()
		{
			s_SyncRoot = (s_Executors as ICollection).SyncRoot;
		}

		public static Executor TryGetExecutor(string serviceName, string serviceDirectory)
		{
			Executor executor;
			lock (s_SyncRoot)
			{
				if (s_Executors.TryGetValue(serviceName, out executor))
				{
					return executor;
				}

				executor = new Executor(serviceName, serviceDirectory);
				executor.LoadAssembly();
				executor.WatchFile();

				s_Executors.Add(serviceName, executor);

				return executor;
			}
		}

		private string serviceName;
		private string serviceDirectory;
		private AppDomain appDomain;
		private Executor(string serviceName, string serviceDirectory)
		{
			this.serviceName = serviceName;
			this.serviceDirectory = serviceDirectory;
		}

		public object Execute(Request request)
		{
			return null;
		}

		private void WatchFile()
		{

		}

		private void LoadAssembly()
		{
			string[] fileNames = Directory.GetFiles("*.dll");
			foreach (string fileName in fileNames)
			{
				Assembly assembly = Assembly.LoadFile(fileName);
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					MethodInfo[] methods = type.GetMethods();
					foreach (MethodInfo method in methods)
					{
					}
				}

			}
		}

		private void CreateAppDomain()
		{
			AppDomainSetup setup = new AppDomainSetup();
			setup.ApplicationName = this.serviceName;
			setup.PrivateBinPath = this.serviceDirectory;
			appDomain = AppDomain.CreateDomain(serviceName, null, setup);

			appDomain
		}
	}
}
