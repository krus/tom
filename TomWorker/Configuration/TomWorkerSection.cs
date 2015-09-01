using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public class TomWorkerSection : ConfigurationSection
	{
		[ConfigurationProperty("services", IsRequired = true)]
		public ServiceElementCollection Services
		{
			get
			{
				return (ServiceElementCollection)base["services"];
			}
		}
	}
}
