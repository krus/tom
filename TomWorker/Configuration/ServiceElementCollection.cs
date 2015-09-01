using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public sealed class ServiceElementCollection : ConfigurationElementCollection
	{
		private Dictionary<string, ServiceElement> services = new Dictionary<string,ServiceElement>();
		protected override ConfigurationElement CreateNewElement()
		{
			ServiceElement element = new ServiceElement();
			services[element.Name] = element;
			return element;
		}

		public ServiceElement FindServiceElement(string name)
		{
			ServiceElement element = null;
			services.TryGetValue(name, out element);
			return element;
		}
	}
}
