using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.ComponentModel.DataAnnotations;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers
{
    [License(CheckMode.Verify)]
    public class NotificationViewController : BaseController
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
        #region 初始化界面
        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            if (request.FilterGroup.Rules.IsNotNullOrEmptyThis())
            {
                var shouldRemoves = request.FilterGroup.Rules.Where(w => string.IsNullOrEmpty(w.Field));
                foreach (var item in shouldRemoves)
                {
                    request.FilterGroup.Rules.Remove(item);
                }
            }
            Expression<Func<Notification, bool>> predicate = FilterHelper.GetExpression<Notification>(request.FilterGroup);

            var uid = AuthorityHelper.OperatorId;

            var data = await Task.Run(() =>
            {
                var query = (from noti in _notificationContract.Notifications.Where(predicate)
                             where noti.PushNotifications.Count(w => w.AdministratorId == uid.Value && !w.IsDeleted && w.IsEnabled) > 0
                             join msgreader in _msgNotificationContract.MsgNotificationReaders on noti.Id equals msgreader.NotificationId
                             where msgreader.AdministratorId == uid.Value
                             where !noti.IsDeleted && noti.IsEnabled && noti.IsSuccessed
                             where (noti.SendTime.HasValue ? noti.SendTime.Value <= DateTime.Now ? true : false : true)
                             select new
                             {
                                 Title = noti.Title,
                                 Description = noti.Description,
                                 NoticeType = noti.NoticeType,
                                 Id = msgreader.Id,
                                 NId = noti.Id,
                                 NoticeTargetType = noti.NoticeTargetType,
                                 IsEnableApp = noti.IsEnableApp,
                                 Time = noti.SendTime.HasValue ? noti.SendTime.Value : noti.CreatedTime,
                                 AdminName = noti.Operator.Member.MemberName,
                                 IsSuccessed = noti.IsSuccessed,
                                 IsRead = msgreader.IsRead
                             });
                var IsRead = Convert.ToBoolean(Request["IsRead"] ?? "false");
                query = query.Where(w => w.IsRead == IsRead);
                int count = query.Count();
                var list = query.OrderBy(o => o.IsRead).ThenByDescending(o => o.Time).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult View(int? Id, int NId)
        {
            var result = _notificationContract.View(NId);
            //看完就标致已读
            //if (Id.HasValue)
            //{
            //    Update(new int[] { Id.Value });
            //}
            return PartialView(result);
        }

        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(int[] Id)
        {
            OperationResult Oresult = new OperationResult(OperationResultType.Error);
            var uid = AuthorityHelper.OperatorId;
            if (Id.IsNotNullOrEmptyThis() && uid.HasValue)
            {
                List<MsgNotificationReader> listmsgreader = _msgNotificationContract.MsgNotificationReaders.Where(w => Id.Contains(w.Id)).ToList();
                listmsgreader.ForEach(f =>
                {
                    f.IsRead = true;
                    f.UpdatedTime = DateTime.Now;
                    f.OperatorId = uid;
                });
                Oresult = _msgNotificationContract.Update(listmsgreader.ToArray());
                if (Oresult.ResultType == OperationResultType.Success)
                {
                    sendNotificationAction(listmsgreader.Select(s => s.AdministratorId.Value).ToList());
                }
            }
            return Json(Oresult, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> getLastNewNotification(int showcount = 3, int? AdminId = null)
        {
            var uid = AdminId ?? AuthorityHelper.OperatorId;
            if (!uid.HasValue)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            var query = (from noti in _notificationContract.Notifications
                         where noti.PushNotifications.Count(w => w.AdministratorId == uid.Value && !w.IsRead && !w.IsDeleted && w.IsEnabled) > 0
                         join msgreader in _msgNotificationContract.MsgNotificationReaders on noti.Id equals msgreader.NotificationId
                         where msgreader.AdministratorId == uid.Value
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

        public ActionResult ReadAll()
        {
            var uid = AuthorityHelper.OperatorId;
            var query = (from noti in _notificationContract.Notifications
                         where noti.PushNotifications.Count(w => w.AdministratorId == uid.Value && !w.IsDeleted && w.IsEnabled) > 0
                         join msgreader in _msgNotificationContract.MsgNotificationReaders on noti.Id equals msgreader.NotificationId
                         where msgreader.AdministratorId == uid.Value && msgreader.IsRead == false
                         where !noti.IsDeleted && noti.IsEnabled && noti.IsSuccessed
                         where (noti.SendTime.HasValue ? noti.SendTime.Value <= DateTime.Now ? true : false : true)
                         select new
                         {
                             Id = msgreader.Id,
                         });
            var IdArry = query.Select(x => x.Id).ToArray();
            return Update(IdArry);
        }

        public int GetReadCount()
        {
            var uid = AuthorityHelper.OperatorId;
            var query = (from noti in _notificationContract.Notifications
                         where noti.PushNotifications.Count(w => w.AdministratorId == uid.Value && !w.IsDeleted && w.IsEnabled) > 0
                         join msgreader in _msgNotificationContract.MsgNotificationReaders on noti.Id equals msgreader.NotificationId
                         where msgreader.AdministratorId == uid.Value && msgreader.IsRead == false
                         where !noti.IsDeleted && noti.IsEnabled && noti.IsSuccessed
                         where (noti.SendTime.HasValue ? noti.SendTime.Value <= DateTime.Now ? true : false : true)
                         select new
                         {
                             Id = msgreader.Id,
                         });
            return query.Count();
        }

        #region 是否需要答题
        [HttpPost]
        public JsonResult IsAnswer(int notificationReadId)
        {
            var notificationReader = _msgNotificationContract.View(notificationReadId);
            if (notificationReader == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "消息不存在"), JsonRequestBehavior.AllowGet);
            }
            int num = _notificationQASystemContract.Entities.Count(q => q.NotificationId == notificationReader.NotificationId && q.IsEnabled && !q.IsDeleted);
            if (num == 0)
            {//看完就标致已读
                if (notificationReadId > 0)
                {
                    Update(new int[] { notificationReadId });
                }

                return Json(new OperationResult(OperationResultType.QueryNull), JsonRequestBehavior.AllowGet);
            }

            int adminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult opera = _notificationQASystemContract.CheckIsAnswer(notificationReadId, adminId);

            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 答题
        public ActionResult Answer(int notificationReadId)
        {
            var opera = _notificationQASystemContract.GetQuestion(notificationReadId, AuthorityHelper.OperatorId ?? 0);

            var questionList = new List<NotificationQuestion>();
            string Ids = "";
            string htmlStr = "";
            if (opera.ResultType == OperationResultType.Success)
            {

                questionList = opera.Data as List<NotificationQuestion>;

                #region 初始化消息对应问题html代码
                int i = 0;

                questionList.Each(q =>
                {
                    htmlStr += "<div id=\"" + q.GuidId + "\"><div class=\"form-group\" ><label class=\"control-label col-md-3\">问题" + (i + 1) + " ：</label><div class=\"col-md-7\"><label id=\"" + q.GuidId + "_QContent\">" + q.Content + "</label>";

                    htmlStr += "</div></div>";

                    int j = 0;
                    switch (q.QuestionType)
                    {
                        case (int)QuestionTypeFlag.Choice:
                            q.AnsweringsList.Each(a =>
                            {
                                htmlStr += "<div class=\"form-group\" ><label class=\"control-label col-md-3\">" + a.Number + "、 </label><div class=\"col-md-7\">" + a.Content + "&nbsp:&nbsp:&nbsp:&nbsp:<input type=\"checkbox\" onclick=\"Answer('" + q.GuidId + "','" + a.GuidId + "')\" class=\"form-control\" /></div></div>";
                            });
                            break;
                        case (int)QuestionTypeFlag.FillIn:
                            q.AnsweringsList.Each(a =>
                            {
                                htmlStr += "<div class=\"form-group\" ><label class=\"control-label col-md-3\">答： </label><div class=\"col-md-7\"><input type='text' value='' onblur=\"Answer('" + q.GuidId + "',$(this).val())\" /></div></div>";
                            });
                            break;
                        case (int)QuestionTypeFlag.Judgment:
                            q.AnsweringsList.Each(a =>
                            {
                                htmlStr += "<div class=\"form-group\" ><label class=\"control-label col-md-3\">答： </label><div class=\"col-md-7\"><input type=\"radio\" value='1' name='" + q.GuidId + "_Judge' onclick=\"Answer('" + q.GuidId + "','1')\"  />对 <input type=\"radio\" value='1' name='" + q.GuidId + "_Judge' onclick=\"Answer('" + q.GuidId + "','0')\"  />错 </div></div>";
                            });
                            break;
                    }

                    htmlStr += "</div>";

                    Ids += q.GuidId + ",";
                    i++;
                });
            }
            Ids = Ids.TrimEnd(',');
            ViewBag.htmlStr = htmlStr;
            ViewBag.Ids = Ids;
            ViewBag.NotificationReadId = notificationReadId;
            #endregion

            return PartialView();
        }
        #endregion

        #region 答题
        [HttpPost]
        public JsonResult InAnswer(List<List<string>> list, string Ids, int notificationReadId)
        {
            if (list.Count() != Ids.Split(",").Length)
            {
                return Json(new OperationResult(OperationResultType.Error, "题目未答完"));
            }

            IDictionary<Guid, string> dic = new Dictionary<Guid, string>();
            foreach (var item in list)
            {
                KeyValuePair<Guid, string> d = new KeyValuePair<Guid, string>(new Guid(item[0]), item[1]);

                dic.Add(d);
            }

            int adminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult opera = _notificationQASystemContract.Answer(adminId, notificationReadId, dic);

            return Json(opera);
        }
        #endregion

    }
}