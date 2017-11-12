using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class AttendanceRepairController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceRepairController));

        protected readonly IAttendanceRepairContract _attendanceRepairContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly INotificationContract _notificationContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IConfigureContract _configureContract;

        public AttendanceRepairController(IAttendanceRepairContract attendanceRepairContract,
            IAttendanceContract attendanceContract,
            INotificationContract notificationContract,
            IAdministratorContract administratorContract,
            IMemberContract memberContract,
            IConfigureContract configureContract
            )
        {
            _attendanceRepairContract = attendanceRepairContract;
            _attendanceContract = attendanceContract;
            _notificationContract = notificationContract;
            _administratorContract = administratorContract;
            _memberContract = memberContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 初始化界面

        [Layout]
        public ActionResult Index()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.depList = list;
            return View();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List(int? SelDepartmentId)
        {
            var listDeps = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false).Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
            GridRequest request = new GridRequest(Request);
            string memberName = Request["memberName"];
            string repairTime = Request["repairTime"];
            string verifyFlag = Request["verifyFlag"];
            Expression<Func<AttendanceRepair, bool>> predicate = FilterHelper.GetExpression<AttendanceRepair>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var query = _attendanceRepairContract.AttendanceRepairs;
                if (!SelDepartmentId.HasValue)
                {
                    query = query.Where(w => listDeps.Contains(w.Administrator.DepartmentId ?? 0));
                }
                if (!string.IsNullOrEmpty(verifyFlag))
                {
                    int _verifyFlag = Convert.ToInt32(verifyFlag);
                    query = query.Where(x => x.ApiAttenFlag == _verifyFlag);
                }
                if (!string.IsNullOrEmpty(repairTime))
                {
                    DateTime _repairTime = DateTime.Parse(repairTime);
                    query = query.Where(w => w.Attendance.AttendanceTime.Year == _repairTime.Year && w.Attendance.AttendanceTime.Month == _repairTime.Month
                    && w.Attendance.AttendanceTime.Day == _repairTime.Day);
                }
                if (!string.IsNullOrEmpty(memberName))
                {
                    query = from a in query
                            join b in _administratorContract.Administrators
        on a.AdminId equals b.Id into a_bJion
                            from x in a_bJion.DefaultIfEmpty()
                            join c in _memberContract.Members
                            on x.MemberId equals c.Id into a_cJoin
                            from y in a_cJoin
                            where y.MemberName.Contains(memberName)
                            select a;

                }
                var list = query.Where<AttendanceRepair, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Administrator.Member.RealName,
                    x.VerifyType,
                    x.PaidScore,
                    x.CreatedTime,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.ApiAttenFlag,
                    x.Administrator.Department.DepartmentName,
                    VerifyName = x.VerifyAdmin.Member.RealName,
                    AttendanceTime = x.Attendance.AttendanceTime,
                    x.Reasons,
                    x.IsDoubleScore
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 审核数据
        /// <summary>
        /// 初始化审核界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Verify(int Id)
        {
            AttendanceRepairDto dto = _attendanceRepairContract.Edit(Id);
            return PartialView(dto);
        }

        /// <summary>
        /// 提交审核数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Verify(AttendanceRepairDto dto)
        {
            OperationResult oper = _attendanceRepairContract.Verify(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                var modAtten = _attendanceContract.View(dto.AttendanceId ?? 0);
                string title = "补卡通知";
                string content = string.Format("{0},{1},", modAtten.AttendanceTime.ToString("yyyy年MM月dd日"), showApiAttenFlag(dto.ApiAttenFlag));
                if (dto.VerifyType == (int)VerifyFlag.NoPass)
                {
                    content += "审核未通过";
                }
                else if (dto.VerifyType == (int)VerifyFlag.Pass)
                {
                    content += "审核通过";
                }
                else if (dto.VerifyType == (int)VerifyFlag.Waitting)
                {
                    content += "待确认";
                }
                _notificationContract.SendNotice(dto.AdminId ?? 0, title, content, sendNotificationAction);
            }
            return Json(oper);
        }
        #endregion
        /// <summary>
        /// 批量审核操作
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult VerifyAll(int type, params int[] Ids)
        {
            int countSuccess = 0;
            int countFail = 0;
            foreach (var id in Ids)
            {
                var curRep = _attendanceRepairContract.Edit(id);
                curRep.VerifyType = type;
                OperationResult result = Verify(curRep).Data as OperationResult;
                if (result.ResultType == OperationResultType.Success)
                    countSuccess++;
                else
                    countFail++;
            }
            var strResult = string.Format("操作完成：成功{0}个，失败{1}个", countSuccess, countFail);
            return Json(strResult);
        }

        #region 查看数据
        public ActionResult View(int Id)
        {
            AttendanceRepair entity = _attendanceRepairContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        /// <summary>
        /// 获取当前补卡标识类型
        /// </summary>
        /// <param name="apiflag"></param>
        /// <returns></returns>
        public string showApiAttenFlag(int apiflag)
        {
            string result = "未知";
            switch (apiflag)
            {
                case (int)ApiAttenFlag.Leave:
                    result = "请假";
                    break;
                case (int)ApiAttenFlag.Overtime:
                    result = "加班";
                    break;
                case (int)ApiAttenFlag.Field:
                    result = "外勤";
                    break;
                case (int)ApiAttenFlag.Repair:
                    result = "补卡";
                    break;
                case (int)ApiAttenFlag.Absence:
                    result = "未签到";
                    break;
                case (int)ApiAttenFlag.NoSignOut:
                    result = "未签退";
                    break;
                case (int)ApiAttenFlag.Late:
                    result = "迟到";
                    break;
                case (int)ApiAttenFlag.LeaveEarly:
                    result = "早退";
                    break;
                case (int)ApiAttenFlag.ArrivalEarly:
                    result = "早到";
                    break;
                case (int)ApiAttenFlag.LeaveLate:
                    result = "晚退";
                    break;
                default:
                    break;
            }
            return result;
        }

        public JsonResult RepairCount()
        {
            var listDeps = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false).Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
            var count = 0;
            var query = _attendanceRepairContract.AttendanceRepairs;
            query = query.Where(w => listDeps.Contains(w.Administrator.DepartmentId ?? 0) && w.VerifyType == 0);
            count = query.Count();
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, count));
        }

        public ActionResult SetPaidScoreIndex()
        {
            //string PaidScore = Utility.XmlStaticHelper.GetXmlNodeByXpath("AttendanceRepair", "AttendanceRepairConfig", "PaidScore");
            string PaidScore = _configureContract.GetConfigureValue("AttendanceRepair","AttendanceRepairConfig", "PaidScore");
            ViewBag.PaidScore = PaidScore;
            return PartialView();
        }

        public JsonResult SetPaidScore(string leavePoints)
        {
            //var status = Utility.XmlStaticHelper.UpdateNode("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", leavePoints);
            var status = _configureContract.SetConfigure("AttendanceRepair","AttendanceRepairConfig", "PaidScore", leavePoints);
            //return Json("1");
            return Json(OperationHelper.ReturnOperationResult(status, "默认扣除积分配置"));
        }
    }
}