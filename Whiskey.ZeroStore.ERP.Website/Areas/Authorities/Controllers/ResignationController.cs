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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    public class ResignationController : BaseController
    {

        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ResignationController));

        protected readonly IResignationContract _resignationContract;

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IMemberContract _memberContract;
        protected readonly IResignationToExamineContract _resignationToExamineContract;
        public readonly ITemplateContract _templateContract;
        public readonly ITemplateNotificationContract _templateNotificationContract;
        public readonly INotificationContract _notificationContract;

        public ResignationController(IResignationContract resignationContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IMemberContract memberContract,
            IResignationToExamineContract resignationToExamineContract,
            ITemplateContract templateContract,
            ITemplateNotificationContract templateNotificationContract,
            INotificationContract notificationContract)
        {
            _resignationContract = resignationContract;
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _memberContract = memberContract;
            _resignationToExamineContract = resignationToExamineContract;
            _templateContract = templateContract;
            _templateNotificationContract = templateNotificationContract;
            _notificationContract = notificationContract;
        }

        #endregion

        #region 初始化界面
        [Layout]
        public ActionResult Index()
        {
            string strTitle = "请选择";
            List<SelectListItem> list = _departmentContract.SelectList(strTitle);
            ViewBag.Departments = list;
            return View();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            var adminId = AuthorityHelper.OperatorId.Value;
            var admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            Expression<Func<Resignation, bool>> predicate = FilterHelper.GetExpression<Resignation>(request.FilterGroup);
            string Realname = Request["Realname"];
            string MobilePhone = Request["MobilePhone"];
            string ToExamineResultStr = Request["ToExamineResult"];
            var data = await Task.Run(() =>
            {
                var count = 0;
                int Auditauthority = 0;
                if (admin != null)
                {
                    IQueryable<Resignation> listEntry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled);
                    if (!string.IsNullOrEmpty(Realname))
                    {
                        listEntry = listEntry.Where(x => x.ResignationModel.Member.MemberName.Contains(Realname));
                    }
                    if (!string.IsNullOrEmpty(MobilePhone))
                    {
                        listEntry = listEntry.Where(x => x.ResignationModel.Member.MobilePhone == MobilePhone);
                    }
                    int ToExamineResultNum = -1;
                    if (!string.IsNullOrEmpty(ToExamineResultStr))
                    {
                        ToExamineResultNum = Convert.ToInt32(ToExamineResultStr);
                    }
                    if (admin.JobPosition != null)
                    {
                        if (admin.JobPosition.Auditauthority != null && admin.JobPosition.Auditauthority.Value != 0)
                        {
                            Auditauthority = admin.JobPosition.Auditauthority.Value;
                            switch (Auditauthority)
                            {
                                case 1:
                                    if (ToExamineResultNum != -1)
                                    {
                                        if (ToExamineResultNum == 2)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == -1 || x.ToExamineResult == -6);
                                        }
                                        else
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == ToExamineResultNum);
                                        }
                                    }
                                    break;
                                case 4:
                                    if (ToExamineResultNum != -1)
                                    {
                                        switch (ToExamineResultNum)
                                        {
                                            case 0:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 0 || x.ToExamineResult == 1);
                                                break;
                                            case 1:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == 2);
                                                break;
                                            case 2:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == -1 || x.ToExamineResult == -2 || x.ToExamineResult == -5 || x.ToExamineResult == -6);
                                                break;
                                        }
                                    }
                                    break;
                                case 5:
                                    if (ToExamineResultNum != -1)
                                    {
                                        switch (ToExamineResultNum)
                                        {
                                            case 0:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 0 || x.ToExamineResult == 2);
                                                break;
                                            case 1:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == 3);
                                                break;
                                            case 2:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == -1 || x.ToExamineResult == -3
                                                || x.ToExamineResult == -6);
                                                break;
                                        }
                                    }
                                    break;
                                case 7:
                                    if (ToExamineResultNum != -1)
                                    {
                                        switch (ToExamineResultNum)
                                        {
                                            case 0:
                                                listEntry = listEntry.Where(x => x.ToExamineResult != 3 && x.ToExamineResult != -6 && x.ToExamineResult != -4
                                                && x.ToExamineResult != -1);
                                                break;
                                            case 1:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == 2 || x.ToExamineResult == 3);
                                                break;
                                            case 2:
                                                listEntry = listEntry.Where(x => x.ToExamineResult < 0);
                                                break;
                                        }
                                    }
                                    break;
                                case 2:
                                    if (ToExamineResultNum != -1)
                                    {
                                        if (ToExamineResultNum == 2)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == -2 || x.ToExamineResult == -5);
                                        }
                                        else if (ToExamineResultNum == 0)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == -3);
                                        }
                                        else if (ToExamineResultNum == 1)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == 2);
                                        }
                                    }
                                    break;
                                case 3:
                                    if (ToExamineResultNum != -1)
                                    {
                                        if (ToExamineResultNum == 2)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == -3 || x.ToExamineResult == -4);
                                        }
                                        else if (ToExamineResultNum == 0)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == 2);
                                        }
                                        else if (ToExamineResultNum == 1)
                                        {
                                            listEntry = listEntry.Where(x => x.ToExamineResult == 3);
                                        }
                                    }
                                    break;
                                case 6:
                                    if (ToExamineResultNum != -1)
                                    {
                                        switch (ToExamineResultNum)
                                        {
                                            case 0:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == 2
                                                || x.ToExamineResult == -3);
                                                break;
                                            case 1:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == 2 || x.ToExamineResult == 3);
                                                break;
                                            case 2:
                                                listEntry = listEntry.Where(x => x.ToExamineResult == -2 || x.ToExamineResult == -3
                                                || x.ToExamineResult == -5 || x.ToExamineResult == -4);
                                                break;
                                        }
                                    }
                                    break;

                            }
                        }
                        else
                        {
                            listEntry = listEntry.Where(x => x.OperatorId == adminId);
                        }
                    }
                    else
                    {
                        listEntry = listEntry.Where(x => x.OperatorId == adminId);
                    }

                    var list = listEntry.Where<Resignation, int>(predicate, request.PageCondition, out count).Select(m => new
                    {
                        m.Id,
                        ResignationName = m.ResignationModel == null ? "" : m.ResignationModel.Member.RealName,
                        m.ResignationModel.Department.DepartmentName,
                        m.ResignationModel.Member.MobilePhone,
                        m.HandoverMan.Member.RealName,
                        OperatorName = m.Operation == null ? "" : m.Operation.Member.RealName,
                        Auditauthority = Auditauthority,
                        m.ToExamineResult,
                        m.operationId,
                        CurrentId = adminId
                    }).ToList();
                    return new GridData<object>(list, count, request.RequestInfo);
                }
                else
                {
                    return new GridData<object>(null, count, request.RequestInfo);
                }
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public JsonResult GetToExamineCount()
        {
            var adminId = AuthorityHelper.OperatorId.Value;
            var admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            var count = 0;
            int Auditauthority = 0;
            if (admin != null)
            {
                IQueryable<Resignation> listEntry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled);
                if (admin.JobPosition != null)
                {
                    if (admin.JobPosition.Auditauthority != null && admin.JobPosition.Auditauthority.Value != 0)
                    {
                        Auditauthority = admin.JobPosition.Auditauthority.Value;
                        switch (Auditauthority)
                        {
                            case 1:
                                count = listEntry.Where(x => x.ToExamineResult == 0).Count();
                                break;
                            case 4:
                                count = listEntry.Where(x => x.ToExamineResult == 0 || x.ToExamineResult == 1).Count();
                                break;
                            case 5:
                                count = listEntry.Where(x => x.ToExamineResult == 0 || x.ToExamineResult == 2).Count();
                                break;
                            case 7:
                                count = listEntry.Where(x => x.ToExamineResult != 3 && x.ToExamineResult != -6 && x.ToExamineResult != -4
                                                && x.ToExamineResult != -1).Count();
                                break;
                            case 2:
                                count = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == -3).Count();
                                break;
                            case 3:
                                count = listEntry.Where(x => x.ToExamineResult == 2).Count();
                                break;
                            case 6:
                                count = listEntry.Where(x => x.ToExamineResult == 1 || x.ToExamineResult == 2
                                                || x.ToExamineResult == -3).Count();
                                break;
                        }
                    }
                }
            }
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, count));
        }

        #region 参数解析
        /// <summary>
        /// 参数解析
        /// </summary>
        /// <param name="listFilterFule"></param>
        /// <param name="strParams"></param>
        /// <returns></returns>
        private string GetParam(ICollection<FilterRule> listFilterFule, string strParams)
        {
            string strValue = string.Empty;
            FilterRule departRule = listFilterFule.FirstOrDefault(x => x.Field == strParams);
            if (departRule != null)
            {
                strValue = departRule.Value.ToString();
                listFilterFule.Remove(departRule);
            }
            return strValue;
        }
        #endregion

        #region 添加数据
        public ActionResult Create()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(_departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.DepartmentName
                }));
            ViewBag.depList = list;
            return PartialView();
        }

        public JsonResult GetUserList(int? departmentId)
        {
            var da = _administratorContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == departmentId).
                Select(x => new { x.Id, x.Member.RealName });
            return Json(da);
        }

        public JsonResult GetMemberInfoByMobilePhone(string MobilePhone)
        {
            string errorMsg = string.Empty;
            int type = 0;
            var member = _memberContract.Members.Where(x => x.MobilePhone == MobilePhone).FirstOrDefault();
            var admin = _administratorContract.Administrators.Where(x => x.MemberId == member.Id).FirstOrDefault();
            if (member != null)
            {


                if (admin == null)
                {
                    errorMsg = "不存在此用户";
                }
                else
                {
                    var entry = _resignationContract.Resignations.Where(x => x.ResignationId == admin.Id).FirstOrDefault();
                    if (entry != null)
                    {
                        if (entry.ToExamineResult == 3)
                        {
                            errorMsg = "此用户离职审核已通过";
                        }
                        else
                        {
                            errorMsg = "此用户离职审核中";
                        }
                    }
                    else
                    {
                        type = 1;
                        errorMsg = "";
                    }
                }
            }
            else
            {
                errorMsg = "不存在手机号";
            }

            var da = new
            {
                type = type,
                Id = admin == null ? 0 : admin.Id,
                RealName = member == null ? "" : member.RealName,
                DepartmentName = admin == null ? "" : admin.Department.DepartmentName,
                errorMsg = errorMsg
            };
            return Json(da, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(ResignationDto dto)
        {
            dto.ToExamineResult = 0;
            dto.operationId = AuthorityHelper.OperatorId;
            OperationResult oper = _resignationContract.Insert(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "离职通知").Select(x => x.Id).FirstOrDefault();
                var orgContent = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId)
                   .Select(x => x.TemplateHtml).FirstOrDefault();
                var title = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId)
                   .Select(x => x.TemplateName).FirstOrDefault();
                List<int> AuthorityList = new List<int>() { 1, 4, 5, 7 };
                var resign = _administratorContract.Administrators.Where(x => x.Id == dto.ResignationId).FirstOrDefault();
                var dicPC = new Dictionary<string, object>();
                dicPC.Add("entryName", resign?.Member?.RealName ?? string.Empty);
                dicPC.Add("entryDep", resign?.Department?.DepartmentName ?? string.Empty);
                dicPC.Add("ToExamine", "");
                var content = NVelocityHelper.Generate(orgContent, dicPC, "Entry_PC");
                var receiveAdminIds = _administratorContract.Administrators.Where(x => x.IsEnabled && !x.IsDeleted &&
               AuthorityList.Contains(x.JobPosition.Auditauthority.Value)).Select(s => s.Id).ToList();
                _notificationContract.Insert(sendNotificationAction, new NotificationDto()
                {
                    Title = title,
                    AdministratorIds = receiveAdminIds,
                    Description = content,
                    IsEnableApp = true,
                    NoticeTargetType = (int)NoticeTargetFlag.Admin,
                    NoticeType = (int)NoticeFlag.Immediate
                });
            }
            return Json(oper);
        }
        #endregion

        public ActionResult Personnelmatters(int? EntryId)
        {
            var entry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            ViewBag.ResignationDepartmentName = entry.ResignationModel == null ? "" : entry.ResignationModel.Department.DepartmentName;
            ViewBag.ResignationName = entry.ResignationModel == null ? "" : entry.ResignationModel.Member.RealName;
            ViewBag.HandoverManDepartmentName = entry.HandoverMan == null ? "" : entry.HandoverMan.Department.DepartmentName;
            ViewBag.HandoverManName = entry.HandoverMan == null ? "" : entry.HandoverMan.Member.RealName;
            ViewBag.SubmitName = entry.Operation == null ? "" : entry.Operation.Member.RealName;
            entry.ResignationReason = entry.ResignationReason.Replace("\n", "").Replace("\r\n", "");
            return PartialView(entry);
        }

        public JsonResult PersonnelmattersDetaile(ResignationDto dto)
        {
            dto.ToExamineResult = 1;
            OperationResult oper = _resignationContract.Update(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                ResignationToExamine ete = new ResignationToExamine();
                ete.AdminId = AuthorityHelper.OperatorId.Value;
                ete.Reason = "";
                ete.ResignationId = dto.Id;
                ete.AuditStatus = 1;
                ete.AuditTime = DateTime.Now;
                ete.OperatorId = AuthorityHelper.OperatorId;
                oper = _resignationToExamineContract.Insert(ete);
                if (oper.ResultType == OperationResultType.Success)
                {
                    var dtoM = _resignationContract.Resignations.Where(x => x.Id == dto.Id).FirstOrDefault();
                    int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "离职通知").Select(x => x.Id).FirstOrDefault();

                    var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                    {
                        s.Id,
                        s.TemplateHtml,
                        s.TemplateName,
                        s.EnabledPerNotifi
                    }).FirstOrDefault();

                    var orgContent = mod.TemplateHtml;
                    var title = mod.TemplateName;
                    List<int> AuthorityList = new List<int>() { 2, 6, 7 };
                    var dicPC = new Dictionary<string, object>();
                    dicPC.Add("entryName", dtoM.ResignationModel?.Member?.RealName ?? string.Empty);
                    dicPC.Add("entryDep", dtoM.Department?.DepartmentName ?? string.Empty);
                    dicPC.Add("ToExamine", AuthorityHelper.RealName);
                    var content = NVelocityHelper.Generate(orgContent, dicPC, "Entry_PC");

                    var dtoNoti = new NotificationDto()
                    {
                        Title = title,
                        Description = content,
                        IsEnableApp = true,
                        NoticeType = (int)NoticeFlag.Immediate,
                        NoticeTargetType = mod.EnabledPerNotifi ? (int)NoticeTargetFlag.Admin : (int)NoticeTargetFlag.Department
                    };

                    if (mod.EnabledPerNotifi)
                    {
                        var receiveAdminIds = _administratorContract.Administrators.Where(x => x.IsEnabled && !x.IsDeleted &&
                        AuthorityList.Contains(x.JobPosition.Auditauthority.Value)).Select(s => s.Id).ToList();
                        dtoNoti.AdministratorIds = receiveAdminIds;
                    }
                    else
                    {
                        var depids = _templateContract.GetNotificationDepartIds(mod.Id);
                        dtoNoti.DepartmentIds = depids;
                    }

                    _notificationContract.Insert(sendNotificationAction, dtoNoti);
                }
            }
            return Json(oper);
        }

        public ActionResult Technology(int? EntryId)
        {
            var entry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            ViewBag.ResignationDepartmentName = entry.ResignationModel == null ? "" : entry.ResignationModel.Department.DepartmentName;
            ViewBag.ResignationName = entry.ResignationModel == null ? "" : entry.ResignationModel.Member.RealName;
            ViewBag.HandoverManDepartmentName = entry.HandoverMan == null ? "" : entry.HandoverMan.Department.DepartmentName;
            ViewBag.HandoverManName = entry.HandoverMan == null ? "" : entry.HandoverMan.Member.RealName;
            ViewBag.SubmitName = entry.Operation == null ? "" : entry.Operation.Member.RealName;
            entry.ResignationReason = entry.ResignationReason.Replace("\n", "").Replace("\r\n", "");
            return PartialView(entry);
        }

        public JsonResult TechnologyDetaile(ResignationDto dto)
        {
            OperationResult oper = _resignationContract.Update(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                if (dto.ToExamineResult == 3)
                {
                    oper = _administratorContract.Disable(dto.ResignationId.Value);
                }
                ResignationToExamine ete = new ResignationToExamine();
                ete.AdminId = AuthorityHelper.OperatorId.Value;
                ete.Reason = "";
                ete.ResignationId = dto.Id;
                ete.AuditStatus = 2;
                ete.AuditTime = DateTime.Now;
                ete.OperatorId = AuthorityHelper.OperatorId;
                oper = _resignationToExamineContract.Insert(ete);
                if (oper.ResultType == OperationResultType.Success)
                {
                    var dtoM = _resignationContract.Resignations.Where(x => x.Id == dto.Id).FirstOrDefault();
                    string title = string.Empty;
                    string orgContent = string.Empty;
                    List<int> AuthorityList = new List<int>() { 3, 7 };
                    var dicPC = new Dictionary<string, object>();
                    dicPC.Add("entryName", dtoM.ResignationModel?.Member?.RealName ?? string.Empty);
                    dicPC.Add("entryDep", dtoM.Department?.DepartmentName ?? string.Empty);
                    dicPC.Add("ToExamine", AuthorityHelper.RealName);

                    var dtoNoti = new NotificationDto()
                    {
                        IsEnableApp = true,
                        NoticeType = (int)NoticeFlag.Immediate,
                        //NoticeTargetType = mod.EnabledPerNotifi ? (int)NoticeTargetFlag.Admin : (int)NoticeTargetFlag.Department
                    };

                    if (dto.ToExamineResult == 3)
                    {
                        int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "离职通知-财务审核").Select(x => x.Id).FirstOrDefault();

                        var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                        {
                            s.Id,
                            s.TemplateHtml,
                            s.TemplateName,
                            s.EnabledPerNotifi
                        }).FirstOrDefault();

                        orgContent = mod.TemplateHtml;
                        title = mod.TemplateName;
                        dicPC.Add("time", dto.ResignationDate.ToLongDateString());
                        if (mod.EnabledPerNotifi)
                        {
                            var receiveAdminIds = _administratorContract.Administrators.Where(x => x.IsEnabled && !x.IsDeleted).Select(s => s.Id).ToList();
                            dtoNoti.AdministratorIds = receiveAdminIds;
                        }
                        else
                        {
                            var depids = _templateContract.GetNotificationDepartIds(mod.Id);
                            dtoNoti.DepartmentIds = depids;
                        }
                        dtoNoti.NoticeTargetType = mod.EnabledPerNotifi ? (int)NoticeTargetFlag.Admin : (int)NoticeTargetFlag.Department;
                    }
                    else
                    {
                        int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "离职通知").Select(x => x.Id).FirstOrDefault();
                        var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                        {
                            s.Id,
                            s.TemplateHtml,
                            s.TemplateName,
                            s.EnabledPerNotifi
                        }).FirstOrDefault();
                        orgContent = mod.TemplateHtml;
                        title = mod.TemplateName;
                        if (mod.EnabledPerNotifi)
                        {
                            var receiveAdminIds = _administratorContract.Administrators.Where(x => x.IsEnabled && !x.IsDeleted &&
                            AuthorityList.Contains(x.JobPosition.Auditauthority.Value)).Select(s => s.Id).ToList();
                            dtoNoti.AdministratorIds = receiveAdminIds;
                        }
                        else
                        {
                            var depids = _templateContract.GetNotificationDepartIds(mod.Id);
                            dtoNoti.DepartmentIds = depids;
                        }
                        dtoNoti.NoticeTargetType = mod.EnabledPerNotifi ? (int)NoticeTargetFlag.Admin : (int)NoticeTargetFlag.Department;
                    }
                    var content = NVelocityHelper.Generate(orgContent, dicPC, "Entry_PC");

                    dtoNoti.Title = title;
                    dtoNoti.Description = content;

                    _notificationContract.Insert(sendNotificationAction, dtoNoti);
                }
            }
            return Json(oper);
        }

        public ActionResult Finance(int? EntryId)
        {
            var entry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            ViewBag.ResignationDepartmentName = entry.ResignationModel == null ? "" : entry.ResignationModel.Department.DepartmentName;
            ViewBag.ResignationName = entry.ResignationModel == null ? "" : entry.ResignationModel.Member.RealName;
            ViewBag.HandoverManDepartmentName = entry.HandoverMan == null ? "" : entry.HandoverMan.Department.DepartmentName;
            ViewBag.HandoverManName = entry.HandoverMan == null ? "" : entry.HandoverMan.Member.RealName;
            ViewBag.SubmitName = entry.Operation == null ? "" : entry.Operation.Member.RealName;
            entry.ResignationReason = entry.ResignationReason.Replace("\n", "").Replace("\r\n", "");
            return PartialView(entry);
        }

        public ActionResult View(int? EntryId)
        {
            var entry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            ViewBag.ResignationDepartmentName = entry.ResignationModel == null ? "" : entry.ResignationModel.Department.DepartmentName;
            ViewBag.ResignationName = entry.ResignationModel == null ? "" : entry.ResignationModel.Member.RealName;
            ViewBag.HandoverManDepartmentName = entry.HandoverMan == null ? "" : entry.HandoverMan.Department.DepartmentName;
            ViewBag.HandoverManName = entry.HandoverMan == null ? "" : entry.HandoverMan.Member.RealName;
            ViewBag.SubmitName = entry.Operation == null ? "" : entry.Operation.Member.RealName;
            entry.ResignationReason = entry.ResignationReason.Replace("\n", "").Replace("\r\n", "");
            return PartialView(entry);
        }

        public JsonResult ToExamine(int ToExamineStatues, string Reason, int EntryId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            ResignationToExamine ete = new ResignationToExamine();
            ete.AdminId = AuthorityHelper.OperatorId.Value;
            ete.Reason = Reason;
            ete.ResignationId = EntryId;
            ete.AuditStatus = ToExamineStatues;
            ete.AuditTime = DateTime.Now;
            ete.OperatorId = AuthorityHelper.OperatorId;
            oper = _resignationToExamineContract.Insert(ete);
            if (oper.ResultType == OperationResultType.Success)
            {
                oper = _resignationContract.ToExamine(ToExamineStatues, EntryId);
                if (oper.ResultType == OperationResultType.Success)
                {
                    var dtoM = _resignationContract.Resignations.Where(x => x.Id == EntryId).FirstOrDefault();
                    int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "离职审核未通过").Select(x => x.Id).FirstOrDefault();
                    var orgContent = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId)
                       .Select(x => x.TemplateHtml).FirstOrDefault();
                    var title = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId)
                       .Select(x => x.TemplateName).FirstOrDefault();
                    List<int> AuthorityList = new List<int>();
                    var receiveAdminIds = 0;
                    if (ToExamineStatues == -3)
                    {
                        receiveAdminIds = _resignationToExamineContract.EntryToExamines.Where(x => x.ResignationId == EntryId && x.AuditStatus == 2)
                            .OrderByDescending(x => x.CreatedTime).Select(x => x.AdminId).FirstOrDefault();
                    }
                    else if (ToExamineStatues == -2 || ToExamineStatues == -5)
                    {
                        receiveAdminIds = _resignationToExamineContract.EntryToExamines.Where(x => x.ResignationId == EntryId && x.AuditStatus == 1)
                                            .OrderByDescending(x => x.CreatedTime).Select(x => x.AdminId).FirstOrDefault();
                    }
                    else if (ToExamineStatues == -1 || ToExamineStatues == -6)
                    {
                        receiveAdminIds = _resignationContract.Resignations.Where(x => x.Id == EntryId).Select(x => x.operationId ?? (AuthorityHelper.OperatorId ?? 0)).FirstOrDefault();
                    }
                    var dicPC = new Dictionary<string, object>();
                    dicPC.Add("entryName", dtoM.ResignationModel?.Member?.RealName ?? string.Empty);
                    dicPC.Add("entryDep", dtoM.Department?.DepartmentName ?? string.Empty);
                    dicPC.Add("ToExamine", AuthorityHelper.RealName);
                    var content = NVelocityHelper.Generate(orgContent, dicPC, "Entry_PC");
                    List<int> lAdmin = new List<int>();
                    lAdmin.Add(receiveAdminIds);
                    _notificationContract.Insert(sendNotificationAction, new NotificationDto()
                    {
                        Title = title,
                        AdministratorIds = lAdmin,
                        Description = content,
                        IsEnableApp = true,
                        NoticeTargetType = (int)NoticeTargetFlag.Admin,
                        NoticeType = (int)NoticeFlag.Immediate
                    });
                }
            }
            return Json(oper);
        }

        public ActionResult Update(int? EntryId)
        {
            var entry = _resignationContract.Resignations.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            ViewBag.ResignationDepartmentName = entry.ResignationModel == null ? "" : entry.ResignationModel.Department.DepartmentName;
            ViewBag.ResignationName = entry.ResignationModel == null ? "" : entry.ResignationModel.Member.RealName;
            ViewBag.HandoverManDepartmentName = entry.HandoverMan == null ? "" : entry.HandoverMan.Department.DepartmentName;
            ViewBag.HandoverManName = entry.HandoverMan == null ? "" : entry.HandoverMan.Member.RealName;
            ViewBag.SubmitName = entry.Operation == null ? "" : entry.Operation.Member.RealName;
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(_departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.DepartmentName
                }));
            ViewBag.depList = list;
            entry.ResignationReason = entry.ResignationReason.Replace("\n", "").Replace("\r\n", "");
            return PartialView(entry);
        }

        [HttpPost]
        public ActionResult Update(ResignationDto dto)
        {
            dto.ToExamineResult = 0;
            dto.operationId = AuthorityHelper.OperatorId;
            OperationResult oper = _resignationContract.Update(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "离职通知").Select(x => x.Id).FirstOrDefault();
                var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                {
                    s.Id,
                    s.TemplateHtml,
                    s.TemplateName,
                    s.EnabledPerNotifi
                }).FirstOrDefault();

                List<int> AuthorityList = new List<int>() { 1, 4, 5, 7 };
                var resign = _administratorContract.Administrators.Where(x => x.Id == dto.ResignationId).FirstOrDefault();
                var dicPC = new Dictionary<string, object>();
                dicPC.Add("entryName", resign?.Member?.RealName ?? string.Empty);
                dicPC.Add("entryDep", resign?.Department?.DepartmentName ?? string.Empty);
                dicPC.Add("ToExamine", "");
                var content = NVelocityHelper.Generate(mod.TemplateHtml, dicPC, "Entry_PC");

                var dtoNoti = new NotificationDto()
                {
                    Title = mod.TemplateName,
                    Description = content,
                    IsEnableApp = true,
                    NoticeType = (int)NoticeFlag.Immediate,
                    NoticeTargetType = mod.EnabledPerNotifi ? (int)NoticeTargetFlag.Admin : (int)NoticeTargetFlag.Department
                };

                if (mod.EnabledPerNotifi)
                {
                    var receiveAdminIds = _administratorContract.Administrators.Where(x => x.IsEnabled && !x.IsDeleted &&
                    AuthorityList.Contains(x.JobPosition.Auditauthority.Value)).Select(s => s.Id).ToList();
                    dtoNoti.AdministratorIds = receiveAdminIds;
                }
                else
                {
                    var depids = _templateContract.GetNotificationDepartIds(mod.Id);
                    dtoNoti.DepartmentIds = depids;
                }

                _notificationContract.Insert(sendNotificationAction, dtoNoti);
            }
            return Json(oper);
        }
    }
}