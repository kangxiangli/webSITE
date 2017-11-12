using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [License(CheckMode.Verify)]
    public class ShoppingCartController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreTypeController));

        protected readonly IShoppingCartItemContract _shoppingCartItemContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IColorContract _colorContract;

        public ShoppingCartController(
            IShoppingCartItemContract shoppingCartItemContract,
            ICategoryContract categoryContract,
            IBrandContract brandContract,
            IColorContract colorContract
            )
        {
            _shoppingCartItemContract = shoppingCartItemContract;
            _categoryContract = categoryContract;
            _brandContract = brandContract;
            _colorContract = colorContract;
        }

   
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }

        [Layout]
        public ActionResult Index()
        {
            ViewBag.Categories = _categoryContract.ParentSelectList("请选择品类").ToList();
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true);
            ViewBag.Colors = CacheAccess.GetColorsName(_colorContract, true);
            return View();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? categoryId, int? brandId, int? colorId)
        {
          
            GridRequest request = new GridRequest(Request);
            Expression<Func<ShoppingCartItem, bool>> predicate = FilterHelper.GetExpression<ShoppingCartItem>(request.FilterGroup);
            int count = 0;
            var query = _shoppingCartItemContract.Entities;
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Product.ProductOriginNumber.Category.ParentId.Value == categoryId.Value);
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.Product.ProductOriginNumber.BrandId == brandId.Value);
            }

            if (colorId.HasValue)
            {
                query = query.Where(p => p.Product.ColorId == colorId.Value);
            }
            var list = query.Where<ShoppingCartItem, int>(predicate, request.PageCondition, out count)
                .Select(s => new
                {
                    s.Id,
                    s.MemberId,
                    s.Member.RealName,
                    s.ProductId,
                    s.ProductNumber,
                    s.Quantity,
                    s.Product.ProductOriginNumber.TagPrice,
                    s.Product.ProductOriginNumber.Brand.BrandName,
                    s.Product.ProductOriginNumber.Category.CategoryName,
                    s.Product.Color.ColorName,
                    s.Product.Size.SizeName,
                    ThumbnailPath = WebUrl + s.Product.ThumbnailPath ?? s.Product.ProductOriginNumber.ThumbnailPath,
                    s.CreatedTime
                })
                .ToList()
                .Select(s => new 
                {
                    Id = s.Id,
                    MemberId = s.MemberId,
                    RealName = s.RealName,
                    ProductId = s.ProductId,
                    ProductNumber = s.ProductNumber,
                    Quantity = s.Quantity,
                    TagPrice = (decimal)s.TagPrice,
                    ThumbnailPath = s.ThumbnailPath,
                    ColorName = s.ColorName,
                    SizeName = s.SizeName,
                    BrandName = s.BrandName,
                    CategoryName = s.CategoryName,
                    CreatedTime = s.CreatedTime.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


    }
}