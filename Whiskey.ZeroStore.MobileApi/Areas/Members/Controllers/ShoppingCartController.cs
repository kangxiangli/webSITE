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

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class ShoppingCartController : Controller
    {

        #region 初始化操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MyWalletController));



        protected readonly IMemberContract _memberContract;

        protected readonly IShoppingCartItemContract _shoppingCartItemContract;
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

        public ShoppingCartController(
            IMemberContract memberContract,
            IShoppingCartItemContract shoppingCartItemContract,
            IProductContract productContract,
            IBrandContract brandContract,
            IInventoryContract inventoryContract,
            IStorageContract storageContract,
            ISalesCampaignContract salesCampaignContract,
            ICollocationContract collocationContract,
            IRetailContract retailContract,
            IRetailItemContract retailItemContract,
            IScoreRuleContract scoreRuleContract,
            ICouponContract couponContract,
            IAdministratorContract administratorContract,
            IMemberDepositContract memberDepositContract,
            ICheckerContract checkerContract,
            IStoreActivityContract storeActivityContract,
            IStoreContract storeContract,
            IProductTrackContract productTrackContract
           )
        {

            _memberContract = memberContract;
            _shoppingCartItemContract = shoppingCartItemContract;
            _productContract = productContract;
            _inventoryContract = inventoryContract;
            _brandContract = brandContract;
            _storageContract = storageContract;
            _salesCampaignContract = salesCampaignContract;
            _memberContract = memberContract;
            _retailContract = retailContract;
            _retailItemContract = retailItemContract;
            _scoreRuleContract = scoreRuleContract;
            _couponContract = couponContract;
            _administratorContract = administratorContract;
            _memberDepositContract = memberDepositContract;
            _checkerContract = checkerContract;
            _storeActivityContract = storeActivityContract;
            _storeContract = storeContract;
            _productTrackContract = productTrackContract;
        }
        #endregion

        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        [HttpPost]
        public ActionResult GetAll(int memberId)
        {
            try
            {
                var list = _shoppingCartItemContract.GetItems(memberId);
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }

        }


        [HttpPost]
        public ActionResult Clear(int memberId)
        {
            try
            {

                var res = _shoppingCartItemContract.ClearItem(memberId);
                return Json(res);

            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }
        }

        [HttpPost]
        public ActionResult Quantity(int memberId, string productNumber, uint quantity)
        {
            try
            {
                var res = _shoppingCartItemContract.UpdateQuantity(memberId, productNumber, (int)quantity);
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }
        }

        [HttpPost]
        public ActionResult Add(int memberId, string data)
        {
            try
            {
                var shoppingCartItems = JsonHelper.FromJson<List<ShoppingCartUpdateDto>>(data);
                shoppingCartItems.RemoveAll(i => i.Quantity <= 0);
                var res = _shoppingCartItemContract.AddItem(memberId, shoppingCartItems.ToArray());
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }
        }


        [HttpPost]
        public ActionResult Del(int memberId, params string[] productNumber)
        {
            try
            {
                if (productNumber == null || productNumber.Length <= 0)
                {
                    return Json(OperationResult.Error("参数错误"));
                }
                var res = _shoppingCartItemContract.DelItem(memberId, productNumber);
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }
        }



       


    }
}