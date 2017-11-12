using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Hubs
{
    public class DeviceHub : HubBase<DeviceHub, DeviceHubInfo>
    {
        #region 初始化

        public static HubList<DeviceHubInfo> _hubInfo = new HubList<DeviceHubInfo>();

        public DeviceHub() : base(_hubInfo) { }

        #endregion

        public void flushLink(string imei)
        {
            if (imei.IsNotNullAndEmpty())
            {
                string cid = Context.ConnectionId;

                var modcon = _hubInfo.FirstOrDefault(f => f.connectionId == cid);
                if (modcon.IsNull())
                {
                    DeviceHubInfo hubinfo = new DeviceHubInfo()
                    {
                        IMEI = imei,
                        connectionId = cid
                    };
                    _hubInfo.Add(hubinfo);
                }
                CurrentClient.uploadLocation();
            }
        }
        /// <summary>
        /// 刷新定位信息
        /// </summary>
        /// <param name="Longitude">经度</param>
        /// <param name="Latitude">纬度</param>
        public OperationResult flushLocation(double Longitude, double Latitude)
        {
            var info = _hubInfo.FirstOrDefault(f => f.connectionId == CurrentConnectionId);
            if (info.IsNotNull())
            {
                var res = HubEntityContract._posLocationContract.UpdateLocation(info.IMEI, Longitude, Latitude);
                return res;
            }
            return OperationHelper.ReturnOperationResultDIY(OperationResultType.QueryNull, "IMEI不存在,请重新flushLink");
        }
        /// <summary>
        /// 刷新所有设备的位置
        /// </summary>
        /// <returns>本次更新在线设备数量</returns>
        public static int FlushAllDeviceLocation()
        {
            var allconIds = _hubInfo.Select(s => s.connectionId).ToList();
            if (allconIds.Count > 0)
            {
                var client = AllClients.Clients(allconIds);
                client.uploadLocation();//向客户端发送上传位置信息事件
            }
            return allconIds.Count;
        }
    }
}