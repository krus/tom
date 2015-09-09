using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace TomComm
{
	public class ServiceLoader : MarshalByRefObject
	{
		private string directory;
		private Dictionary<string, MethodInfo> services = new Dictionary<string, MethodInfo>();
		public ServiceLoader()
		{
		}

		public void Load(string directory)
		{
			this.directory = directory;
			this.LoadAssembly();
		}

		private MethodInfo GetMethod(string serviceName)
		{
			MethodInfo m;
			services.TryGetValue(serviceName, out m);
			return m;
		}

		public object Invoke(string serviceName, params object[] args)
		{
			MethodInfo method = GetMethod(serviceName);
			if (method == null)
			{
				return null;
			}

			if (method.IsStatic)
			{
				return method.Invoke(null, args);
			}
			else
			{
				Type type = method.DeclaringType;
				object instance = type.Assembly.CreateInstance(type.FullName);
				return method.Invoke(instance, args);
			}
		}

		private void RegisterMethod(string serviceName, MethodInfo method)
		{
			if (!services.ContainsKey(serviceName))
			{
				services.Add(serviceName, method);
			}
		}

		private void LoadAssembly()
		{
			DirectoryInfo d = new DirectoryInfo(directory);
			FileInfo[] fis = d.GetFiles("*.dll");
			foreach (FileInfo fi in fis)
			{
				Assembly assembly = AppDomain.CurrentDomain.Load(fi.Name.Replace(fi.Extension, ""));
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					MethodInfo[] methods = type.GetMethods();
					foreach (MethodInfo method in methods)
					{
						ParameterInfo[] pars = method.GetParameters();
						if (pars != null && pars.Length == 1)
						{
							Type parType = pars[0].ParameterType;
							if (parType.FullName == typeof(Request).FullName)
							{
								Type returnType = method.ReturnType;
								if (returnType != null)
								{
									object[] objs = method.GetCustomAttributes(true);
									foreach (object obj in objs)
									{
										if (obj is TomServiceAttribute)
										{
											var attr = (TomServiceAttribute)obj;
											RegisterMethod(attr.ServiceName, method);
										}
									}
								}
							}
						}
					}
				}

			}
		}
	}
}
