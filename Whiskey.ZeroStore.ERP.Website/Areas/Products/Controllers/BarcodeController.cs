using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Antlr3.ST.Language;
using AutoMapper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Services.Implements;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles;
using Whiskey.ZeroStore.ERP.Website.Areas.Products.Models;
using Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using XKMath36;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Controllers
{
    public class BarcodeController : BaseController
    {
        //
        // GET: /Stores/Barcode/

        /// <summary>
        /// 网址链接
        /// </summary>
        private readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        protected readonly IProductContract _productContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IProductBarcodeDetailContract _productBarcodeDetailContract;
        protected readonly IProductBarcodePrintInfoContract _productBarcodePrintInfoContract;
        protected readonly IBarCodeConfigContract _barCodeConfigContract;
        protected readonly IAdministratorContract _administratorContract;
        protected static object objlock = new object();
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IStoreContract _StoreContract;
        public BarcodeController(IProductContract productContract, IStorageContract storageContract
            , IBrandContract brandContract, ICategoryContract categoryContract, ISeasonContract seasonContract
            , IInventoryContract inventoryContract, IProductBarcodeDetailContract productBarcodeDetailContract
            , IBarCodeConfigContract _barCodeConfigContract
            , IProductBarcodePrintInfoContract productBarcodePrintInfoContract,
            IStoreContract _StoreContract,
            IProductTrackContract productTrackContract,
            IAdministratorContract administratorContract)
        {
            _administratorContract = administratorContract;
            _productContract = productContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _seasonContract = seasonContract;
            _inventoryContract = inventoryContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _productBarcodePrintInfoContract = productBarcodePrintInfoContract;
            this._barCodeConfigContract = _barCodeConfigContract;
            _productTrackContract = productTrackContract;
            this._StoreContract = _StoreContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetProductInfoByNums()
        {

            GridRequest gr = new GridRequest(Request);
            Expression<Func<Inventory, bool>> exp = FilterHelper.GetExpression<Inventory>(gr.FilterGroup);
            GridData<object> data = new GridData<object>(new List<object>(), 0, Request);
            var _li1 = _inventoryContract.Inventorys.ToList();
            var _li = _inventoryContract.Inventorys
                 .Where(exp);
            var li =
                _inventoryContract.Inventorys
                    .Where(exp)
                    .GroupBy(x => x.Product.ProductNumber)
                    .Select(g => new {
                        g.FirstOrDefault().Id,
                        g.FirstOrDefault().Product.ProductNumber,
                        g.FirstOrDefault().Product.ProductName,
                        g.FirstOrDefault().Product.ProductOriginNumber.Brand.BrandName,
                        g.FirstOrDefault().Product.ProductOriginNumber.Category.CategoryName,
                        g.FirstOrDefault().Product.Size.SizeName,
                        g.FirstOrDefault().Product.Color.ColorName,
                        g.FirstOrDefault().Product.ThumbnailPath,
                        //Coun = g.Select(j => j.IsLock ? j.Quantity - j.LockCoun : j.Quantity).Sum()
                    }).ToList();
            data = new GridData<object>(li, li.Count, Request);
            return Json(data);
        }

        public ActionResult GetProductInfo(string[] nums)
        {
            List<string> numList = new List<string>();
            if (nums != null && nums.Any())
            {
                foreach (var _num in nums)
                {
                    if (_num.IndexOf(',') > -1)
                    {
                        numList.AddRange(_num.Trim().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                    }
                    else numList.Add(_num);
                }
            }
            var IsDefaultBrand = true;
            //var IsDefaultBrandPrice = true;
            var diyBarnd = "0FASHION";
            var modbcc = _barCodeConfigContract.BarCodeConfigs.FirstOrDefault(f => !f.IsDeleted && f.IsEnabled);
            if (modbcc.IsNotNull())
            {
                IsDefaultBrand = modbcc.IsDefaultBrand;
                if (modbcc.DIYBrand.IsNotNullAndEmpty())
                {
                    diyBarnd = modbcc.DIYBrand;
                }
                //IsDefaultBrandPrice = modbcc.IsDefaultBrandPrice;
            }
            Func<Product, dynamic> func = x =>
            {
                var prtag = x.ProductOriginNumber.ProductOriginNumberTag;
                return new
                {
                    x.Id,
                    x.ProductNumber,
                    ProductName = x.ProductName ?? "",
                    x.Color.ColorName,
                    x.Color.IconPath,
                    BrandName = IsDefaultBrand ? x.ProductOriginNumber.Brand.BrandName : diyBarnd,
                    x.Size.SizeName,
                    TagPrice = x.ProductOriginNumber.TagPrice,
                    HtmlPath = strWebUrl + "/" + x.ProductOriginNumber.HtmlPhonePath,
                    x.ProductOriginNumber.Category.CategoryName,
                    x.ThumbnailPath,
                    BigProdNum = x.ProductOriginNumber.BigProdNum,
                    Category = prtag.IsNotNull() ? prtag.Category ?? "" : "",
                    Level = prtag.IsNotNull() ? prtag.Level ?? "" : "",
                    ProductionPlace = prtag.IsNotNull() ? prtag.ProductionPlace ?? "" : "",
                    Standard = prtag.IsNotNull() ? prtag.Standard ?? "" : "",
                    Inspector = prtag.IsNotNull() ? prtag.Inspector ?? "" : "",
                    //Ingredient = prtag.IsNotNull() ? string.Format("{0} {1} {2}", prtag.Fabric, prtag.Material, prtag.batching) : ""
                    Fabric = prtag.IsNotNull() ? prtag.Fabric ?? "" : "",
                    Material = prtag.IsNotNull() ? prtag.Material ?? "" : "",
                    batching = prtag.IsNotNull() ? prtag.batching ?? "" : "",
                    Manufacturer = prtag.IsNotNull() ? prtag.Manufacturer ?? "" : "",
                    PostCode = prtag.IsNotNull() ? prtag.PostCode ?? "" : "",
                    CateName = prtag.IsNotNull() ? prtag.CateName ?? "" : "",
                };
            };

            var li = _productContract.Products.Where(c => numList.Contains(c.ProductNumber)).Select(func).ToList();
            GridData<object> da = new GridData<object>(li, li.Count, Request);
            return Json(da);
        }

        public ActionResult BarcodeView()
        {
            var defaultpaper = "0";
            var paperDirection = "0";
            var modbcc = _barCodeConfigContract.BarCodeConfigs.FirstOrDefault(f => !f.IsDeleted && f.IsEnabled);
            if (modbcc.IsNotNull())
            {
                defaultpaper = modbcc.PrinterPaperType == ERP.Models.Enums.PrinterPaperType._30_80 ? "0" : "1";
                paperDirection = modbcc.PrinterPaperDirection == ERP.Models.Enums.PrinterPaperDirection._横版 ? "0" : "1";
            }
            ViewBag.defaultpaper = defaultpaper;
            ViewBag.paperDirection = paperDirection;
            return PartialView();
        }

        public ActionResult BitchProduct()
        {
            ViewBag.storages = CacheAccess.GetOrderStorages(_storageContract, true);

            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            ViewBag.Categor = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Season = CacheAccess.GetSeason(_seasonContract, true);
            return PartialView();
        }

        public ActionResult GetBitchProduct()
        {
            GridRequest rq = new GridRequest(Request);
            Expression<Func<Inventory, bool>> exp = FilterHelper.GetExpression<Inventory>(rq.FilterGroup);
            var allprod = CacheAccess.GetAccessibleInventorys(_inventoryContract, _StoreContract).Where(exp).GroupBy(x => x.Product.ProductNumber);
            var data = allprod.Select(x => new {
                x.FirstOrDefault().Id,
                x.FirstOrDefault().Product.ProductName,
                x.FirstOrDefault().Product.ProductNumber,
                x.FirstOrDefault().Product.ThumbnailPath,
                x.FirstOrDefault().Product.ProductOriginNumber.Season.SeasonName,
                x.FirstOrDefault().Product.Color.ColorName,
                x.FirstOrDefault().Product.Size.SizeName,
                x.FirstOrDefault().Storage.StorageName,
                x.FirstOrDefault().Product.ProductOriginNumber.Brand.BrandName,
                //Quantity = x.Select(g => g.IsLock ? g.Quantity - g.LockCoun : g.Quantity).Sum()
            }).ToList();

            GridData<object> obj = new GridData<object>(data, allprod.Count(), Request);
            return Json(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBarCodePrinterInfo(BarcodeInfo[] barcodeInfos)
        {

            OperationResult resul = new OperationResult(OperationResultType.Success);
            var nums = barcodeInfos.Select(c => c.ProductNumber).ToList();
            //string absrpath = Request.Url.Authority;

            var IsDefaultBrand = true;
            //var IsDefaultBrandPrice = true;
            var diyBarnd = "0FASHION";
            var modbcc = _barCodeConfigContract.BarCodeConfigs.FirstOrDefault(f => !f.IsDeleted && f.IsEnabled);
            if (modbcc.IsNotNull())
            {
                IsDefaultBrand = modbcc.IsDefaultBrand;
                if (modbcc.DIYBrand.IsNotNullAndEmpty())
                {
                    diyBarnd = modbcc.DIYBrand;
                }
                //IsDefaultBrandPrice = modbcc.IsDefaultBrandPrice;
            }

            var li = (from c in _productContract.Products.Where(c => nums.Contains(c.ProductNumber) && c.IsEnabled && !c.IsDeleted)
                 .GroupBy(x => x.ProductNumber)
                      let p = c.FirstOrDefault()
                      select new
                      {
                          p.Id,
                          p.ProductNumber,
                          p.ProductName,
                          BrandName = IsDefaultBrand ? p.ProductOriginNumber.Brand.BrandName : diyBarnd,
                          p.ProductOriginNumber.Category.CategoryName,
                          TagPrice = c.FirstOrDefault().ProductOriginNumber.TagPrice,
                          p.Size.SizeName,
                          p.Color.ColorName,
                          HtmlPath = strWebUrl + p.ProductOriginNumber.HtmlPhonePath,
                          Enti = c.Select(g => g.BarcodePrintInfo),
                          ProductOriginNumber = p.ProductOriginNumber,
                          SizeExtention = p.Size.SizeExtention.Name,
                      }).ToList();


            List<BarcodeInfoBase> datli = new List<BarcodeInfoBase>();
            foreach (var prli in li)
            {
                ProductBarcodePrintInfo info = null;
                if (prli.Enti != null)
                    info = prli.Enti.First();
                var t = GetPrintBarcodeNumbs(info,
                      barcodeInfos.FirstOrDefault(c => c.ProductNumber == prli.ProductNumber).PrintCount);
                var prtag = prli.ProductOriginNumber.ProductOriginNumberTag;
                datli.Add(new BarcodeInfoBase()
                {
                    BarcodeNumbers = t,
                    ProductNumber = prli.ProductNumber,
                    ProductName = prli.ProductName ?? "",
                    QRcode = prli.HtmlPath ?? "",
                    BrandName = prli.BrandName,
                    CategoryName = prli.CategoryName,
                    ColorName = prli.ColorName,
                    SizeName = prli.SizeName,
                    TagPrice = prli.TagPrice,
                    BigProdNum = prli.ProductOriginNumber.BigProdNum,
                    Category = prtag.IsNotNull() ? prtag.Category ?? "" : "",
                    Level = prtag.IsNotNull() ? prtag.Level ?? "" : "",
                    ProductionPlace = prtag.IsNotNull() ? prtag.ProductionPlace ?? "" : "",
                    Standard = prtag.IsNotNull() ? prtag.Standard ?? "" : "",
                    Inspector = prtag.IsNotNull() ? prtag.Inspector ?? "" : "",
                    Fabric = prtag.IsNotNull() ? prtag.Fabric ?? "" : "",
                    Material = prtag.IsNotNull() ? prtag.Material ?? "" : "",
                    batching = prtag.IsNotNull() ? prtag.batching ?? "" : "",
                    Manufacturer = prtag.IsNotNull() ? prtag.Manufacturer ?? "" : "",
                    PostCode = prtag.IsNotNull() ? prtag.PostCode ?? "" : "",
                    CateName = prtag.IsNotNull() ? prtag.CateName ?? "" : "",
                    SizeExtention = prli.SizeExtention ?? "",
                });

            }
            resul = new OperationResult(OperationResultType.Success) { Data = datli };

            return Json(resul);
        }

        public ActionResult GetBarCodePrinterInfoOnly(BarcodeOnlyInfo boi)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error,"数据不完整");
            string absrpath = Request.Url.Authority;

            var IsDefaultBrand = true;
            //var IsDefaultBrandPrice = true;
            var diyBarnd = "0FASHION";
            var modbcc = _barCodeConfigContract.BarCodeConfigs.FirstOrDefault(f => !f.IsDeleted && f.IsEnabled);
            if (modbcc.IsNotNull())
            {
                IsDefaultBrand = modbcc.IsDefaultBrand;
                if (modbcc.DIYBrand.IsNotNullAndEmpty())
                {
                    diyBarnd = modbcc.DIYBrand;
                }
                //IsDefaultBrandPrice = modbcc.IsDefaultBrandPrice;
            }
            var curpro = _productContract.Products.Where(c => c.ProductNumber == boi.ProductNumber && c.IsEnabled && !c.IsDeleted).FirstOrDefault();

            List<BarcodeInfoBase> datli = new List<BarcodeInfoBase>();
            if (curpro.IsNotNull())
            {
                var curpon = curpro.ProductOriginNumber;
                var curtag = curpon.ProductOriginNumberTag;
                if (curpro.IsNotNull())
                {
                    foreach (var item in boi.BarcodeInfos)
                    {
                        datli.Add(new BarcodeInfoBase()
                        {
                            BarcodeNumbers = Enumerable.Range(1, item.PrintCount).ToList()
                                            .ConvertAll(c => item.ProductNumber),
                            ProductNumber = curpro.ProductNumber,
                            ProductName = curpro.ProductName ?? "",
                            QRcode = strWebUrl + curpon.HtmlPhonePath,
                            BrandName = IsDefaultBrand ? curpon.Brand.BrandName : diyBarnd,
                            CategoryName = curpon.Category.CategoryName,
                            ColorName = curpro.Color.ColorName,
                            SizeName = curpro.Size.SizeName,
                            TagPrice = curpon.TagPrice,//调当前设置的价格规则
                            //TagPrice = orgPrice.IsNotNull() ? orgPrice.PrintPrice : 0,//调打印时的价格规则
                            BigProdNum = curpon.BigProdNum,
                            Category = curtag.IsNotNull() ? curtag.Category ?? "" : "",
                            Level = curtag.IsNotNull() ? curtag.Level ?? "" : "",
                            ProductionPlace = curtag.IsNotNull() ? curtag.ProductionPlace ?? "" : "",
                            Standard = curtag.IsNotNull() ? curtag.Standard ?? "" : "",
                            Inspector = curtag.IsNotNull() ? curtag.Inspector ?? "" : "",
                            Fabric = curtag.IsNotNull() ? curtag.Fabric ?? "" : "",
                            Material = curtag.IsNotNull() ? curtag.Material ?? "" : "",
                            batching = curtag.IsNotNull() ? curtag.batching ?? "" : "",
                            Manufacturer = curtag.IsNotNull() ? curtag.Manufacturer ?? "" : "",
                            PostCode = curtag.IsNotNull() ? curtag.PostCode ?? "" : "",
                            CateName = curtag.IsNotNull() ? curtag.CateName ?? "" : "",
                            SizeExtention = curpro.Size.SizeExtention?.Name ?? string.Empty,
                        });
                    }
                    resul = new OperationResult(OperationResultType.Success) { Data = datli };
                }
            }

            return Json(resul);
        }

        /// <summary>
        /// 打印完成
        /// </summary>
        /// <returns></returns>
        public ActionResult PrintSuccess(BarcodeTem[] da, DateTime? PrintTime = null)
        {
            string[] nums = da.Select(c => c.ProductNumber).ToArray();
            var products = _productContract.Products.Where(c => nums.Contains(c.ProductNumber)).DistinctBy(c => c.ProductNumber).ToList();
            List<Product> li = new List<Product>();
            List<ProductBarcodeDetail> details = new List<ProductBarcodeDetail>();
            for (int i = 0; i < products.Count(); i++)
            {
                Product prod = products[i];
                string[] lastcode = da.FirstOrDefault(c => c.ProductNumber == prod.ProductNumber).LastCode.DistinctBy(c => c).ToArray();

                string maxcode = MaxCode(lastcode);
                prod.BarcodePrintCount += lastcode.Count();
                if (prod.BarcodePrintInfo == null)
                {
                    ProductBarcodePrintInfo barcodePrintInfo = new ProductBarcodePrintInfo()
                    {
                        CurPrintFlag = maxcode,
                        ProductNumber = prod.ProductNumber,
                        LastUpdateTime = DateTime.Now
                    };
                    // _productBarcodePrintInfoContract.Insert(true, barcodePrintInfo);
                    prod.BarcodePrintInfo = barcodePrintInfo;
                }
                else
                {
                    prod.BarcodePrintInfo.CurPrintFlag = maxcode;
                    prod.BarcodePrintInfo.LastUpdateTime = DateTime.Now;
                    // _productBarcodePrintInfoContract.Update(new ProductBarcodePrintInfo[] { prod.BarcodePrintInfo }, true);
                }
               
                Math36 math = new Math36();
                foreach (var code in lastcode)
                {
                    if (!details.Any(c => c.OnlyFlag == code && c.ProductNumber == prod.ProductNumber))
                    {
                        details.Add(new ProductBarcodeDetail()
                        {
                            ProductNumber = prod.ProductNumber,
                            OnlyFlag = code,
                            OnlfyFlagOfInt = (int)math.To10(code, 0),
                            LogFlag = Guid.NewGuid().ToString().Replace("-", ""),
                            Status = 0,
                            OperatorId = AuthorityHelper.OperatorId,
                            ProductId=prod.Id,
                            CreatedTime = PrintTime ?? DateTime.Now,
                        });
                        #region 商品追踪
                        ProductTrackDto pt = new ProductTrackDto();
                        pt.ProductNumber = prod.ProductNumber;
                        pt.ProductBarcode = prod.ProductNumber+code;
                        pt.Describe = ProductOptDescTemplate.ON_PRODUCT_PRINT;
                        pt.CreatedTime = PrintTime;
                        _productTrackContract.Insert(pt);
                        #endregion
                    }
                }
                li.Add(prod);
            }
            OperationResult resul = _productBarcodeDetailContract.Insert(details.ToArray());
            return Json(resul);
        }
        /// <summary>
        /// 获取最大的打印数
        /// </summary>
        /// <param name="lastcode"></param>
        /// <returns></returns>
        private string MaxCode(string[] lastcode)
        {
            float maxcode = 0;
            Math36 math = new Math36();
            foreach (var ite in lastcode)
            {
                float te = math.To10(ite, 0);
                if (te > maxcode)
                    maxcode = te;
            }
            return math.To36(maxcode);
        }
        /// <summary>
        /// 获取商品的打印条码的后三位
        /// </summary>
        /// <param name="product"></param>
        /// <param name="prcou">打印数量</param>
        /// <returns></returns>
        private List<string> GetPrintBarcodeNumbs(ProductBarcodePrintInfo product, int prcou)
        {
            List<string> li = new List<string>();
            string starFlg = "0";
            if (product != null)
            {
                starFlg = product.CurPrintFlag;
            }
            lock (objlock)
            {
                string _key = "cur_barcode_flgnum_1109";
                var tecurNum = CacheAccess.Get(_key) as string;
                if (!string.IsNullOrEmpty(tecurNum))
                {
                    starFlg = tecurNum;
                }
            }
            Math36 math = new Math36();
            int curstar = (int)(math.To10(starFlg, 0));
            for (int i = 0; i < prcou; i++)
            {
                curstar += 1;
                li.Add((math.To36(curstar)).PadLeft(3, '0'));
            }
            lock (objlock)
            {
                string _key = "cur_barcode_flgnum_1109";
                CacheAccess.Set(_key, curstar, 2);
            }
            return li;
        }


    }
}