using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

// GET: Stores/Online
namespace Whiskey.ZeroStore.MobileApi.Areas.Stores.Controllers
{

    public class RetailProductFeedbackController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OnlineController));
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberConsumeContract _memberConsumeContract;
        protected readonly IRetailContract _retailContract;
        protected readonly IRetailProductFeedbackContract _contract;


        protected readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        public RetailProductFeedbackController(
            IStoreContract storeContract,
            IMemberContract memberContract,
            IMemberConsumeContract memberConsumeContract,
            IRetailContract retailContract,
            IRetailProductFeedbackContract contract
            )
        {
            _storeContract = storeContract;
            _memberContract = memberContract;
            _memberConsumeContract = memberConsumeContract;
            _retailContract = retailContract;
            _contract = contract;
        }

        public ActionResult SubmitFeedbacks(int memberId, string retailNumber, string feedbacks)
        {
            var entries = JsonHelper.FromJson<List<RetailProductFeedbackEntry>>(feedbacks);
            var res = _contract.SubmitFeedbacks(memberId, retailNumber, entries);
            return Json(res);
        }

        public ActionResult GetProducts(int memberId, string retailNumber)
        {
            var data = _retailContract.Retails.Where(r => !r.IsDeleted && r.IsEnabled && r.ConsumerId.Value == memberId)


                .SelectMany(r => r.RetailItems.Select(p => new
                {
                    RetailNumber = p.Retail.RetailNumber,
                    ProductNumber = p.Product.ProductNumber,
                    ThumbnailPath = p.Product.ThumbnailPath ?? p.Product.ProductOriginNumber.ThumbnailPath,
                    Cnt = p.RetailCount,
                    Color = p.Product.Color.ColorName,
                    Size = p.Product.Size.SizeName,
                    Price = p.ProductRetailPrice,
                    p.Retail.CreatedTime,
                    p.HasFeedback,
                })).ToList()
                .Select(p => new
                {
                    p.RetailNumber,
                    p.ProductNumber,
                    p.ThumbnailPath,
                    p.Cnt,
                    p.Color,
                    p.Size,
                    p.Price,
                    p.HasFeedback,
                    CreatedTime = p.CreatedTime.ToUnixTime()
                }).ToList();
            


            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);

        }

        [OutputCache(Duration = 600)]
        public ActionResult GetOptions()
        {
            var res = _contract.GetOptions();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

    }
}