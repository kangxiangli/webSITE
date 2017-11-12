using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// WebUrl地址，来自web.config
        /// </summary>
        protected readonly string WebUrl = ConfigurationHelper.WebUrl;
        /// <summary>
        /// ApiUrl地址，来自web.config
        /// </summary>
        protected readonly string ApiUrl = ConfigurationHelper.ApiUrl;
        /// <summary>
        /// 当前日期
        /// </summary>
        protected DateTime DateNowDate { get { return DateTime.Now.Date; } }
        /// <summary>
        /// 当前时间
        /// </summary>
        protected DateTime DateNow { get { return DateTime.Now; } }
        /// <summary>
        /// 发送通知Action，通知指定的人
        /// </summary>
        protected Action<List<int>> sendNotificationAction = (adminids) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendNotification(adminids.ToArray()); }); } catch { } };
        /// <summary>
        /// 发送通知Action，通知所有人
        /// </summary>
        protected Action sendNotificationAllAction = () => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendNotificationAll(); }); } catch { } };
        /// <summary>
        /// 发送消息Action，通知指定的人
        /// </summary>
        protected Action<List<int>> sendMessageAction = (adminids) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendMessage(adminids.ToArray()); }); } catch { } };
        /// <summary>
        /// 发送消息Action，通知所有人
        /// </summary>
        protected Action sendMessageAllAction = () => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendMessageAll(); }); } catch { } };

        /// <summary>
        /// 发送弹窗Loading，通知指定人,bool=true关闭弹窗,int[]为通知人Id
        /// </summary>
        protected Action<bool, string, int[]> sendPopLoadingAction = (isclose, content, adminIds) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendPopLoading(isclose, content, adminIds); }); } catch { } };
    }
}