using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.StoreCollocation;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.StoreCollocation.Controllers
{
    //[License(CheckMode.Verify)]
    public class StoreController : Controller
    {

        protected readonly IInventoryContract _inventoryContract;
        protected readonly IProductContract _productContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IStoreProductCollocationContract _storeProductCollocationContract;
        protected readonly IStoreCollocationInfoContract _storeCollocationInfoContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly ISizeContract _sizeContract;
        protected readonly IProductAttributeContract _productAttributeContract;

        protected readonly IStoreNoRecommendContract _storeNoRecommendContract;

        public StoreController(IInventoryContract inventoryContract, IProductContract productContract,
           IProductOrigNumberContract productOrigNumberContract,
           ICategoryContract categoryContract,
           IStoreProductCollocationContract storeProductCollocationContract,
           IStoreCollocationInfoContract storeCollocationInfoContract,
           IBrandContract brandContract,
           IStoreContract storeContract,
           IColorContract colorContract,
           ISeasonContract seasonContract,
           ISizeContract sizeContract,
           IProductAttributeContract productAttributeContract,
           IStoreNoRecommendContract storeNoRecommendContract)
        {
            _inventoryContract = inventoryContract;
            _productContract = productContract;
            _productOrigNumberContract = productOrigNumberContract;
            _categoryContract = categoryContract;
            _storeProductCollocationContract = storeProductCollocationContract;
            _storeCollocationInfoContract = storeCollocationInfoContract;
            _brandContract = brandContract;
            _storeContract = storeContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _productAttributeContract = productAttributeContract;
            _storeNoRecommendContract = storeNoRecommendContract;
        }

        public JsonResult GetCategory()
        {
            var a = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(
                x => new
                {
                    Id = x.Id,
                    ParentId = x.ParentId == null ? 0 : x.ParentId,
                    x.CategoryName,
                    x.CategoryCode,
                    x.IconPath
                });

            return Json(new OperationResult(OperationResultType.Success, "获取成功！", a));
        }


        public JsonResult CategoryProduct(int storeId, int categoryId, int pageSize, int pageIndex)
        {
            pageIndex = pageIndex - 1;
            var bigProdNumList = _storeNoRecommendContract.StoreNoRecommends.Where(x => !x.IsDeleted && x.IsEnabled && x.StoreId == storeId).Select(x => x.BigProdNum).ToList();
            if (categoryId != 0)
            {
                var a = from inventory in _inventoryContract.Inventorys
                        join product in _productContract.Products
                        on inventory.ProductId equals product.Id
                        join b in _productOrigNumberContract.OrigNumbs
                        on product.ProductOriginNumber.Id equals b.Id
                        where !inventory.IsDeleted && inventory.IsEnabled
                        && inventory.StoreId == storeId && b.CategoryId == categoryId
                        && b.IsRecommend.Value
                        && !bigProdNumList.Contains(b.BigProdNum)
                        group b by new
                        {
                            b.ProductName,
                            b.OriginalPath,
                            b.ProductCollocationImg,
                            b.TagPrice,
                            b.Id,
                            product.BigProdNum
                        } into g
                        select new
                        {
                            ProductName = g.Key.ProductName,
                            OriginalPath = g.Key.OriginalPath,
                            ProductCollocationImg = g.Key.ProductCollocationImg,
                            TagPrice = g.Key.TagPrice,
                            Id = g.Key.TagPrice,
                            BigProdNum = g.Key.BigProdNum
                        };
                int toalCount = a.Count();
                var dataList = a.OrderByDescending(x => x.Id).Skip(pageIndex* pageSize).Take(pageSize).Select(x => new
                {
                    x.ProductName,
                    x.OriginalPath,
                    x.ProductCollocationImg,
                    x.TagPrice,
                    x.Id,
                    x.BigProdNum,
                    toalCount = toalCount
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", dataList), JsonRequestBehavior.AllowGet);
            }
            else {
                var a = from inventory in _inventoryContract.Inventorys
                        join product in _productContract.Products
                        on inventory.ProductId equals product.Id
                        join b in _productOrigNumberContract.OrigNumbs
                        on product.ProductOriginNumber.Id equals b.Id
                        where !inventory.IsDeleted && inventory.IsEnabled
                        && inventory.StoreId == storeId 
                        && b.IsRecommend.Value
                        && !bigProdNumList.Contains(b.BigProdNum)
                        group b by new
                        {
                            b.ProductName,
                            b.OriginalPath,
                            b.ProductCollocationImg,
                            b.TagPrice,
                            b.Id,
                            product.BigProdNum
                        } into g
                        select new
                        {
                            ProductName = g.Key.ProductName,
                            OriginalPath = g.Key.OriginalPath,
                            ProductCollocationImg = g.Key.ProductCollocationImg,
                            TagPrice = g.Key.TagPrice,
                            Id = g.Key.TagPrice,
                            BigProdNum = g.Key.BigProdNum
                        };
                int toalCount = a.Count();
                var dataList = a.OrderByDescending(x => x.Id).Skip(pageIndex* pageSize).Take(pageSize).Select(x => new
                {
                    x.ProductName,
                    x.OriginalPath,
                    x.ProductCollocationImg,
                    x.TagPrice,
                    x.Id,
                    x.BigProdNum,
                    toalCount = toalCount
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", dataList), JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 获取商品属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns></returns>
        public JsonResult GetProductAttributesByName(string name)
        {
            var id = _productAttributeContract.ProductAttributes.Where(x => x.AttributeName == name).Select(x => x.Id).FirstOrDefault();
            var list = _productAttributeContract.ProductAttributes.Where(x => x.ParentId == id && x.IsDeleted == false && x.IsEnabled == true)
                .Select(x => new
                {
                    Text = x.AttributeName,
                    Value = x.Id.ToString(),
                    IconPath = string.IsNullOrEmpty(x.IconPath) ? "" : x.IconPath.Replace("http://www.0-fashion.com", "")
                });
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", list));
        }

        /// <summary>
        /// 根据货号 店铺 获取货号下单品搭配信息
        /// </summary>
        /// <param name="BigProdNum"></param>
        /// <param name="stroeId"></param>
        /// <returns></returns>
        public JsonResult GetCollocationProduct(string BigProdNum, string storeId)
        {
            try
            {
                var colorList = _productContract.Products.Where(x => x.BigProdNum == BigProdNum && x.IsDeleted == false && x.IsEnabled == true)
                    .GroupBy(x => x.Color).Select(x => x.Key.ColorName).ToList();
                var spd = _storeProductCollocationContract.StoreProductCollocations.Where(x => x.IsDeleted == false && x.IsEnabled == true
                && x.CollocationName != null).ToList();
                //过滤当前 店铺搭配
                var filterList = new List<StoreProductCollocation>();
                if (!string.IsNullOrEmpty(storeId) && storeId != "0")
                {
                    foreach (var item in spd)
                    {
                        if (!string.IsNullOrEmpty(item.StoreId))
                        {
                            var arry = GetStoreNameArry(item.StoreId);
                            if (arry.Contains(storeId))
                            {
                                filterList.Add(item);
                            }
                        }
                    }
                }
                //获取当前款号下所有商品
                var product = _productContract.Products.Where(x => x.BigProdNum == BigProdNum && x.IsDeleted == false && x.IsEnabled == true
                ).ToList();
                List<object> restultList = new List<object>();
                List<object> child = new List<object>();
                foreach (var item in colorList)
                {
                    List<object> cList = new List<object>();
                    string OriginalPath = "";
                    string ProductCollocationImg = "";
                    string colorImg = _colorContract.Colors.Where(x => x.ColorName == item).Select(
                        x => x.IconPath).FirstOrDefault();
                    foreach (var collocationInfo in product)
                    {
                        var productM = _productContract.Products.Where(x => x.Id == collocationInfo.Id)
                            .FirstOrDefault();
                        if (productM.Color.ColorName == item)
                        {
                            if (productM != null)
                            {
                                //商品 主图和搭配图
                                if (!string.IsNullOrEmpty(productM.OriginalPath))
                                {
                                    OriginalPath = productM.OriginalPath;
                                }
                                if (!string.IsNullOrEmpty(productM.ProductCollocationImg))
                                {
                                    ProductCollocationImg = productM.ProductCollocationImg;
                                }
                            }
                            var collocationList = _storeCollocationInfoContract.StoreCollocationInfos.Where(x =>
                                  x.ProductOrigNumberId == productM.Id)
                            .GroupBy(x => x.StoreCollocationId)
                            .Select(x => x.Key.Value).ToList();
                            foreach (var itemA in filterList)
                            {
                                if (collocationList.Contains(itemA.Id))
                                {
                                    var collocationM = _storeProductCollocationContract.StoreProductCollocations.Where(x =>
                                     x.Id == itemA.Id).FirstOrDefault();
                                    if (collocationM != null)
                                    {
                                        var cm = new
                                        {
                                            Id = collocationM.Id,
                                            Guid = collocationM.Guid,
                                            CollocationName = collocationM.CollocationName,
                                            ThumbnailPath = collocationM.ThumbnailPath
                                        };
                                        cList.Add(cm);//相关搭配图
                                    }
                                }
                            }
                        }
                    }
                    var colorChild = new
                    {
                        colorName = item,
                        colorImg = colorImg,
                        OriginalPath = OriginalPath,
                        ProductCollocationImg = ProductCollocationImg,
                        colorChild = cList
                    };
                    child.Add(colorChild);
                }
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", child), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败！", e.Message));
            }
        }

        public JsonResult GetCollocationList()
        {
            try
            {
                string colorid = Request["colorid"];
                string season = Request["season"];
                string styles = Request["styles"];
                string situation = Request["situation"];
                string shape = Request["shape"];
                string storeId = Request["StoreId"];
                var spd = _storeProductCollocationContract.StoreProductCollocations.Where(x => x.CollocationName != ""
                && x.IsEnabled == true && x.IsDeleted == false).ToList();
                var filterList = new List<StoreProductCollocation>();
                if (!string.IsNullOrEmpty(storeId) && storeId != "0")
                {
                    foreach (var item in spd)
                    {
                        var arry = GetStoreNameArry(item.StoreId);
                        if (arry.Contains(storeId))
                        {
                            filterList.Add(item);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(colorid))
                {
                    filterList = filterByAttributes(filterList, colorid, 0);
                }
                if (!string.IsNullOrEmpty(season))
                {
                    filterList = filterByAttributes(filterList, season, 1);
                }
                if (!string.IsNullOrEmpty(styles))
                {
                    filterList = filterByAttributes(filterList, styles, 2);
                }
                if (!string.IsNullOrEmpty(situation))
                {
                    filterList = filterByAttributes(filterList, situation, 3);
                }
                if (!string.IsNullOrEmpty(shape))
                {
                    filterList = filterByAttributes(filterList, shape, 4);
                }
                var data = filterList.OrderByDescending(x => x.UpdatedTime).Select(x => new
                {
                    Id = x.Id,
                    x.Guid,
                    x.ThumbnailPath,
                    color = GetAttributes("颜色", x.Colour),
                    season = GetAttributes("季节", x.Season),
                    styles = GetAttributes("风格", x.Styles),
                    situation = GetAttributes("场合", x.Situation),
                    shape = GetAttributes("体型", x.Shape)
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", data));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败！", e.Message));
            }
        }


        public List<string> GetStoreNameArry(string storeId)
        {
            if (!string.IsNullOrEmpty(storeId))
            {
                var store = storeId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return store;
            }
            else
            {
                return null;
            }
        }

        public string GetAttributes(string name, string idStr)
        {
            string returnStr = string.Empty;
            if (!string.IsNullOrEmpty(idStr))
            {
                var id = _productAttributeContract.ProductAttributes.Where(x => x.AttributeName == name).Select(x => x.Id).FirstOrDefault();
                var list = _productAttributeContract.ProductAttributes.Where(x => x.ParentId == id && x.IsDeleted == false && x.IsEnabled == true)
                    .Select(x => new
                    {
                        name = x.AttributeName,
                        id = x.Id.ToString()
                    }).ToList();
                var idArry = idStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var item in idArry)
                {
                    var nameStr = list.Where(x => x.id == item).Select(x => x.name).FirstOrDefault();
                    returnStr += nameStr + ",";
                }
                if (idArry.Count > 0)
                {
                    returnStr = returnStr.Substring(0, returnStr.Length - 1);
                }
            }
            return returnStr;
        }

        public List<StoreProductCollocation> filterByAttributes(List<StoreProductCollocation> spList, string filterStr, int type)
        {
            List<StoreProductCollocation> list = new List<StoreProductCollocation>();

            foreach (var a in spList)
            {
                List<string> arry = new List<string>();
                switch (type)
                {
                    case 0:
                        if (!string.IsNullOrEmpty(a.Colour))
                        {
                            arry = GetStoreNameArry(a.Colour);
                        }
                        break;
                    case 1:
                        if (!string.IsNullOrEmpty(a.Season))
                        {
                            arry = GetStoreNameArry(a.Season);
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(a.Styles))
                        {
                            arry = GetStoreNameArry(a.Styles);
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(a.Situation))
                        {
                            arry = GetStoreNameArry(a.Situation);
                        }
                        break;
                    case 4:
                        if (!string.IsNullOrEmpty(a.Shape))
                        {
                            arry = GetStoreNameArry(a.Shape);
                        }
                        break;
                }

                if (arry.Contains(filterStr))
                {
                    list.Add(a);
                }
            }
            return list;
        }

        //获取搭配信息
        public JsonResult CollocationInfo(string guid)
        {
            try
            {
                var collocationInfo = _storeProductCollocationContract.StoreProductCollocations.Where(
                    x => x.Guid == guid).FirstOrDefault();
                List<object> info = new List<object>();
                if (collocationInfo != null)
                {
                    var querySouce = from item in _storeCollocationInfoContract.StoreCollocationInfos
                                     join product in _productContract.Products on item.ProductOrigNumberId equals product.Id into Joinitem
                                     from product in Joinitem.DefaultIfEmpty()
                                     where item.IsDeleted == false && item.IsEnabled == true
                                     && product.IsDeleted == false && product.IsEnabled == true
                                     && item.StoreCollocationId == collocationInfo.Id
                                     select new
                                     {
                                         product.Id,
                                         product.ProductOriginNumber.ThumbnailPath,
                                         product.ProductOriginNumber.OriginalPath,
                                         product.ProductOriginNumber.ProductCollocationImg,
                                         product.ProductNumber,
                                         product.ProductOriginNumber,
                                         product.BigProdNum,
                                         product.Size,
                                         product.Color
                                     };
                    var queryList = new List<object>();
                    foreach (var item in querySouce)
                    {
                        if (item != null)
                        {

                            var AttributList = item.ProductOriginNumber.ProductAttributes.ToList();

                            var da = new
                            {
                                Id = item.Id,
                                item.BigProdNum,
                                item.ProductNumber,
                                item.ThumbnailPath,
                                item.OriginalPath,
                                item.ProductCollocationImg,
                                styles = GetAttributArry(AttributList, "风格"),
                                season = GetAttributArry(AttributList, "季节"),
                                situation = GetAttributArry(AttributList, "场合"),
                                shape = GetAttributArry(AttributList, "体型"),
                                item.ProductOriginNumber.TagPrice
                            };
                            queryList.Add(da);
                        }
                    }
                    var data = new
                    {
                        infoId = collocationInfo.Id,
                        collocationInfo.ThumbnailPath,
                        collocationInfo.Guid,
                        child = queryList
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", data));
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", null));
                }

            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败！", e.Message));
            }
        }

        public string GetAttributArry(List<ProductAttribute> list, string AttributeName)
        {
            string name = string.Empty;
            var attribute = list.Where(x => x.AttributeName == AttributeName).FirstOrDefault();
            if (attribute != null)
            {
                foreach (var item in attribute.Children)
                {
                    name += item.AttributeName + ",";
                }
                if (attribute.Children.Count() > 0)
                {
                    name = name.Substring(0, name.Length - 1);
                }
            }
            return name;
        }


    }
}