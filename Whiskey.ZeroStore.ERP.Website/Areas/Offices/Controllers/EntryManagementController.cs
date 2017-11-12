/*
 * 身份标记：1人事，2技术，3财务，4人事+技术，5人事+财务，6技术+财务，7人事+技术+财务
 * 审核通过标识：0员工->人事,1人事->技术,2技术->财务，3审核通过
 * 审核拒绝标记：-1人事->员工,-2技术->人事,-3财务->技术,-4财务->员工（拒绝）,-5财务->技术->人事,-6财务->技术->人事->员工
 * 审核拒绝标记为：-1,-6时 员工可以重新修改提交审核，-2,-5时 人事可以修改，-3时 技术可以修改
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Utility.Filter;
using Whiskey.Core.Data.Extensions;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Whiskey.ZeroStore.ERP.Transfers;
using System.IO;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class EntryManagementController : BaseController
    {

        public readonly IEntryContract _entryContract;

        public readonly IAdministratorContract _administratorContract;

        public readonly IDepartmentContract _departmentContract;

        public readonly IJobPositionContract _jobPositionContract;
        public readonly IEntryToExamineContract _entryToExamineContract;
        public readonly IRoleContract _roleContract;
        public readonly IMemberContract _memberContract;
        public readonly ITemplateContract _templateContract;
        public readonly ITemplateNotificationContract _templateNotificationContract;
        public readonly INotificationContract _notificationContract;
        protected readonly ITrainintBlogContract _blogContract;

        public EntryManagementController(IEntryContract entryContract, IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IJobPositionContract jobPositionContract,
            IEntryToExamineContract entryToExamineContract,
            IRoleContract roleContract,
            IMemberContract memberContract,
            ITemplateContract templateContract,
            ITemplateNotificationContract templateNotificationContract,
            INotificationContract notificationContract,
            ITrainintBlogContract blogContract)
        {
            _entryContract = entryContract;
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _jobPositionContract = jobPositionContract;
            _entryToExamineContract = entryToExamineContract;
            _roleContract = roleContract;
            _memberContract = memberContract;
            _templateContract = templateContract;
            _templateNotificationContract = templateNotificationContract;
            _notificationContract = notificationContract;
            _blogContract = blogContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            var adminId = AuthorityHelper.OperatorId.Value;
            var admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            Expression<Func<Entry, bool>> predicate = FilterHelper.GetExpression<Entry>(request.FilterGroup);
            string Realname = Request["Realname"];
            string MobilePhone = Request["MobilePhone"];
            string ToExamineResultStr = Request["ToExamineResult"];
            var data = await Task.Run(() =>
            {
                var count = 0;
                int Auditauthority = 0;
                if (admin != null)
                {
                    IQueryable<Entry> listEntry = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled);
                    if (!string.IsNullOrEmpty(Realname))
                    {
                        listEntry = listEntry.Where(x => x.Member.MemberName.Contains(Realname));
                    }
                    if (!string.IsNullOrEmpty(MobilePhone))
                    {
                        listEntry = listEntry.Where(x => x.Member.MobilePhone == MobilePhone);
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
                                case 1://人事
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
                                case 4://人事+技术
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
                                case 5://人事+财务
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
                                case 7://人事+技术+财务
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
                                case 2://技术
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
                                case 3://财务
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
                                case 6://技术+财务
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

                    var list = listEntry.Where<Entry, int>(predicate, request.PageCondition, out count).Select(m => new
                    {
                        m.Id,
                        m.Member.MemberName,
                        m.Member.RealName,
                        m.Member.MobilePhone,
                        m.Member.Gender,
                        OperatorName = m.Operation.Member.RealName,
                        Auditauthority = Auditauthority,
                        m.ToExamineResult,
                        m.operationId,
                        CurrentId = adminId,
                        m.Member.UniquelyIdentifies
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
        public JsonResult GetToExamineCount()
        {
            var adminId = AuthorityHelper.OperatorId.Value;
            var admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            var count = 0;
            int Auditauthority = 0;
            if (admin != null)
            {
                IQueryable<Entry> listEntry = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled);
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

        public ActionResult CreateMember()
        {
            return PartialView();
        }

        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult Create(Entry dto)
        {
            dto.OperatorId = AuthorityHelper.OperatorId;
            dto.operationId = AuthorityHelper.OperatorId;
            dto.EntryTime = DateTime.Now;
            dto.ToExamineResult = 0;
            OperationResult oper = _entryContract.Insert(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "入职通知").Select(x => x.Id).FirstOrDefault();
                var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                {
                    s.Id,
                    s.TemplateHtml,
                    s.TemplateName,
                    s.EnabledPerNotifi
                }).FirstOrDefault();
                List<int> AuthorityList = new List<int>() { 1, 4, 5, 7 };
                string entryName = _memberContract.Members.Where(x => x.Id == dto.MemberId).Select(x => x.RealName).FirstOrDefault();
                var dicPC = new Dictionary<string, object>();
                dicPC.Add("entryName", entryName ?? string.Empty);
                dicPC.Add("entryDep", "");
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

        public ActionResult Personnelmatters(int? EntryId)
        {
            var entry = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            var department = (_departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled).Select(x =>
                new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.DepartmentName
                })).ToList();
            ViewBag.Department = department;

            return PartialView(entry);
        }

        public ActionResult Technology(int? EntryId)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            var rols = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            foreach (var item in rols)
            {
                dic.Add(item.Id, item.RoleName);
            }
            ViewBag.roles = dic;

            var entry = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();

            return PartialView(entry);
        }

        public JsonResult GetJobPosition(int? departmentId)
        {
            var da = _jobPositionContract.JobPositions.Where(x => x.IsEnabled && !x.IsDeleted && x.DepartmentId == departmentId.Value).Select(x => new
            {
                x.Id,
                x.JobPositionName
            });
            return Json(da);
        }

        public JsonResult PersonnelmattersDetaile(EntryDto dto)
        {
            dto.ToExamineResult = 1;

            var model = _entryContract.Entrys.Where(x => x.Id == dto.Id).FirstOrDefault();
            dto.MemberId = model.MemberId;
            dto.MacAddress = model.MacAddress;
            dto.BankcardImgPath = model.BankcardImgPath;
            dto.IdCardImgPath = model.IdCardImgPath;
            dto.HealthCertificateImgPath = model.HealthCertificateImgPath;
            dto.PhotoImgPath = model.PhotoImgPath;
            //dto.InterviewEvaluation = model.InterviewEvaluation;
            dto.operationId = model.operationId == null ? 0 : model.operationId.Value;
            OperationResult oper = _entryContract.Update(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                EntryToExamine ete = new EntryToExamine();
                ete.AdminId = AuthorityHelper.OperatorId.Value;
                ete.Reason = "";
                ete.EntryId = dto.Id;
                ete.AuditStatus = 1;
                ete.AuditTime = DateTime.Now;
                ete.OperatorId = AuthorityHelper.OperatorId;
                oper = _entryToExamineContract.Insert(ete);
                if (oper.ResultType == OperationResultType.Success)
                {
                    var dtoM = _entryContract.Entrys.Where(x => x.Id == dto.Id).FirstOrDefault();
                    int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "入职通知").Select(x => x.Id).FirstOrDefault();
                    var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                    {
                        s.Id,
                        s.TemplateHtml,
                        s.TemplateName,
                        s.EnabledPerNotifi
                    }).FirstOrDefault();
                    List<int> AuthorityList = new List<int>() { 2, 6, 7 };
                    string entryName = _memberContract.Members.Where(x => x.Id == dtoM.MemberId).Select(x => x.RealName).FirstOrDefault();
                    var dicPC = new Dictionary<string, object>();
                    dicPC.Add("entryName", dtoM.Member.RealName ?? string.Empty);
                    dicPC.Add("entryDep", dtoM.Department.DepartmentName ?? string.Empty);
                    dicPC.Add("ToExamine", AuthorityHelper.RealName);
                    dicPC.Add("Sex", dtoM.Member.Gender == 0 ? "女" : "男");
                    dicPC.Add("InterviewEvaluation", dtoM.InterviewEvaluation);
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
            }
            return Json(oper);
        }
        [HttpPost]
        public ActionResult CreateMember(MemberDto dto, MemberFigure fig)
        {
            string apparelSize = Request["ShangZhuang"] + "," + Request["XiaZhuang"];
            string figureDes = Request["FigureDes"];
            string color = Request["PreferenceColor"];
            fig.ApparelSize = apparelSize;
            fig.PreferenceColor = color;
            fig.FigureDes = figureDes;
            fig.Birthday = dto.DateofBirth;
            dto.MemberFigures.Add(fig);
            dto.UserPhoto = "/Content/Images/logo-_03.png";
            var result = _memberContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult TechnologyDetaile(string Jurisdiction, string InterviewEvaluation, int entryId)
        {
            InterviewEvaluation = System.Web.HttpUtility.UrlDecode(InterviewEvaluation);
            OperationResult oper = _entryContract.RoleJurisdiction(Jurisdiction, InterviewEvaluation, entryId);
            if (oper.ResultType == OperationResultType.Success)
            {
                EntryToExamine ete = new EntryToExamine();
                ete.AdminId = AuthorityHelper.OperatorId.Value;
                ete.Reason = "";
                ete.EntryId = entryId;
                ete.AuditStatus = 2;
                ete.AuditTime = DateTime.Now;
                ete.OperatorId = AuthorityHelper.OperatorId;
                oper = _entryToExamineContract.Insert(ete);
                if (oper.ResultType == OperationResultType.Success)
                {
                    oper = _entryContract.ToExamine(2, entryId);

                    if (oper.ResultType == OperationResultType.Success)
                    {
                        var dtoM = _entryContract.Entrys.Where(x => x.Id == entryId).FirstOrDefault();
                        int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "入职通知").Select(x => x.Id).FirstOrDefault();
                        var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                        {
                            s.Id,
                            s.TemplateHtml,
                            s.TemplateName,
                            s.EnabledPerNotifi
                        }).FirstOrDefault();
                        List<int> AuthorityList = new List<int>() { 3, 7 };
                        string entryName = _memberContract.Members.Where(x => x.Id == dtoM.MemberId).Select(x => x.RealName).FirstOrDefault();
                        var dicPC = new Dictionary<string, object>();
                        dicPC.Add("entryName", dtoM.Member.RealName ?? string.Empty);
                        dicPC.Add("entryDep", dtoM.Department.DepartmentName ?? string.Empty);
                        dicPC.Add("ToExamine", AuthorityHelper.RealName);
                        dicPC.Add("Sex", dtoM.Member.Gender == 0 ? "女" : "男");
                        dicPC.Add("InterviewEvaluation", dtoM.InterviewEvaluation);
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
                }
            }
            return Json(oper);
        }

        public JsonResult ToExamine(int ToExamineStatues, string Reason, int EntryId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            EntryToExamine ete = new EntryToExamine();
            ete.AdminId = AuthorityHelper.OperatorId.Value;
            ete.Reason = Reason;
            ete.EntryId = EntryId;
            ete.AuditStatus = ToExamineStatues;
            ete.AuditTime = DateTime.Now;
            ete.OperatorId = AuthorityHelper.OperatorId;
            if (ToExamineStatues == 3)
            {
                oper = AddAdmin(EntryId);
                if (oper.ResultType == OperationResultType.Success)
                {
                    oper = _entryToExamineContract.Insert(ete);
                    if (oper.ResultType == OperationResultType.Success)
                    {
                        oper = _entryContract.ToExamine(ToExamineStatues, EntryId);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            var dtoM = _entryContract.Entrys.Where(x => x.Id == EntryId).FirstOrDefault();
                            int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "入职通知-财务审核").Select(x => x.Id).FirstOrDefault();
                            var mod = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId).Select(s => new
                            {
                                s.Id,
                                s.TemplateHtml,
                                s.TemplateName,
                                s.EnabledPerNotifi
                            }).FirstOrDefault();

                            string entryName = _memberContract.Members.Where(x => x.Id == dtoM.MemberId).Select(x => x.RealName).FirstOrDefault();
                            var dicPC = new Dictionary<string, object>();
                            dicPC.Add("entryName", dtoM.Member.RealName ?? string.Empty);
                            dicPC.Add("entryDep", dtoM.Department.DepartmentName ?? string.Empty);
                            dicPC.Add("ToExamine", AuthorityHelper.RealName);
                            dicPC.Add("Sex", dtoM.Member.Gender == 0 ? "女" : "男");
                            dicPC.Add("InterviewEvaluation", dtoM.InterviewEvaluation);
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
                                var receiveAdminIds = _administratorContract.Administrators.Where(x => x.IsEnabled && !x.IsDeleted).Select(s => s.Id).ToList();
                                dtoNoti.AdministratorIds = receiveAdminIds;
                            }
                            else
                            {
                                var depids = _templateContract.GetNotificationDepartIds(mod.Id);
                                dtoNoti.DepartmentIds = depids;
                            }

                            List<NotificationDto> dtoNotis = new List<NotificationDto> { dtoNoti };

                            var trains = _blogContract.Entities.Where(b => !b.IsDeleted && b.IsEnabled && b.IsTrain).Select(b => new { b.Id, b.ExamId });
                            var admin = _administratorContract.Administrators.FirstOrDefault(a => a.MemberId == dtoM.MemberId && !a.IsDeleted && a.IsEnabled);
                            foreach (var t in trains)
                            {
                                var ntf = new NotificationDto()
                                {
                                    Title = "入职培训考试",
                                    Description = "恭喜入职！请您先参加以下入职培训考试",
                                    IsEnableApp = true,
                                    NoticeType = (int)NoticeFlag.Immediate,
                                    NoticeTargetType = (int)NoticeTargetFlag.Admin,
                                    AdministratorIds = new int[] { admin.Id },
                                    //DepartmentIds = dtoNoti.DepartmentIds,
                                    IsEntryTrain = true,
                                    BlogId = t.Id,
                                    ExamId = t.ExamId
                                };

                                dtoNotis.Add(ntf);
                            }

                            _notificationContract.Insert(sendNotificationAction, dtoNotis.ToArray());

                        }
                    }
                }
            }
            else
            {
                oper = _entryToExamineContract.Insert(ete);
                var _entry = _entryContract.Entrys.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId);
                if (oper.ResultType == OperationResultType.Success)
                {
                    oper = _entryContract.ToExamine(ToExamineStatues, EntryId);
                    if (oper.ResultType == OperationResultType.Success)
                    {
                        var dtoM = _entryContract.Entrys.Where(x => x.Id == EntryId).FirstOrDefault();
                        int NotificationId = _templateNotificationContract.templateNotifications.Where(x => x.Name == "入职审核未通过").Select(x => x.Id).FirstOrDefault();
                        var orgContent = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId)
                           .Select(x => x.TemplateHtml).FirstOrDefault();
                        var title = _templateContract.Templates.Where(x => !x.IsDeleted && x.IsEnabled && x.TemplateNotificationId == NotificationId)
                           .Select(x => x.TemplateName).FirstOrDefault();
                        string entryName = _memberContract.Members.Where(x => x.Id == dtoM.MemberId).Select(x => x.RealName).FirstOrDefault();
                        List<int> AuthorityList = new List<int>();
                        var receiveAdminIds = 0;
                        if (ToExamineStatues == -3)
                        {
                            receiveAdminIds = _entryToExamineContract.EntryToExamines.Where(x => x.EntryId == EntryId && x.AuditStatus == 2)
                                .OrderByDescending(x => x.CreatedTime).Select(x => x.AdminId).FirstOrDefault();
                        }
                        else if (ToExamineStatues == -2 || ToExamineStatues == -5)
                        {
                            receiveAdminIds = _entryToExamineContract.EntryToExamines.Where(x => x.EntryId == EntryId && x.AuditStatus == 1)
                                            .OrderByDescending(x => x.CreatedTime).Select(x => x.AdminId).FirstOrDefault();
                        }
                        else if (ToExamineStatues == -1 || ToExamineStatues == -6)
                        {
                            receiveAdminIds = _entryContract.Entrys.Where(x => x.Id == EntryId).Select(x => x.operationId.Value).FirstOrDefault();
                        }
                        var dicPC = new Dictionary<string, object>();
                        dicPC.Add("entryName", dtoM.Member.RealName ?? string.Empty);
                        dicPC.Add("entryDep", dtoM.Department.DepartmentName ?? string.Empty);
                        dicPC.Add("ToExamine", AuthorityHelper.RealName);
                        dicPC.Add("Sex", dtoM.Member.Gender == 0 ? "女" : "男");
                        dicPC.Add("InterviewEvaluation", dtoM.InterviewEvaluation);
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
                        if (ToExamineStatues == -2 || ToExamineStatues == -5)
                        {
                            oper = _entryContract.RoleJurisdiction("", _entry.InterviewEvaluation, EntryId);
                        }
                    }

                }
            }

            return Json(oper);
        }

        public ActionResult EntryDetaile(int? EntryId)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            var rols = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            foreach (var item in rols)
            {
                dic.Add(item.Id, item.RoleName);
            }
            ViewBag.roles = dic;

            var entry = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            return PartialView(entry);
        }

        public OperationResult AddAdmin(int EntryId)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            Administrator admin = new Administrator();
            var entry = _entryContract.Entrys.Where(x => x.Id == EntryId).FirstOrDefault();
            if (entry != null)
            {
                int count = _administratorContract.Administrators.Where(x => x.MemberId == entry.MemberId).Count();
                if (count > 0)
                {
                    resul = new OperationResult(OperationResultType.Error, "用户已经升级为员工");
                }
                count = _administratorContract.Administrators.Where(x => x.MacAddress == entry.MacAddress).Count();
                if (count > 0)
                {
                    resul = new OperationResult(OperationResultType.Error, "Mac地址已经存在");
                }
                bool exis = _administratorContract.Administrators.Where(c => c.MemberId == entry.MemberId && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;
                if (exis)
                {
                    resul = new OperationResult(OperationResultType.Error, "已存在同名的用户，操作失败");

                }
                else
                {
                    admin.MemberId = entry.MemberId;
                    admin.DepartmentId = entry.DepartmentId;
                    admin.JobPositionId = entry.JobPositionId;
                    admin.Notes = entry.InterviewEvaluation;
                    admin.IsLogin = true;
                    admin.MacAddress = entry.MacAddress;
                    admin.EntryTime = entry.EntryTime;
                    admin.IsDeleted = false;
                    admin.IsEnabled = true;
                    string role = entry.RoleJurisdiction;
                    if (!string.IsNullOrEmpty(role))
                    {
                        string[] rolear = role.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> roleids = new List<int>();
                        foreach (var item in rolear)
                        {
                            roleids.Add(Convert.ToInt32(item));
                        }
                        admin.Roles = _roleContract.Roles.Where(c => roleids.Contains(c.Id) && c.IsDeleted == false && c.IsEnabled == true).ToList();
                    }
                    admin.Member = _memberContract.Members.FirstOrDefault(c => c.Id == entry.MemberId);
                    JobPosition jobPos = _jobPositionContract.View(entry.JobPositionId.Value);
                    AnnualLeave ann = jobPos.AnnualLeave.Children.Where(x => x.StartYear == 0).FirstOrDefault();
                    Rest rest = new Rest()
                    {
                        AnnualLeaveDays = 0,
                        ChangeRestDays = 0,
                        PaidLeaveDays = 0,
                        OperatorId = AuthorityHelper.OperatorId
                    };
                    if (ann != null)
                    {
                        rest.AnnualLeaveDays = ann.Days;
                    }

                    admin.Rest = rest;
                    admin.whetherChange = false;
                    admin.whetherDateTime = DateTime.Now;
                    //admin.AdministratorTypeId = 1;
                    resul = _administratorContract.Insert(admin);
                    Administrator _admin = _administratorContract.Administrators.Where(x => x.MemberId == entry.MemberId).FirstOrDefault();
                    _admin.Rest.AdminId = _admin.Id;
                    resul = _administratorContract.Update(_admin);
                }
            }
            return resul;
        }

        public JsonResult GetMemberInfoByMobilePhone(string MobilePhone)
        {
            string errorMsg = string.Empty;
            int type = 0;
            var member = _memberContract.Members.Where(x => x.MobilePhone == MobilePhone).FirstOrDefault();
            if (member != null)
            {
                int adminCount = _administratorContract.Administrators.Where(x => x.MemberId == member.Id).Count();
                var entry = _entryContract.Entrys.Where(x => x.MemberId == member.Id).FirstOrDefault();
                if (adminCount == 0)
                {
                    if (entry != null)
                    {
                        if (entry.ToExamineResult == 3)
                        {
                            errorMsg = "此会员已经成为员工";
                        }
                        else
                        {
                            errorMsg = "此会员已经在审核中";
                        }
                    }
                    else
                    {
                        type = 1;
                        errorMsg = "";
                    }
                }
                else
                {
                    errorMsg = "此会员已经成为员工";
                }
            }
            else
            {
                errorMsg = "不存在此会员手机号";
            }

            var da = new
            {
                type = type,
                Id = member == null ? 0 : member.Id,
                RealName = member == null ? "" : member.RealName,
                Gender = member == null ? -1 : member.Gender,
                errorMsg = errorMsg
            };
            return Json(da, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Thumbnails(int Id, int type)
        {
            var result = new List<object>();
            var entity = _entryContract.Entrys.Where(x => x.Id == Id).FirstOrDefault();
            string imgPath = entity.LaborContractImgPath;
            switch (type)
            {
                case 1:
                    imgPath = entity.BankcardImgPath;
                    break;
                case 2:
                    imgPath = entity.IdCardImgPath;
                    break;
                case 3:
                    imgPath = entity.HealthCertificateImgPath;
                    break;
                case 4:
                    imgPath = entity.PhotoImgPath;
                    break;
                case 5:
                    imgPath = entity.LaborContractImgPath;
                    break;
            }
            if (entity != null && !string.IsNullOrEmpty(imgPath))
            {
                var counter = 1;
                var filePath = FileHelper.UrlToPath(imgPath);
                if (System.IO.File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    result.Add(new
                    {
                        ID = counter.ToString(),
                        FileName = imgPath,
                        FilePath = imgPath,
                        FileSize = fileInfo.Length
                    });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(int? EntryId)
        {
            var entry = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == EntryId).FirstOrDefault();
            return PartialView(entry);
        }

        [HttpPost]
        public ActionResult Update(EntryDto dto)
        {
            dto.ToExamineResult = 0;
            dto.operationId = AuthorityHelper.OperatorId.Value;
            dto.LaborContractImgPath = string.Empty;
            dto.RoleJurisdiction = string.Empty;
            dto.EntryTime = DateTime.Now;
            OperationResult oper = _entryContract.Update(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                EntryToExamine ete = new EntryToExamine();
                ete.AdminId = AuthorityHelper.OperatorId.Value;
                ete.Reason = "";
                ete.EntryId = dto.Id;
                ete.AuditStatus = 1;
                ete.AuditTime = DateTime.Now;
                ete.OperatorId = AuthorityHelper.OperatorId;
                oper = _entryToExamineContract.Insert(ete);
            }
            return Json(oper);
        }

        public ActionResult DownFile(string imgPath)
        {
            string filePath = Server.MapPath(imgPath);
            if (System.IO.File.Exists(filePath))
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                FileStream fs = new FileStream(filePath, FileMode.Open);
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                Response.ContentType = "application/octet-stream";

                Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(fileName));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
                return new EmptyResult();
            }
            else
            {
                return new EmptyResult();
            }

        }

        public ActionResult Sync(int entryId)
        {
            var entry = _entryContract.Entrys.FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == entryId);
            if (entry == null)
            {
                return Json(OperationResult.Error("没有入职信息"));
            }

            var adminEntity = _administratorContract.Administrators.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.MemberId.Value == entry.MemberId);
            if (adminEntity == null)
            {
                return Json(OperationResult.Error("没有管理员信息"));
            }

            // 同步数据
            entry.UpdatedTime = DateTime.Now;
            entry.EntryTime = adminEntity.CreatedTime;        // 入职时间
            entry.DepartmentId = adminEntity.DepartmentId;  // 部门
            entry.JobPositionId = adminEntity.JobPositionId;  // 职位
            entry.RoleJurisdiction = string.Join(",", adminEntity.Roles.Select(r => r.Id).ToList()); //角色ids
            entry.ToExamineResult = 3;


            var res = _entryContract.Update(entry);
            return Json(res);
        }

        public ActionResult BatchSync(params int[] ids)
        {
            List<Entry> updates = new List<Entry>();
            var entries = _entryContract.Entrys.Where(e => !e.IsDeleted && e.IsEnabled && ids.Contains(e.Id)).ToList();
            var err = 0;
            foreach (var entryId in ids)
            {
                var entry = entries.FirstOrDefault(e => e.Id == entryId);
                if (entry == null)
                {
                    err++;
                    continue;
                }

                var adminEntity = _administratorContract.Administrators.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.MemberId.Value == entry.MemberId);
                if (adminEntity == null)
                {
                    err++;
                    continue;
                }

                // 同步数据
                entry.UpdatedTime = DateTime.Now;
                entry.EntryTime = adminEntity.CreatedTime;        // 入职时间
                entry.DepartmentId = adminEntity.DepartmentId;  // 部门
                entry.JobPositionId = adminEntity.JobPositionId;  // 职位
                entry.RoleJurisdiction = string.Join(",", adminEntity.Roles.Select(r => r.Id).ToList()); //角色ids
                entry.ToExamineResult = 3;
                updates.Add(entry);
            }


            var res = _entryContract.Update(updates.ToArray());
            if (err > 0)
            {
                res.Message += ",失败个数:" + err.ToString();
            }
            return Json(res);
        }
    }
}