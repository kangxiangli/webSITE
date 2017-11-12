using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    [CheckCookieAttrbute]
    public class ApiTimeoutRequestController : BaseController
    {
        string _strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        protected readonly IAppointmentContract _appointmentContract;
        private readonly IModuleContract _moduleContract;
        private readonly IPermissionContract _permissionContract;
        private readonly IAdministratorContract _adminContract;
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
        private readonly ITimeoutSettingContract _timeoutSettingContract;
        private readonly ITimeoutRequestContract _timeoutRequestContract;
        private string[] _filterFlags = new string[]
        {
            "#pass","#nopass"
        };
        private const string _superVerifyFlags = ".superverify";
        public ApiTimeoutRequestController(
             IAppointmentContract appointmentContract,
             IModuleContract moduleContract,
             IPermissionContract permissionContract,
             IAdministratorContract adminContract,
             IAppointmentFeedbackContract appointmentFeedbackContract,
             ICategoryContract categoryContract,
             IBrandContract brandContract,
             IColorContract colorContract,
             IStoreContract storeContract,
             IMemberContract memberContract,
             IMemberDepositContract memberDepositContract,
             ICollocationQuestionnaireContract collocationQuestionnaireContract,
             IOrderblankContract orderblankContract,
             ICollocationPlanContract collocationPlanContract,
             ITimeoutSettingContract timeoutSettingContract,
             ITimeoutRequestContract timeoutRequestContract
            )
        {

            _appointmentContract = appointmentContract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;
            _adminContract = adminContract;
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
            _timeoutSettingContract = timeoutSettingContract;
            _timeoutRequestContract = timeoutRequestContract;
        }



        [OutputCache(Duration = 600)]
        public ActionResult GetTimeoutType()
        {
            var types = _timeoutSettingContract.Settings.Where(s => !s.IsDeleted && s.IsEnabled).Select(s => new
            {
                s.Name,
                s.Id
            }).ToList();
            return Json(OperationResult.OK(types), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Apply([Bind(Include = "TimeoutSettingId,Number,Notes")]TimeoutRequest dto)
        {
            try
            {
                var res = _timeoutRequestContract.Create(dto, sendNotificationAction);
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }
        }



        public async Task<ActionResult> GetHistoryApply(string number, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);

                var departmentIds = _storeContract.QueryAllStore().Where(s => storeIds.Contains(s.Id)).Select(s => s.DepartmentId.Value).ToList();

                var query = _timeoutRequestContract.Entities.Where(r => !r.IsDeleted && r.IsEnabled && departmentIds.Contains(r.DepartmentId));
                if (!string.IsNullOrWhiteSpace(number))
                {
                    query = query.Where(r => r.Number.StartsWith(number));
                }
                var data = await query.OrderByDescending(r => r.UpdatedTime)
                    .Select(r => new { r.Number, r.CreatedTime, State = r.State.ToString(), TimeoutType = r.TimeoutType.Name, DepartmentName = r.Department.DepartmentName })
                    .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                    .ToListAsync();
                var res = data.Select(r => new
                {
                    CreatedTime = r.CreatedTime.ToUnixTime(),
                    r.DepartmentName,
                    r.Number,
                    r.State,
                    r.TimeoutType
                });
                return Json(OperationResult.OK(res), JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }
        }


    }
}