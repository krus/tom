using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading;
using TomComm;
using System.IO;

namespace TomExecutor
{
	public class MyAppDomain
	{
		private static int s_AppDomainId;
		private static Dictionary<string, MyAppDomain> s_AppDomains = new Dictionary<string, MyAppDomain>();
		private static object s_SyncRoot;

		static MyAppDomain()
		{
			s_SyncRoot = (s_AppDomains as ICollection).SyncRoot;
		}

		public static MyAppDomain Acquried(string directory)
		{
			MyAppDomain appDomain;
			lock (s_SyncRoot)
			{
				if (s_AppDomains.TryGetValue(directory, out appDomain))
				{
					return appDomain;
				}

				appDomain = new MyAppDomain(directory);
				appDomain.Init();
				s_AppDomains.Add(directory, appDomain);
			}
			return appDomain;
		}

		private string directory;
		private ServiceLoader serviceLoader;
		private MyAppDomain(string directory)
		{
			this.directory = directory;
		}

		private void Init()
		{
			try
			{
				Interlocked.Increment(ref s_AppDomainId);

				AppDomainSetup setup = new AppDomainSetup();
				setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
				setup.ApplicationName = "Executor";
				setup.ApplicationBase = this.directory;
				setup.CachePath = setup.ApplicationBase;
				setup.ShadowCopyFiles = "true";
				setup.ShadowCopyDirectories = setup.ApplicationBase;
				AppDomain.CurrentDomain.SetShadowCopyFiles();


				AppDomain appDomain = AppDomain.CreateDomain("AppDomain #" + s_AppDomainId, null, setup);

				serviceLoader = (ServiceLoader)appDomain.CreateInstanceAndUnwrap("TomComm", typeof(ServiceLoader).FullName);
				serviceLoader.Load(directory);
			}
			catch (Exception ex)
			{
				System.Console.Write(ex.ToString());
			}
		}

		public object Invoke(string serviceName, params object[] args)
		{
			return serviceLoader.Invoke(serviceName, args);
		}
	}
}
