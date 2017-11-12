using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Notices.Controllers
{
    public class NotificationViewController : Controller
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(NotificationViewController));

        protected readonly INotificationContract _notificationContract;
        protected readonly IMsgNotificationContract _msgNotificationContract;
        protected readonly INotificationQASystemContract _notificationQASystemContract;

        public NotificationViewController(INotificationContract notificationContract
            , IMsgNotificationContract msgNotificationContract
            , INotificationQASystemContract notificationQASystemContract)
        {
            this._notificationContract = notificationContract;
            this._msgNotificationContract = msgNotificationContract;
            this._notificationQASystemContract = notificationQASystemContract;
        }
        #endregion

        public async Task<JsonResult> getLastNewNotification(int showcount = 3, int? AdminId = null)
        {
            if (AdminId == null)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            var query = (from noti in _notificationContract.Notifications
                         where noti.PushNotifications.Count(w => w.AdministratorId == AdminId.Value && !w.IsRead && !w.IsDeleted && w.IsEnabled) > 0
                         join msgreader in _msgNotificationContract.MsgNotificationReaders on noti.Id equals msgreader.NotificationId
                         where msgreader.AdministratorId == AdminId.Value
                         where !noti.IsDeleted && noti.IsEnabled && noti.IsSuccessed
                         where (noti.SendTime.HasValue ? noti.SendTime.Value <= DateTime.Now ? true : false : true)
                         select new
                         {
                             Id = msgreader.Id,
                             NId = noti.Id,
                             Title = noti.Title,
                             Description = noti.Description,
                             Time = noti.SendTime.HasValue ? noti.SendTime.Value : noti.CreatedTime,
                             AdminName = noti.Operator.Member.MemberName,
                         });
            var list = await Task.Run(() =>
            {
                return query.OrderByDescending(o => o.Time).Take(showcount).ToList().Select(s => new
                {
                    s.Id,
                    s.NId,
                    s.Title,
                    Description = !string.IsNullOrWhiteSpace(s.Description) ? s.Description.Split(new string[] { "\r\n", "<br/>", "<br>", "<br />" }, StringSplitOptions.None).FirstOrDefault(f => !f.IsNullOrWhiteSpace()) : s.Description,
                    s.AdminName,
                    Time = s.Time.ToLostTimeStr()
                }).ToList();
            });
            return Json(new { list = list, count = query.Count() }, JsonRequestBehavior.AllowGet);
        }

    }
}