using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Areas.PunchClock.Controllers
{
    public class PunchSignController : BaseController
    {

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IAttendanceContract _attendanceContract;

        private readonly IStoreStatisticsContract _statContract;

        protected readonly IDepartmentContract _departmentContract;

        public PunchSignController(IAdministratorContract administratorContract,
            IAttendanceContract attendanceContract,
            IStoreStatisticsContract statContract,
            IDepartmentContract departmentContract)
        {
            _administratorContract = administratorContract;
            _attendanceContract = attendanceContract;
            _statContract = statContract;
            _departmentContract = departmentContract;
        }

        // GET: PunchClock/PunchSign
        public ActionResult Index()
        {
            return View();
        }

        #region 签到
        /// <summary>
        /// 签到
        /// </summary>
        /// <returns></returns>
        public JsonResult LoginIn(string userNamer)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "打卡异常");

            try
            {
                int adminId = _administratorContract.Administrators.Where(x => x.Member.MemberName == userNamer &&
                 x.DepartmentId == 7).Select(x => x.Id).FirstOrDefault();

                if (adminId == 4147 || adminId == 2119|| adminId== 2114|| adminId== 3122)
                {
                    Department department = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true && !string.IsNullOrEmpty(x.MacAddress)
                    && x.MacAddress == "D4EE074E6AF2").FirstOrDefault();
                    oper = _attendanceContract.LoginIn(adminId, department);
                    Administrator admin = _administratorContract.View(adminId);
                    admin.UpdatedTime = DateTime.Now;
                    admin.LoginTime = DateTime.Now;
                    _administratorContract.Update(admin);
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "没有权限"), JsonRequestBehavior.AllowGet);
            }


        }
        #endregion

        #region 签退
        /// <summary>
        /// 签退
        /// </summary>
        /// <returns></returns>        
        public JsonResult LoginOut(string userNamer)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "打卡异常");
            try
            {
                int adminId = _administratorContract.Administrators.Where(x => x.Member.MemberName == userNamer &&
     x.DepartmentId == 7).Select(x => x.Id).FirstOrDefault();
                if (adminId == 4147 || adminId == 2119)
                {
                    Department department = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true && !string.IsNullOrEmpty(x.MacAddress)
&& x.MacAddress == "D4EE074E6AF2").FirstOrDefault();
                    oper = _attendanceContract.LoginOut(adminId, department);
                    var res = _statContract.StatStoreWhenSignOut(adminId);

                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(oper, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

    }
}