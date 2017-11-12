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
    public class ApiAppointmentController : BaseController
    {
        string _strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        protected readonly IAppointmentContract _appointmentContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IColorContract _colorContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly ICollocationQuestionnaireContract _collocationQuestionnaireContract;
        private readonly IOrderblankContract _orderblankContract;
        protected readonly ICollocationPlanContract _collocationPlanContract;
        public ApiAppointmentController(
             IAppointmentContract appointmentContract,
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

        public ActionResult GetBoxInfo(string appointmentNumber)
        {
            var entity = _appointmentContract.Entities.FirstOrDefault(a => a.Number == appointmentNumber);
            if (entity == null)
            {
                return Json(OperationResult.Error("未找到预约信息"));
            }
            var memberData = new
            {
                entity.MemberId,
                entity.Member.RealName,
                entity.Member.MobilePhone,
                entity.Store.StoreName
            };
            var figure = entity.Member.MemberFigures.Select(f => new
            {
                f.Birthday,
                f.Height,
                f.Weight,
                f.Shoulder,
                f.PreferenceColor,
                f.Waistline,
                f.Hips,
                f.Gender,
                f.FigureType,
                f.FigureDes,
                f.Bust,
                f.ApparelSize
            }).FirstOrDefault();
            var figureData = new
            {
                Birthday = figure.Birthday.HasValue ? figure.Birthday.Value.ToUnixTime() : 0,
                Height = figure.Height,
                Weight = figure.Weight,
                Shoulder = figure.Shoulder,
                PreferenceColor = figure.PreferenceColor,
                Waistline = figure.Waistline,
                Hips = figure.Hips,
                Gender = figure.Gender,
                FigureType = figure.FigureType,
                FigureDesc = figure.FigureDes,
                Bust = figure.Bust,
                ApparelSize = figure.ApparelSize
            };


            var options = _appointmentContract.GetOptions().Data as AppointmentOption;
            var appointmentData = new
            {
                entity.Quantity,
                entity.Season,
                entity.Situation,
                entity.Top,
                entity.Bottom,
                entity.Jumpsuit,
                entity.Notes,
                entity.Style,
                entity.Color,
                entity.Fabric,
                entity.Budget
            };
            return Json(new OperationResult(OperationResultType.Success, string.Empty, new { memberData, figureData, appointmentData }), JsonRequestBehavior.AllowGet);
        }


        [OutputCache(Duration = 600)]
        public ActionResult GetOptions()
        {
            try
            {
                var res = _appointmentContract.GetOptions();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPlans(string number)
        {
            try
            {
                var res = _appointmentContract.GetPlans(number);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }
        [OutputCache(Duration = 600)]
        public async Task<ActionResult> GetLikes(string number)
        {
            try
            {
                var res = await _appointmentContract.GetLikes(number);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> GetBoxToAccept(AppointmentState filter = AppointmentState.已装箱)
        {
            try
            {
                var storeId = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value)?.FirstOrDefault();
                if (!storeId.HasValue)
                {
                    return Json(OperationResult.Error("店铺权限不足"), JsonRequestBehavior.AllowGet);
                }
                var res = await _appointmentContract.GetBoxToAccept(storeId.Value, filter);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ConfirmAccept(string appointmentNumber)
        {
            var number = _appointmentContract.Entities.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Number == appointmentNumber)?.AppointmentPacking?.Orderblank?.OrderBlankNumber;
            if (string.IsNullOrEmpty(number))
            {
                return Json(OperationResult.Error("未找到配货单号"));

            }
            var res = _orderblankContract.ReceptProduct(number);
            return Json(res);
        }
    }
}