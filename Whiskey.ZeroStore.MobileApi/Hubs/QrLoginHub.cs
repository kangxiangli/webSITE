using Microsoft.AspNet.SignalR.Hubs;
using Whiskey.Utility.Data;
using System.Linq;
using System;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.MobileApi
{
    [HubName("QrLoginHub")]
    public class QrLoginHub : BaseHub<QrLoginHub>
    {
        public QrLoginHub() : base(_hubInfo) { }

        public static HubList<HubInfo> _hubInfo = new HubList<HubInfo>();

        public void flushQrCode(string uuid)
        {
            string cid = Context.ConnectionId;

            var modcon = _hubInfo.FirstOrDefault(f => f.connectionId == cid);

            if (modcon.IsNotNull())
            {
                modcon.uuid = uuid;
                modcon.CreateTime = DateTime.Now;//时间被刷新
            }
            else// if (!_hubInfo.Exists(e => e.uuid == uuid))不会出现这种情况uuid唯一
            {
                HubInfo hubinfo = new HubInfo();
                hubinfo.uuid = uuid;
                hubinfo.connectionId = cid;
                hubinfo.CreateTime = DateTime.Now;
                _hubInfo.Add(hubinfo);
            }
        }

        public static void sendStatus(string uuid)
        {
            var Clients = GetClients();
            var hubinfo = _hubInfo.FirstOrDefault(f => f.uuid == uuid);
            if (hubinfo.IsNotNull())
            {
                var client = Clients.Client(hubinfo.connectionId);
                OperationResult result = new OperationResult(OperationResultType.Success);
                result.Data = uuid;
                client.getStatus(result);
            }
        }
        /// <summary>
        /// 扫码成功后，未登录点击了退出，需要返回重新扫码
        /// </summary>
        /// <param name="uuid"></param>

        public static void backQrCode(string uuid)
        {
            var Clients = GetClients();
            var hubinfo = _hubInfo.FirstOrDefault(f => f.uuid == uuid);
            if (hubinfo.IsNotNull())
            {
                var client = Clients.Client(hubinfo.connectionId);
                OperationResult result = new OperationResult(OperationResultType.Success);
                client.backQrCode(result);
            }
        }

        public static void scanComplete(string uuid, string adminImg)
        {
            var Clients = GetClients();
            var hubinfo = _hubInfo.FirstOrDefault(f => f.uuid == uuid);
            if (hubinfo.IsNotNull())
            {
                var client = Clients.Client(hubinfo.connectionId);
                OperationResult result = new OperationResult(OperationResultType.Success);
                result.Data = new
                {
                    //uuid = uuid,
                    adminImg = adminImg
                };
                client.scanComplete(result);
            }
        }
    }
    public class HubInfo
    {
        public string uuid { get; set; }
        public string AdminId { get; set; }
        public string browserId { get; set; }
        public string connectionId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}