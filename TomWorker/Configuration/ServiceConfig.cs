using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Data;
using LiveTK.Data;
using MySql.Data.MySqlClient;

namespace TomWorker.Configuration
{
	public class ServiceConfig
	{
		public static ServiceConfig GetConfig(int appId)
		{
			ServiceConfig config = new ServiceConfig();
			DataTable dt = GetConfigDataFormDB(appId);
			foreach (DataRow dr in dt.Rows)
			{
				ServiceNode node = new ServiceNode();
				node.AppId = appId;
				node.Name = dr["Code"].ToString();
				node.BinDirectory = dr["BinPath"].ToString();

				config.ServiceNodes.Add(node);
			}
			return config;
		}

		private static DataTable GetConfigDataFormDB(int appId)
		{
			MySqlDBAccess db = new MySqlDBAccess(MySqlDBAccess.GetConnectString(System.Configuration.ConfigurationManager.AppSettings["TomDBConnectName"]));
			return db.GetTable("select c.BinPath,b.Code from tom_app_services a left join tom_services b on a.ServiceId=b.ServiceId left join tom_apps c on c.appid=a.appid where a.appId=@AppId", 
				 new MySqlParameter[]{
					 new MySqlParameter("AppId", appId)
				 }
				);
		}

		private ServiceConfig()
		{
			this.ServiceNodes = new ServiceNodeCollection();
		}

		public ServiceNodeCollection ServiceNodes
		{
			get;
			private set;
		}

	}
}
