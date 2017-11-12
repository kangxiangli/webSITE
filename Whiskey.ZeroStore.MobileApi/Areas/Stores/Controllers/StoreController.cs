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
   
    public class StoreController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OnlineController));
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberConsumeContract _memberConsumeContract;


        protected readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        public StoreController(
            IStoreContract storeContract,
            IMemberContract memberContract,
            IMemberConsumeContract memberConsumeContract
            )
        {
            _storeContract = storeContract;
            _memberContract = memberContract;
            _memberConsumeContract = memberConsumeContract;
        }


        public ActionResult QueryAttachStore()
        {
            var query = _storeContract.QueryAllStore().Where(s => s.IsAttached);
         
            var stores = query.Select(s => new
            {
                s.Id,
                s.StoreName,
                s.StoreTypeName,
                s.City,
                s.Telephone
            }).ToList();

            return Json(new OperationResult(OperationResultType.Success, string.Empty, stores), JsonRequestBehavior.AllowGet);
        }

        

    }
}