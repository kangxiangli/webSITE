using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

//using Microsoft.Ajax.Utilities;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Extensions.License;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Entities;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using XKMath36;
namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class StoreRecommendController : BaseController
    {
        protected readonly IProductContract _productContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;
        protected readonly IMemberContract _memberContract;
        protected readonly ICollocationContract _collocationContract;
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
        protected readonly IProductOrigNumberContract _productOrignNumberContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly ISizeContract _sizeContract;
        protected readonly IStoreRecommendContract _storeRecommendContract;
        protected readonly IStoreNoRecommendContract _storeNoRecommendContract;
        protected readonly IRecommendMemberSingleProductContract _recommendMemberSingleProductContract;
        private readonly static object objlock = new object();

        public StoreRecommendController(IProductContract productContract,
            IBrandContract brandContract,
            ICategoryContract categoryContract,
            IColorContract colorContract,
            ISeasonContract seasonContract,
            ISizeContract sizeContract,
            IInventoryContract inventoryContract,
            IStorageContract storageContract,
            ISalesCampaignContract salesCampaignContract,
            IMemberContract memberContract,
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
            IProductTrackContract productTrackContract,
            IPermissionContract permissionContract,
            IProductOrigNumberContract productOrigNumberContract,
            IStoreRecommendContract storeRecommendContract,
            IStoreNoRecommendContract storeNoRecommendContract,
            IRecommendMemberSingleProductContract recommendMemberSingleProductContract
           )
        {
            _storeRecommendContract = storeRecommendContract;
            _storeNoRecommendContract = storeNoRecommendContract;
            _productContract = productContract;
            _inventoryContract = inventoryContract;
            _brandContract = brandContract;
            _storageContract = storageContract;
            _salesCampaignContract = salesCampaignContract;
            _memberContract = memberContract;
            _collocationContract = collocationContract;
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
            _permissionContract = permissionContract;
            _productOrignNumberContract = productOrigNumberContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _recommendMemberSingleProductContract = recommendMemberSingleProductContract;
        }


        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        [Layout]
        public ActionResult Create()
        {

            return View();
        }


        [HttpGet]
        public ActionResult GetOptions()
        {
            var brands = CacheAccess.GetBrand(_brandContract, true, false);
            var brandOptions = brands.Select(s => new { s.Text, s.Value }).ToList();
            var categories = CacheAccess.GetCategorys(_categoryContract).Select(s => new SelectListItem() { Text = s.CategoryName, Value = s.Id.ToString() });
            var categoryOptions = categories.Select(s => new { Text = s.Text, Value = s.Value }).ToList();
            categoryOptions.Insert(0, new { Text = "请选择品类", Value = "" });
            var seasons = CacheAccess.GetSeason(_seasonContract, true);
            var seasonOptions = seasons.Select(s => new { s.Text, s.Value }).ToList();
            //var storeOptions = _storeContract .Stores.Where(s => !s.IsDeleted && s.IsEnabled && s.IsAttached)
            //                                        .Select(s => new { Text = s.StoreName, Value = s.Id })
            //                                        .ToList();
            // storeOptions.Insert(0, new { Text = "请选择店铺", Value = 0 });
            return Json(new OperationResult(OperationResultType.Success, string.Empty, new
            {
                brandOptions,
                categoryOptions,
                seasonOptions,
                //storeOptions
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetNumbersAsync(GetNumberReq req, int currentPage = 1, int pageSize = 10)
        {
            var querySource = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled);

            //获取所有已设置的款号
            var recommendList = querySource.Where(o => o.IsRecommend.Value == true).Select(o => o.BigProdNum).Distinct().ToList();
            if (!string.IsNullOrEmpty(req.BigProductNumber))
            {
                querySource = querySource.Where(o => o.BigProdNum == req.BigProductNumber);
            }
            if (req.BrandId.HasValue && req.BrandId > 0)
            {
                querySource = querySource.Where(o => o.BrandId == req.BrandId.Value);
            }
            if (req.CategoryId.HasValue && req.CategoryId > 0)
            {
                var cateIds = _categoryContract.Categorys.Where(c => !c.IsDeleted && c.IsEnabled && c.ParentId == req.CategoryId.Value).Select(c => c.Id).ToList();

                if (!cateIds.Any())
                {

                    querySource = querySource.Where(o => o.CategoryId == req.CategoryId.Value);
                }
                else
                {
                    querySource = querySource.Where(o => cateIds.Contains(o.CategoryId));
                }

            }
            if (req.SeasonId.HasValue && req.SeasonId > 0)
            {
                querySource = querySource.Where(o => o.SeasonId == req.SeasonId.Value);
            }

            var pageInfo = new PageInfo()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = querySource.Count()
            };
            var data = await querySource.OrderByDescending(o => o.UpdatedTime)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    BigProductNumber = o.BigProdNum,
                    BrandName = o.Brand.BrandName,
                    CategoryName = o.Category.CategoryName,
                    Season = o.Season.SeasonName,
                    IsRecommend = recommendList.Contains(o.BigProdNum),
                    ThumbnailPath = WebUrl + o.ThumbnailPath,
                    InTable = false
                }).ToListAsync();
            var resData = new
            {
                pageInfo = pageInfo,
                pageData = data
            };
            return Json(new OperationResult(OperationResultType.Success, string.Empty, resData), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetRecommendNumbersAsync(GetNumberReq req, int currentPage = 1, int pageSize = 10)
        {
            var querySource = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && o.IsRecommend.Value == true);
            var dict = _storeRecommendContract.GetOnlineStoreBigProdNumState();
            // 获取所有已设置的款号
            if (!string.IsNullOrEmpty(req.BigProductNumber))
            {
                querySource = querySource.Where(o => o.BigProdNum == req.BigProductNumber);
            }
            if (req.BrandId.HasValue && req.BrandId > 0)
            {
                querySource = querySource.Where(o => o.BrandId == req.BrandId.Value);
            }
            if (req.CategoryId.HasValue && req.CategoryId > 0)
            {
                // 大小分类转换
                var cateIds = _categoryContract.Categorys.Where(c => !c.IsDeleted && c.IsEnabled && c.ParentId == req.CategoryId.Value).Select(c => c.Id).ToList();

                if (!cateIds.Any())
                {

                    querySource = querySource.Where(o => o.CategoryId == req.CategoryId.Value);
                }
                else
                {
                    querySource = querySource.Where(o => cateIds.Contains(o.CategoryId));
                }
            }
            if (req.SeasonId.HasValue && req.SeasonId > 0)
            {
                querySource = querySource.Where(o => o.SeasonId == req.SeasonId.Value);
            }
            if (req.StoreId.HasValue && req.StoreId > 0)
            {
                querySource = querySource.Where(o => o.RecommendStoreIds.Contains(req.StoreId.ToString()));
            }

            if (req.State.HasValue)
            {
                var bigNums = dict.Where(d => d.Value == req.State.Value).Select(d => d.Key).ToList();
                querySource = querySource.Where(o => bigNums.Contains(o.BigProdNum));

            }
            var pageInfo = new PageInfo()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = querySource.Count()
            };

            var data = await querySource.OrderByDescending(o => o.UpdatedTime)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    BigProductNumber = o.BigProdNum,
                    BrandName = o.Brand.BrandName,
                    CategoryName = o.Category.CategoryName,
                    Season = o.Season.SeasonName,
                    ThumbnailPath = WebUrl + o.ThumbnailPath,
                    TagPrice = o.TagPrice,
                    ProductName = o.ProductName,
                    RecommendMemberCount = o.RecommendMemberSingleProducts.Count()

                }).ToListAsync();
            var res = data.Select(o => new
            {
                BigProductNumber = o.BigProductNumber,
                BrandName = o.BrandName,
                CategoryName = o.CategoryName,
                Season = o.Season,
                ThumbnailPath = o.ThumbnailPath,
                TagPrice = o.TagPrice,
                ProductName = o.ProductName,
                State = dict.ContainsKey(o.BigProductNumber) ? dict[o.BigProductNumber].ToString() : BigProdNumStateEnum.普通.ToString(),
                o.RecommendMemberCount
            }).ToList();

            var resData = new
            {
                pageInfo = pageInfo,
                pageData = res
            };
            return Json(new OperationResult(OperationResultType.Success, string.Empty, resData), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult Save(string numbers)
        {
            return Json(_storeRecommendContract.SaveRecommend(numbers));
        }

        [HttpPost]
        public async Task<ActionResult> ValidateNumber(string numbers)
        {
            if (string.IsNullOrEmpty(numbers))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数错误"));
            }
            var numberArr = numbers.Split(",").ToList();
            var querySource = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && numberArr.Contains(o.BigProdNum));
            if (querySource.Count() != numberArr.Count)
            {
                return Json(new OperationResult(OperationResultType.Error, "数量不一致"));
            }
            if (querySource.Any(o => o.IsRecommend.Value == true))
            {
                return Json(new OperationResult(OperationResultType.Error, "提交数据中有已经设为推荐的数据"));
            }

            var data = await querySource.OrderByDescending(o => o.UpdatedTime)
                                          .Select(o => new
                                          {
                                              BigProductNumber = o.BigProdNum,
                                              BrandName = o.Brand.BrandName,
                                              CategoryName = o.Category.CategoryName,
                                              Season = o.Season.SeasonName,
                                              IsRecommend = o.IsRecommend == true,
                                              ThumbnailPath = WebUrl + o.ThumbnailPath,
                                              InTable = false
                                          }).ToListAsync();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
        }

        /// <summary>
        /// 查询款号的推荐店铺
        /// </summary>
        /// <param name="number">productoriginnumber.bigporodnum</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetRecommendStores(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数错误"));
            }
            var querySource = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && o.BigProdNum == number);
            if (!querySource.Any())
            {
                return Json(new OperationResult(OperationResultType.Error, "没有款号信息"));
            }
            if (querySource.Any(o => o.IsRecommend.Value == false))
            {
                return Json(new OperationResult(OperationResultType.Error, "款号没有设置为推荐"));
            }

            // 获取店铺权限
            var recommendStoreIds = querySource.First().RecommendStoreIds;
            if (recommendStoreIds == null)
            {
                recommendStoreIds = string.Empty;
            }
            var enableStoreIds = recommendStoreIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i)).ToList();

            // 获取权限范围内所有店铺
            var list = _storeContract.QueryManageStore(AuthorityHelper.OperatorId.Value).Select(s => new StoreEntry
            {
                Id = s.Id,
                StoreName = s.StoreName,
                StoreTypeName = s.StoreTypeName,
            }).ToList();

            // 计算所有店铺是否推荐的状态
            list.Each(entry =>
            {
                if (enableStoreIds.Any(id => id == entry.Id))
                {
                    entry.HasRecommend = true;
                }
            });
            return Json(new OperationResult(OperationResultType.Success, string.Empty, list), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveEdit(string number, string recommendStoreIds)
        {
            return Json(_storeRecommendContract.UpdateRecommendStoreId(number, recommendStoreIds));
        }

        [HttpPost]
        public ActionResult DeleteRecommend(string number)
        {
            return Json(_storeRecommendContract.DeleteRecommend(number));
        }


        [HttpGet]
        public ActionResult EditConfig()
        {
            var config = _storeRecommendContract.GetConfig();

            return PartialView(config);
        }

        [HttpPost]
        public ActionResult EditConfig([Bind(Include = "NewProductTime,ClassicProductTime")]BigProdNumStateConfigEntry entry)
        {

            var res = _storeRecommendContract.UpdateConfig(entry);
            return Json(res);

        }


        /// <summary>
        /// 选择会员
        /// </summary>
        /// <param name="bigProdNumber">要推荐的款号</param>
        /// <param name="isLimit">是否只展示推荐到的会员</param>
        /// <returns></returns>
        public ActionResult MemberSelect(string bigProdNumber, int isLimit = 0)
        {
            var recommendMembers = _recommendMemberSingleProductContract.Entities
                                            .Where(m => !m.IsDeleted && m.IsEnabled && m.BigProdNumber == bigProdNumber)
                                            .Select(m => new { m.MemberId, m.ColorId, m.Color.ColorName })
                                            .ToList();

            var colors = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled && p.BigProdNum == bigProdNumber)
                .GroupBy(p => p.ColorId)
                .Select(g => new { ColorId = g.Key, ColorName = g.FirstOrDefault().Color.ColorName })
                .ToList();
            ViewBag.BigProdNumber = bigProdNumber;
            ViewBag.Colors = JsonHelper.ToJson(colors);
            ViewBag.RecommendMemberIds = JsonHelper.ToJson(recommendMembers);
            ViewBag.IsLimit = isLimit;
            return PartialView();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberList(string name, string mobilePhone, string memberIds, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _memberContract.Members;
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {
                query = query.Where(e => e.MemberName.StartsWith(name) || e.RealName.StartsWith(name));
            }
            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {
                query = query.Where(e => e.MobilePhone.StartsWith(mobilePhone));
            }
            if (!string.IsNullOrEmpty(memberIds) && memberIds.Length > 0)
            {
                var arr = memberIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i)).ToList();
                query = query.Where(e => arr.Contains(e.Id));
            }

            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.MemberName,
                                e.RealName,
                                e.MobilePhone,
                                e.Store.StoreName,
                                IsChecked = false
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SaveMemberId(string bigProdNumber, SaveMemberRecommendEntry[] recommendMembers)
        {
            var res = _recommendMemberSingleProductContract.SaveMemberId(bigProdNumber, recommendMembers);
            return Json(res);
        }



        private class StoreEntry
        {
            public StoreEntry()
            {
                HasRecommend = false;
                Disabled = false;
            }
            public int Id { get; set; }
            public string StoreName { get; set; }
            public bool HasRecommend { get; set; }
            public bool Disabled { get; set; }

            public string StoreTypeName { get; set; }
        }

        #region 初始化批量导出界面
        public ActionResult BatchImport()
        {
            return PartialView();
        }
        #endregion




        public class GetNumberReq
        {
            public string BigProductNumber { get; set; }
            public int? BrandId { get; set; }
            public int? CategoryId { get; set; }
            public int? SeasonId { get; set; }
            public int? StoreId { get; set; }
            public BigProdNumStateEnum? State { get; set; }
        }


    }
}