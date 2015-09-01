using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public sealed class ServiceElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}

			set
			{
				base["address"] = value;
			}
		}

		[ConfigurationProperty("path", IsRequired = true, IsKey = true)]
		public string Path
		{
			get
			{
				return (string)base["path"];
			}

			set
			{
				base["path"] = value;
			}
		}
	}
}
