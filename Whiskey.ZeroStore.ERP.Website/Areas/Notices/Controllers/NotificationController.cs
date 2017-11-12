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
using System.Web.Script.Serialization;
using System.Web.Security;
using Whiskey.Utility.Helper;
using Whiskey.Utility;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers
{

    [License(CheckMode.Verify)]
    public class NotificationController : BaseController
    {

        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(NotificationController));

        protected readonly INotificationContract _notificationContract;
        protected readonly IDepartmentContract _departmentContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMsgNotificationContract _msgNotificationContract;
        //protected readonly IAdministratorTypeContract _administratorTypeContract;
        protected readonly INotificationQASystemContract _notificationQASystemContract;
        protected readonly IConfigureContract _configureContract;

        public NotificationController(INotificationContract notificationContract
            , IDepartmentContract departmentContract
            //, IAdministratorTypeContract administratorTypeContract
            , IAdministratorContract adminContract
            , IMemberContract memberContract
            , IMsgNotificationContract msgNotificationContract
            , INotificationQASystemContract notificationQASystemContract
            , IConfigureContract configureContract)
        {
            this._notificationContract = notificationContract;
            this._departmentContract = departmentContract;
            this._adminContract = adminContract;
            this._memberContract = memberContract;
            this._msgNotificationContract = msgNotificationContract;
            //this._administratorTypeContract = administratorTypeContract;
            this._notificationQASystemContract = notificationQASystemContract;
            this._configureContract = configureContract;

        }

        public static List<NotificationQuestion> QuestionList = new List<NotificationQuestion>();
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

        #region 添加数据

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create(int? examId, int? blogId)
        {
            ClearQuestionList();

            #region 获取问题类型
            var questionTypeList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(QuestionTypeFlag)))
            {
                questionTypeList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((QuestionTypeFlag)value)) });
            }
            ViewBag.QuestionTypes = questionTypeList;
            #endregion

            ViewBag.ExamId = examId;
            ViewBag.BlogId = blogId;
            ViewBag.QuestionList = QuestionList;
            return PartialView();
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(NotificationDto dto)
        {
            if (QuestionList.Exists(q => q.AnsweringsList == null || q.AnsweringsList.Count == 0))
            {
                return Json(new OperationResult(OperationResultType.Error, "问题必须存在答案"), JsonRequestBehavior.AllowGet);
            }
            if (QuestionList.Exists(q => q.QuestionType == 0 && !q.AnsweringsList.Exists(a => a.IsRight)))
            {
                return Json(new OperationResult(OperationResultType.Error, "选择题必须有一个正确答案"), JsonRequestBehavior.AllowGet);
            }
            if (QuestionList.Exists(q => q.QuestionType == 0 && q.AnsweringsList.Count(a => a.IsRight) > 1))
            {
                return Json(new OperationResult(OperationResultType.Error, "选择题只允许有一个正确答案"), JsonRequestBehavior.AllowGet);
            }

            var result = _notificationContract.Insert(true, sendNotificationAction, dto);
            int Id = result.Other != null && result.Other.ToString().Trim() != "" ? Convert.ToInt32(result.Other) : 0;

            if (Id > 0)
            {
                if (QuestionList != null && QuestionList.Count() > 0)
                {
                    _notificationQASystemContract.InsertQuestions(QuestionList.ToArray(), Id);
                }
                QuestionList = new List<NotificationQuestion>();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 更新数据

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(NotificationDto dto)
        {
            if (QuestionList.Exists(q => q.AnsweringsList == null || q.AnsweringsList.Count == 0))
            {
                return Json(new OperationResult(OperationResultType.Error, "问题必须存在答案"), JsonRequestBehavior.AllowGet);
            }
            if (QuestionList.Exists(q => q.QuestionType == 0 && !q.AnsweringsList.Exists(a => a.IsRight)))
            {
                return Json(new OperationResult(OperationResultType.Error, "选择题必须有一个正确答案"), JsonRequestBehavior.AllowGet);
            }
            if (QuestionList.Exists(q => q.QuestionType == 0 && q.AnsweringsList.Count(a => a.IsRight) > 1))
            {
                return Json(new OperationResult(OperationResultType.Error, "选择题只允许有一个正确答案"), JsonRequestBehavior.AllowGet);
            }

            var modNoti = _notificationContract.Edit(dto.Id);
            dto.AdministratorIds = modNoti.AdministratorIds;
            dto.DepartmentIds = modNoti.DepartmentIds;
            dto.PushNotifications = modNoti.PushNotifications;
            dto.IsSuccessed = modNoti.IsSuccessed;

            var result = _notificationContract.Update(sendNotificationAllAction, dto);

            if (QuestionList != null && QuestionList.Count() > 0)
            {
                _notificationQASystemContract.UpdateQuestions(QuestionList.ToArray(), dto.Id);
            }
            QuestionList = new List<NotificationQuestion>();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            ClearQuestionList();
            var result = _notificationContract.Edit(Id);

            #region 获取部门信息

            var list = _departmentContract.Departments.Where(w => !w.IsDeleted && w.IsEnabled).Select(m => new SelectListItem
            {
                Text = m.DepartmentName,
                Value = m.Id.ToString()
            }).ToList().Select(s => new SelectListItem()
            {
                Text = s.Text,
                Value = s.Value,
                Selected = (((NoticeTargetFlag)result.NoticeTargetType) == NoticeTargetFlag.Department) ?
                            result.PushNotifications.Count(c => c.Administrator.DepartmentId == s.Value.CastTo<int>()) > 0
                            : false
            }).Where(w => w.Selected).ToList();
            ViewData["departments"] = list;

            #endregion

            #region 获取员工或会员信息

            var listids = new List<SelectListItem>();
            if ((NoticeTargetFlag)result.NoticeTargetType == NoticeTargetFlag.Admin)
            {
                listids = _adminContract.Administrators.Where(w => !w.IsDeleted && w.IsEnabled).Select(m => new SelectListItem()
                {
                    Text = m.Member.RealName,
                    Value = m.Id.ToString()
                }).ToList().Select(s => new SelectListItem()
                {
                    Text = s.Text,
                    Value = s.Value,
                    Selected = (((NoticeTargetFlag)result.NoticeTargetType) == NoticeTargetFlag.Admin) ?
                                result.PushNotifications.Count(c => c.AdministratorId == s.Value.CastTo<int>()) > 0
                                : false
                }).Where(w => w.Selected).ToList();
            }

            ViewBag.AdministratorIds = listids;

            #endregion

            #region 获取问题类型
            var questionTypeList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(QuestionTypeFlag)))
            {
                questionTypeList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((QuestionTypeFlag)value)) });
            }
            ViewBag.QuestionTypes = questionTypeList;
            #endregion

            #region 获取消息对应问题
            QuestionList = _notificationQASystemContract.Entities.Where(q => q.NotificationId == Id && !q.IsDeleted && q.IsEnabled).ToList();
            ViewBag.QuestionList = QuestionList;
            #endregion

            #region 初始化消息对应问题html代码
            string htmlStr = "";
            int i = 0;
            QuestionList.Each(q =>
            {
                htmlStr += "<div id=\"" + q.GuidId + "\" class=\"ShowQuestion\"><div class=\"form-group\" ><input type=\"hidden\" value=\"" + q.QuestionType + "\" id=\"" + q.GuidId + "_QType\" /><input type=\"hidden\" value=\"" + q.GuidId + "\" id=\"" + q.GuidId + "_Q\" /><label class=\"control-label col-md-3\">问题" + (i + 1) + "(" + (GetQTypeName(q.QuestionType)) + ") ：</label><div class=\"col-md-9 cur-col\"><input readonly=\"readonly\" type=\"text\" id=\"" + q.GuidId + "_QContent\" value=\"" + q.Content + "\" /><input type=\"button\" class='btn btn-success btn-padding-right'  value=\"添加答案\" onclick=\"AddAnswering('" + q.GuidId + "')\" /><input type=\"button\" class='btn btn-success btn-padding-right'  value=\"修改\" onclick=\"UpdateQuestion('" + q.GuidId + "')\" />";

                htmlStr += "<input type=\"button\" class='btn btn-danger btn-padding-right'  value=\"清空答案\" onclick=\"ClearAnswering('" + q.GuidId + "')\" /><input type=\"button\" value=\"删除\" class='btn btn-danger btn-padding-right'  onclick=\"DeleteQuestion('" + q.GuidId + "')\" />";

                htmlStr += "</div></div><div id='" + q.GuidId + "_Answerings_show'>";

                int j = 0;
                q.AnsweringsList.Each(a =>
                {
                    htmlStr += "<div id='" + a.GuidId + "' class=\"form-group\" ><input type=\"hidden\" value=\"" + a.GuidId + "\" id=\"" + q.GuidId + "_" + a.GuidId + "_A\" /><label class=\"control-label col-md-3\" id='" + q.GuidId + "_" + a.GuidId + "_No'>" + a.Number + "、 </label><div class=\"col-md-9 cur-col\"><input readonly=\"readonly\"  class='form-word' id=\"" + q.GuidId + "_" + a.GuidId + "_AContent\" value=\"" + (q.QuestionType != 2 ? a.Content : a.Content == "1" ? "对" : "错") + "\"/><input type=\"checkbox\" readonly='readonly' onclick='return false;' id=\"" + q.GuidId + "_" + a.GuidId + "_A_IsRight\" class=\"form-box\"  " + (a.IsRight == true ? (" checked=\"checked\"") : "") + " " + (q.QuestionType == 0 && a.IsRight == true ? "" : (" style=\"visibility:hidden\"")) + " /><input type=\"button\" value=\"修改\" class='btn btn-success btn-padding-right'  onclick=\"UpdateAnswering('" + q.GuidId + "','" + a.GuidId + "')\" />";

                    htmlStr += "<input type=\"button\" value=\"删除\" class='btn btn-danger btn-padding-right' onclick=\"DeleteAnswering('" + q.GuidId + "','" + a.GuidId + "')\" />";

                    htmlStr += "</div></div>";
                });
                htmlStr += "</div></div>";

                i++;
            });
            ViewBag.htmlStr = htmlStr;
            #endregion

            return PartialView(result);
        }
        #endregion

        #region 查看数据详情

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _notificationContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 获取数据列表

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            ClearQuestionList();
            GridRequest request = new GridRequest(Request);
            Expression<Func<Notification, bool>> predicate = FilterHelper.GetExpression<Notification>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _notificationContract.Notifications.Where<Notification, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Title,
                    m.Description,
                    m.NoticeType,
                    m.Id,
                    m.NoticeTargetType,
                    m.IsEnableApp,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                    m.IsSuccessed,
                    IsReadCount = m.PushNotifications.Count(w => w.IsRead && !w.IsDeleted && w.IsEnabled),
                    AllNotiCount = m.PushNotifications.Count,
                    QuestionListCount = _notificationQASystemContract.Entities.Count(q => q.NotificationId == m.Id && !q.IsDeleted && q.IsEnabled)
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _notificationContract.Remove(sendNotificationAllAction, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [Log]
        [HttpPost]
        public ActionResult RemoveQuestions(int notificationId, int[] Id)
        {
            var ids = GetQList(notificationId).Where(q => Id.Contains(q.Id)).Select(q => q.Id).ToArray();
            var result = _notificationQASystemContract.DeleteOrRecovery(true, ids);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _notificationContract.Delete(sendNotificationAllAction, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _notificationContract.Recovery(sendNotificationAllAction, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [Log]
        [HttpPost]
        public ActionResult RecoveryQuestions(int notificationId, int[] Id)
        {
            var ids = GetQList(notificationId).Where(q => Id.Contains(q.Id)).Select(q => q.Id).ToArray();
            var result = _notificationQASystemContract.DeleteOrRecovery(false, ids);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用数据

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _notificationContract.Enable(sendNotificationAllAction, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _notificationContract.Disable(sendNotificationAllAction, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印数据

        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _notificationContract.Notifications.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 导出数据

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _notificationContract.Notifications.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取员工
        /// <summary>
        /// 初始化员工界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
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
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _adminContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Member.RealName,
                    m.Department.DepartmentName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetAdminSelectList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var list = _adminContract.Administrators.Where(predicate).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Member.RealName
                }).ToList();
                return list;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取会员信息
        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> MemberList()
        {
            GridRequest request = new GridRequest(Request);
            var rules = request.FilterGroup.Rules.Where(x => string.IsNullOrEmpty(x.Field)).ToList();
            foreach (FilterRule item in rules)
            {
                request.FilterGroup.Rules.Remove(item);
            }
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _memberContract.Members.Where<Member, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    //m.AdminName,
                    m.RealName,
                    m.JPushRegistrationID
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 消息通知

        public async Task<ActionResult> ListMyNotification()
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

        #endregion

        #region 获取通知的人数信息
        /// <summary>
        /// 获取通知人数信息
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewNotiPeo()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<ActionResult> ViewNotiPeoList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MsgNotificationReader, bool>> predicate = FilterHelper.GetExpression<MsgNotificationReader>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _msgNotificationContract.MsgNotificationReaders.Where<MsgNotificationReader, int>(predicate, request.PageCondition, out count).Select(s => new
                {
                    s.Administrator.Id,
                    s.Administrator.Member.MemberName,
                    s.Administrator.Member.RealName,
                    s.UpdatedTime
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 设置答题冷却时间
        /// <summary>
        /// 载入设置答题冷却时间
        /// </summary>
        /// <param name="answerTime">时间</param>
        /// <param name="unitOfTime">时间单位（hh:时;mm:分;ss:秒）,默认为秒</param>
        /// <returns></returns>
        public ActionResult SetAnswerTime()
        {
            string[] s_value = GetAnswerTime().Split("_");
            ViewBag.AnswerTime = s_value[0];
            ViewBag.UnitOfTime = s_value[1];

            return PartialView();
        }

        /// <summary>
        /// 设置答题冷却时间
        /// </summary>
        /// <param name="answerTime">时间</param>
        /// <param name="unitOfTime">时间单位（hh:时;mm:分;ss:秒）,默认为秒</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetAnswerTime(int answerTime, string unitOfTime = "ss")
        {
            int s_value = answerTime;
            switch (unitOfTime)
            {
                case "hh":
                    s_value = answerTime * 60 * 60;
                    break;
                case "mm":
                    s_value = answerTime * 60;
                    break;
                default:
                    break;
            }

            //if (XmlStaticHelper.UpdateNode("QASystem", "QASystemConfiguration", "answerTime", s_value.ToString()) && XmlStaticHelper.UpdateNode("QASystem", "QASystemConfiguration", "unitOfTime", unitOfTime))
            if (_configureContract.SetConfigure("QASystem", "QASystemConfiguration", "answerTime", s_value.ToString()) && _configureContract.SetConfigure("QASystem", "QASystemConfiguration", "unitOfTime", unitOfTime))
            {
                return Json(new OperationResult(OperationResultType.Success, "设置成功"));
            }
            return Json(new OperationResult(OperationResultType.Error, "设置失败"));
        }
        #endregion

        #region 获取答题冷却时间
        /// <summary>
        /// 获取答题冷却时间
        /// </summary>
        /// <returns>返回的格式为"xx_xx","_"前面是展示的冷却时间，后面是单位</returns>
        private string GetAnswerTime()
        {
            //int answerTime = Convert.ToInt32(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "answerTime"));
            //string unitOfTime = XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "unitOfTime", "ss");
            int answerTime = Convert.ToInt32(_configureContract.GetConfigureValue("QASystem","QASystemConfiguration", "answerTime"));
            string unitOfTime = _configureContract.GetConfigureValue("QASystem","QASystemConfiguration", "unitOfTime", "ss");

            int s_value = answerTime;
            switch (unitOfTime)
            {
                case "hh":
                    s_value = answerTime / 60 / 60;
                    break;
                case "mm":
                    s_value = answerTime / 60;
                    break;
                default:
                    break;
            }

            return s_value + "_" + unitOfTime;
        }
        #endregion

        #region 设置答题数量
        /// <summary>
        /// 载入设置答题冷却时间
        /// </summary>
        /// <param name="answerTime">时间</param>
        /// <param name="unitOfTime">时间单位（hh:时;mm:分;ss:秒）,默认为秒</param>
        /// <returns></returns>
        public ActionResult SetSingleAnswerQuantity()
        {
            ViewBag.SingleAnswerQuantity = GetSingleAnswerQuantity();

            return PartialView();
        }

        /// <summary>
        /// 设置答题数量
        /// </summary>
        /// <param name="singleAnswerQuantity">答题数量</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetSingleAnswerQuantity(int singleAnswerQuantity)
        {
            //if (XmlStaticHelper.UpdateNode("QASystem", "QASystemConfiguration", "SingleAnswerQuantity", singleAnswerQuantity.ToString()))
            //{
            if (_configureContract.SetConfigure("QASystem", "QASystemConfiguration", "SingleAnswerQuantity", singleAnswerQuantity.ToString()))
            {
                return Json(new OperationResult(OperationResultType.Success, "设置成功"));
            }
            return Json(new OperationResult(OperationResultType.Error, "设置失败"));
        }
        #endregion

        #region 获取答题数量
        /// <summary>
        /// 设置答题数量
        /// </summary>
        /// <param name="singleAnswerQuantity">答题数量</param>
        /// <returns></returns>
        [HttpPost]
        private int GetSingleAnswerQuantity()
        {
            //return Convert.ToInt32(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "SingleAnswerQuantity", "1"));
            return Convert.ToInt32(_configureContract.GetConfigureValue("QASystem","QASystemConfiguration", "SingleAnswerQuantity", "1"));
        }
        #endregion

        #region 添加问题
        [HttpPost]
        public JsonResult AddQuestion(int questionType, string content)
        {
            NotificationQuestion model = new NotificationQuestion();
            model.Content = content;
            model.QuestionType = questionType;
            model.GuidId = Guid.NewGuid();

            QuestionList.Add(model);
            OperationResult opera = new OperationResult(OperationResultType.Success, "添加成功", model);
            opera.Other = QuestionList.Count();
            return Json(opera);
        }
        #endregion

        #region 修改问题
        [HttpPost]
        public JsonResult UpdateQuestion(string content, int questionType, Guid questionGuidId)
        {
            if (QuestionList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请先添加问题"));
            }
            var question = QuestionList.FirstOrDefault(q => q.GuidId == questionGuidId);
            if (question == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加此问题"));
            }

            if (question.QuestionType != questionType)
            {
                question.AnsweringsList = new List<NotificationAnswering>();
            }

            question.Content = content;

            int index = QuestionList.FindIndex(q => q.GuidId == question.GuidId);
            QuestionList.RemoveAt(index);
            QuestionList.Insert(index, question);
            OperationResult opera = new OperationResult(OperationResultType.Success, "修改成功", question);
            opera.Other = QuestionList.Count();
            return Json(opera);
        }
        #endregion

        #region 删除问题
        public JsonResult DeleteQuestion(Guid questionGuidId)
        {
            QuestionList.RemoveAll(q => q.GuidId == questionGuidId);

            OperationResult opera = new OperationResult(OperationResultType.Success, "删除成功");
            return Json(opera);
        }
        #endregion

        #region 清空问题
        public JsonResult ClearQuestionList()
        {
            QuestionList = new List<NotificationQuestion>();

            OperationResult opera = new OperationResult(OperationResultType.Success, "修改成功");
            return Json(opera);
        }
        #endregion

        #region 添加答案
        public JsonResult AddAnswer(bool isRight, Guid questionGuidId, string content = "")
        {
            if (QuestionList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请先添加问题"), JsonRequestBehavior.AllowGet);
            }
            var question = QuestionList.FirstOrDefault(q => q.GuidId == questionGuidId);
            if (question == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加此问题"), JsonRequestBehavior.AllowGet);
            }
            if (question.AnsweringsList == null)
            {
                question.AnsweringsList = new List<NotificationAnswering>();
            }
            if (question.QuestionType != (int)QuestionTypeFlag.Judgment && content.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "答案内容不可为空"), JsonRequestBehavior.AllowGet);
            }
            int length = question.AnsweringsList.Count();
            if (question.QuestionType != (int)QuestionTypeFlag.Choice && length > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "您已添加过此问题的答案"), JsonRequestBehavior.AllowGet);
            }
            if (length > 25)
            {
                return Json(new OperationResult(OperationResultType.Error, "问题的答案数量已超过上限"), JsonRequestBehavior.AllowGet);
            }
            NotificationAnswering model = new NotificationAnswering();
            if (question.QuestionType == (int)QuestionTypeFlag.Judgment)
            {
                model.Content = isRight ? "1" : "0";
            }
            else
            {
                model.Content = content;
            }
            model.QuestionGuidId = question.GuidId;
            model.GuidId = Guid.NewGuid();
            model.IsRight = question.QuestionType == (int)QuestionTypeFlag.Choice ? isRight : true;
            model.Number = GetAnswerNo(length).Data.ToString();
            question.AnsweringsList.Add(model);
            length++;
            int index = QuestionList.FindIndex(q => q.GuidId == question.GuidId);
            QuestionList.RemoveAt(index);
            QuestionList.Insert(index, question);
            OperationResult opera = new OperationResult(OperationResultType.Success, "添加成功", model);
            opera.Other = length;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改答案
        public JsonResult UpdateAnswer(bool isRight, Guid questionGuidId, Guid answerGuidId, string content = "")
        {
            if (QuestionList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请先添加问题"));
            }
            var question = QuestionList.FirstOrDefault(q => q.GuidId == questionGuidId);
            if (question == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加此问题"));
            }
            if (question.QuestionType != (int)QuestionTypeFlag.Judgment && content.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "答案内容不可为空"), JsonRequestBehavior.AllowGet);
            }
            if (question.AnsweringsList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加过此答案"));
            }
            var answering = question.AnsweringsList.FirstOrDefault(a => a.QuestionGuidId == questionGuidId && a.GuidId == answerGuidId);
            if (answering == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加过此答案"));
            }

            if (question.QuestionType == (int)QuestionTypeFlag.Judgment)
            {
                answering.Content = isRight ? "1" : "0";
            }
            else
            {
                answering.Content = content;
            }
            answering.IsRight = question.QuestionType == (int)QuestionTypeFlag.FillIn ? true : isRight;

            int index_a = question.AnsweringsList.FindIndex(a => a.GuidId == answering.GuidId);
            question.AnsweringsList.RemoveAt(index_a);
            question.AnsweringsList.Insert(index_a, answering);

            int index = QuestionList.FindIndex(q => q.GuidId == question.GuidId);
            QuestionList.RemoveAt(index);
            QuestionList.Insert(index, question);

            OperationResult opera = new OperationResult(OperationResultType.Success, "修改成功", answering);

            int length = question.AnsweringsList.Count();
            opera.Other = length;
            return Json(opera);
        }
        #endregion

        #region 删除答案
        [HttpPost]
        public JsonResult DeleteAnswer(Guid questionGuidId, Guid answerGuidId)
        {
            if (QuestionList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请先添加问题"));
            }
            var question = QuestionList.FirstOrDefault(q => q.GuidId == questionGuidId);
            if (question == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加此问题"));
            }
            if (question.AnsweringsList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加过此答案"));
            }
            question.AnsweringsList.RemoveAll(a => a.QuestionGuidId == questionGuidId && a.GuidId == answerGuidId);

            for (int i = 0; i < question.AnsweringsList.Count; i++)
            {
                question.AnsweringsList[i].Number = GetAnswerNo(i).Data.ToString();
            }

            int index = QuestionList.FindIndex(q => q.GuidId == question.GuidId);
            QuestionList.RemoveAt(index);
            QuestionList.Insert(index, question);

            OperationResult opera = new OperationResult(OperationResultType.Success, "删除成功");
            opera.Data = question.AnsweringsList;
            return Json(opera);
        }
        #endregion

        #region 清空答案
        [HttpPost]
        public JsonResult ClearAnswer(Guid questionGuidId)
        {
            if (QuestionList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请先添加问题"));
            }
            var question = QuestionList.FirstOrDefault(q => q.GuidId == questionGuidId);
            if (question == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "该问题不存在"));
            }

            question.AnsweringsList = new List<NotificationAnswering>();

            int index = QuestionList.FindIndex(q => q.GuidId == question.GuidId);
            QuestionList.RemoveAt(index);
            QuestionList.Insert(index, question);

            OperationResult opera = new OperationResult(OperationResultType.Success, "答案已清空");
            opera.Other = question.AnsweringsList.Count();
            return Json(opera);
        }
        #endregion

        #region 检测是否可以添加新答案
        public JsonResult IsCanAddAnswerings(Guid questionGuidId)
        {
            if (QuestionList == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请先添加问题"));
            }
            var question = QuestionList.FirstOrDefault(q => q.GuidId == questionGuidId);
            if (question == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您未添加此问题"));
            }
            int length = question.AnsweringsList == null ? 0 : question.AnsweringsList.Count();

            if (question.QuestionType != (int)QuestionTypeFlag.Choice && length > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "您已添加过此问题的答案"));
            }
            if (length > 25)
            {
                return Json(new OperationResult(OperationResultType.Error, "问题的答案数量已超过上限"));
            }
            return Json(new OperationResult(OperationResultType.Success));
        }
        #endregion

        #region 获取答题类型名称
        public JsonResult GetQTypeNameByType(int type)
        {
            return Json(GetQTypeName(type));
        }
        private string GetQTypeName(int type)
        {
            return EnumHelper.GetValue<string>((QuestionTypeFlag)type);
        }
        #endregion

        #region 获取答案序号
        private OperationResult GetAnswerNo(int num)
        {
            string[] Nos = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            if (num > 25)
            {
                return new OperationResult(OperationResultType.Error, "同一个问题最多可添加26个答案");
            }

            return new OperationResult(OperationResultType.Success, "", Nos[num]);
        }
        #endregion

        #region 问题列表
        /// <summary>
        /// 加载问题列表页面
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        public ActionResult QuestionIndex(int notificationId)
        {
            ViewBag.NotificationId = notificationId;
            return PartialView();
        }

        /// <summary>
        /// 获取问题列表数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetQuestionList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<NotificationQuestion, bool>> predicate = FilterHelper.GetExpression<NotificationQuestion>(request.FilterGroup);
            var data = await Task.Run(() =>
            {

                Func<ICollection<QASystemInfo>, List<QASystemInfo>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sort).ThenBy(o => o.Id);
                    List<QASystemInfo> tree = new List<QASystemInfo>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        if (child.Children != null)
                        {
                            tree.AddRange(getTree(child.Children));
                        }
                    }
                    return tree;
                };

                var count = 0;
                var parants = _notificationQASystemContract.Entities.Where<NotificationQuestion, int>(predicate, request.PageCondition, out count).ToList();
                var listParants = parants != null ? parants.Select(q => new QASystemInfo
                {
                    Id = q.Id,
                    GuidId = q.GuidId,
                    ParentGuidId = Guid.Empty,
                    ParentId = null,
                    Content = q.Content,
                    IsDeleted = q.IsDeleted,
                    IsEnabled = q.IsEnabled,
                    QuestionType = EnumHelper.GetValue<string>((QuestionTypeFlag)q.QuestionType),
                    UpdateTime = q.UpdatedTime,
                    Category = 0,
                    IsRight = true,
                    Sort = q.Sequence.ToString(),
                    OperationName = q.Operator != null && q.Operator.Member != null ? q.Operator.Member.RealName : "",
                    AnswerersCount = _notificationQASystemContract.AnswererEntities.Count(a => a.QuestionGuidId == q.GuidId),
                    Children = q.AnsweringsList.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => new QASystemInfo
                    {
                        Id = a.Id,
                        GuidId = a.GuidId,
                        ParentGuidId = a.QuestionGuidId,
                        ParentId = q.Id,
                        Content = a.Content,
                        IsDeleted = a.IsDeleted,
                        IsEnabled = a.IsEnabled,
                        QuestionType = "",
                        UpdateTime = a.UpdatedTime,
                        Category = 1,
                        IsRight = a.IsRight,
                        Sort = a.Number,
                        OperationName = a.Operator != null && a.Operator.Member != null ? a.Operator.Member.RealName : "",
                        AnswerersCount = _notificationQASystemContract.GetAnsweringsCheckedCount(a.Id)
                    }).ToList()
                }).ToList() : new List<QASystemInfo>();

                var list = getTree(listParants).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取问题列表
        public List<QASystemInfo> GetQList(int notificationId)
        {
            var list = _notificationQASystemContract.Entities.Where(q => q.NotificationId == notificationId).Select(q => new QASystemInfo
            {
                Id = q.Id,
                GuidId = q.GuidId,
                ParentGuidId = Guid.Empty,
                Content = q.Content,
                IsDeleted = q.IsDeleted,
                IsEnabled = q.IsEnabled,
                QuestionType = EnumHelper.GetValue<string>((QuestionTypeFlag)q.QuestionType),
                UpdateTime = q.UpdatedTime,
                Category = 0,
                IsRight = true,
                Sort = q.Sequence.ToString(),
                OperationName = q.Operator != null && q.Operator.Member != null ? q.Operator.Member.RealName : "",
                AnswerersCount = _notificationQASystemContract.AnswererEntities.Count(a => a.QuestionGuidId == q.GuidId),
                Children = q.AnsweringsList.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => new QASystemInfo
                {
                    Id = a.Id,
                    GuidId = a.GuidId,
                    ParentGuidId = a.QuestionGuidId,
                    Content = a.Content,
                    IsDeleted = a.IsDeleted,
                    IsEnabled = a.IsEnabled,
                    QuestionType = "",
                    UpdateTime = a.UpdatedTime,
                    Category = 1,
                    IsRight = a.IsRight,
                    Sort = a.Number,
                    OperationName = a.Operator != null && a.Operator.Member != null ? a.Operator.Member.RealName : "",
                    AnswerersCount = _notificationQASystemContract.GetAnsweringsCheckedCount(a.Id)
                }).ToList()
            }).ToList();

            return list;
        }
        #endregion
    }

    public class QASystemInfo
    {
        [Description("唯一标识")]
        public int Id { get; set; }

        [Description("唯一标识")]
        public Guid GuidId { get; set; }

        [Description("父标识")]
        public Guid ParentGuidId { get; set; }

        public int? ParentId { get; set; }

        [Description("内容")]
        public string Content { get; set; }

        [Description("是否删除")]
        public bool IsDeleted { get; set; }

        [Description("是否启用")]
        public bool IsEnabled { get; set; }

        [Description("问题类型")]
        public string QuestionType { get; set; }

        [Description("更新时间")]
        public DateTime UpdateTime { get; set; }

        [Description("类型（0：问题；1：答案）")]
        public int Category { get; set; }

        [Description("是否正确")]
        public bool IsRight { get; set; }

        [Description("排序")]
        public string Sort { get; set; }

        [Description("操作人")]
        public string OperationName { get; set; }

        [Description("子集")]
        public List<QASystemInfo> Children { get; set; }

        [Description("若为问题则为答题者数量，若为答案则是选择该答案的人的数量")]
        public int AnswerersCount { get; set; }
    }
}
