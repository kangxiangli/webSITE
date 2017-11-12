using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime.Tree;
using Microsoft.Ajax.Utilities;
using Whiskey.Utility.Filter;

using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.Logs;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Logs
{
    public class ProductOperationLogController : Controller
    {
        //
        // GET: /Logs/ProductOperationLog/
        protected readonly IProductOperationLogContract _productOperationLogContract;
        protected readonly IProductContract _productContract;
        protected readonly IProductBarcodeDetailContract _productBarcodeDetailContract;

        protected readonly IInventoryContract _inventoryContract;
        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IRetailInventoryContract _retailInventoryContract;
        protected readonly IRetailContract _retailContract;
        protected readonly IReturnedItemContract _returnedItemContract;
        protected readonly IReturnedContract _returnedContract;
        protected readonly IOrderblankContract _orderblankContract;
        protected readonly IOrderblankItemContract _orderblankItemContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IAdministratorContract _administratorContract;
        public ProductOperationLogController(IProductOperationLogContract productOperationLogContrac, IProductContract productContract, IProductBarcodeDetailContract productBarcodeDetailContract,
            IInventoryContract inventoryContract,
            IRetailItemContract retailItemContract,
            IRetailInventoryContract retailInventoryContract,
            IRetailContract retailContract,
            IReturnedItemContract returnedItemContract,
            IReturnedContract returnedContract,
            IOrderblankContract orderblankContract,
            IOrderblankItemContract orderblankItemContract,
            IStorageContract storageContract,
            IStoreContract storeContract,
            IMemberContract memberContract,
            IAdministratorContract administratorContract)
        {
            _productOperationLogContract = productOperationLogContrac;
            _productContract = productContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _inventoryContract = inventoryContract;
            _retailItemContract = retailItemContract;
            _retailInventoryContract = retailInventoryContract;
            _retailContract = retailContract;
            _returnedItemContract = returnedItemContract;
            _returnedContract = returnedContract;
            _orderblankContract = orderblankContract;
            _orderblankItemContract = orderblankItemContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _memberContract = memberContract;
            _administratorContract = administratorContract;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductLogDetails(string num)
        {
            ViewBag.proNum = num;
            return PartialView();
        }

        public ActionResult GetProductLogByNumber()
        {
            GridRequest req = new GridRequest(Request);
            Expression<Func<ProductOperationLog, bool>> exp =
                FilterHelper.GetExpression<ProductOperationLog>(req.FilterGroup);
            var alllogs = _productOperationLogContract.ProductLogs.Where(exp);
            var li = alllogs.OrderByDescending(c => c.CreatedTime).ThenByDescending(c => c.Id)
                .Skip(req.PageCondition.PageIndex)
                .Take(req.PageCondition.PageSize).Select(c => new
                {
                    c.Id,
                    c.ProductNumber,
                    c.Description,
                    LogFlag = c.LogFlag ?? "",
                    AdminName = c.Operator.Member.MemberName ?? "",
                    c.CreatedTime,
                    MaxFlag = c.ProdutBarcodeDetails.OrderByDescending(g => g.OnlfyFlagOfInt).Select(t => t.OnlyFlag).FirstOrDefault() ?? ""
                }).ToList();
            GridData<object> objda = new GridData<object>(li, alllogs.Count(), Request);
            return Json(objda);
        }
        [HttpPost]
        public ActionResult GetProductLogByProductId(int id)
        {
            string productNum = _productContract.Products.Where(c => c.Id == id).Select(c => c.ProductNumber).FirstOrDefault();
            var logli = _productOperationLogContract.ProductLogs.Where(c => c.ProductNumber == productNum).Select(c => new
            {
                c.ProductNumber,
                c.LogFlag,
                c.Description
            }).ToList();

            return Json(logli);

        }

        [HttpPost]
        public ActionResult GetBarcodesByNum()
        {
            try
            {
                GridRequest gr = new GridRequest(Request);
                var pred = FilterHelper.GetExpression<ProductBarcodeDetail>(gr.FilterGroup);
                var allpre = _productBarcodeDetailContract.productBarcodeDetails.Where(pred);
                var li = allpre.Select(c => new
                {
                    c.Id,
                    c.ProductNumber,
                    c.OnlyFlag,
                    c.Status,
                    c.CreatedTime,
                    AdminName = c.Administrator == null ? "" : c.Administrator.Member.MemberName
                }).OrderByDescending(c => c.CreatedTime).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize);
                int cou = allpre.Count();
                GridData<object> data = new GridData<object>(li, cou, Request);
                return Json(data);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ActionResult ProductOperationLogDetails(string num)
        {
            ViewBag.proNum = num;
            return PartialView();
        }

        public ActionResult GetProductOperationLogByNumber()
        {
            string OnlyFlag = Request.Form["OnlyFlag"].ToString();
            string typeFlag= Request.Form["typeFlag"].ToString();

            if (typeFlag == "1")
            {
                //入库
                var inventoryQuery = (from c in _inventoryContract.Inventorys
                                      where c.ProductBarcode == OnlyFlag
                                      select new
                                      {
                                          ProductNumber = "-",
                                          OperationType = "入库",
                                          Description = "商品入库",
                                          StoreName = c.Store.StoreName,
                                          OutgoingPerson="-",
                                          Consignee="-",
                                          AdminName = (from a in _administratorContract.Administrators
                                                       join
                                   b in _memberContract.Members on a.MemberId equals b.Id
                                                       where a.Id == c.OperatorId
                                                       select b.MemberName).ToList().FirstOrDefault(),
                                          CreatedTime = c.UpdatedTime,
                                      });
                var retailQuery = from a in _retailItemContract.RetailItems  //零售
                                  join
                                  b in _retailInventoryContract.RetailInventorys on a.Id equals b.RetailItemId
                                  join
                                  c in _inventoryContract.Inventorys on b.InventoryId equals c.Id
                                  join
                                  d in _retailContract.Retails on a.RetailId equals d.StoreId
                                  where c.ProductBarcode == OnlyFlag
                                  select new
                                  {
                                      ProductNumber = d.Store.StoreName,
                                      OperationType = "零售",
                                      Description = "商品零售",
                                      StoreName = "-",
                                      OutgoingPerson = "-",
                                      Consignee = "-",
                                      AdminName = (from a in _administratorContract.Administrators
                                                   join
                               b in _memberContract.Members on a.MemberId equals b.Id
                                                   where a.Id == d.RecommenderId
                                                   select b.MemberName).ToList().FirstOrDefault(),
                                      CreatedTime = a.UpdatedTime,
                                  };

                var returnQuery = from a in _returnedItemContract.ReturnedItems //退货
                                  join
                                  d in _returnedContract.Returneds on a.ReturnedId equals d.Id
                                  join
                                  b in _retailContract.Retails on a.RetailId equals b.Id
                                  join
                                  c in _inventoryContract.Inventorys on a.InventoryId equals c.Id
                                  where c.ProductBarcode == OnlyFlag
                                  select new
                                  {
                                      ProductNumber = "-",
                                      OperationType = "退货",
                                      Description = "商品退货",
                                      StoreName = d.Store.StoreName,
                                      OutgoingPerson = "-",
                                      Consignee = "-",
                                      AdminName = (from s in _administratorContract.Administrators
                                                   join
                               b in _memberContract.Members on s.MemberId equals b.Id
                                                   where s.Id == a.OperatorId
                                                   select b.MemberName).ToList().FirstOrDefault(),
                                      CreatedTime = a.UpdatedTime,
                                  };

                var OrderQuery = from a in _orderblankContract.Orderblanks //商品配货-发货
                                 join
                                 b in _orderblankItemContract.OrderblankItems on a.Id equals b.OrderblankId
                                 join
                                 c in _productContract.Products on b.ProductId equals c.Id
                                 where b.OrderBlankBarcodes.Contains(OnlyFlag)
                                 select new
                                 {
                                     ProductNumber = _storageContract.Storages.Where(x => x.Id == a.OutStorageId).Select(x => x.StorageName).FirstOrDefault(),
                                     OperationType = "配货",
                                     Description = a.Status+ "商品配货",
                                     StoreName = _storeContract.Stores.Where(x => x.Id == a.ReceiverStoreId).Select(x => x.StoreName).FirstOrDefault(),
                                     OutgoingPerson = _storeContract.Stores.Where(x => x.Id == a.OutStoreId).Select(x => x.StoreName).FirstOrDefault(),
                                     Consignee = _storeContract.Stores.Where(x => x.Id == a.ReceiverStoreId).Select(x => x.StoreName).FirstOrDefault(),
                                     AdminName = "-",
                                     CreatedTime = b.UpdatedTime,
                                 };
                var allResults = inventoryQuery.Union(retailQuery).Union(returnQuery).Union(OrderQuery).OrderByDescending(x => x.CreatedTime).ToList();

                GridData<object> objda = new GridData<object>(allResults, allResults.Count(), Request);
                return Json(objda);
            }
            else {
                //入库
                //var inventoryQuery = (from c in _inventoryContract.Inventorys
                //                      where c.ProductNumber == OnlyFlag
                //                      select new
                //                      {
                //                          ProductNumber = "-",
                //                          OperationType = "入库",
                //                          Description = "商品入库",
                //                          StoreName = c.Store.StoreName,
                //                          OutgoingPerson = "-",
                //                          Consignee = "-",
                //                          AdminName = (from a in _administratorContract.Administrators
                //                                       join
                //                   b in _memberContract.Members on a.MemberId equals b.Id
                //                                       where a.Id == c.OperatorId
                //                                       select b.MemberName).ToList().FirstOrDefault(),
                //                          CreatedTime = c.UpdatedTime,
                //                      });
                //var retailQuery = from a in _retailItemContract.RetailItems  //零售
                //                  join
                //                  b in _retailInventoryContract.RetailInventorys on a.Id equals b.RetailItemId
                //                  join
                //                  c in _inventoryContract.Inventorys on b.InventoryId equals c.Id
                //                  join
                //                  d in _retailContract.Retails on a.RetailId equals d.StoreId
                //                  where c.ProductNumber == OnlyFlag
                //                  select new
                //                  {
                //                      ProductNumber = d.Store.StoreName,
                //                      OperationType = "零售",
                //                      Description = "商品零售"+ d.Store.StoreName,
                //                      StoreName = "-",
                //                      OutgoingPerson = "-",
                //                      Consignee = "-",
                //                      AdminName = (from a in _administratorContract.Administrators
                //                                   join
                //               b in _memberContract.Members on a.MemberId equals b.Id
                //                                   where a.Id == d.RecommenderId
                //                                   select b.MemberName).ToList().FirstOrDefault(),
                //                      CreatedTime = a.UpdatedTime,
                //                  };

                //var returnQuery = from a in _returnedItemContract.ReturnedItems //退货
                //                  join
                //                  d in _returnedContract.Returneds on a.ReturnedId equals d.Id
                //                  join
                //                  b in _retailContract.Retails on a.RetailId equals b.Id
                //                  join
                //                  c in _inventoryContract.Inventorys on a.InventoryId equals c.Id
                //                  where c.ProductNumber == OnlyFlag
                //                  select new
                //                  {
                //                      ProductNumber = "-",
                //                      OperationType = "退货",
                //                      Description = "商品退货("+ d.Store.StoreName+")",
                //                      StoreName = d.Store.StoreName,
                //                      OutgoingPerson = "-",
                //                      Consignee = "-",
                //                      AdminName = (from s in _administratorContract.Administrators
                //                                   join
                //               b in _memberContract.Members on s.MemberId equals b.Id
                //                                   where s.Id == a.OperatorId
                //                                   select b.MemberName).ToList().FirstOrDefault(),
                //                      CreatedTime = a.UpdatedTime,
                //                  };

                //var OrderQuery = from a in _orderblankContract.Orderblanks //商品配货-发货
                //                 join
                //                 b in _orderblankItemContract.OrderblankItems on a.Id equals b.OrderblankId
                //                 join
                //                 c in _productContract.Products on b.ProductId equals c.Id
                //                 where b.OrderBlankBarcodes.Contains(OnlyFlag)
                //                 select new
                //                 {
                //                     ProductNumber = _storageContract.Storages.Where(x => x.Id == a.OutStorageId).Select(x => x.StorageName).FirstOrDefault(),
                //                     OperationType = "配货",
                //                     Description = a.Status+ "商品配货由" + _storageContract.Storages.Where(x => x.Id == a.OutStorageId).Select(x => x.StorageName).FirstOrDefault()
                //                     + "配送到" + _storeContract.Stores.Where(x => x.Id == a.ReceiverId).Select(x => x.StoreName).FirstOrDefault() ,
                //                     StoreName = _storeContract.Stores.Where(x => x.Id == a.ReceiverId).Select(x => x.StoreName).FirstOrDefault(),
                //                     OutgoingPerson = _storeContract.Stores.Where(x => x.Id == a.DeliverId).Select(x => x.StoreName).FirstOrDefault(),
                //                     Consignee = _storeContract.Stores.Where(x => x.Id == a.ReceiverUser).Select(x => x.StoreName).FirstOrDefault(),
                //                     AdminName = (from s in _administratorContract.Administrators
                //                                  join
                //              b in _memberContract.Members on s.MemberId equals b.Id
                //                                  where s.Id == a.DeliverId
                //                                  select b.MemberName).ToList().FirstOrDefault(),
                //                     CreatedTime = b.UpdatedTime,
                //                 };
                //var allResults = inventoryQuery.Union(retailQuery).Union(returnQuery).Union(OrderQuery).OrderByDescending(x => x.CreatedTime).ToList();

                GridData<object> objda = new GridData<object>(new List<object>(), 0, Request);
                return Json(objda);
            }

        }
        public string GetNameByOperatorId(int? id)
        {
           return (from a in _administratorContract.Administrators
                    join
b in _memberContract.Members on a.MemberId equals b.Id
                    where a.Id == id
                    select b.MemberName).ToList().FirstOrDefault();
        }
    }

    
}