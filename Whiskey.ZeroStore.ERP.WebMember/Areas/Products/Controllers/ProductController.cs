using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Products.Controllers
{
    // GET: Products/Product
    public class ProductController : BaseController
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ProductController));

        protected readonly IProductContract _productContract;

        protected readonly IColorContract _colorContract;

        protected readonly ISeasonContract _seasonContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly ISizeContract _sizeContract;

        protected readonly IInventoryContract _inventoryContract;

        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IMemberCardContract _membercardContract;

        public ProductController(IProductContract productContract,
            IColorContract colorContract,
            ISeasonContract seasonContract,
            ICategoryContract categoryContract,
            ISizeContract sizeContract,
            IInventoryContract inventoryContract,
            IStorageContract storageContract,
            IStoreContract storeContract,
            IProductOrigNumberContract productOrigNumberContract,
            IMemberCardContract _membercardContract
            )
        {
            _productContract = productContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _categoryContract = categoryContract;
            _sizeContract = sizeContract;
            _inventoryContract = inventoryContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _productOrigNumberContract = productOrigNumberContract;
            this._membercardContract = _membercardContract;
        }
        #endregion


        [Layout]
        public ActionResult Detail(string BigProdNum)
        {
            var modPON = _productOrigNumberContract.OrigNumbs.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.BigProdNum == BigProdNum);
            return View(modPON);
        }

        public ActionResult Index()
        {
            return View();
        }

        #region 获取商品详情
        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProductDetail()
        {
            try
            {
                string strProductId = Request["ProductId"];
                int productId = int.Parse(strProductId);
                Product product = _productContract.Products.Where(x => x.Id == productId).FirstOrDefault();
                var entity = new
                {
                    ProductId = product.Id,
                    product.ProductName,
                    product.ProductNumber,
                    product.ProductOriginNumber.TagPrice,
                    CoverImagePath = WebUrl + product.ProductCollocationImg,
                    ProductImages = product.ProductImages.Select(x => new
                    {
                        ThumbnailSmallPath = WebUrl + x.OriginalPath
                    }),
                    ProductDetail = WebUrl + product.ProductOriginNumber.HtmlPath
                };
                return Json(new OperationResult(OperationResultType.Success, "", entity));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 获取所有店铺推荐商品

        public JsonResult GetStoreProducts(string BigProdNum, int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                string strColorId = Request["ColorId"];
                string strCategoryId = Request["CategoryId"];

                // 总店铺
                var mainStoreId = int.Parse(ConfigurationHelper.GetAppSetting("OnlineStorage"));
                // 校验商铺信息
                var storeEntity = _storeContract.Stores.FirstOrDefault(s => s.IsDeleted == false && s.IsEnabled == true && s.Id == mainStoreId);
                if (storeEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商铺不存在！"), JsonRequestBehavior.AllowGet);
                }

                //所有推荐的款号
                var recommendBigProdNums = _productOrigNumberContract.OrigNumbs.Where(s => !s.IsDeleted && s.IsEnabled)
                                                                               .Where(s => s.IsRecommend.Value == true)
                                                                               .Select(s => s.BigProdNum).ToList();

                // 校验商品分类信息
                if (!string.IsNullOrEmpty(strCategoryId))
                {
                    var CategoryId = int.Parse(strCategoryId);
                    if (_categoryContract.Categorys.Count(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == CategoryId && c.ParentId == null) <= 0)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "分类信息不存在！"), JsonRequestBehavior.AllowGet);
                    }
                }

                // 从库存信息中多条件筛选
                var data = GetNoCacheData(mainStoreId);

                var listInventory = data.Where(i => recommendBigProdNums.Contains(i.BigProdNum));

                // 大分类筛选
                if (!string.IsNullOrEmpty(BigProdNum))
                {
                    listInventory = listInventory.Where(i => i.BigProdNum == BigProdNum);
                }
                // 颜色筛选
                if (!string.IsNullOrEmpty(strColorId))
                {
                    int colorId = int.Parse(strColorId);
                    listInventory = listInventory.Where(x => x.ColorId == colorId);
                }

                // 分类筛选
                if (!string.IsNullOrEmpty(strCategoryId))
                {
                    int categoryId = int.Parse(strCategoryId);
                    Category category = _categoryContract.View(categoryId);
                    List<int> realCategoryId = new List<int>();
                    if (category != null)
                    {
                        List<Category> listCategory = category.Children.ToList();
                        if (listCategory != null && listCategory.Count > 0)
                        {
                            realCategoryId = listCategory.Select(x => x.Id).ToList();
                        }
                    }

                    listInventory = listInventory.Where(x => realCategoryId.Contains(x.CategoryId));
                }

                // 分页 按款号的更新时间倒序
                listInventory = listInventory.OrderByDescending(x => x.UpdatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);

                var res = listInventory.Select(g => new
                {
                    ProductId = g.ProductId,
                    BigProdNum = g.BigProdNum,
                    CategoryName = g.CategoryName,
                    SeasonName = g.SeasonName,
                    SizeName = g.SizeName,
                    ColorName = g.ColorName,
                    Price = g.Price,
                    ImagePath = WebUrl + g.ImagePath,
                    ColorIconPath = WebUrl + g.ColorIconPath,
                    ImageOrgPath = WebUrl + g.ImageOrgPath,
                    HtmlPath = WebUrl + g.HtmlPath,
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, res), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }
        }

        private IQueryable<GetListByStoreRes> GetNoCacheData(int mainStoreId)
        {
            // 筛选店铺
            var storeIds = new List<int>();
            var storageIds = new List<int>();
            var storageQuery = _storageContract.Storages.Where(s => !s.IsDeleted && s.IsEnabled);

            //所有店铺
            storeIds.AddRange(_storeContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled).Select(s => s.Id).ToList());

            //所有仓库
            storageIds.AddRange(storageQuery.Select(s => s.Id).ToList());

            // 查询店铺下的所有库存
            var inventoryQuery = _inventoryContract.Inventorys.Where(x => !x.IsDeleted && x.IsEnabled)
                                                              .Where(x => x.Status == (int)InventoryStatus.Default && !x.IsLock)
                                                              .Where(x => storeIds.Contains(x.StoreId))
                                                              .Where(x => storageIds.Contains(x.StorageId));
            // 按款号,颜色分组
            var groupQuery = inventoryQuery.GroupBy(i => new { i.Product.BigProdNum, i.Product.Color.ColorName });

            var query = from g in groupQuery
                        let inventory = g.FirstOrDefault()
                        let product = g.FirstOrDefault().Product
                        let pon = g.FirstOrDefault().Product.ProductOriginNumber
                        select new GetListByStoreRes
                        {
                            StoreId = inventory.StoreId,
                            UpdatedTime = pon.UpdatedTime,
                            ProductId = inventory.ProductId,
                            CategoryId = pon.CategoryId,
                            CategoryName = pon.Category.CategoryName,
                            SeasonName = pon.Season.SeasonName,
                            BigProdNum = product.BigProdNum,
                            Price = pon.TagPrice,
                            ColorId = product.ColorId,
                            SizeName = product.Size.SizeName,
                            ColorName = product.Color.ColorName,
                            ImagePath = product.ThumbnailPath ?? pon.ThumbnailPath,
                            ImageOrgPath = product.OriginalPath ?? pon.OriginalPath,
                            ColorIconPath = product.Color.IconPath,
                            HtmlPath = pon.HtmlPath,
                        };
            return query;
        }

        #endregion

        #region 获取品类列表

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Category, bool>> predicate = FilterHelper.GetExpression<Category>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _categoryContract.Categorys.Where<Category, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Description,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}