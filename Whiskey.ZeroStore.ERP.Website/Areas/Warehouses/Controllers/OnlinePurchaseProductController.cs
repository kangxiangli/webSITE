using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory;
using System.Text;
using System.Web.Security;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Content;
using AutoMapper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Newtonsoft.Json;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    /// <summary>
    /// 在线采购
    /// </summary>
    public class OnlinePurchaseProductController : BaseController
    {
        #region 初始化操作对象
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(OnlinePurchaseProductController));

        protected readonly IOnlinePurchaseProductContract _onlinePurchaseProductContract;

        protected readonly IProductContract _productContract;

        protected readonly IBrandContract _brandContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IColorContract _colorContract;

        protected readonly IStoreContract _storeContract;

        protected readonly INotificationContract _notificationContract;

        public OnlinePurchaseProductController(IOnlinePurchaseProductContract onlinePurchaseProductContract,
            IProductContract productContract,
            IBrandContract brandContract,
            ICategoryContract categoryContract,
            IColorContract colorContract,
            IStoreContract storeContract,
            INotificationContract notificationContract
            )
        {
            _onlinePurchaseProductContract = onlinePurchaseProductContract;
            _productContract = productContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _colorContract = colorContract;
            _storeContract = storeContract;
            _notificationContract = notificationContract;
        }
        #endregion

        /// <summary>
        /// 定义当前写入cookie的键
        /// </summary>
        internal readonly string CookieNumber = "2ac5975b964b7152";

        /// <summary>
        /// 每次插入数据库的数据条数
        /// </summary>
        internal readonly int InsertCount = 1;

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




        #region 获取采购数据列表
        /// <summary>
        /// 获取采购数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List(BigProdNumStateEnum? state)
        {

            GridRequest request = new GridRequest(Request);
            Expression<Func<OnlinePurchaseProduct, bool>> predicate = FilterHelper.GetExpression<OnlinePurchaseProduct>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                int count = 0;
                int adminId = AuthorityHelper.OperatorId ?? 0;
                var query = _onlinePurchaseProductContract.OnlinePurchaseProducts;
                var config = _onlinePurchaseProductContract.GetConfig();
                if (state.HasValue)
                {

                    switch (state.Value)
                    {
                        case BigProdNumStateEnum.普通:
                            query = query.Where(o => o.CreatedTime < config.NewProductTime && o.CreatedTime > config.ClassicProductTime);
                            break;
                        case BigProdNumStateEnum.新品:
                            query = query.Where(o => o.CreatedTime >= config.NewProductTime);
                            break;
                        case BigProdNumStateEnum.经典:
                            query = query.Where(o => o.CreatedTime <= config.ClassicProductTime);
                            break;
                        default:
                            break;
                    }
                }

                query = query.Where<OnlinePurchaseProduct, int>(predicate, request.PageCondition, out count);
                var list = query.Select(x => new
                {
                    x.Id,
                    x.CreatedTime,
                    x.StartDate,
                    x.EndDate,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.NoticeTitle,
                    x.NoticeQuantity,
                    x.UpdatedTime,
                    x.UniqueCode,
                    AdminName = x.Operator.Member.MemberName,
                    Quantity = x.OnlinePurchaseProductItems.Where(w => w.IsEnabled && !w.IsDeleted).Count(),
                }).ToList()
                .Select(x => new
                {

                    x.Id,
                    x.StartDate,
                    x.EndDate,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.NoticeTitle,
                    x.NoticeQuantity,
                    x.UpdatedTime,
                    x.UniqueCode,
                    x.AdminName,
                    x.Quantity,
                    x.CreatedTime,
                    State = _onlinePurchaseProductContract.GetOnlinePurchaseState(x.CreatedTime, config).ToString()
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 初始化添加数据
        /// </summary>
        /// <param name="num">可选参数</param>
        /// <returns></returns>
        [Layout]
        public ActionResult Create(string num = null)
        {
            OnlinePurchaseProductDto onlineDto = new OnlinePurchaseProductDto();
            DateTime current = DateTime.Now;
            string strDate = current.ToString("yyyy-MM-dd 00:00:00");
            string strEndDate = current.ToString("yyyy-MM-dd 23:23:59");
            onlineDto.StartDate = DateTime.Parse(strDate);
            onlineDto.EndDate = DateTime.Parse(strEndDate);
            int Quantity = 0;
            if (num != null)
            {
                OnlinePurchaseProduct online = _onlinePurchaseProductContract.OnlinePurchaseProducts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.UniqueCode == num);
                if (online != null)
                {
                    Quantity = online.OnlinePurchaseProductItems.Where(w => w.IsEnabled && !w.IsDeleted).Count();
                    Mapper.CreateMap<OnlinePurchaseProduct, OnlinePurchaseProductDto>();
                    onlineDto = Mapper.Map<OnlinePurchaseProduct, OnlinePurchaseProductDto>(online);
                    CheckResultEdo edo = new CheckResultEdo();
                    edo.UniqueCode = onlineDto.UniqueCode;
                    edo.ValidCount = Quantity;
                    WriteCookie(edo);
                }
            }
            ViewBag.Quantity = Quantity;
            return View(onlineDto);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(OnlinePurchaseProductDto dto)
        {
            dto.EndDate = dto.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            OperationResult oper = _onlinePurchaseProductContract.Insert(dto);
            if (oper.ResultType == OperationResultType.Success)
            {
                CheckResultEdo edo = new CheckResultEdo();
                edo.UniqueCode = dto.UniqueCode;
                WriteCookie(edo);
            }
            return Json(oper);
        }
        #endregion

        #region 获取商品列表
        /// <summary>
        /// 初始化商品列表界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Products()
        {
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true, false);
            ViewBag.Color = (_colorContract.SelectList("请选择").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            return PartialView();
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ProductList()
        {
            GridRequest request = new GridRequest(Request);
            FilterRule imgRule = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "hasImg");
            int? hasImg = null;
            if (imgRule != null)
            {
                hasImg = Convert.ToInt32(imgRule.Value);
                request.FilterGroup.Rules.Remove(imgRule);
            }

            Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                int pageIndex = request.PageCondition.PageIndex;
                int pageSize = request.PageCondition.PageSize;
                IQueryable<Product> listProduct = _productContract.Products.Where(predicate);
                //是否有图片筛选
                if (hasImg.HasValue)
                {
                    if (hasImg.Value == 1)
                    {
                        listProduct = listProduct.Where(p => !string.IsNullOrEmpty(p.ThumbnailPath));
                    }
                    else
                    {
                        listProduct = listProduct.Where(p => string.IsNullOrEmpty(p.ThumbnailPath));
                    }
                }

                IQueryable<string> listNum = listProduct.Select(x => x.BigProdNum).Distinct();
                count = listProduct.Select(x => x.BigProdNum).Distinct().Count();
                List<string> listBigProdNum = listNum.OrderByDescending(x => x).Skip(pageIndex).Take(pageSize).ToList();
                List<ProductEdo> listProductEdo = new List<ProductEdo>();
                List<Product> listEntity = listProduct.Where(x => listBigProdNum.Where(k => k == x.BigProdNum).Count() > 0).ToList();
                string strColor = string.Empty;
                string strSize = string.Empty;
                string strImagePath = string.Empty;
                string strBrandName = string.Empty;
                string strCategoryName = string.Empty;
                foreach (string num in listBigProdNum)
                {
                    IEnumerable<Product> entities = listEntity.Where(x => x.BigProdNum == num);
                    List<string> listColors = listEntity.Select(x => x.Color.ColorName).Distinct().ToList();
                    List<string> listSize = listEntity.Select(x => x.Size.SizeName).Distinct().ToList();
                    strColor = ExpendTostring(listColors);
                    strSize = ExpendTostring(listSize);
                    Product product = entities.FirstOrDefault();
                    strImagePath = entities.FirstOrDefault().ThumbnailPath ?? entities.FirstOrDefault().ProductOriginNumber.ThumbnailPath;
                    strBrandName = product.ProductOriginNumber == null ? "" : product.ProductOriginNumber.Brand.BrandName;
                    strCategoryName = product.ProductOriginNumber == null ? "" : product.ProductOriginNumber.Category.CategoryName;
                    listProductEdo.Add(new ProductEdo
                    {
                        Id = num,
                        ColorName = strColor,
                        SizeName = strSize,
                        ImagePath = strImagePath,
                        ColorCount = listColors.Count(),
                        SizeCount = listSize.Count(),
                        BrandName = strBrandName,
                        CategoryName = strCategoryName,
                    });

                }
                return new GridData<object>(listProductEdo, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 转换成字符串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private string ExpendTostring(List<string> list)
        {
            StringBuilder sbText = new StringBuilder();
            foreach (string item in list)
            {
                sbText.Append(item + ",");
            }
            return sbText.ToString();
        }
        #endregion

        #region 添加可采购的数据
        /// <summary>
        /// 添加采购商品，当要添加的商品满足InsetCount时，才进行添加操作；
        /// 否则，将用户选择的数据写入用户的Cookie中，如果用户仅选取的数量少于InsetCount，
        /// 那么用户在确认本次采购完成的时候，会读取cookie强制插入数据;
        /// 没有使用session，session容易丢失和占用服务器资源
        /// </summary>
        /// <param name="Id"></param>        
        /// <returns></returns>
        [HttpPost]
        public JsonResult Add(string[] Ids)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            CheckResultEdo edo = ReadCookie(Request);
            //List<string> list = edo.Valids;
            //list.AddRange(Ids.ToList());
            edo.Valids.AddRange(Ids.ToList());
            int count = edo.Valids.Count();
            if (count >= InsertCount)
            {
                oper = _onlinePurchaseProductContract.Insert(edo.UniqueCode, edo);
                edo.Valids.Clear();
            }
            WriteCookie(edo);
            oper.Data = new { InvalidCount = edo.InvalidCount, ValidCount = edo.ValidCount };
            return Json(oper);
        }

        /// <summary>
        /// 创建完成时，添加可采购的数据
        /// 读取cookie数据插入到数据库中
        /// </summary>
        /// <param name="OnlinePurchaseProductId"></param>
        /// <returns></returns>
        public JsonResult Add()
        {
            CheckResultEdo edo = ReadCookie(Request);
            List<string> list = edo.Valids;
            OperationResult oper = new OperationResult(OperationResultType.Success);
            if (list.Count() > 0)
            {
                oper = _onlinePurchaseProductContract.Insert(edo.UniqueCode, edo);
                oper.Data = new { InvalidCount = edo.InvalidCount, ValidCount = edo.ValidCount };
            }
            this.InvalidCookie(Request);
            return Json(oper);
        }


        /// <summary>
        /// 写入cookie
        /// </summary>
        /// <param name="list"></param>
        private void WriteCookie(CheckResultEdo edo)
        {
            DateTime expiration = DateTime.Now.Add(FormsAuthentication.Timeout);
            DateTime issueDate = DateTime.Now;
            //票证的版本号
            int version = 3;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string strData = jss.Serialize(edo);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(version, CookieNumber, issueDate, expiration, false, strData, FormsAuthentication.FormsCookiePath);
            HttpCookie cookie = new HttpCookie(CookieNumber, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                Path = FormsAuthentication.FormsCookiePath,
            };
            Response.Cookies.Remove(cookie.Name);
            Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 使cookie失效
        /// </summary>
        /// <param name="Request"></param>
        private void InvalidCookie(HttpRequestBase Request)
        {
            HttpCookie cookie = Request.Cookies[CookieNumber];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
            }
        }

        /// <summary>
        /// 读取出存在cookie内的值
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        private CheckResultEdo ReadCookie(HttpRequestBase Request)
        {
            HttpCookie cookie = Request.Cookies[CookieNumber];
            CheckResultEdo edo = new CheckResultEdo();
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null && !string.IsNullOrEmpty(ticket.Name) && !string.IsNullOrEmpty(ticket.UserData))
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    edo = jss.Deserialize<CheckResultEdo>(ticket.UserData);
                }
            }
            return edo;
        }
        #endregion

        #region 获取采购数量列表
        //public async Task<ActionResult> PurchaseProducts()
        //{

        //}
        #endregion

        #region 查看有效数据
        /// <summary>
        /// 查看有效数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Valid()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取有效数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ValidList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<OnlinePurchaseProductItem, bool>> predicate = FilterHelper.GetExpression<OnlinePurchaseProductItem>(request.FilterGroup);
            CheckResultEdo edo = ReadCookie(Request);
            var data = await Task.Run(() =>
            {
                int count = 0;
                int pageIndex = request.PageCondition.PageIndex;
                int pageSize = request.PageCondition.PageSize;
                IQueryable<Product> listProduct = _productContract.Products.Where(w => w.IsEnabled && !w.IsDeleted);
                OnlinePurchaseProduct onlinePurchaseProduct = _onlinePurchaseProductContract.OnlinePurchaseProducts.FirstOrDefault(x => x.UniqueCode == edo.UniqueCode);
                List<string> listNumber = onlinePurchaseProduct.OnlinePurchaseProductItems.AsQueryable()
                    .Where<OnlinePurchaseProductItem, int>(predicate, request.PageCondition, out count)
                    .Select(x => x.BigProdNum).Distinct().ToList();
                List<ProductEdo> listProductEdo = new List<ProductEdo>();
                List<Product> listEntity = listProduct.Where(x => listNumber.Any(k => k == x.BigProdNum) == true).ToList();
                string strColor = string.Empty;
                string strSize = string.Empty;
                string strImagePath = string.Empty;
                string strBrandName = string.Empty;
                string strCategoryName = string.Empty;
                foreach (string number in listNumber)
                {
                    IEnumerable<Product> entities = listEntity.Where(x => x.BigProdNum == number);
                    List<string> listColors = listEntity.Select(x => x.Color.ColorName).Distinct().ToList();
                    List<string> listSize = listEntity.Select(x => x.Size.SizeName).Distinct().ToList();
                    strColor = ExpendTostring(listColors);
                    strSize = ExpendTostring(listSize);
                    Product product = entities.FirstOrDefault();
                    strImagePath = product.ProductOriginNumber.ThumbnailPath;
                    strBrandName = product.ProductOriginNumber == null ? "" : product.ProductOriginNumber.Brand.BrandName;
                    strCategoryName = product.ProductOriginNumber == null ? "" : product.ProductOriginNumber.Category.CategoryName;
                    listProductEdo.Add(new ProductEdo
                    {
                        Id = number,
                        ColorName = strColor,
                        SizeName = strSize,
                        ImagePath = strImagePath,
                        ColorCount = listColors.Count(),
                        SizeCount = listSize.Count(),
                        BrandName = strBrandName,
                        CategoryName = strCategoryName,
                        BigProdNum = number
                    });
                }
                return new GridData<object>(listProductEdo, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 移除采购商品数据
        public JsonResult RemoveProduct(params string[] BigProdNums)
        {
            CheckResultEdo edo = ReadCookie(Request);
            OperationResult oper = _onlinePurchaseProductContract.Remove(edo.UniqueCode, BigProdNums);
            return Json(oper);
        }
        #endregion

        #region 移除采购单
        /// <summary>
        /// 移除采购单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int[] Id)
        {
            OperationResult oper = _onlinePurchaseProductContract.Remove(Id);
            return Json(oper);
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Disable(int Id)
        {
            OperationResult oper = _onlinePurchaseProductContract.Disable(Id);
            return Json(oper);
        }
        #endregion

        #region 查看数据详情
        public ActionResult View(int Id)
        {
            OnlinePurchaseProduct entity = _onlinePurchaseProductContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 推送通知
        public JsonResult Notice(int Id)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "操作异常");
            OnlinePurchaseProduct online = _onlinePurchaseProductContract.View(Id);
            List<Store> listStore = _storeContract.Stores.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Administrators.Any()).ToList();
            List<int> listAdminId = new List<int>();
            foreach (Store item in listStore)
            {
                listAdminId.AddRange(item.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => x.Id));
            }
            string strContent = string.IsNullOrEmpty(online.NoticeContent) ? online.NoticeTitle : online.NoticeContent;
            NotificationDto dto = new NotificationDto()
            {
                AdministratorIds = listAdminId,
                Title = online.NoticeTitle,
                Description = strContent,
                IsEnableApp = true,
                NoticeTargetType = (int)NoticeTargetFlag.Admin,
                NoticeType = (int)NoticeFlag.Immediate
            };
            oper = _notificationContract.Insert(sendNotificationAction, dto);
            return Json(oper);
        }
        #endregion

        #region 创建完成
        public EmptyResult Complete()
        {
            this.InvalidCookie(Request);
            return null;
        }
        #endregion

        [HttpGet]
        public ActionResult EditConfig()
        {
            var config = _onlinePurchaseProductContract.GetConfig();

            return PartialView(config);
        }

        [HttpPost]
        public ActionResult EditConfig([Bind(Include = "NewProductTime,ClassicProductTime")]BigProdNumStateConfigEntry entry)
        {

            var res = _onlinePurchaseProductContract.UpdateConfig(entry);
            return Json(res);

        }



    }
}