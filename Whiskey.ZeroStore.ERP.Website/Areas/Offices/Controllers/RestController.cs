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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class RestController : Controller
    {

        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly ILeaveInfoContract _leaveInfoContract;

        public RestController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            ILeaveInfoContract leaveInfoContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _leaveInfoContract = leaveInfoContract;
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
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            string strRealName = string.Empty;
            var field = request.FilterGroup.Rules.Where(x => x.Field == "RealName").FirstOrDefault();
            if (field != null)
            {
                strRealName = field.Value.ToString();
                request.FilterGroup.Rules.Remove(field);
            }
            Expression<Func<Rest, bool>> predicate = FilterHelper.GetExpression<Rest>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Rest> listRest = _restContract.Rests;
                if (!string.IsNullOrEmpty(strRealName))
                {
                    listRest = listRest.Where(x => x.Admin.Member.RealName.Contains(strRealName));
                }
                var list = listRest.Where<Rest, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Admin.Member.RealName,
                    m.PaidLeaveDays,
                    m.AnnualLeaveDays,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据
        #region 出初始化添加界面

        /// <summary>
        /// 出初始化添加界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Create()
        {
            string day = ConfigurationHelper.GetAppSetting("RestDay");
            ViewBag.RestDay = day;
            return View();
        }

        [HttpPost]
        public JsonResult Create(string AdminIds, RestDto dto)
        {
            OperationResult oper;
            try
            {
                string strDay = ConfigurationHelper.GetAppSetting("RestDay");
                int day = int.Parse(strDay);
                //Rest rest = _restContract.Rests.Where(x => x.AdminId == dto.AdminId && x.IsEnabled == true && x.IsDeleted == false).FirstOrDefault();  
                if (AdminIds == null || string.IsNullOrEmpty(AdminIds))
                {
                    return Json(oper = new OperationResult(OperationResultType.Error, "请选择员工或部门"));
                }
                List<int> adminIds = AdminIds.Split(',').Select(a => int.Parse(a)).ToList();
                int error = 0;
                OperationResult oper_single = null;
                foreach (var id in adminIds)
                {
                    Administrator admin = _administratorContract.Administrators.FirstOrDefault(a => a.Id == id);
                    if (admin == null)
                    {
                        oper_single = new OperationResult(OperationResultType.Error, "该用户不存在");
                        error++;
                        continue;
                    }

                    Rest rest = _restContract.Rests.Where(x => x.AdminId == admin.Id && x.IsEnabled == true && x.IsDeleted == false).FirstOrDefault();
                    if (rest == null)
                    {
                        dto.AdminId = admin.Id;
                        dto.RealName = admin.Member.RealName;
                        if (day < dto.PaidLeaveDays)
                        {
                            oper_single = new OperationResult(OperationResultType.Error, "最多奖励" + strDay + "天");
                            error++;
                            continue;
                        }
                        oper_single = _restContract.Insert(dto);
                    }
                    else
                    {
                        double restDay = rest.PaidLeaveDays + dto.PaidLeaveDays;
                        if (restDay > day)
                        {
                            oper_single = new OperationResult(OperationResultType.Error, "超过最多奖励" + strDay + "天");
                            error++;
                            continue;
                        }
                        rest.PaidLeaveDays = restDay;
                        rest.UpdatedTime = DateTime.Now;
                        oper_single = _restContract.Update(rest);
                    }
                    if (oper_single.ResultType == OperationResultType.Error)
                    {
                        error++;
                    }
                }
                if (error == 0)
                {
                    oper = new OperationResult(OperationResultType.Success, "添加成功");
                }
                else if (error == adminIds.Count())
                {
                    oper = new OperationResult(OperationResultType.Error, "添加失败");
                }
                else
                {
                    oper = new OperationResult(OperationResultType.Success, "添加成功" + (adminIds.Count() - error) + "条，失败" + error + "条");
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
                return Json(oper);
            }

        }
        #endregion
        #endregion

        #region 获取员工
        /// <summary>
        /// 初始化员工界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.depList = list;
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            FilterRule rule = request.FilterGroup.Rules.Where(r => r.Field == "RealName").FirstOrDefault();
            if (rule != null)
            {
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            if (rule != null)
            {
                string realName = rule.Value.ToString();
                predicate = predicate.And(a => a.Member.RealName.Contains(realName));
            }
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _administratorContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Member.RealName,
                    AnnualLeaveDays = m.Rest != null ? m.Rest.AnnualLeaveDays : 0,
                    ChangeRestDays = m.Rest != null ? m.Rest.ChangeRestDays : 0,
                    PaidLeaveDays = m.Rest != null ? m.Rest.PaidLeaveDays : 0,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public  ActionResult GetAdminListByDepartmentId(int DepartmentId)
        {
                if (DepartmentId > 0)
                {
                    var list = _administratorContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled && a.DepartmentId == DepartmentId).Select(m => new
                    {
                        m.Id,
                        m.Member.RealName,
                    }).ToList();
                    return Json(list,JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = _administratorContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(m => new
                    {
                        m.Id,
                        m.Member.RealName,
                    }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 编辑数据
        [Layout]
        public ActionResult Update(int Id)
        {
            var rest = _restContract.Edit(Id);
            var admin = _administratorContract.View(rest.AdminId);
            rest.RealName = admin.Member.RealName;
            return View(rest);
        }
        [HttpPost]
        public JsonResult Update(RestDto dto)
        {
            OperationResult oper;
            try
            {
                string strDay = ConfigurationHelper.GetAppSetting("RestDay");
                int day = int.Parse(strDay);
                Rest rest = _restContract.View(dto.Id);
                if (dto.PaidLeaveDays > day)
                {
                    oper = new OperationResult(OperationResultType.Error, "最多奖励" + strDay + "天");
                }
                double restDay = rest.PaidLeaveDays + dto.PaidLeaveDays;
                if (restDay > day)
                {
                    oper = new OperationResult(OperationResultType.Error, "超过最多奖励" + strDay + "天");
                }
                else
                {
                    rest.PaidLeaveDays = restDay;
                    rest.UpdatedTime = DateTime.Now;
                    oper = _restContract.Update(rest);
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
                return Json(oper);
            }

        }
        #endregion

        #region 重置年假
        /// <summary>
        /// 重置年假
        /// </summary>
        /// <returns></returns>
        public JsonResult Reset(int DepartmentId)
        {
            var result = _restContract.Reset(DepartmentId);
            return Json(result);
        }
        #endregion
    }
}