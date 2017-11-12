using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Core.Data.Extensions;
using System.Linq.Expressions;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Article;
using Whiskey.Web.Helper;
using Whiskey.Utility.Helper;
using System.Text;
using System.Drawing.Imaging;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Coupon;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models.Enums;
using System.Data.Entity;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class AppointmentController : Controller
    {

        #region 初始化操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AppointmentController));



        protected readonly IMemberContract _memberContract;

        protected readonly IAppointmentContract _appointmentContract;
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;

        protected readonly IRetailContract _retailContract;
        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IScoreRuleContract _scoreRuleContract;
        protected readonly ICouponContract _couponContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly ICheckerContract _checkerContract;
        protected readonly IStoreActivityContract _storeActivityContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IMemberConsumeContract _memberConsumeContract;
        protected readonly IAppointmentFeedbackContract _appointmentFeedbackContract;

        public AppointmentController(
            IMemberContract memberContract,
            IAppointmentContract appointmentContract,
            IProductContract productContract,
            IBrandContract brandContract,
            ICouponContract couponContract,
            IAdministratorContract administratorContract
            , IAppointmentFeedbackContract appointmentFeedbackContract
           )
        {

            _memberContract = memberContract;
            _appointmentContract = appointmentContract;
            _productContract = productContract;
            _brandContract = brandContract;
            _memberContract = memberContract;
            _couponContract = couponContract;
            _administratorContract = administratorContract;
            _appointmentFeedbackContract = appointmentFeedbackContract;


        }
        #endregion

        private string _strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        [HttpPost]
        public ActionResult GetAll(int memberId, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var pagedResult = _appointmentContract.GetItems(memberId, pageIndex, pageSize);

                return Json(pagedResult);
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }

        }


        [HttpPost]
        [Obsolete("请使用新接口AddV2")]
        public ActionResult Add(int memberId, string data)
        {
            try
            {
                var shoppingCartItems = JsonHelper.FromJson<List<AppointmentProductEntry>>(data);
                var likeNumbers = shoppingCartItems.Select(i => i.ProductNumber).Distinct().ToArray();
                var dislikeNumbers = new string[0];
                var res = _appointmentContract.Add(memberId, string.Empty, likeNumbers, dislikeNumbers, new Dictionary<string, string>());
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }
        }



        [HttpPost]
        public ActionResult AddV2(int memberId, string likes, string dislikes, string notes, string options)
        {
            try
            {
                var likeNumberArr = likes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                var dislikeNumberArr = dislikes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                var checkOptions = JsonHelper.FromJson<Dictionary<string, string>>(options);
                notes = notes ?? string.Empty;
                var res = _appointmentContract.Add(memberId, notes, likeNumberArr, dislikeNumberArr, checkOptions);
                return Json(res);
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }
        }
        /// <summary>
        /// 快速预约
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        public JsonResult QuickAdd(int memberId, DateTime start, DateTime end)
        {
            var data = _appointmentContract.QuickAdd(memberId, start, end);
            return Json(data);
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

        public ActionResult GetPlans(int id)
        {
            try
            {
                var res = _appointmentContract.GetPlans(id);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ConfirmPlans(int id, int planId, DateTime start, DateTime end)
        {

            try
            {
                var res = _appointmentContract.ConfirmPlans(id, planId, start, end);
                return Json(res);
            }
            catch (Exception)
            {
                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult RejectAllPlans(int id)
        {

            try
            {
                var res = _appointmentContract.RejectAllPlans(id);
                return Json(res);
            }
            catch (Exception)
            {
                return Json(OperationResult.Error("系统错误"), JsonRequestBehavior.AllowGet);
            }
        }




    }
}