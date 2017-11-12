using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices.Controllers
{
    public class AttenController : Controller
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttenController));

        protected readonly IAttendanceRepairContract _attendanceRepairContract;

        public AttenController(
            IAttendanceRepairContract attendanceRepairContract
            )
        {
            this._attendanceRepairContract = attendanceRepairContract;
        }
        #endregion

        #region 系统自动生成确认扣除积分补卡记录(扣除双倍积分)
        [HttpPost]
        public JsonResult ApplyRepairBySystem(int adminId)
        {
            var oper = _attendanceRepairContract.ApplyRepairBySystem(adminId, "api");
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        #region 系统确认补卡
        [HttpPost]
        public JsonResult ConfirmRepairBySystem(int AdminId)
        {
            OperationResult oper = _attendanceRepairContract.ConfirmRepairBySystem(AdminId, "api");
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}