using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomExecutor
{
	internal class ServiceMethod
	{
		public static AppDomain GetMethod(string serviceName, string serviceDirectory)
		{
			AppDomainSetup info = new AppDomainSetup();
           info.LoaderOptimization = LoaderOptimization.SingleDomain;

		   AppDomain domain = AppDomain.CreateDomain(serviceName, null, info);
            domain.("C:\\test\\DomainCom.exe");
            AppDomain.Unload(domain);
		}

		private static LoadAssembly(string directory)
		{

		}
	}
}
