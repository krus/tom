using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public sealed class ServiceNodeCollection
	{
		private Dictionary<string, ServiceNode> services = new Dictionary<string,ServiceNode>();
		
		public ServiceNode FindServiceElement(string name)
		{
			ServiceNode element = null;
			services.TryGetValue(name, out element);
			return element;
		}
	}
}
