using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Appointments.Controllers
{
    // GET: Appointments/Appointment
    [License(CheckMode.Verify)]
    public class AppointmentController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AppointmentController));
        protected readonly IAppointmentContract _AppointmentContract;

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public AppointmentController(
            IAppointmentContract _AppointmentContract
            )
        {
            this._AppointmentContract = _AppointmentContract;
        }

        public JsonResult Add()
        {
            try
            {
                return Json(OperationResult.Error("敬请期待....."));
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }
        }

        public JsonResult GetList(int PageIndex = 1, int PageSize = 10, AppointmentState? Stat = null)
        {
            try
            {
                var pagedResult = _AppointmentContract.GetItems(AuthorityMemberHelper.OperatorId.Value, PageIndex, PageSize, Stat);

                return Json(pagedResult);
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }

        }

    }
}