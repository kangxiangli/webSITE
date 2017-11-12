using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class ApiAppointmentFeedbackController : BaseController
    {
        string _strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        protected readonly IAppointmentContract _appointmentContract;
        private readonly IAppointmentFeedbackContract _appointmentFeedbackContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IColorContract _colorContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly ICollocationQuestionnaireContract _collocationQuestionnaireContract;
        private readonly IOrderblankContract _orderblankContract;
        protected readonly ICollocationPlanContract _collocationPlanContract;
        public ApiAppointmentFeedbackController(
             IAppointmentContract appointmentContract,
             IAppointmentFeedbackContract appointmentFeedbackContract,
            ICategoryContract categoryContract,
            IBrandContract brandContract,
            IColorContract colorContract,
            IStoreContract storeContract,
            IMemberContract memberContract,
            IMemberDepositContract memberDepositContract,
            ICollocationQuestionnaireContract collocationQuestionnaireContract,
            IOrderblankContract orderblankContract,
            ICollocationPlanContract collocationPlanContract
            )
        {

            _appointmentContract = appointmentContract;
            _appointmentFeedbackContract = appointmentFeedbackContract;
            _categoryContract = categoryContract;
            _brandContract = brandContract;
            _colorContract = colorContract;
            _storeContract = storeContract;
            _memberContract = memberContract;
            _memberDepositContract = memberDepositContract;
            _collocationQuestionnaireContract = collocationQuestionnaireContract;
            _orderblankContract = orderblankContract;
            _collocationPlanContract = collocationPlanContract;
        }




        [OutputCache(Duration = 600)]
        public ActionResult GetOptions()
        {
            try
            {
                var res = _appointmentFeedbackContract.GetOptions();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SubmitFeedbacks(string appointmentNumber, string feedbacks)
        {
            if (string.IsNullOrEmpty(appointmentNumber))
            {
                return Json(OperationResult.Error("预约号不能为空"));
            }
            var adminId = AuthorityHelper.OperatorId.Value;
            var entries = JsonHelper.FromJson<List<FeedbackEntry>>(feedbacks);
            var res = _appointmentFeedbackContract.SubmitFeedbacks(adminId, appointmentNumber, entries);
            return Json(res);
        }



    }
}