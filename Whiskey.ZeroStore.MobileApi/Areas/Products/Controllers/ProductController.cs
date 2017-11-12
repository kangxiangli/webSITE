using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Products.Controllers
{
    //[License(CheckMode.Verify)]
    public class ProductController : Controller
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ProductController));

        protected readonly IProductContract _productContract;

        protected readonly IColorContract _colorContract;

        protected readonly ISeasonContract _seasonContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly ISizeContract _sizeContract;

        protected readonly IApprovalContract _productApprovalContract;

        protected readonly ICommentContract _productCommentContract;

        protected readonly IInventoryContract _inventoryContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IApprovalContract _approvalContract;

        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStoreRecommendContract _storeRecommendContract;
        protected readonly IStoreNoRecommendContract _storeNoRecommendContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;

        public ProductController(IProductContract productContract,
            IColorContract colorContract,
            ISeasonContract seasonContract,
            ICategoryContract categoryContract,
            ISizeContract sizeContract,
            IApprovalContract productApprovalContract,
            ICommentContract productCommentContract,
            IInventoryContract inventoryContract,
            ICommentContract commentContract,
            IApprovalContract approvalContract,
            IStorageContract storageContract,
            IStoreContract storeContract,
            IStoreRecommendContract storeRecommendContract,
            IProductOrigNumberContract productOrigNumberContract,
            IStoreNoRecommendContract storeNoRecommendContract
            )
        {
            _productContract = productContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _categoryContract = categoryContract;
            _sizeContract = sizeContract;
            _productApprovalContract = productApprovalContract;
            _productCommentContract = productCommentContract;
            _inventoryContract = inventoryContract;
            _commentContract = commentContract;
            _approvalContract = approvalContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _storeRecommendContract = storeRecommendContract;
            _productOrigNumberContract = productOrigNumberContract;
            _storeNoRecommendContract = storeNoRecommendContract;
        }
        #endregion

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
                string strMemberId = Request["MemberId"];
                string strCategoryId = Request["CategoryId"];
                IQueryable<Product> listProduct = _productContract.Products.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ProductOriginNumber.IsVerified == CheckStatusFlag.通过);

                int memberId = int.Parse(strMemberId);
                #region 注释代码

                //listProduct=listProduct.OrderByDescending(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);                
                //IQueryable<Whiskey.ZeroStore.ERP.Models.Color> listColor=_colorContract.Colors.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                //IQueryable<Season> listSeason=_seasonContract.Seasons.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                //IQueryable<Category> listCategory=_categoryContract.Categorys.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                //IQueryable<Whiskey.ZeroStore.ERP.Models.Size> listSize=_sizeContract.Sizes.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                //获取线上销售的仓库
                //string strStorageIds = ConfigurationHelper.GetAppSetting("OnlineStorage");
                //string[] arrIds = strStorageIds.Split(',');
                //List<int> listIds = new List<int>();
                //foreach (string strId in arrIds)
                //{
                //    if (!string.IsNullOrEmpty(strId))
                //    {
                //        listIds.Add(int.Parse(strId));
                //    }
                //}
                #endregion
                List<int> listIds = this.GetStore();

                IQueryable<Inventory> listInventory = _inventoryContract.Inventorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && listIds.Contains(x.StoreId) && x.Status == 0 && x.IsLock == false);//库存状态 0：待采购 1：已采购，未出库 2：已出库 3：欠损 4：退货                 
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
                    Category category = _categoryContract.View(categoryId);
                    List<int> listId = new List<int>();
                    if (category != null)
                    {
                        List<Category> listCategory = category.Children.ToList();
                        if (listCategory != null && listCategory.Count > 0)
                        {
                            listId = listCategory.Select(x => x.Id).ToList();
                        }
                    }

                    listInventory = listInventory.Where(x => listId.Contains(x.Product.ProductOriginNumber.CategoryId));
                }
                listInventory = listInventory.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.CommentSource == (int)CommentSourceFlag.StoreProduct && x.IsDeleted == false && x.IsEnabled == true);
                int approvalCount = _approvalContract.Approvals.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == memberId).Count();
                #region 注释代码

                //var data = (from pr in listProduct
                //           join
                //           nv in listInventory
                //           on
                //           pr.Id equals nv.ProductId
                //           where nv.Status == 1//库存状态 0：待采购 1：已采购，未出库 2：已出库 3：欠损 4：退货 
                //           select new { 
                //               ProductId=pr.Id,
                //               CoverImagePath = pr.ProductCollocationImg??pr.ThumbnailPath,
                //               Price=pr.TagPrice,
                //               pr.Color.ColorName,
                //               ColorPath=pr.Color.IconPath,
                //               SizeName=pr.Size.SizeName,
                //               SeasonName=pr.Season.SeasonName,
                //               CategoryName=pr.Category.CategoryName
                //           }).ToList();

                //if (data.Count()>0)
                //{
                //    var entiy = data.Select(x => new
                //    {
                //        x.ProductId,
                //        x.CoverImagePath,
                //        x.Price,
                //        x.ColorName,
                //        x.ColorPath,
                //        x.SizeName,
                //        x.SeasonName,
                //        x.CategoryName,
                //        CommentCount = listComment.Where(k => k.SourceId == x.ProductId).Count(),
                //        IsApproval = approvalCount > 0 ? (int)IsApproval.Yes : (int)IsApproval.No,
                //        ImageSize =  System.IO.File.Exists(FileHelper.UrlToPath(x.CoverImagePath))?"{" + Image.FromFile(FileHelper.UrlToPath(x.CoverImagePath)).Width.ToString() + "," + Image.FromFile(FileHelper.UrlToPath(x.CoverImagePath)).Height.ToString() + "}":string.Empty ,
                //    });

                //    return Json(new OperationResult(OperationResultType.Success, "获取成功", entiy),JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new OperationResult(OperationResultType.Success, "获取成功", null), JsonRequestBehavior.AllowGet);
                //}
                #endregion
                var data = listInventory.Select(x => new
                {
                    x.ProductId,
                    CoverImagePath = strWebUrl + x.Product.ProductCollocationImg ?? strWebUrl + x.Product.OriginalPath,
                    Price = x.Product.ProductOriginNumber.TagPrice,
                    ColorName = x.Product.Color.ColorName,
                    ColorPath = x.Product.Color.IconPath,
                    SizeName = x.Product.Size.SizeName,
                    SeasonName = x.Product.ProductOriginNumber.Season.SeasonName,
                    CategoryName = x.Product.ProductOriginNumber.Category.CategoryName,
                    CommentCount = listComment.Where(k => k.SourceId == x.ProductId).Count(),
                    IsApproval = approvalCount > 0 ? (int)IsApproval.Yes : (int)IsApproval.No,
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
                    product.ProductOriginNumber.BigProdNum,

                    CoverImagePath = strWebUrl + product.ProductCollocationImg,
                    ProductImages = product.ProductImages.Select(x => new
                    {
                        ThumbnailSmallPath = strWebUrl + x.OriginalPath
                    }),
                    ProductDetail = product.ProductOriginNumber.HtmlPhonePath
                };
                return Json(new OperationResult(OperationResultType.Success, "获取成功", entity));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 获取同品牌同分类下的商品列表
        /// <summary>
        /// 获取同品牌同分类下的商品列表
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public JsonResult GetProductColorSize(string bigProdNum)
        {
            try
            {

                OperationResult oper = _productContract.GetProductColorSize(bigProdNum);
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }

        #endregion

        #region 获取商品内容
        /// <summary>
        /// 获取商品内容
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetContent(int ProductId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            Product product = _productContract.View(ProductId);
            if (product == null)
            {
                oper.ResultType = OperationResultType.Error;
                oper.Message = "商品不存在";
                return Json(oper);
            }
            else
            {
                oper.Data = strWebUrl + product.ProductOriginNumber.HtmlPath;
                return Json(oper);
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
            List<int> listStoreId = _storageContract.Storages.Where(x => x.StorageType == (int)StorageFlag.OnLine && x.IsDeleted == false && x.IsEnabled == true && listIds.Contains(x.Id)).Select(x => x.StoreId).ToList();
            return listStoreId;
        }
        #endregion

        #region 根据店铺获取分类下的商品列表
        public JsonResult GetListByStore(string BigProdNum, int PageIndex = 1, int PageSize = 10, int flag = 0)
        {
            try
            {

                string strColorId = Request["ColorId"];
                string strProductAttrId = Request["ProductAttrId"];
                string strCategoryId = Request["CategoryId"];
                string strStoreId = Request["StoreId"];

                // 参数校验
                if (string.IsNullOrEmpty(strStoreId))
                {
                    return Json(new OperationResult(OperationResultType.ValidError, "参数错误！"), JsonRequestBehavior.AllowGet);
                }


                var currentStoreId = int.Parse(strStoreId);

                // 校验商铺信息
                var storeEntity = _storeContract.Stores.FirstOrDefault(s => s.IsDeleted == false && s.IsEnabled == true && s.Id == currentStoreId);
                if (storeEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商铺不存在！"), JsonRequestBehavior.AllowGet);
                }

                //所有推荐的款号
                var query = _productOrigNumberContract.OrigNumbs.Where(s => !s.IsDeleted && s.IsEnabled)
                                                                .Where(s => s.IsRecommend.Value == true);
                var recommendBigProdNums = new List<string>();


                // 是否是总店铺
                var mainStoreId = int.Parse(ConfigurationHelper.GetAppSetting("OnlineStorage"));
                var isMainStore = currentStoreId == mainStoreId;
                if (isMainStore)
                {
                    recommendBigProdNums = query.Select(r => r.BigProdNum).ToList();
                }
                else
                {
                    recommendBigProdNums = query.Where(r => r.RecommendStoreIds.Contains(currentStoreId.ToString())).Select(r => r.BigProdNum).ToList();
                }

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
                var data = GetNoCacheData(mainStoreId, currentStoreId);


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

                // 风格筛选
                if (!string.IsNullOrEmpty(strProductAttrId))
                {
                    int productAttrId = int.Parse(strProductAttrId);
                    listInventory = listInventory.Where(x => x.ProductAttributes.Contains(productAttrId));
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
                var allCount = listInventory.Count();
                if (allCount <= 0)
                {
                    return Json(new PagedOperationResult(OperationResultType.Success, string.Empty, new List<object>()) { AllCount = allCount, PageSize = PageSize }, JsonRequestBehavior.AllowGet);
                }
                var pageCount = (int)Math.Ceiling(allCount * 1.0 / PageSize);
                // 分页 按款号的修改时间倒序
                listInventory = listInventory.OrderByDescending(x => x.BigProductNumberUpdateTime)
                                             .Skip((PageIndex - 1) * PageSize)
                                             .Take(PageSize);

                var res = listInventory.Select(g => new
                {
                    ProductId = g.ProductId,
                    ProductNumber = g.ProductNumber,

                    BigProdNum = g.BigProdNum,
                    CategoryName = g.CategoryName,
                    SeasonName = g.SeasonName,
                    SizeName = g.SizeName,
                    ColorName = g.ColorName,
                    Price = g.Price,
                    ImagePath = flag == 0 ? strWebUrl + g.ImagePath : strWebUrl + g.ColloImgPath,
                    ColorIconPath = g.ColorIconPath,
                    IsApproval = g.IsApproval,
                    CommentCount = g.CommentCount,
                    ColloImgPath = strWebUrl + g.ColloImgPath,
                    HtmlPhonePath = strWebUrl + g.HtmlPhonePath,
                }).ToList();

                return Json(new PagedOperationResult(OperationResultType.Success, string.Empty, res) { AllCount = allCount, PageSize = PageSize }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult Search(string number, int PageIndex = 1, int PageSize = 10, int flag = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(number))
                {
                    return Json(OperationResult.OK());
                }
                //所有推荐的款号
                var query = _productOrigNumberContract.OrigNumbs.Where(s => !s.IsDeleted && s.IsEnabled)
                                                                .Where(s => s.IsRecommend.Value == true);
                var recommendBigProdNums = new List<string>();


                // 是否是总店铺
                var mainStoreId = int.Parse(ConfigurationHelper.GetAppSetting("OnlineStorage"));

                recommendBigProdNums = query.Select(r => r.BigProdNum).ToList();
                // 从库存信息中多条件筛选
                var data = GetNoCacheData(mainStoreId, mainStoreId, false);


                var listInventory = data.Where(i => recommendBigProdNums.Contains(i.BigProdNum));

                // 大分类筛选
                if (!string.IsNullOrEmpty(number))
                {
                    listInventory = listInventory.Where(i => i.BigProdNum.StartsWith(number)
                                                          || i.ProductNumber.StartsWith(number));
                }

                var allCount = listInventory.Count();
                var pageCount = (int)Math.Ceiling(allCount * 1.0 / PageSize);
                // 分页 按款号的修改时间倒序
                listInventory = listInventory.OrderByDescending(x => x.BigProductNumberUpdateTime)
                                             .Skip((PageIndex - 1) * PageSize)
                                             .Take(PageSize);

                var res = listInventory.Select(g => new
                {
                    StoreId = g.StoreId,
                    ProductId = g.ProductId,
                    ProductNumber = g.ProductNumber,
                    BigProdNum = g.BigProdNum,
                    CategoryName = g.CategoryName,
                    SeasonName = g.SeasonName,
                    SizeName = g.SizeName,
                    ColorName = g.ColorName,
                    Price = g.Price,
                    ImagePath = flag == 0 ? strWebUrl + g.ImagePath : strWebUrl + g.ColloImgPath,
                    ColorIconPath = g.ColorIconPath,
                    IsApproval = g.IsApproval,
                    CommentCount = g.CommentCount,
                    ColloImgPath = strWebUrl + g.ColloImgPath,
                    HtmlPhonePath = strWebUrl + g.HtmlPhonePath,
                }).ToList();

                return Json(new PagedOperationResult(OperationResultType.Success, string.Empty, res) { AllCount = allCount, PageSize = PageSize }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }
        }

        private IQueryable<SeachDataRes> GetNoCacheData(int mainStoreId, int currentStoreId, bool groupData = true)
        {
            // 筛选店铺
            var isMainStore = currentStoreId == mainStoreId;
            var storeIds = new List<int>();
            var storageIds = new List<int>();
            var storageQuery = _storageContract.Storages.Where(s => !s.IsDeleted && s.IsEnabled);

            if (isMainStore)
            {
                //所有店铺
                storeIds.AddRange(_storeContract.QueryAllStore().Select(s => s.Id).ToList());

                //所有仓库
                storageIds.AddRange(_storeContract.QueryAllStorage().Select(s => s.Id).ToList());
            }
            else
            {

                //筛选店铺
                storeIds.Add(currentStoreId);

                //筛选仓库
                storageIds.AddRange(_storeContract.QueryAllStorage().Where(s => s.StoreId == currentStoreId).Select(s => s.Id).ToList());
            }



            // 查询店铺下的所有库存
            var inventoryQuery = _inventoryContract.Inventorys.Where(x => !x.IsDeleted && x.IsEnabled)
                                                              .Where(x => x.Status == (int)InventoryStatus.Default && !x.IsLock)
                                                              .Where(x => storeIds.Contains(x.StoreId))
                                                              .Where(x => storageIds.Contains(x.StorageId));
            IQueryable<SeachDataRes> query;
            if (!groupData)
            {
                query = from g in inventoryQuery
                        let inventory = g
                        let product = g.Product
                        let productOriginNumber = g.Product.ProductOriginNumber
                        select new SeachDataRes
                        {
                            StoreId = inventory.StoreId,
                            ProductId = inventory.ProductId,
                            ProductNumber = inventory.ProductNumber,
                            CategoryId = productOriginNumber.CategoryId,
                            ProductAttributes = productOriginNumber.ProductAttributes.Select(attr => attr.Id).ToList(),
                            CategoryName = productOriginNumber.Category.CategoryName,
                            SeasonName = productOriginNumber.Season.SeasonName,
                            BigProdNum = product.BigProdNum,
                            Price = productOriginNumber.TagPrice,
                            ColorId = product.ColorId,
                            SizeName = product.Size.SizeName,
                            ColorName = product.Color.ColorName,
                            ImagePath = product.ThumbnailPath ?? productOriginNumber.ThumbnailPath,
                            ColloImgPath = product.ProductCollocationImg ?? productOriginNumber.ProductCollocationImg ?? string.Empty,
                            ColorIconPath = strWebUrl + product.Color.IconPath,
                            IsApproval = 0,
                            CommentCount = 0,
                            BigProductNumberUpdateTime = productOriginNumber.UpdatedTime,
                            HtmlPhonePath = productOriginNumber.HtmlPhonePath,
                        };
            }
            else // 按款号,颜色分组
            {

                var groupQuery = inventoryQuery.GroupBy(i => new { i.Product.BigProdNum, i.Product.Color.ColorName });

                query = from g in groupQuery
                        let inventory = g.FirstOrDefault()
                        let product = g.FirstOrDefault().Product
                        let productOriginNumber = g.FirstOrDefault().Product.ProductOriginNumber
                        select new SeachDataRes
                        {
                            StoreId = inventory.StoreId,
                            ProductId = inventory.ProductId,
                            ProductNumber = inventory.ProductNumber,
                            CategoryId = productOriginNumber.CategoryId,
                            ProductAttributes = productOriginNumber.ProductAttributes.Select(attr => attr.Id).ToList(),
                            CategoryName = productOriginNumber.Category.CategoryName,
                            SeasonName = productOriginNumber.Season.SeasonName,
                            BigProdNum = product.BigProdNum,
                            Price = productOriginNumber.TagPrice,
                            ColorId = product.ColorId,
                            SizeName = product.Size.SizeName,
                            ColorName = product.Color.ColorName,
                            ImagePath = product.ThumbnailPath ?? productOriginNumber.ThumbnailPath,
                            ColloImgPath = product.ProductCollocationImg ?? productOriginNumber.ProductCollocationImg ?? string.Empty,
                            ColorIconPath = strWebUrl + product.Color.IconPath,
                            IsApproval = 0,
                            CommentCount = 0,
                            BigProductNumberUpdateTime = productOriginNumber.UpdatedTime,
                            HtmlPhonePath = productOriginNumber.HtmlPhonePath,
                        };
            }

            return query;
        }


        [Serializable]
        private class SeachDataRes
        {
            public SeachDataRes()
            {
                ProductAttributes = new List<int>();
            }
            public DateTime CreatedTime { get; set; }
            public int StoreId { get; set; }
            public int CategoryId { get; set; }
            public int ProductId { get; set; }
            public string ProductNumber { get; set; }
            public int ColorId { get; set; }
            public IList<int> ProductAttributes { get; set; }
            public string BigProdNum { get; set; }
            public string CategoryName { get; set; }
            public string SeasonName { get; set; }
            public string SizeName { get; set; }
            public string ColorName { get; set; }
            public float Price { get; set; }
            public string ImagePath { get; set; }
            public string ColloImgPath { get; set; }
            public string ColorIconPath { get; set; }
            public int IsApproval { get; set; }
            public int CommentCount { get; set; }
            public DateTime BigProductNumberUpdateTime { get; internal set; }

            public string HtmlPhonePath { get; set; }
        }
        #endregion
    }
}