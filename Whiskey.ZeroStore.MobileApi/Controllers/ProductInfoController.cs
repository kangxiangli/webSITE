using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class ProductInfoController : Controller
    {
        private readonly IInventoryContract _inventoryService;
        private readonly IProductContract _productService;
        public static readonly string HostUrlPrefix = ConfigurationHelper.GetAppSetting("WebUrl");
        public ProductInfoController(IInventoryContract s1, IProductContract s2)
        {
            _inventoryService = s1;
            _productService = s2;
        }


        [HttpPost]
        public ActionResult GetInventory(string barcode)
        {
            barcode = barcode.ToUpper();
            var bigProdNum = string.Empty;
            try
            {
                Inventory inventoryEntity = null;
                if (string.IsNullOrEmpty(barcode) || barcode.Length < 7)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误"));
                }

                //验证库存
                inventoryEntity = _inventoryService.Inventorys.FirstOrDefault(i => !i.IsDeleted && i.IsEnabled && i.ProductBarcode == barcode);
                if (inventoryEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "库存不存在"));
                }
                //获取product list
                bigProdNum = _productService.Products
                                .Where(p => !p.IsDeleted && p.IsEnabled && p.Id == inventoryEntity.ProductId)
                                .Select(p => p.BigProdNum).FirstOrDefault();

                var productEntity = inventoryEntity.Product;


                var productList = _productService.Products.Where(p => !p.IsDeleted && p.IsEnabled && p.BigProdNum == bigProdNum).ToList();
                var productIds = productList.Select(p => p.Id).ToList();

                // 获取缩略图
                var img = string.Empty;

                if (!string.IsNullOrEmpty(productEntity.ProductCollocationImg))
                {
                    img = HostUrlPrefix + productEntity.ProductCollocationImg;
                }
                else
                {
                    var originNumberEntity = productList.First().ProductOriginNumber;
                    img = string.IsNullOrEmpty(originNumberEntity.ProductCollocationImg) ? string.Empty : HostUrlPrefix + originNumberEntity.ProductCollocationImg;
                }



                //获取库存list
                var inventoryList = _inventoryService.Inventorys.Where(p => !p.IsDeleted && p.IsEnabled)
                    .Where(p => !p.IsLock && p.Status == (int)InventoryStatus.Default)
                    .Where(p => productIds.Contains(p.ProductId)).ToList();
                var groupData = inventoryList.GroupBy(i => new { i.Store.StoreName, i.Product.Color.IconPath, i.Product.Size.SizeName })
                    .Select(g => new
                    {
                        g.Key.StoreName,
                        g.Key.IconPath,
                        g.Key.SizeName,
                        Count = g.Count()
                    }).ToList()
                    .OrderBy(g => g.StoreName)
                    .ThenBy(g => g.SizeName)
                    .Select(g => new
                    {
                        g.StoreName,
                        IconPath = HostUrlPrefix + g.IconPath,
                        g.SizeName,
                        g.Count
                    }).ToList();

                // 组装数据
                var res = new
                {
                    img = img,
                    bigProdNum = bigProdNum,
                    data = groupData
                };
                return Json(new OperationResult(OperationResultType.Success, string.Empty, res));

            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }
    }
}