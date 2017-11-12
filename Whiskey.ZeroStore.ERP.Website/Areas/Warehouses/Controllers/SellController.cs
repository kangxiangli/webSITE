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
using Whiskey.ZeroStore.ERP.Website.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class SellController : Controller
    {
        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(BuyController));

        private readonly IProductContract _productContract;

        public SellController(IProductContract productContract)
        {
            _productContract = productContract;
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
                    if (validList.Where(m => m.UUID == uuid).Count() == 0)
                    {
                        var entity = _productContract.Products.Where(m => m.IsDeleted == false && m.ProductOriginNumber.IsVerified == CheckStatusFlag.通过 && m.IsEnabled == true).FirstOrDefault(m => m.ProductNumber == number);
                        if (entity != null)
                        {
                            validItem = validList.FirstOrDefault(m => m.ProductNumber == entity.ProductNumber);
                            if (validItem != null)
                            {
                                validItem.Amount++;
                                validItem.UpdateTime = DateTime.Now;
                            }
                            else
                            {
                                validList.Add(new Product_Model
                                {
                                    Id = entity.Id,
                                    UUID = uuid,
                                    Thumbnail = entity.ThumbnailPath,
                                    ProductNumber = entity.ProductNumber,
                                    TagPrice = entity.ProductOriginNumber.TagPrice,
                                    WholesalePrice = entity.ProductOriginNumber.WholesalePrice,
                                    Season = entity.ProductOriginNumber.Season.SeasonName,
                                    Size = entity.Size.SizeName,
                                });
                            }
                            Session["ScanValid"] = validList;
                        }
                        else
                        {
                            var invalidItem = invalidList.FirstOrDefault(m => m.ProductNumber == number);
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
                                    ProductNumber = number
                                });
                            }
                            Session["ScanInvalid"] = invalidList;
                        }


                        result = new OperationResult(OperationResultType.Success, "产品已进入缓存列表！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });

                    }
                    else
                    {

                        result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：此UUID已存在，不允许重复提交！");

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

        public ActionResult SetAmount(string name, int value)
        {
            if (name != null && name.Length > 0)
            {
                var validList = (Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>());
                var entity = validList.FirstOrDefault(m => m.ProductNumber == name);
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
                list = list.FindAll(m => (m.ProductNumber + m.Color + m.Size).Contains(param.sSearch));
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

            return Json(new
            {
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