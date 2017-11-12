using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Data;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Web.Helper;
using System.IO;
using System.Threading;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class ApiNotificationController: BaseController
    {
        #region 初始化

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ApiNotificationController));
        protected readonly IMemberContract _memberContract;
        protected readonly ISmsContract _SmsContract;
        protected readonly IOrderFoodContract _OrderFoodContract;

        public ApiNotificationController(IMemberContract _memberContract
            , ISmsContract _SmsContract,
            IOrderFoodContract _OrderFoodContract
            )
        {
            this._memberContract = _memberContract;
            this._SmsContract = _SmsContract;
            this._OrderFoodContract = _OrderFoodContract;
        }

        #endregion

        /// <summary>
        /// 推送服务器准备发布通知
        /// </summary>
        public void SendStartServerRelease(int seconds = 60)
        {
            NotificationHub.StartServerRelease(seconds);
        }
        /// <summary>
        /// 发送弹窗通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult SendPopNotification(string title, string content, params int[] AdminIds)
        {
            return Json(NotificationHub.SendPopNotification(title, content, AdminIds));
        }

        /// <summary>
        /// 向会员App发布通知,不会记录到表
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="memberIds"></param>
        public void SendMemberApp(string title, string content, params int[] memberIds)
        {
            _memberContract.SendAppNotification(title, content, memberIds);
        }
        /// <summary>
        /// 向网页版App指定会员发送消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="MemberIds"></param>
        public void SendNoti(string content, params int[] MemberIds)
        {
            ApiMemberHub.SendNoti(content, MemberIds);
        }
    }
}