using System;
using System.Collections;
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
using System.Web.UI;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Website.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Web.Script.Serialization;
using YxkSabri;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class TransferController : BaseController
    {

        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(BuyController));

        private readonly IProductContract _productContract;
        private readonly IColorContract _colorContract;
        private readonly IProductBarcodeDetailContract _productBarcodeDetailContract;
        private readonly IInventoryContract _inventoryContract;

        public TransferController(IProductContract productContract, IColorContract colorContract, IProductBarcodeDetailContract productBarcodeDetailContract, IInventoryContract inventoryContract)
        {
            _productContract = productContract;
            _colorContract = colorContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _inventoryContract = inventoryContract;
        }


        [Layout]
        public ActionResult Index()
        {
            ViewBag.StoreList = new List<SelectListItem>();
            ViewBag.ScanValidCount = (Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>()).Sum(m => m.Amount);
            ViewBag.ScanInvalidCount = (Session["ScanInvalid"] != null ? (List<Product_Model>)Session["ScanInvalid"] : new List<Product_Model>()).Sum(m => m.Amount);
            return View();
        }


        public ActionResult Create(PurchaseDto model)
        {
            var result = new OperationResult(OperationResultType.Error, "保存采购订单出错！");
            var list = Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>();

            if (list.Count > 0)
            {

            }
            else
            {

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 商品入库时校验
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult AddToScan(string uuid, string number)
        {

            var result = new OperationResult(OperationResultType.Error, "");
            if (Session["ScanValid"] == null) Session["ScanValid"] = new List<Product_Model>();
            if (Session["ScanInvalid"] == null) Session["ScanInvalid"] = new List<Product_Model>();

            if (number != null && number.Length > 0)
            {
                try
                {
                    var validItem = new Product_Model();
                    var validList = (List<Product_Model>)Session["ScanValid"];
                    var invalidList = (List<Product_Model>)Session["ScanInvalid"];
                    number = InputHelper.SafeInput(number);
                    #region MyRegion
                    if (!validList.Any(m => m.ProductBarcode == number))
                    {
                        string lastcode = number.Substring(number.Length - 3);
                        string _number = number.Substring(0, number.Length - 3);


                        var entity =
                            _productBarcodeDetailContract.productBarcodeDetails.FirstOrDefault(
                                c =>
                                    c.IsEnabled && !c.IsDeleted && c.ProductNumber.ToUpper() == _number.ToUpper() &&
                                    c.OnlyFlag == lastcode);
                        //Product entity = _productContract.Products.Where(m => !m.IsDeleted && m.IsVerified  && m.IsEnabled).FirstOrDefault(m => m.ProductNumber.ToUpper() == number.ToUpper()).ProductBarcodeDetails.Where();

                        if (entity != null)
                        {
                            if (entity.Status != 0)
                            {
                                invalidList.Add(new Product_Model()
                                {
                                    UUID = uuid,
                                    ProductBarcode = number.ToUpper(),
                                    Notes = "此商品已经入库或者已经被禁用"
                                });
                                result = new OperationResult(OperationResultType.Error, "此商品已经入库或者已经被禁用！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });
                            }
                            else
                            {

                                validItem = validList.FirstOrDefault(m => m.ProductBarcode.ToUpper() == number.ToUpper());
                                if (validItem != null)
                                {
                                    validItem.Amount++;
                                    validItem.UpdateTime = DateTime.Now;
                                }
                                else
                                {
                                    var col = "";
                                    if (entity.Product != null)
                                    {
                                        Product product = entity.Product;
                                        col = product.Color.ColorName;
                                        //var colent = _colorContract.Colors.Where(c => c.Id == entity.ColorId).FirstOrDefault();
                                        //if (colent != null)
                                        //    col = colent.ColorName;


                                        validList.Add(new Product_Model
                                        {
                                            Id = product.Id,
                                            UUID = uuid,
                                            Thumbnail = product.ThumbnailPath??product.ProductOriginNumber.ThumbnailPath,
                                            ProductNumber = entity.ProductNumber.ToUpper(),
                                            ProductBarcode = number,
                                            TagPrice = product.ProductOriginNumber.TagPrice,
                                            WholesalePrice = product.ProductOriginNumber.WholesalePrice,
                                            Season = product.ProductOriginNumber.Season.SeasonName,
                                            Size = product.Size.SizeName,
                                            Brand = product.ProductOriginNumber.Brand.BrandName,
                                            Category = product.ProductOriginNumber.Category.CategoryName,
                                            Color = col,
                                            ProductName = product.ProductName
                                        });
                                    }
                                    else
                                    {//在商品记录表中不存在该商品
                                        invalidList.Add(new Product_Model
                                        {
                                            UUID = uuid,
                                            ProductBarcode = number.ToUpper(),
                                            Notes = "商品记录不存在"
                                        });
                                        result = new OperationResult(OperationResultType.Error, "商品记录不存在！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });
                                    }
                                }
                                Session["ScanValid"] = validList;
                            }
                        }
                        else
                        {
                            var invalidItem = invalidList.FirstOrDefault(m => m.ProductNumber.ToUpper() == number.ToUpper());
                            if (invalidItem != null)
                            {
                                invalidItem.Amount++;
                                invalidItem.UpdateTime = DateTime.Now;
                            }
                            else
                            {
                                invalidList.Add(new Product_Model
                                {
                                    UUID = uuid,
                                    ProductBarcode = number.ToUpper(),
                                    Notes = "商品不存在"
                                });
                            }
                            Session["ScanInvalid"] = invalidList;
                        }

                        result = new OperationResult(OperationResultType.Success, "产品已进入缓存列表！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });

                    }
                    #endregion
                    else
                    {


                        invalidList.Add(new Product_Model()
                        {
                            UUID = uuid,
                            ProductBarcode = number.ToUpper(),
                            Notes = "此商品一维码已进入缓存列表"
                        });
                        Session["ScanInvalid"] = invalidList;
                        result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：此商品一维码已存在：" + number + "，不允许重复提交！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });
                    }

                    var resulData = validList.Where(c => c.ProductBarcode.ToUpper() == number.ToUpper()).FirstOrDefault();
                    if (resulData != null)
                    {
                        result.Other = new JavaScriptSerializer().Serialize(resulData);
                    }

                }
                catch (Exception ex)
                {
                    result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：" + ex.Message, ex.ToString());
                }
            }
            else
            {
                result = new OperationResult(OperationResultType.Error, "扫码货号不能为空！");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 入库校验
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult InputStorageCheck(string uuid, string number)
        {

            Dictionary<string, string> dicpre = new Dictionary<string, string>();
            dicpre.Add(uuid, number);
            BatchInputStorageCheck(dicpre);

            var scanValidKey = "ScanValid";
            var scanInvalidKey = "ScanInvalid";
            List<Product_Model> validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
            List<Product_Model> inValidModels = SessionAccess.Get(scanInvalidKey) as List<Product_Model>;

            OperationResult resul = new OperationResult(OperationResultType.Success);
            resul.Other = new {
                uuid,
                validCoun = validModels.Count,
                inValidCoun = inValidModels.Count
            };
            return Json(resul);
        }
        /// <summary>
        /// 批量导入页面分页获取excel的数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBatchImportExcelData()
        {
            GridRequest gr = new GridRequest(Request);

            var _key = "sess_excel_inpor_instorage_11";
            var dat = SessionAccess.Get(_key) as List<List<string>>;
            GridData<object> da = new GridData<object>(new List<object>(), 0, Request);
            if (dat != null)
            {
                var te = dat.Select(c => new {
                    Barcode = c[0],
                    RowInd = Convert.ToInt32(c[1]),
                }).ToList();

                var li =
                    te.OrderBy(c => c.RowInd).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize).Select(c => new {
                        ProductBarcode = c.Barcode,
                        c.RowInd
                    }
                        ).ToList();
                da = new GridData<object>(li, dat.Count, Request);
            }
            return Json(da);
        }
        public ActionResult ExcelBatchStrageCheck()
        {
            var _key = "sess_excel_inpor_instorage_11";
            var dat = SessionAccess.Get(_key) as List<List<string>>;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (dat != null && dat.Any())
            {
                foreach (var li in dat)
                {
                    var te = li[0];
                    Guid guid = Guid.NewGuid();
                    var key = guid.ToString();
                    dic.Add(key, te);
                }
            }
            BatchInputStorageCheck(dic, true);
            var scanValidKey = "ScanValid";
            var scanInvalidKey = "ScanInvalid";
            List<Product_Model> validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
            List<Product_Model> inValidModels = SessionAccess.Get(scanInvalidKey) as List<Product_Model>;

            OperationResult resul = new OperationResult(OperationResultType.Success);
            resul.Other = new {
                validCoun = validModels.Count,
                inValidCoun = inValidModels.Count
            };
            SessionAccess.Remove(_key);
            return Json(resul);
        }
        /// <summary>
        /// 获取基于缓存的将要入库的分页数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInputStorageCheckData()
        {
            GridRequest gr = new GridRequest(Request);
            var exp = FilterHelper.GetExpression<Product_Model>(gr.FilterGroup);

            var scanValidKey = "ScanValid";
            List<Product_Model> validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
            if (validModels == null) validModels = new List<Product_Model>();
            List<object> da = new List<object>();
            if (validModels.Any())
            {
                var codes = validModels.OrderBy(c => c.Id).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize).Select(c => c.ProductBarcode);

                da = _productBarcodeDetailContract.productBarcodeDetails.Where(c => codes.Contains(c.ProductNumber + c.OnlyFlag)).Select(c => new {
                    c.Product.Id,
                    c.ProductNumber,
                    Barcode = c.ProductNumber + c.OnlyFlag,
                    ThumbnailPath = c.Product.ThumbnailPath??c.Product.ProductOriginNumber.ThumbnailPath,
                    c.Product.ProductOriginNumber.Brand.BrandName,
                    c.Product.ProductOriginNumber.Category.CategoryName,
                    c.Product.Size.SizeName
                }).ToList().Select(c => new {
                    c.Id,
                    Pind = (validModels.FirstOrDefault(g => g.ProductBarcode == c.Barcode)) == null ? 0 : validModels.FirstOrDefault(g => g.ProductBarcode == c.Barcode).Id,
                    c.ProductNumber,
                    c.Barcode,
                    c.ThumbnailPath,
                    c.BrandName,
                    c.CategoryName,
                    c.SizeName,
                }).OrderBy(c => c.Pind).ToList<object>();
            }
            GridData<object> resda = new GridData<object>(da, validModels.Count, Request);
            return Json(resda);
        }
        [HttpPost]
        public ActionResult RemoveInstoraggeByCach(string[] barcodes)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);

            var scanValidKey = "ScanValid";
            var scanInvalidKey = "ScanInvalid";
            List<Product_Model> validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
            List<Product_Model> inValidModels = SessionAccess.Get(scanInvalidKey) as List<Product_Model>;

            if (validModels != null && validModels.Any())
            {
                var ts = validModels.Where(c => barcodes.Contains(c.ProductBarcode)).ToList();
                for (int i = 0; i < ts.Count(); i++)
                {
                    validModels.Remove(ts[i]);
                }

                SessionAccess.Set(scanValidKey, validModels, true);


                //从错误列表中也移除该货号
                if (inValidModels != null && inValidModels.Any())
                {
                    var list = inValidModels.Where(c => barcodes.Contains(c.ProductBarcode)).ToList();
                    for (int i = 0; i < list.Count(); i++)
                    {
                        inValidModels.Remove(list[i]);
                    }

                    SessionAccess.Set(scanInvalidKey, inValidModels, true);
                    
                }

                resul = new OperationResult(OperationResultType.Success);

                resul.Other = new {
                    validCoun = validModels.Count,
                    inValidCoun = inValidModels.Count
                };
            }

            


            return Json(resul);
        }
        /// <summary>
        /// 入库数据批量校验
        /// </summary>
        /// <param name="pre"></param>
        private void BatchInputStorageCheck(Dictionary<string, string> pre, bool isclear = false)
        {
            var scanValidKey = "ScanValid";
            var scanInvalidKey = "ScanInvalid";
            List<string> errli = new List<string>();

            List<Product_Model> validModels = new List<Product_Model>();
            List<Product_Model> inValidModels = new List<Product_Model>();
            if (!isclear)
            {
                validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
                inValidModels = SessionAccess.Get(scanInvalidKey) as List<Product_Model>;
                if (validModels == null)
                    validModels = new List<Product_Model>();
                if (inValidModels == null)
                    inValidModels = new List<Product_Model>();
            }

            List<string> numbs = pre.Select(c => c.Value).ToList();
            List<string> valied = new List<string>();

            //是否与已校验通过的结果重复
            var exisVali = validModels.Where(c => numbs.Contains(c.ProductBarcode)).ToList();
            //是否与校验不通过的结果重复
            var exisInvali = inValidModels.Where(c => numbs.Contains(c.ProductBarcode)).ToList();
            int cuind = inValidModels.Count + validModels.Count + 1;
            if (exisVali.Any())
            {
                valied.AddRange(exisVali.Select(c => c.ProductBarcode));
                for (int i = 0; i < exisVali.Count(); i++)
                {
                    var ite = exisVali[0];
                    var t = CacheAccess.Clone<Product_Model>(ite);
                    t.Id = cuind;
                    t.Notes = "已进入缓存队列";
                    inValidModels.Add(t);
                }
                var exiscodes = exisVali.Select(c => c.ProductBarcode).ToList();
                numbs.RemoveAll(c => exiscodes.Contains(c));
            }
            else if (exisInvali.Any())
            {
                valied.AddRange(exisInvali.Select(c => c.ProductBarcode));
                for (int i = 0; i < exisInvali.Count(); i++)
                {
                    var ite = exisInvali[i];
                    var t = CacheAccess.Clone<Product_Model>(ite);
                    t.Id = cuind;
                    t.Notes += "，且已经重复";
                    inValidModels.Add(t);
                }

                var exiscodes = exisVali.Select(c => c.ProductBarcode).ToList();
                numbs.RemoveAll(c => exiscodes.Contains(c));
            }

            var plbarcode = numbs.Where(c => !valied.Contains(c)).ToList();//没有经过校验的条码
            if (plbarcode.Any())
            {
                //商品的打印记录
                var vadali = _productBarcodeDetailContract.productBarcodeDetails.Where(c => plbarcode.Contains(c.ProductNumber + c.OnlyFlag));
                //根据条码得到编号
                var pnums = numbs.Where(c => c.Length == 14).Select(c => c.Substring(0, 11)).ToList();
                //存在商品檔案的库存
                var exisnum = _productContract.Products.Where(c => pnums.Contains(c.ProductNumber))
                      .Select(c => c.ProductNumber)
                      .ToList();
                //入库校验
                foreach (var inda in plbarcode)
                {
                    //序列号
                    var ind = inValidModels.Count + validModels.Count + 1;
                    if (validModels.Any(c => c.ProductBarcode == inda))
                    {
                        var exc = validModels.FirstOrDefault(c => c.ProductBarcode == inda);
                        if (exc != null)
                        {
                            var t = CacheAccess.Clone<Product_Model>(exc);
                            t.Id = ind;
                            t.Notes = "已进入缓存队列";
                            inValidModels.Add(t);
                        }
                    }
                    else if (inValidModels.Any(c => c.ProductBarcode == inda))
                    {
                        var exc = inValidModels.FirstOrDefault(c => c.ProductBarcode == inda);
                        if (exc != null)
                        {
                            var te = CacheAccess.Clone<Product_Model>(exc);
                            te.Id = ind;
                            te.Notes += "，且已经重复";
                            inValidModels.Add(te);
                        }
                        var exiscodes = exisVali.Select(c => c.ProductBarcode).ToList();
                        numbs.RemoveAll(c => exiscodes.Contains(c));
                    }
                    else
                    {
                        //带校验对象
                        var di = pre.FirstOrDefault(c => c.Value == inda);
                        if (inda.Length == 14)
                        {
                            var prnum = inda.Substring(0, 11);
                            //打印记录
                            var barcode = vadali.FirstOrDefault(c => c.ProductNumber + c.OnlyFlag == inda);
                            //商品档案
                            var detai = exisnum.FirstOrDefault(c => c == prnum);

                            if (detai != null)
                            {
                                if (barcode != null)
                                {
                                    if (barcode.IsDeleted)
                                    {
                                        inValidModels.Add(new Product_Model
                                        {
                                            Id = ind,
                                            UUID = di.Key,
                                            ProductBarcode = di.Value,
                                            Notes = "商品档案存在,且有打印记录,但已经被移除到回收站"
                                        });
                                    }
                                    else
                                    {
                                        if (barcode.Status == 0)
                                        {
                                            validModels.Add(new Product_Model
                                            {
                                                Id = ind,
                                                UUID = di.Key,
                                                ProductBarcode = di.Value,
                                                Notes = "商品档案存在,且有打印记录,可以入库"
                                            });
                                        }
                                        else
                                        {
                                            string err = barcode.Status == 1 ? "已入库" : "已删除或禁用";
                                            inValidModels.Add(new Product_Model
                                            {
                                                Id = ind,
                                                UUID = di.Key,
                                                ProductBarcode = di.Value,
                                                Notes = err
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    inValidModels.Add(new Product_Model
                                    {
                                        Id = ind,
                                        UUID = di.Key,
                                        ProductBarcode = di.Value,
                                        Notes = "商品档案存在,但是没有打印记录"
                                    });
                                }
                            }
                            else
                            {
                                inValidModels.Add(new Product_Model
                                {
                                    Id = ind,
                                    UUID = di.Key,
                                    ProductBarcode = di.Value,
                                    Notes = "商品档案不存在"
                                });
                            }
                        }
                        else
                        {
                            inValidModels.Add(new Product_Model
                            {
                                Id = ind,
                                UUID = di.Key,
                                ProductBarcode = di.Value,
                                Notes = "录入的条码不符合14位数"
                            });
                        }
                    }
                }
            }

            SessionAccess.Set(scanValidKey, validModels, true);
            SessionAccess.Set(scanInvalidKey, inValidModels, true);
        }

        public ActionResult SetAmount(string name, int value)
        {
            if (name != null && name.Length > 0)
            {
                var validList = (Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>());
                var entity = validList.FirstOrDefault(m => m.ProductBarcode.ToUpper() == name.ToUpper());
                if (entity != null)
                {
                    entity.Amount = value;
                    entity.UpdateTime = DateTime.Now;
                }
                Session["ScanValid"] = validList;
                return Json(new OperationResult(OperationResultType.Success, "商品数量已修改！", new { validCount = validList.Sum(m => m.Amount) }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error, "商品货号及数量不能为空！"), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ExceptionDataExportToExcel()
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            var scanValidKey = "ScanValid";
            var scanInvalidKey = "ScanInvalid";

            List<Product_Model> validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
            List<Product_Model> inValidModels = SessionAccess.Get(scanInvalidKey) as List<Product_Model>;

            List<List<string>> validDa = new List<List<string>>();
            List<List<string>> inValidDa = new List<List<string>>();
            if (validModels != null && validModels.Any())
            {
                foreach (var valda in validModels)
                {
                    validDa.Add(new List<string>() { valda.Id.ToString(), valda.ProductBarcode });
                }
            }
            if (inValidModels != null && inValidModels.Any())
            {
                foreach (var valda in inValidModels)
                {
                    inValidDa.Add(new List<string>() { valda.Id.ToString(), valda.ProductBarcode, valda.Notes });
                }
            }
            List<List<List<string>>> li = new List<List<List<string>>>();
            li.Add(validDa);
            li.Add(inValidDa);

            YxkSabri.ExcelUtility excel = new ExcelUtility();
            var name = DateTime.Now.ToString("yyyyMMddHHff") + ".xls";
            string basedir = Server.MapPath(@"\Content\UploadFiles\Excels\ExportDa\");
            if (Directory.Exists(basedir))
                Directory.Delete(basedir, true);
            Directory.CreateDirectory(basedir);


            string path = basedir + name;
            var rsul = excel.ExportMulitExcelSheet(li, path,new string[]{"有效","无效"});
            if (rsul)
                result = new OperationResult(OperationResultType.Success);

            return File(path, "application/ms-excel", "导出数据.xls");


            //// string path = "D:/test/销售月报表.xlsx";
            // FileStream fs = new FileStream(path, FileMode.Open);
            // byte[] buffer = new byte[fs.Length];
            // fs.Read(buffer, 0, buffer.Length);

            // fs.Close();
            //System.IO.File.Delete(path);

            // Response.ContentType = "application/ms-excel";
            // Response.Charset = "GB2312";
            // Response.ContentEncoding = System.Text.Encoding.UTF8;

            // Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode("销售月报表.xlsx"));
            // Response.OutputStream.Write(buffer, 0, buffer.Length);
            // Response.Flush();
            // Response.End();


            //  return Json(result);

        }
        public ActionResult Invalid()
        {
            var entities = (Session["ScanInvalid"] != null ? (List<Product_Model>)Session["ScanInvalid"] : new List<Product_Model>());
            return PartialView(entities);
        }
        public ActionResult List(DataTable_Model param)
        {

            var list = Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>();
            var count = list.Count();

            if (param.sSearch != null && param.sSearch.Length > 0)
            {
                list = list.FindAll(m => (m.ProductNumber.ToUpper() + m.Color + m.Size).Contains(param.sSearch));
            }

            Reverser<Product_Model> reverser;
            var columns = param.sColumns.Split(',');
            if (columns.Length > 0)
            {
                var sortColumn = columns[param.iSortCol_0];
                if (sortColumn != null)
                {
                    var sortDirection = param.sSortDir_0;

                    if (sortDirection == "asc")
                    {
                        reverser = new Reverser<Product_Model>((new Product_Model()).GetType(), sortColumn, ReverserInfo.Direction.ASC);
                    }
                    else
                    {
                        reverser = new Reverser<Product_Model>((new Product_Model()).GetType(), sortColumn, ReverserInfo.Direction.DESC);
                    }

                    list.Sort(reverser);
                }

            }
            var data = param.iDisplayLength > 0 ? list.Skip(param.iDisplayStart).Take(param.iDisplayLength) : list.Skip(param.iDisplayStart);

            return Json(new {
                sEcho = param.sEcho,
                iDisplayStart = param.iDisplayStart,
                iTotalRecords = count,
                iTotalDisplayRecords = count,
                aaData = data
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            if (Id != null && Id.Length > 0)
            {
                var validList = (Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>());
                validList.RemoveAll(m => Id.Contains(Convert.ToInt32(m.Id)));
                Session["ScanValid"] = validList;
                return Json(new OperationResult(OperationResultType.Success, "采购商品移除成功！", new { validCount = validList.Sum(m => m.Amount) }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error, "移除ID列表不能为空！"), JsonRequestBehavior.AllowGet);
            }
        }
    }
}