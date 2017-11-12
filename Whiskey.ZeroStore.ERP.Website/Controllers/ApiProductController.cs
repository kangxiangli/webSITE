using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class ApiProductController : Controller
    {
        #region 初始化操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ApiProductController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IProductContract _productContract;

        protected readonly IInventoryContract _inventoryContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IApprovalContract _approvalContract;

        protected readonly IStorageContract _storageContract;

        protected readonly ICategoryContract _categotyContract;
        public ApiProductController(IAdministratorContract administratorContract,
            IProductContract productContract,
            IInventoryContract inventoryContract,
            ICommentContract commentContract,
            IApprovalContract approvalContract,
            IStorageContract storageContract,
            ICategoryContract categotyContract)
        {
			_administratorContract = administratorContract;
            _productContract = productContract;
            _inventoryContract=inventoryContract;
            _commentContract = commentContract;
            _approvalContract = approvalContract;
            _storageContract = storageContract;
            _categotyContract = categotyContract;
		}
        #endregion

        string apiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示条数</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetList(int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                string strColorId = Request["ColorId"];
                string strProductAttrId = Request["ProductAttrId"];                
                string strCategoryId = Request["CategoryId"];
                IQueryable<Product> listProduct = _productContract.Products.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ProductOriginNumber.IsVerified == CheckStatusFlag.通过);                
                List<int> listIds = this.GetStore();

                IQueryable<Inventory> listInventory = _inventoryContract.Inventorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && listIds.Contains(x.StorageId) && x.Status == 0 && x.IsLock == false);//库存状态 0：待采购 1：已采购，未出库 2：已出库 3：欠损 4：退货                 
                if (!string.IsNullOrEmpty(strColorId))
                {
                    int colorId = int.Parse(strColorId);
                    listInventory = listInventory.Where(x => x.Product.ColorId == colorId);
                }
                if (!string.IsNullOrEmpty(strProductAttrId))
                {
                    int productAttrId = int.Parse(strProductAttrId);
                    listInventory = listInventory.Where(x => x.Product.ProductOriginNumber.ProductAttributes.Where(k => k.Id == productAttrId).Count() > 0);
                }
                if (!string.IsNullOrEmpty(strCategoryId))
                {
                    int categoryId = int.Parse(strCategoryId);
                    List<int> listCategoryId = _categotyContract.View(categoryId).Children.Select(x => x.Id).ToList();
                    listInventory = listInventory.Where(x => listCategoryId.Contains(x.Product.ProductOriginNumber.CategoryId));
                }
                listInventory = listInventory.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.CommentSource == (int)CommentSourceFlag.StoreProduct && x.IsDeleted == false && x.IsEnabled == true);
                //int approvalCount = _approvalContract.Approvals.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == memberId).Count();
                var data = listInventory.Select(x => new
                {
                    x.ProductId,
                    CoverImagePath = string.IsNullOrEmpty(x.Product.ProductCollocationImg) ? x.Product.ThumbnailPath : x.Product.ProductCollocationImg,
                    Price = x.Product.ProductOriginNumber.TagPrice,
                    ColorName = x.Product.Color.ColorName,
                    ColorPath = x.Product.Color.IconPath,
                    SizeName = x.Product.Size.SizeName,
                    SeasonName = x.Product.ProductOriginNumber.Season.SeasonName,
                    CategoryName = x.Product.ProductOriginNumber.Category.CategoryName,
                    x.Product.ProductOriginNumber.JumpLink,
                    CommentCount = listComment.Where(k => k.SourceId == x.ProductId).Count(),
                    //IsApproval = approvalCount > 0 ? (int)IsApproval.Yes : (int)IsApproval.No,
                    //ImageSize = System.IO.File.Exists(FileHelper.UrlToPath(x.Product.ProductCollocationImg ?? x.Product.ThumbnailPath)) ? "{" + Image.FromFile(FileHelper.UrlToPath(x.Product.ProductCollocationImg ?? x.Product.ThumbnailPath)).Width.ToString() + "," + Image.FromFile(FileHelper.UrlToPath(x.Product.ProductCollocationImg ?? x.Product.ThumbnailPath)).Height.ToString() + "}" : string.Empty,
                });
                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取线上仓库所属店铺Id
        private List<int> GetStore()
        {
            List<int> listIds = new List<int>();
            string strStorageIds = ConfigurationHelper.GetAppSetting("OnlineStorage");
            string[] arrIds = strStorageIds.Split(',');
            foreach (string strId in arrIds)
            {
                if (!string.IsNullOrEmpty(strId))
                {
                    listIds.Add(int.Parse(strId));
                }
            }
            List<int> listStorageId = _storageContract.Storages.Where(x => x.StorageType == (int)StorageFlag.OnLine && x.IsDeleted == false && x.IsEnabled == true && listIds.Contains(x.Id)).Select(x => x.Id).ToList();
            return listStorageId;
        }
        #endregion

    }
}