using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomComm
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TomServiceAttribute : Attribute
	{
		public string ServiceName{get;private set;}
		public TomServiceAttribute(string serviceName)
		{
			this.ServiceName = serviceName;
		}
	}
}
