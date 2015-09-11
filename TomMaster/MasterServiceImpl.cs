using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomMasterServiceContract;
using TomMasterServiceContract.Entities;
using LiveTK.Data;

namespace TomMaster
{
	public class MasterServiceImpl : IMasterService
	{
		static MySqlDBAccess DB = new MySqlDBAccess(MySqlDBAccess.GetConnectString(System.Configuration.ConfigurationManager.AppSettings["TomDBConnectName"]));

		#region IMasterService 成员

		public WorkerInfo AcquireWorker(int appId)
		{
			object objMQUri = DB.ExecuteScalar("select mquri from tom_apps where appid=" + appId);
			if (objMQUri == null || objMQUri == DBNull.Value)
			{
				return null;
			}

			int workerId = (int)DB.InsertData(string.Format("insert into tom_app_workers(AppId,Status,Created) value({0},1,sysdate())", appId), null);
			WorkerInfo info = new WorkerInfo();
			info.MQUri = objMQUri.ToString();
			info.WorkerId = workerId;
			info.WorkerServiceName = "WorkerServiceName_" + info.WorkerId;
			info.QueueName = "tom.app." + appId;
			return info;
		}

		public void CloseWorker(int workerId)
		{

		}


		public void Heartbeat(int workerId)
		{
		}

		#endregion
	}
}
