using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGrease.Css.Extensions;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{
    public class SalesCampaignController : BaseController
    {
        //
        // GET: /Stores/SalesCampaign/
        //促销活动 
        //yxk 2016-1-7

        protected readonly ISalesCampaignContract _saleCampaignContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IProductContract _productContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IProductOrigNumberContract _productOriginNumberContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly ICategoryContract _categoryContract;
        public SalesCampaignController(ISalesCampaignContract saleCampaignContract,
            IStoreContract storeContract,
            IInventoryContract inventoryContract,
            IBrandContract brandContract,
            IProductContract productContract,
            IStorageContract storageContract,
            IAdministratorContract _administratorContract,
            ISeasonContract _seasonContract,
            IProductOrigNumberContract productOriginNumberContract,
            ICategoryContract categoryContract)
        {
            _saleCampaignContract = saleCampaignContract;
            _storeContract = storeContract;
            _inventoryContract = inventoryContract;
            _brandContract = brandContract;
            _productContract = productContract;
            _storageContract = storageContract;
            _productOriginNumberContract = productOriginNumberContract;
            this._administratorContract = _administratorContract;
            this._seasonContract = _seasonContract;
            _categoryContract = categoryContract;

        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetSaleCampaignStoreName(int campaignNumber)
        {
            var entity = _saleCampaignContract.SalesCampaigns.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.Id == campaignNumber);
            if (entity == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "活动不存在"));
            }
            var storeIds = entity.StoresIds;
            var storeArr = storeIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToList();
            var storeNames = _storeContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled && storeArr.Contains(s.Id)).Select(s => s.StoreName).ToList();

            return Json(new OperationResult(OperationResultType.Success, string.Join(",", storeNames)));

        }

        public async Task<ActionResult> List()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);
            Expression<Func<SalesCampaign, bool>> predicate = FilterHelper.GetExpression<SalesCampaign>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                var lisall = _saleCampaignContract.SalesCampaigns
                                                    .Where(predicate)
                                                    .OrderByDescending(c => c.CreatedTime)
                                                    .ThenByDescending(c => c.Id);
                var li = lisall.Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(c => new
                {
                    Id = c.Id,
                    CampaignNumber = c.CampaignNumber,
                    CampaignName = c.CampaignName,
                    Descript = c.Descript,
                    CampaignStartTime = c.CampaignStartTime,
                    CampaignEndTime = c.CampaignEndTime,
                    CreatedTime = c.CreatedTime,
                    IsPass = DateTime.Now.CompareTo(c.CampaignEndTime) > 0,
                    StoresIds = c.StoresIds,
                    IsDeleted = c.IsDeleted,
                    IsEnabled = c.IsEnabled,
                    SalesCampaignType = c.SalesCampaignType,
                    MemberDiscount = c.MemberDiscount,
                    NoMmebDiscount = c.NoMmebDiscount,
                    BigProdNumCount = c.ProductOriginNumbers.Count
                }).ToList();
                // new GridData
                return new GridData<object>(li, lisall.Count(), request.RequestInfo);
            });

            return Json(data);
        }
        [HttpGet]
        public ActionResult Create()
        {
            var model = new SalesCampaign()
            {
                MemberDiscount = 7.0f,
                NoMmebDiscount = 7.0f,
                CampaignStartTime = DateTime.Now,
                CampaignEndTime = DateTime.Now,
            };

            ViewBag.OriginNumberList = model.ProductOriginNumbers.ToList();
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult Create(SalesCampaign saleCap)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var prods = Request["saleCap[prod]"];
            if (prods != "all")
            {
                string[] bigProdNumbs = prods.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //var saleprods = _productContract.Products.Where(c => bigProdNumbs.Contains(c.ProductNumber)).ToList();
                var saleprods = _productOriginNumberContract.OrigNumbs.Where(c => bigProdNumbs.Contains(c.BigProdNum)).ToList();
                saleCap.ProductOriginNumbers = saleprods;

            }


            var storeids = Request["saleCap[StoreIds]"];
            if (!string.IsNullOrEmpty(storeids))
            {
                //判断店铺中是否已存在同名且未过期的活动
                int[] storeidarr = storeids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToArray();
                var storeEntities = _storeContract.Stores.Where(s => storeidarr.Contains(s.Id)).ToList();
                DateTime curtime = DateTime.Now;
                var camp = _saleCampaignContract.SalesCampaigns.Where(
                     c =>
                         //storeidarr.Contains((int)c.CampaignStoreId) && c.CampaignName == saleCap.CampaignName && c.CampaignEndTime >= curtime &&
                         c.CampaignName == saleCap.CampaignName && c.CampaignEndTime >= curtime &&
                         !c.IsDeleted).Select(c => c.Id);

                if (camp.Any())
                {
                    resul.Message = "已存在同名且未过期的活动";
                }
                else
                {
                    saleCap.StoresIds = string.Join(",", storeidarr);
                    saleCap.OperatorId = AuthorityHelper.OperatorId;
                    resul = _saleCampaignContract.Insert(false, saleCap);
                }

            }

            return Json(resul);
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var entity = _saleCampaignContract.SalesCampaigns.FirstOrDefault(s => !s.IsDeleted && s.IsEnabled && s.Id == id);
            ViewBag.OriginNumberList = entity.ProductOriginNumbers.ToList();
            return PartialView("Create", entity);
        }
        [HttpPost]
        public JsonResult Update(SalesCampaign saleCap)
        {


            OperationResult resul = new OperationResult(OperationResultType.Error);
            if (saleCap.CampaignNumber == 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "操作错误-number为0"));
            }
            var saleCampaignEntity = _saleCampaignContract.SalesCampaigns.Where(s => !s.IsDeleted && s.IsEnabled && s.CampaignNumber == saleCap.CampaignNumber)
                                                                            .Include(s => s.ProductOriginNumbers)
                                                                            .FirstOrDefault();
            if (saleCampaignEntity == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "活动不存在"));
            }


            var prods = Request["saleCap[prod]"];
            string[] bigProdNumbs = prods.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var saleprods = _productOriginNumberContract.OrigNumbs.Where(c => bigProdNumbs.Contains(c.BigProdNum)).ToList();
            var beforeNums = saleCampaignEntity.ProductOriginNumbers.Select(p => p.BigProdNum).ToList();

            var needDel = beforeNums.Except(bigProdNumbs).ToList();
            var needAdd = bigProdNumbs.Except(beforeNums).ToList();
            foreach (var bigProdNum in needDel)
            {
                var entity = saleCampaignEntity.ProductOriginNumbers.First(p => p.BigProdNum == bigProdNum);
                saleCampaignEntity.ProductOriginNumbers.Remove(entity);
            }
            var needAddEntities = _productOriginNumberContract.OrigNumbs.Where(p => !p.IsDeleted && p.IsEnabled && needAdd.Contains(p.BigProdNum)).ToList();
            foreach (var bigProdNumEntity in needAddEntities)
            {
                saleCampaignEntity.ProductOriginNumbers.Add(bigProdNumEntity);
            }


            var storeids = Request["saleCap[StoreIds]"];
            if (string.IsNullOrEmpty(storeids))
            {
                return Json(new OperationResult(OperationResultType.Error, "活动店铺不可为空"));
            }

            // 参与店铺
            int[] storeidarr = storeids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToArray();
            var storeEntities = _storeContract.Stores.Where(s => storeidarr.Contains(s.Id)).ToList();
            if (storeEntities.Count != storeidarr.Length)
            {
                return Json(new OperationResult(OperationResultType.Error, "活动店铺id有误"));
            }
            saleCampaignEntity.StoresIds = storeids;

            // 名称
            saleCampaignEntity.CampaignName = saleCap.CampaignName;

            // 日期
            saleCampaignEntity.CampaignStartTime = saleCap.CampaignStartTime;
            saleCampaignEntity.CampaignEndTime = saleCap.CampaignEndTime;

            // 类型
            saleCampaignEntity.SalesCampaignType = saleCap.SalesCampaignType;

            // 折扣
            saleCampaignEntity.MemberDiscount = saleCap.MemberDiscount;
            saleCampaignEntity.NoMmebDiscount = saleCap.NoMmebDiscount;

            // 描述
            saleCampaignEntity.Descript = saleCap.Descript;

            // 是否可叠加优惠券
            saleCampaignEntity.OtherCashCoupon = saleCap.OtherCashCoupon;

            // 是否可叠加店铺活动
            saleCampaignEntity.OtherCampaign = saleCap.OtherCampaign;

            var res = _saleCampaignContract.Update(saleCampaignEntity);
            return Json(res);
        }

        /// <summary>
        /// 获取指定店铺的商品
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetProductsByStore(string storeId)
        {
            ViewBag.storeid = storeId;
            ViewBag.Season = CacheAccess.GetSeason(_seasonContract, true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            ViewBag.Categories = CacheAccess.GetCategory(_categoryContract, true);

            return PartialView();
        }
        /// <summary>
        /// 从基础属性中选择要搞活动的商品款号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductsByStore(int? brandId, string productName, string bigProdNum, int? SeasonId, int? categoryId)
        {

            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);

            var query = _productOriginNumberContract.OrigNumbs;
            if (brandId.HasValue)
            {
                query = query.Where(i => i.BrandId == brandId.Value);
            }
            if (SeasonId.HasValue)
            {
                query = query.Where(i => i.SeasonId == SeasonId.Value);
            }
            if (!productName.IsNullOrEmpty())
            {
                query = query.Where(i => i.ProductName == productName);
            }
            if (!bigProdNum.IsNullOrEmpty())
            {
                query = query.Where(i => i.BigProdNum == bigProdNum);
            }
            if (categoryId.HasValue)
            {

                query = query.Where(i => i.CategoryId == categoryId);

            }
            var proli = query.OrderByDescending(c => c.CreatedTime)
                                            .Skip(request.PageCondition.PageIndex)
                                            .Take(request.PageCondition.PageSize)
                                            .Select(c => new
                                            {
                                                c.Id,
                                                c.Category.CategoryName,
                                                Name = c.ProductName,
                                                ProNum = c.BigProdNum,
                                                Brand = c.Brand.BrandName,
                                                Seaso = c.Season.SeasonName,
                                                TagPrice = c.TagPrice,
                                                Thumbnail = c.ThumbnailPath,
                                            }).ToList();
            return Json(new GridData<object>(proli, query.Count(), request.RequestInfo));
        }
        /// <summary>
        /// 根据商品id获取库存数
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private object GetQuantityByProductId(int pid)
        {
            return _inventoryContract.Inventorys
                .Count(c => c.ProductId == pid && c.IsLock);


        }
        //public ActionResult GetProductsByStore(int storeId)
        //{ 
        //    _inventoryContract.Inventorys.Where(c=>c)
        //}
        public ActionResult GetProductsByNums(string[] bigProdNums)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "操作异常");
            if (bigProdNums != null && bigProdNums.Count() > 0)
            {
                var t = _productOriginNumberContract.OrigNumbs.Where(c => bigProdNums.Contains(c.BigProdNum))
                    .Select(c => new
                    {
                        Id = c.Id,
                        BigProdNum = c.BigProdNum,
                        ProductName = c.ProductName ?? string.Empty,
                        ThumbnailPath = c.ThumbnailPath,
                        TagPrice = c.TagPrice,
                        BrandName = c.Brand.BrandName,
                        CategoryName = c.Category.CategoryName,
                        SeasonName = c.Season.SeasonName,
                    }).ToList();
                if (t.Count > 0)
                {
                    resul = new OperationResult(OperationResultType.Success, "ok") { Data = t };
                }
            }
            return Json(resul);
        }
        public ActionResult View(int Id, string tabName)
        {
            var resul = _saleCampaignContract.SalesCampaigns.Where(c => c.Id == Id).FirstOrDefault();
            var originStoreId = resul.StoresIds.Split(",", true);
            var storeIds = originStoreId.Select(id => int.Parse(id)).ToList();
            var selectItemList = _storeContract.QueryManageStore(AuthorityHelper.OperatorId.Value)
                                .Select(s => new SelectListItem() { Text = s.StoreName, Value = s.Id.ToString() })
                                .ToList();
            selectItemList.Each(item =>
            {
                if (originStoreId.Contains(item.Value))
                {
                    item.Selected = true;
                }
            });

            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, false, false);
            ViewBag.Stores = selectItemList;
            ViewBag.TabName = tabName;
            return PartialView(resul);
        }
        /// <summary>
        /// 查看可以参与制定活动的商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ViewProduDa()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);

            var filterRules = request.FilterGroup.Rules.Where(w => w.Field == "BigProdNum" || w.Field == "ProductName" || w.Field == "BrandId").ToList();
            foreach (var item in filterRules)
            {
                request.FilterGroup.Rules.Remove(item);
            }

            Expression<Func<SalesCampaign, bool>> predicate = FilterHelper.GetExpression<SalesCampaign>(request.FilterGroup);
            var originNumbersList = new List<ProductOriginNumber>();
            var prod = _saleCampaignContract.SalesCampaigns.Where(predicate).FirstOrDefault();
            if (prod != null)
            {
                var query = prod.ProductOriginNumbers.AsQueryable();

                if (filterRules.Count > 0)
                {
                    if (filterRules.Any(c => c.Field == "ProductName"))
                    {
                        query = query.Where(w => w.ProductName != null);
                    }

                    FilterGroup fg = new FilterGroup();
                    fg.Rules = filterRules;

                    Expression<Func<ProductOriginNumber, bool>> predicate2 = FilterHelper.GetExpression<ProductOriginNumber>(fg);

                    query = query.Where(predicate2);
                }

                originNumbersList = query.ToList();
            }
            var da = originNumbersList.OrderByDescending(c => c.CreatedTime).ThenByDescending(c => c.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(c => new
            {
                Id = c.Id,
                ProductName = c.ProductName,
                TagPrice = c.TagPrice,
                BigProdNum = c.BigProdNum,
                BrandName = c.Brand.BrandName,
                SeasonName = c.Season.SeasonName,
                CategoryName = c.Category.CategoryName,
                CreatedTime = c.CreatedTime,
                ThumbnailPath = WebUrl + c.ThumbnailPath
            });
            GridData<object> data = new GridData<object>(da, originNumbersList.Count, request.RequestInfo);
            return Json(data);
        }



        public ActionResult Disable(int id)
        {
            var res = _saleCampaignContract.Disable(id);
            return Json(res);
        }

        public ActionResult Enable(int id)
        {
            var res = _saleCampaignContract.Enable(id);
            return Json(res);
        }

        public ActionResult Recovery(int id)
        {
            var res = _saleCampaignContract.Recovery(id);
            return Json(res);
        }

        public ActionResult Remove(int[] ids)
        {
            var res = _saleCampaignContract.Remove(ids);
            return Json(res);
        }


        //yxk 2015-9
        //upload file
        public JsonResult ExcelFileUpload()
        {

            OperationResult resul = new OperationResult(OperationResultType.Error);
            //string fileName = uploadfile.FileName;
            //string savepath = "./Warehouses/Content/uploadFiles/" + fileName;
            //uploadfile.SaveAs(savepath);
            int te = Request.Files.Count;
            if (System.Web.HttpContext.Current.Request.Files.Count > 0)
            {

                var file = Request.Files[0];
                string fileName = file.FileName;
                string savePath = Server.MapPath("/Content/UploadFiles/Excels/") + DateTime.Now.ToString("yyyyMMddHH");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fullName = savePath + "\\" + fileName;

                if (System.IO.File.Exists(fullName))
                {
                    System.IO.File.Delete(fullName);
                }
                file.SaveAs(fullName);
                var reda = ExcelToJson(fullName);
                System.IO.File.Delete(fullName);
                if (reda.Any())
                {
                    var bigProdNums = reda.Select(l => l.First()).ToList();
                    resul = new OperationResult(OperationResultType.Success, string.Empty, bigProdNums);
                }
            }
            return Json(resul);
        }

        private List<List<String>> ExcelToJson(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var da = new List<List<String>>();
                if (Path.GetExtension(fileName) == ".txt")
                {
                    string st = System.IO.File.ReadAllText(fileName);
                    var retda = st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var li = new List<List<string>>();
                    retda.Each(c =>
                    {
                        var t = new List<string>() { c };
                        li.Add(t);
                    });
                    da = li;
                }
                else
                {
                    YxkSabri.ExcelUtility excel = new YxkSabri.ExcelUtility();
                    da = excel.ExcelToArray(fileName, 0, 0);
                }
                return da;
            }
            return null;
        }
    }
}