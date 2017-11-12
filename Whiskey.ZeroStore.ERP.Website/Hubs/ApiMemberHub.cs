using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website
{
    public class ApiMemberHub : BaseHub<ApiMemberHub>
    {
        public static HubList<HubInfo> _hubInfo = new HubList<HubInfo>();
        public ApiMemberHub() : base(_hubInfo) { }

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
                        modcon.AdminId = MemberId;
                        modcon.CreateTime = DateTime.Now;//时间被刷新
                    }
                    else
                    {
                        HubInfo hubinfo = new HubInfo();
                        hubinfo.AdminId = MemberId;
                        hubinfo.connectionId = cid;
                        hubinfo.CreateTime = DateTime.Now;
                        _hubInfo.Add(hubinfo);
                    }
                }
            });
        }

        /// <summary>
        /// 获取需要通知的设备
        /// </summary>
        /// <param name="AdminIds"></param>
        /// <returns></returns>
        private static List<HubInfo> GetCurAdminDeviceIds(params int[] MemberIds)
        {
            List<HubInfo> list = new List<HubInfo>();
            foreach (var AdminId in MemberIds)
            {
                var curDeviceIds = _hubInfo.Where(w => w.AdminId == AdminId.ToString()).ToList();
                list.AddRange(curDeviceIds);
            }
            return list;
        }

        public static void SendNoti(object Data, params int[] MemberIds)
        {
            if (Data.IsNotNull())
            {
                var list = GetCurAdminDeviceIds(MemberIds);
                if (list.Count > 0)
                {
                    var _members = list.Select(s => s.connectionId).ToList();
                    var client = AllClients.Clients(_members);
                    client.GetNoti(Data);
                }
            }
        }

        public void SendNoti(int AdminId, string Content)
        {
            NotificationHub.ReceiveApiMember(AdminId, Content);
        }

    }
}