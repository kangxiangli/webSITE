using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.WebMember
{
    [HubName("NotificationHub")]
    public partial class NotificationHub : BaseHub<NotificationHub>
    {
        public static HubList<HubInfo> _hubInfo = new HubList<HubInfo>();

        public string currentConnectionId { get; set; }

        public NotificationHub() : base(_hubInfo) { }

        static NotificationHub()
        {
            _hubInfo.ItemAdded += _hubInfo_ItemAdded;
            _hubInfo.ItemRemoved += _hubInfo_ItemRemoved;
        }

        private static void _hubInfo_ItemRemoved(params HubInfo[] t)
        {

        }

        private static void _hubInfo_ItemAdded(params HubInfo[] t)
        {

        }

        public void flushSocketLink(string MemberId)
        {
            Task.Run(() =>
            {
                if (MemberId.IsNotNullAndEmpty())
                {
                    string cid = Context.ConnectionId;

                    var modcon = _hubInfo.FirstOrDefault(f => f.connectionId == cid);

                    if (modcon.IsNotNull())
                    {
                        modcon.MemberId = MemberId;
                        modcon.CreateTime = DateTime.Now;//时间被刷新
                    }
                    else
                    {
                        HubInfo hubinfo = new HubInfo();
                        hubinfo.MemberId = MemberId;
                        hubinfo.connectionId = cid;
                        hubinfo.CreateTime = DateTime.Now;
                        _hubInfo.Add(hubinfo);
                    }
                }
            });
        }

        /// <summary>
        /// 推送服务器准备发布通知
        /// </summary>
        public static void StartServerRelease(int seconds = 60)
        {
            if (_hubInfo.Count > 0)
            {
                var clients = GetClients();
                foreach (var item in _hubInfo)
                {
                    var client = clients.Client(item.connectionId);
                    client.startServerRelease(seconds);
                }
            }
        }
    }
}