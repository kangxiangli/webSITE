using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Website
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
            var listAdminIds = t.Where(w => w.AdminId.IsNotNullAndEmpty()).Select(s => s.AdminId).ToList();
            SendAdminExit(listAdminIds);
        }

        private static void _hubInfo_ItemAdded(params HubInfo[] t)
        {
            try
            {
                var listAdminIds = t.Where(w => w.AdminId.IsNotNullAndEmpty()).Select(s => s.AdminId.CastTo<int>());// && w.connectionId != currentConnectionId

                var listAdmins = HubEntityContract._adminContract.Administrators.Where(w => listAdminIds.Contains(w.Id)).Select(s => new
                {
                    AdminId = s.Id,
                    RealName = s.Member.RealName,
                    UserPhoto = s.Member.UserPhoto,
                    JobName = s.JobPosition.JobPositionName,
                    DepId = s.Department.Id,
                    DepName = s.Department.DepartmentName,
                }).ToList<dynamic>();

                SendAdminLogin(listAdmins);
            }
            catch (Exception)
            {

            }

        }

        public void flushSocketLink(string AdminId)
        {
            Task.Run(() =>
            {
                if (AdminId.IsNotNullAndEmpty())
                {
                    string cid = Context.ConnectionId;

                    var modcon = _hubInfo.FirstOrDefault(f => f.connectionId == cid);

                    if (modcon.IsNotNull())
                    {
                        modcon.AdminId = AdminId;
                        modcon.CreateTime = DateTime.Now;//时间被刷新
                    }
                    else
                    {
                        HubInfo hubinfo = new HubInfo();
                        hubinfo.AdminId = AdminId;
                        hubinfo.connectionId = cid;
                        hubinfo.CreateTime = DateTime.Now;
                        _hubInfo.Add(hubinfo);
                    }

                    int AdminIdInt = AdminId.CastTo<int>();

                    GetMessage(AdminIdInt);
                    GetNotification(AdminIdInt);
                    SendAllLoginDevices(AdminIdInt, cid);
                }
            });
        }

        public async Task GetNotification(int AdminId)
        {
            await SendNotification(AdminId);
        }

        public void GetMessage(int AdminId)
        {
            SendMessage(AdminId);
        }
        /// <summary>
        /// 发送弹窗通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="AdminIds"></param>
        /// <returns></returns>
        public static OperationResult SendPopNotification(string title, string content, params int[] AdminIds)
        {
            OperationResult result = new OperationResult(OperationResultType.Error, "被通知人不存在");
            if (AdminIds.IsNotNullThis())
            {
                var devices = GetCurAdminDeviceIds(AdminIds);
                if (devices.Count > 0)
                {
                    foreach (var item in devices)
                    {
                        var client = AllClients.Client(item.connectionId);
                        client.PopNoti(title, content);
                    }
                    result = new OperationResult(OperationResultType.Success);
                }
            }
            return result;
        }

        #region PopLoading

        public static void SendPopLoading(bool isClose, string content, params int[] AdminIds)
        {
            if (AdminIds.IsNotNullThis())
            {
                var devices = GetCurAdminDeviceIds(AdminIds);
                if (devices.Count > 0)
                {
                    foreach (var item in devices)
                    {
                        var client = AllClients.Client(item.connectionId);
                        client.PopLoading(content, isClose);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 发送条码信息
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="strbarcode"></param>
        /// <returns></returns>
        public static OperationResult SendBarCodeInfo(int AdminId, params string[] strbarcode)
        {
            var result = new OperationResult(OperationResultType.Error, "条码信息无效");
            var devices = GetCurAdminDeviceIds(AdminId);

            #region 条码内容校验

            if (strbarcode.IsNotNull())
            {
                strbarcode = strbarcode.Where(w => w.IsNotNullAndEmpty()).ToArray();
                if (!strbarcode.IsNotNullOrEmptyThis())
                {
                    return result;
                }
            }
            else
            {
                return result;
            }

            #endregion

            if (devices.Count > 0)
            {
                foreach (var item in devices)
                {
                    var client = AllClients.Client(item.connectionId);
                    client.GetBarCodeInfo(strbarcode);
                }
                result.ResultType = OperationResultType.Success;
                result.Message = "发送成功";
            }
            else
            {
                result.Message = "未检测到PC端";
            }
            return result;
        }
        /// <summary>
        /// 更新页面菜单Badge数值
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="ChangeCount"></param>
        /// <param name="BadgeTag"></param>
        public static void UpdateBadgeCount(int AdminId, int ChangeCount, string BadgeTag, bool autoCalc = false)
        {
            var devices = GetCurAdminDeviceIds(AdminId);
            if (devices.Count>0)
            {
                foreach (var item in devices)
                {
                    var client = AllClients.Client(item.connectionId);
                    client.UpdateBadgeCount(ChangeCount, BadgeTag, autoCalc);
                }
            }
        }

        public async static Task SendNotification(params int[] AdminIds)
        {
            if (AdminIds.IsNotNullThis())
            {
                NotificationViewController controllerNotifciationView = new NotificationViewController(HubEntityContract._notificationContract, HubEntityContract._msgNotificationContract, HubEntityContract._notificationQASystemContract);
                foreach (var curDevice in GetCurAdminDeviceIds(AdminIds))
                {
                    var client = AllClients.Client(curDevice.connectionId);
                    int AdminId = curDevice.AdminId.CastTo<int>();
                    var data = await controllerNotifciationView.getLastNewNotification(3, AdminId);
                    client.GetNotification(data.Data);
                }
            }
        }

        /// <summary>
        /// 通知所以人
        /// </summary>
        /// <returns></returns>
        public async static Task SendNotificationAll()
        {
            if (_hubInfo.Count > 0)
            {
                NotificationViewController controllerNotifciationView = new NotificationViewController(HubEntityContract._notificationContract, HubEntityContract._msgNotificationContract, HubEntityContract._notificationQASystemContract);
                foreach (var item in _hubInfo)
                {
                    var client = AllClients.Client(item.connectionId);
                    int AdminId = item.AdminId.CastTo<int>();
                    var data = await controllerNotifciationView.getLastNewNotification(3, AdminId);
                    client.GetNotification(data.Data);
                }
            }
        }

        public static void SendMessage(params int[] AdminIds)
        {
            if (AdminIds.IsNotNullThis())
            {
                MessagerController controllerMessage = new MessagerController(HubEntityContract._messageerContract, HubEntityContract._adminContract);
                foreach (var curDevice in GetCurAdminDeviceIds(AdminIds))
                {
                    var client = AllClients.Client(curDevice.connectionId);
                    int AdminId = curDevice.AdminId.CastTo<int>();
                    var data = controllerMessage.GetMsgCount(AdminId);
                    client.GetMessage(data.Data);
                }
            }
        }

        /// <summary>
        /// 所有登录的客户端主动拉取消息数量
        /// </summary>
        public static void SendMessageAll()
        {
            if (_hubInfo.Count > 0)
            {
                MessagerController controllerMessage = new MessagerController(HubEntityContract._messageerContract, HubEntityContract._adminContract);
                foreach (var item in _hubInfo)
                {
                    var client = AllClients.Client(item.connectionId);
                    int AdminId = item.AdminId.CastTo<int>();
                    var data = controllerMessage.GetMsgCount(AdminId);
                    client.GetMessage(data.Data);
                }
            }
        }

        /// <summary>
        /// 推送服务器准备发布通知
        /// </summary>
        public static void StartServerRelease(int seconds = 60)
        {
            if (_hubInfo.Count > 0)
            {
                foreach (var item in _hubInfo)
                {
                    var client = AllClients.Client(item.connectionId);
                    client.startServerRelease(seconds);
                }
            }
        }

        /// <summary>
        /// 获取需要通知的设备
        /// </summary>
        /// <param name="AdminIds"></param>
        /// <returns></returns>
        private static List<HubInfo> GetCurAdminDeviceIds(params int[] AdminIds)
        {
            List<HubInfo> list = new List<HubInfo>();
            foreach (var AdminId in AdminIds)
            {
                var curDeviceIds = _hubInfo.Where(w => w.AdminId == AdminId.ToString()).ToList();
                list.AddRange(curDeviceIds);
            }
            return list;
        }
    }
    /// <summary>
    /// 此类中的接口数据不经过数据库(数据保存在客户端localStorage中)
    /// 及时聊天功能代码块,暂时先不考虑 group的概念(通过部门分组)
    /// </summary>
    public partial class NotificationHub
    {
        public void SendMessageTo(int AdminId, string content, params int[] AdminIds)
        {
            if (content.IsNotNullAndEmpty())
            {
                var listAdmins = GetCurAdminDeviceIds(AdminIds);
                var now = DateTime.Now;
                foreach (var item in listAdmins)
                {
                    var client = AllClients.Client(item.connectionId);
                    var data = new
                    {
                        adminId = AdminId,
                        content = content,
                        time = now.ToUnixTime(),
                    };
                    client.GetMessageTo(data);
                }
            }
        }

        private static void SendAllLoginDevices(int adminId, string curConnectId)
        {
            Task.Run(() =>
            {
                var strAdminId = adminId.ToString();
                var listHubs = _hubInfo;
                var listAdminIds = listHubs.Select(s => s.AdminId).Distinct().ToList().ConvertAll(c => c.CastTo<int>());
                var listAdmins = HubEntityContract._adminContract.Administrators.Where(w => listAdminIds.Contains(w.Id)).Select(s => new
                {
                    AdminId = s.Id,
                    RealName = s.Member.RealName,
                    UserPhoto = s.Member.UserPhoto,
                    JobName = s.JobPosition.JobPositionName,
                    DepId = s.Department.Id,
                    DepName = s.Department.DepartmentName,
                }).ToList();

                var selfHub = listHubs.FirstOrDefault(f => f.connectionId == curConnectId);
                if (selfHub.IsNotNull())
                {
                    var self = listAdmins.FirstOrDefault(w => w.AdminId == adminId);//自己
                    var listNotself = listAdmins.Where(w => w.AdminId != adminId);//不包含自己

                    var client = AllClients.Client(selfHub.connectionId);

                    client.selfInfo(self);

                    if (listNotself.IsNotNullOrEmptyThis())
                    {
                        //按部门分组
                        var listdepadmins = listNotself.GroupBy(g => new { g.DepId, g.DepName }).Select(s => new
                        {
                            s.Key.DepId,
                            s.Key.DepName,
                            Admins = s.Select(ss => new
                            {
                                ss.AdminId,
                                ss.RealName,
                                ss.UserPhoto,
                                ss.JobName,
                            })
                        }).ToList();

                        client.GetAllLoginAdmin(listdepadmins);
                    }
                }
            });
        }

        private static void SendAdminExit(List<string> adminIds)
        {
            if (adminIds.IsNotNullOrEmptyThis())
            {
                //当同一账号在多个设备登录时，一个设备退出不向其它账号发送该账号已退出命令
                adminIds = adminIds.Where(w => _hubInfo.Count(c => c.AdminId == w) == 0).ToList();
                if (adminIds.IsNotNullOrEmptyThis())
                {
                    foreach (var item in _hubInfo)
                    {
                        var client = AllClients.Client(item.connectionId);
                        client.exit(adminIds);
                    }
                }
            }
        }

        private static void SendAdminLogin(List<dynamic> adminIds)
        {
            if (adminIds.IsNotNullOrEmptyThis())
            {
                //筛选掉自己已登录过的设备，防止重复推送自己
                adminIds = adminIds.Where(w => _hubInfo.Count(c => c.AdminId == w.AdminId.ToString()) == 1).ToList();

                foreach (var item in _hubInfo)
                {
                    var notself = adminIds.Where(f => f.AdminId != item.AdminId.CastTo<int>());
                    if (notself.IsNotNullOrEmptyThis())
                    {
                        var client = AllClients.Client(item.connectionId);
                        client.login(notself);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 会员和员工交互用
    /// </summary>
    public partial class NotificationHub
    {
        /// <summary>
        /// 接收到ApiMember发来的消息
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="Content"></param>
        public static void ReceiveApiMember(int AdminId, string Content)
        {
            if (Content.IsNotNullAndEmpty())
            {
                var list = GetCurAdminDeviceIds(AdminId);
                if (list.Count > 0)
                {
                    var _admins = list.Select(s => s.connectionId).ToList();
                    var client = AllClients.Clients(_admins);
                    client.GetApiMemberNoti(Content);
                }
            }
        }
    }
}