using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{
    /// <summary>
    /// 商品提成
    /// </summary>
    [License(CheckMode.Verify)]
    public class ProductCommissionController : BaseController
    {
        #region 初始化操作对象
        protected static readonly ILogger _log = LogManager.GetLogger(typeof(ProductCommissionController));
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreContract _storeContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly IProductCommissionContract _productCommissionContract;

        public ProductCommissionController(IBrandContract brandContract, 
            IStoreContract storeContract, 
            ISeasonContract seasonContract,
            IProductCommissionContract productCommissionContract)
        {
            _brandContract = brandContract;
            _storeContract = storeContract;
            _seasonContract = seasonContract;
            _productCommissionContract = productCommissionContract;
            ViewBag.Brand = (_brandContract.SelectList().Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Season = (_seasonContract.SelectList().Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Discount = StaticHelper.CommList("选择提成点");
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

        #region 获取列表
                
        /// <summary>
        /// 获取折扣列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<ProductCommission, bool>> predicate = FilterHelper.GetExpression<ProductCommission>(request.FilterGroup);
            var data = await Task.Run(() =>
            {                
                //var parents = _colorContract.Colors.Where(m => m.ParentId == null).ToList();
                var list = _productCommissionContract.ProductCommissions.Where(predicate).Select(m => new
                {
                    m.Id,
                    m.CommissionName,
                    m.Percentage,
                    m.Store.StoreName,
                    m.Brand.BrandName,
                    m.Season.SeasonName,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();

                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 添加数据
        /// </summary>         
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [Log]
        public JsonResult Create(ProductCommissionDto dto)
        {
            var result =_productCommissionContract.Insert(dto);
            return Json(result);
            #region 注释代码 
            //if (Brands != null)
            //{

            //    var checkName = _productCommissionContract.CheckName(productCommission.CommissionName);
            //    if (!checkName)
            //    {
            //        return Json(new OperationResult(OperationResultType.Error, "用户名已经存在！"), JsonRequestBehavior.AllowGet);
            //    }
            //    var listDiscount = _productCommissionContract.ProductCommissions.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            //    List<ProductCommission> listBrand = listDiscount.Where(x => Brands.Where(k => k == x.BrandId.ToString()).Count() > 0).ToList();
            //    if (Seasons != null)
            //    {
            //        listBrand = listBrand.Where(x => Seasons.Where(k => k == x.SeasonId.ToString()).Count() > 0).ToList();
            //    }
            //    if (Stores != null)
            //    {
            //        listBrand = listBrand.Where(x => Stores.Where(k => k == x.StoreId.ToString()).Count() > 0).ToList();
            //    }
            //    if (listBrand.Count > 0)
            //    {
            //        bool isDel = _productCommissionContract.Delete(listBrand);
            //        if (isDel == false)
            //        {
            //            return Json(new OperationResult(OperationResultType.Error, "添加失败"), JsonRequestBehavior.AllowGet);
            //        }
            //    }
            //    List<ProductCommission> listProductComm = new List<ProductCommission>();
            //    for (int i = 0; i < Brands.Length; i++)
            //    {
            //        ProductCommission comm = new ProductCommission();
            //        comm.BrandId = int.Parse(Brands[i]);
            //        comm.CommissionName = productCommission.CommissionName;
            //        comm.Percentage = productCommission.Percentage;
            //        comm.OperatorId = AuthorityHelper.OperatorId;
            //        listProductComm.Add(comm);
            //    }
            //    if (Stores != null)
            //    {
            //        List<ProductCommission> listStore = new List<ProductCommission>();
            //        for (int i = 0; i < Stores.Length; i++)
            //        {
            //            foreach (var item in listProductComm)
            //            {
            //                item.StoreId = int.Parse(Stores[i]);
            //                listStore.Add(item);
            //            }
            //        }
            //        listProductComm = listStore;
            //    }
            //    if (Seasons != null)
            //    {
            //        List<ProductCommission> listSeason = new List<ProductCommission>();
            //        for (int i = 0; i < Seasons.Length; i++)
            //        {
            //            foreach (var item in listProductComm)
            //            {
            //                item.SeasonId = int.Parse(Seasons[i]);
            //                listSeason.Add(item);
            //            }
            //        }
            //        listProductComm = listSeason;
            //    }
            //    productCommission.OperatorId = AuthorityHelper.OperatorId;
            //    productCommission.Children = listProductComm;
            //    var result = _productCommissionContract.Insert(productCommission);
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    return Json(new OperationResult(OperationResultType.Error, "请选择品牌"), JsonRequestBehavior.AllowGet);
            //}
            #endregion  
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Update(int Id)
        {
            ProductCommissionDto dto = _productCommissionContract.Edit(Id);
            string strStoreName = string.Empty;
            string strBrandName = string.Empty;
            string strSeasonName = string.Empty;
            if (dto != null)
            {
                Store store = _storeContract.View(dto.StoreId);
                if (store != null)
                {
                    strStoreName = store.StoreName;
                }
                Brand brand = _brandContract.View(dto.BrandId);
                if (brand != null)
                {
                    strBrandName = brand.BrandName;
                }
                Season season = _seasonContract.View(dto.SeasonId);
                if (season != null)
                {
                    strSeasonName = season.SeasonName;
                }
            }
            dto.StoreName = strStoreName;
            dto.BrandName = strBrandName;
            dto.SeasonName = strSeasonName;
            return View(dto);
        }

        [HttpPost]
        public JsonResult Update(ProductCommissionDto dto)
        {
            var result = _productCommissionContract.Update(dto);
            return Json(result);
        }
        #endregion

        #region 查看详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            ProductCommission ProductCommission = _productCommissionContract.View(Id);
            string strStoreName = string.Empty;
            string strBrandName = string.Empty;
            string strSeasonName = string.Empty;
            if (ProductCommission != null)
            {
                Store store = _storeContract.View(ProductCommission.StoreId);
                if (store != null)
                {
                    strStoreName = store.StoreName;
                }
                Brand brand = _brandContract.View(ProductCommission.BrandId);
                if (brand != null)
                {
                    strBrandName = brand.BrandName;
                }
                Season season = _seasonContract.View(ProductCommission.SeasonId);
                if (season != null)
                {
                    strSeasonName = season.SeasonName;
                }
            }
            ViewBag.StoreName = strStoreName;
            ViewBag.SeasonName = strSeasonName;
            ViewBag.BrandName = strBrandName;
            return PartialView(ProductCommission);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int Id)
        {
            var result = _productCommissionContract.Remove(Id);
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
            var result = _productCommissionContract.Recovery(Id);
            return Json(result);
        }
        #endregion
         
    }
}