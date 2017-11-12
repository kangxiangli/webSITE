using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using System.Web.Script.Serialization;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebGrease.Css.Extensions;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Orders;
using Whiskey.ZeroStore.ERP.Website.Areas.Properties.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{
    [License(CheckMode.Verify)]
    public class ProductDiscountController : BaseController
    {
        #region 初始化操作对象
        protected static readonly ILogger _log = LogManager.GetLogger(typeof(ProductDiscountController));
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreContract _storeContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISizeContract _sizeContract;
        protected readonly IProductDiscountContract _productDiscountContract;
        protected readonly IProductContract _productContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IProductAttributeContract _productAttributeContract;
        protected readonly IProductOrigNumberContract _ProductOrigNumberContract;
        protected readonly IInventoryContract _invenrtoryContract;

        public ProductDiscountController(IBrandContract brandContract,
            IStoreContract storeContract,
            ISeasonContract seasonContract,
            IColorContract colorContract,
            ISizeContract sizeContract,
            IProductDiscountContract productDiscountContract,
            IProductContract productContract,
            ICategoryContract categoryContract,
            IProductAttributeContract productAttributeContract,
            IProductOrigNumberContract ProductOrigNumberContract,
            IInventoryContract invenrtoryContract)
        {
            _brandContract = brandContract;
            _storeContract = storeContract;
            _seasonContract = seasonContract;
            _colorContract = colorContract;
            _sizeContract = sizeContract;
            _productContract = productContract;
            _productDiscountContract = productDiscountContract;
            _categoryContract = categoryContract;
            _productAttributeContract = productAttributeContract;
            _ProductOrigNumberContract = ProductOrigNumberContract;
            _invenrtoryContract = invenrtoryContract;

            ViewBag.Brand = (_brandContract.SelectList().Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Season = (_seasonContract.SelectList().Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Color = (_colorContract.SelectList().Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Size = (_sizeContract.SelectList().Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Discount = StaticHelper.DiscountList("选择折扣");
        }
        #endregion

        #region 初始化界面

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion


        /// <summary>
        /// 获取折扣列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<ProductDiscount, bool>> predicate = FilterHelper.GetExpression<ProductDiscount>(request.FilterGroup);
            var data = await Task.Run(() => {
                var entitys = _productDiscountContract.ProductDiscounts.Where(predicate);
                var list = entitys.OrderByDescending(c => c.UpdatedTime).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(m => new {
                    m.Id,
                    m.DiscountName,
                    m.DiscountCode,
                    BrandCou = m.BrandCount,
                    m.RetailDiscount,
                    m.WholesaleDiscount,
                    m.PurchaseDiscount,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();

                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Create1()
        {
            return View();
        }
        [Layout]
        public ActionResult CreateDiscount()
        {

            ViewBag.Color = CacheAccess.GetColorsName(_colorContract);

            // ViewBag.Color = (_colorContract.SelectList("选择颜色").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();

            ViewBag.Brand = CacheAccess.GetBrand(_brandContract);
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract);

            ViewBag.Season = CacheAccess.GetSeason(_seasonContract, false);
            // ViewBag.Size = CacheAccess.GetSize(_sizeContract, _categoryContract);
            ViewBag.Discount = StaticHelper.DiscountList("选择折扣");
            ViewBag.OneCollo = CacheAccess.GetOneCollo(_productAttributeContract, false);
            return View();
        }

        /// <summary>
        /// 添加折扣
        /// </summary>        
        /// <param name="productDiscount"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [Log]
        public JsonResult Create(ProductDiscountDto dto)
        {
            var resul = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(dto.DiscountName))
            {
                bool exis = _productDiscountContract.ProductDiscounts.Any(
                     c => c.DiscountName == dto.DiscountName && !c.IsDeleted && c.IsEnabled);
                if (exis)
                {
                    resul.Message = "已存在同名的折扣方案";
                    return Json(resul);
                }
                dto.DiscountType = 3;
                resul = _productDiscountContract.Insert(dto);
            }

            return Json(resul);
        }

        public JsonResult BathcProductCreate(int?[] ids, ProductDiscountDto dto)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            #region 添加前校验

            if (!string.IsNullOrEmpty(dto.DiscountName))
            {
                bool exis = _productDiscountContract.ProductDiscounts.Any(
                     c => c.DiscountName == dto.DiscountName && !c.IsDeleted && c.IsEnabled);
                if (exis)
                {
                    resul.Message = "已存在同名的折扣方案";
                    return Json(resul);
                }
            }
            #endregion
            string _validSessionKey = "1102_validsessionkey_09";
            string _invaliSessionKey = "1102_invalidsessionkey_03";

            if (ids.Any(c => c != null))
            {
                if (dto.DiscountType == 1)
                    dto.BigNumbers = string.Join(",", ids);
                resul = _productDiscountContract.Insert(dto);
            }
            else
            {
                var discountvalids = SessionAccess.Get(_validSessionKey) as List<DiscountValidSession>;
                if (discountvalids != null)
                {
                    resul = _productDiscountContract.Insert(dto);
                }
                else
                {
                    resul = new OperationResult(OperationResultType.ValidError);
                }
            }
            if (resul.ResultType == OperationResultType.Success)
            {
                SessionAccess.Remove(_validSessionKey);
                SessionAccess.Remove(_invaliSessionKey);
            }
            return Json(resul);
        }
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Update(int Id)
        {
            ProductDiscountDto dto = _productDiscountContract.Edit(Id);
            return View(dto);
        }

        [HttpPost]
        public JsonResult Update(ProductDiscountDto dto)
        {
            var result = _productDiscountContract.Update(dto);
            return Json(result);
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            ProductDiscount productDiscount = _productDiscountContract.View(Id);
            return PartialView(productDiscount);
        }

        /// <summary>
        /// 启用相应的折扣方案
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Enable(int Id)
        {

            OperationResult resul = _productDiscountContract.Enable(Id);
            //启用折扣的同时，修改系相应的商品价格
            if (resul.ResultType == OperationResultType.Success)
                resul = EnabledDiscount(Id);
            return Json(resul);
        }

        public ActionResult GetSizesByCategoryIds(string[] ids)
        {
            //int[] catids = ids.Where(c => !string.IsNullOrEmpty(c)).Select(c => int.Parse(c)).ToArray();
            //if (catids.Any())
            //{
            //    var sizes = CacheAccess.GetSize(_sizeContract, _categoryContract, c => catids.Contains((int)c.CategoryId));
            //    var resdat = GetSizeDropData(sizes);
            //    return Json(resdat);
            //    //return Json(sizes);
            //}
            //else
            //{

            //    return null;
            //}
            return null;
        }

        /// <summary>
        /// 禁用相应的折扣方案
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Disable(int Id)
        {
            OperationResult resul = _productDiscountContract.Disable(Id);
            return Json(resul);
        }
        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int Id)
        {
            var result = _productDiscountContract.Remove(Id);
            return Json(result);
        }

        #endregion

        #region 恢复数据

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <returns></returns>
        public JsonResult Recovery(int Id)
        {
            var result = _productDiscountContract.Recovery(Id);
            return Json(result);
        }
        #endregion

        #region 获取店铺列表

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Color()
        {
            return PartialView();
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ColorList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Color, bool>> predicate = FilterHelper.GetExpression<Color>(request.FilterGroup);
            var data = await Task.Run(() => {
                var count = 0;
                var list = _colorContract.Colors.Where<Color, int>(predicate, request.PageCondition, out count).Select(m => new {
                    m.ColorName,
                    m.Id,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region 获取品牌列表
        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Brand()
        {
            return PartialView();
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> BrandList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Brand, bool>> predicate = FilterHelper.GetExpression<Brand>(request.FilterGroup);
            var data = await Task.Run(() => {

                Func<List<Brand>, List<Brand>> getTree = null;
                getTree = (source) => {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<Brand> tree = new List<Brand>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        var chil = _brandContract.Brands.Where(c => c.ParentId == child.Id).ToList();
                        tree.AddRange(getTree(chil));
                    }
                    return tree;
                };

                var parents = _brandContract.Brands.Where(m => m.ParentId == null).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new {
                    ParentId = m.ParentId == null ? "" : m.ParentId.ToString(),
                    m.BrandName,
                    m.Id,
                    m.DefaultDiscount
                }).ToList();
                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #endregion

        #region 获取季节列表

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Season()
        {
            return PartialView();
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> SeasonList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Season, bool>> predicate = FilterHelper.GetExpression<Season>(request.FilterGroup);
            var data = await Task.Run(() => {
                var count = 0;
                var list = _seasonContract.Seasons.Where<Season, int>(predicate, request.PageCondition, out count).Select(m => new {
                    m.SeasonName,
                    m.SeasonCode,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                    m.IconPath
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #endregion
        //数据在服务端校验并保持在session中
        [HttpPost]
        public ActionResult AddToScan(string uuid, string number, bool isbigNumb)
        {
            OperationResult resul = new OperationResult(OperationResultType.Success);
            string _validSessionKey = "1102_validsessionkey_09";
            string _invaliSessionKey = "1102_invalidsessionkey_03";
            Product product = null;
            ProductOriginNumber orignum = null;

            List<DiscountValidSession> discouvalidssion =
                (SessionAccess.Get(_validSessionKey) as List<DiscountValidSession>) ?? new List<DiscountValidSession>();

            List<DiscountValidSession> discouInvalidSession =
                     (SessionAccess.Get(_invaliSessionKey) as List<DiscountValidSession>) ?? new List<DiscountValidSession>();

            if (discouvalidssion.Any() && discouvalidssion.FirstOrDefault(c => c.Uuid == uuid) != null)
            {
                discouvalidssion.FirstOrDefault(c => c.Uuid == uuid).Count += 1;

            }
            else
            {
                if (discouInvalidSession.Any() && discouInvalidSession.FirstOrDefault(c => c.Uuid == uuid) != null)
                    discouInvalidSession.FirstOrDefault(c => c.Uuid == uuid).Count += 1;
                else
                {
                    DiscountValidSession dis = new DiscountValidSession()
                    {
                        Uuid = uuid,
                        Number = number,
                        Count = 1
                    };
                    if (isbigNumb)
                    {//传入的是大款号
                        orignum = ProductOrigNumberValid(number);
                        dis.Type = 1;
                        if (orignum != null)
                        {
                            dis.Id = orignum.Id;
                            dis.Number = orignum.BigProdNum;
                            discouvalidssion.Add(dis);
                        }
                        else
                        {
                            discouInvalidSession.Add(dis);
                            resul.ResultType = OperationResultType.Error;
                            resul.Message = "商品货号不存在";
                        }

                    }
                    else
                    {//传入的是商品货号
                        product = ProductValid(number);
                        dis.Type = 0;
                        if (product != null)
                        {
                            dis.Id = product.Id;
                            dis.Number = product.ProductNumber;
                            discouvalidssion.Add(dis);
                        }
                        else
                        {
                            discouInvalidSession.Add(dis);
                            resul.ResultType = OperationResultType.Error;
                            resul.Message = "商品货号不存在";
                        }
                    }

                }
            }
            try
            {
                SessionAccess.Set(_validSessionKey, discouvalidssion, true);
                SessionAccess.Set(_invaliSessionKey, discouInvalidSession, true);
            }
            catch (Exception)
            {
                throw;
            }
            resul.Data = new {
                uuid = uuid,
                validCou = discouvalidssion.GroupBy(c => c.Number).Count(),
                invalidCou = discouInvalidSession.GroupBy(c => c.Number).Count()
            };
            if (isbigNumb)
            {
                orignum = orignum ?? ProductOrigNumberValid(number);
                if (orignum == null)
                {
                    resul.ResultType = OperationResultType.Error;
                    resul.Message = "商品款号不存在";
                }
                else
                {
                    Product prod = _productContract.Products.FirstOrDefault(c => c.OriginNumber == orignum.OriginNumber);
                    resul.Other = new {
                        Id = orignum.Id,
                        ProductNumber = orignum.BigProdNum,
                        Brand = prod.ProductOriginNumber.Brand.BrandName,
                        Category = prod.ProductOriginNumber.Category.CategoryName,
                        Size = "",
                        Thumbnail = prod.ThumbnailPath,

                    };
                }

            }
            else
            {
                product = product ?? ProductValid(number);
                if (product != null)
                    resul.Other = new {
                        Id = product.Id,
                        ProductNumber = product.ProductNumber,
                        Brand = product.ProductOriginNumber.Brand.BrandName,
                        Category = product.ProductOriginNumber.Category.CategoryName,
                        Size = product.Size.SizeName,
                        Thumbnail = product.ThumbnailPath,
                    };
                else
                {
                    resul.ResultType = OperationResultType.Error;
                    resul.Message = "商品编号不存在";
                }
            }
            return Json(resul);
        }
        [HttpGet]
        public ActionResult GetValidList()
        {
            ViewBag.actid = Request["actid"];
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetValidDataList()
        {
            string _validSessionKey = "1102_validsessionkey_09";
            string _invaliSessionKey = "1102_invalidsessionkey_03";
            var validData = SessionAccess.Get(_validSessionKey) as List<DiscountValidSession>;
            var invalidData = SessionAccess.Get(_invaliSessionKey) as List<DiscountValidSession>;

            return Json(new {
                validDa = validData,
                invaliDa = invalidData
            });
        }

        public ActionResult GetProducts()
        {
            GridRequest gr = new GridRequest(Request);
            var expre = FilterHelper.GetExpression<Product>(gr.FilterGroup);
            var alldat = _productContract.Products.Where(expre);
            var da = alldat.OrderBy(c => c.CreatedTime).ThenByDescending(c => c.Id).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize).Select(c => new {
                c.Id,
                c.ProductName,
                c.ProductNumber,
                c.ThumbnailPath,
                c.ProductOriginNumber.Brand.BrandName,
                c.ProductOriginNumber.Category.CategoryName,
                c.Size.SizeName,
                c.ProductOriginNumber.Season.SeasonName,
                c.Color.ColorName,
                c.ProductOriginNumber.TagPrice
            }).ToList();
            int cou = alldat.Count();
            var data = new GridData<object>(da, cou, Request);
            return Json(data);
        }

        /// <summary>
        /// 当刷新页面的时候清空session
        /// </summary>
        [HttpPost]
        public void UnloadCurPage()
        {
            string _validSessionKey = "1102_validsessionkey_09";
            string _invaliSessionKey = "1102_invalidsessionkey_03";
            SessionAccess.Remove(_validSessionKey);
            SessionAccess.Remove(_invaliSessionKey);
        }
        /// <summary>
        /// 根据折扣ID获取与该折扣关联的商品或者款号信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetProductsByDiscountId()
        {
            GridRequest gr = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<ProductDiscount>(gr.FilterGroup);
            var ent = _productDiscountContract.ProductDiscounts.FirstOrDefault(pred);
            List<object> li = new List<object>();
            if (ent != null)
            {
                if (ent.DiscountType == 1)
                {
                    li = ent.ProductOrigNumbers.Select(c => new {
                        c.Id,
                        c.ProductName,
                        c.OriginNumber,
                        c.CreatedTime,
                        c.UpdatedTime,
                        c.TagPrice,

                    }).ToList<object>();
                }
                else if (ent.DiscountType == 2)
                {
                    //li = ent.Products.Select(c => new {
                    //    c.Id,
                    //    c.ProductName,
                    //    c.ProductNumber,
                    //    c.CreatedTime,
                    //    c.UpdatedTime,
                    //    c.ProductOrigNumb.TagPrice,

                    //}).ToList<object>();
                }
                else if (ent.DiscountType == 3)
                {

                }
                else
                {

                }
            }
            GridData<object> data = new GridData<object>(li, li.Count, Request);
            return Json(data);
        }
        /// <summary>
        /// 校验商品原始货号是否存在
        /// </summary>
        /// <param name="number"></param>
        private ProductOriginNumber ProductOrigNumberValid(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length < 7)
            {
                // throw new NullReferenceException("参数numeber存在异常");
                return null;
            }
            else
            {
                number = number.Substring(0, 7);//取商品大款号
                return _ProductOrigNumberContract.OrigNumbs.FirstOrDefault(
                      c => c.BigProdNum == number && c.IsEnabled && !c.IsDeleted);

            }
        }
        /// <summary>
        /// 校验商品是否存在
        /// </summary>
        /// <param name="number"></param>
        private Product ProductValid(string number)
        {

            if (string.IsNullOrEmpty(number))
            {
                throw new NullReferenceException("参数不为空");
            }
            else
            {
                //说明校验的是商品的一维码
                if (number.Trim().Length >= 13)
                    number = number.Substring(0, number.Length - 3);

                Product product = _productContract.Products.FirstOrDefault(
                       c => c.ProductNumber == number && c.IsEnabled && !c.IsDeleted);
                return product;
            }
        }
        /// <summary>
        /// 根据折扣方案更新相关的商品价格
        /// </summary>
        /// <param name="Id"></param>
        private OperationResult EnabledDiscount(int id)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);

            var discount = _productDiscountContract.ProductDiscounts.FirstOrDefault(c => c.Id == id && !c.IsDeleted && c.IsEnabled);
            if (discount != null)
            {

                switch (discount.DiscountType)
                {
                    //大款号
                    case 1:
                        {
                            try
                            {
                                result = UpdateBigNumberPrice(discount);
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }
                            break;
                        }
                    //商品
                    case 2:
                        {
                            //var products = discount.Products;
                            //int[] productIds = products.Select(c => c.Id).ToArray();
                            //string[] productNumbs = products.Select(c => c.ProductNumber).ToArray();
                            ////更新库存商品价格
                            //_invenrtoryContract.UpdatePriceByDiscount(discount, null, productNumbs, true);
                            ////更新商品档案价格
                            //try
                            //{
                            //    result = _productContract.UpdatePriceByDiscount(discount, productIds, false);
                            //}
                            //catch (Exception ex)
                            //{

                            //    throw;
                            //}

                            break;
                        }

                    //复杂条件筛选
                    case 3:
                        {
                            result = UpdatePriceOfMulti(discount);

                            break;
                        }
                }
            }
            return result;
        }

        /// <summary>
        /// 根据复杂折扣方案更新商品档案和库存价格
        /// </summary>
        /// <param name="discount"></param>
        private OperationResult UpdatePriceOfMulti(ProductDiscount discount)
        {
            #region 
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var _brandIds = discount.Brands.Select(c =>JToken.Parse(c.Id.ToString())).ToArray();

            JArray brandIds = new JArray(_brandIds);
            
            FilterGroup fg = new FilterGroup(FilterOperate.And);
           
            if (brandIds.Any())
            {
                fg.Rules.Add(new FilterRule("BrandId", brandIds, FilterOperate.In));
            }

            #endregion

            var exp = FilterHelper.GetExpression<Product>(fg);
            var products = _productContract.Products.Where(exp);

            products.Each(c => {

                c.ProductOriginNumber.PurchasePrice = c.ProductOriginNumber.TagPrice;
                c.ProductOriginNumber.WholesalePrice = c.ProductOriginNumber.TagPrice;
                c.ProductOperationLogs.Add(new ProductOperationLog()
                {
                    Description = "根据折扣方案调整商品价格,折扣方案ID：" + discount.Id,
                    OperatorId = AuthorityHelper.OperatorId,

                });
            });
            var productids = products.Select(c => c.Id).ToList();
            //在更新商品价格时，已销售的商品部更新
            var invents =
            _invenrtoryContract.Inventorys.Where(
                c =>
                    productids.Contains(c.ProductId) && c.IsEnabled && !c.IsDeleted &&
                    c.Status == (int)InventoryStatus.Default);

            //invents.Each(c => {
            //    c.WholesalePrice = c.WholesalePrice * discount.WholesaleDiscount / 10;
            //    c.PurchasePrice = c.PurchasePrice * discount.PurchaseDiscount / 10;
            //});
            var invetdtos = AutoMapper.Mapper.Map<InventoryDto[]>(invents.ToArray());

            try
            {
                _invenrtoryContract.Update(invetdtos, true);
                resul = _productContract.Update(products.ToArray(), false);

            }
            catch (Exception ex)
            {
                
                throw;
            }
            return resul;
        }
        /// <summary>
        /// 根据折扣方案更新大款号的商品价格
        /// </summary>
        /// <param name="discount"></param>
        /// <returns></returns>
        private OperationResult UpdateBigNumberPrice(ProductDiscount discount)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var bigproduct = discount.ProductOrigNumbers;
            var origId = bigproduct.Select(c => c.Id).ToArray();

            string[] orignums = bigproduct.Select(c => c.OriginNumber).ToArray();

            var products = _productContract.Products.Where(c => orignums.Contains(c.OriginNumber)).Select(c => new { c.Id, c.ProductNumber }).ToArray();

            int[] productIds = products.Select(c => c.Id).ToArray();
            string[] productNumb = products.Select(c => c.ProductNumber).ToArray();

            //更新大款号对应的价格
            _ProductOrigNumberContract.UpdatePriceByDiscount(discount, origId, true);
            //更新关联的库存商品价格
            _invenrtoryContract.UpdatePriceByDiscount(discount, null, productNumb, true);
            //更新大款号对应的商品价格
            resul = _productContract.UpdatePriceByDiscount(discount, productIds, false);
            return resul;
        }

        private List<SelectListDat> GetSizeDropData(SelectList li)
        {
            var sel = new List<SelectListDat>();
            foreach (var _item in li)
            {
                var selda = sel.FirstOrDefault(c => c.label == _item.Group.Name);
                if (selda != null)
                {
                    var childs = selda.children.ToList();
                    var exis = childs.Any(c => c.value == _item.Value);
                    if (!exis)
                    {
                        childs.Add(new SelectListDat()
                        {
                            label = _item.Text,
                            value = _item.Value,
                            selected = _item.Selected,
                        });
                    }
                    selda.children = childs.ToArray();
                }
                else
                {
                    sel.Add(new SelectListDat()
                    {
                        label = _item.Group.Name,
                        children = new SelectListDat[]
                        {
                            new SelectListDat()
                            {
                                label = _item.Text,
                                 value=_item.Value,
                               selected = _item.Selected,
                            }, 
                        }
                    });
                }

            }
            return sel;
        }
    }

}