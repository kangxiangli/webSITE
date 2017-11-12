using Antlr.Runtime.Tree;
using Antlr3.ST;
using Antlr3.ST.Language;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
//using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class RetailDetailController : BaseController
    {
        //
        // GET: /Stores/RetailDetail/
        protected readonly IRetailContract _retailContract;

        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IProductContract _productContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IReturnedContract _returnedContract;
        protected readonly IRetailInventoryContract _retailInventoryContract;
        protected readonly IAdministratorContract _administratorContract;

        public RetailDetailController(IRetailContract retailContract, IProductContract productContract, IStoreContract storeContract, IStorageContract storageContract, IRetailItemContract retailItemContract,
            IAdministratorContract _administratorContract, IInventoryContract inventoryContract, IReturnedContract returnedContract, IRetailInventoryContract retailInventoryContract)
        {
            _retailContract = retailContract;
            _productContract = productContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
            _retailItemContract = retailItemContract;
            _inventoryContract = inventoryContract;
            _returnedContract = returnedContract;
            _retailInventoryContract = retailInventoryContract;
            this._administratorContract = _administratorContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult List(int? storeId)
        {
            try
            {
                var gr = new GridRequest(Request);
                Expression<Func<Retail, bool>> predict = FilterHelper.GetExpression<Retail>(gr.FilterGroup);

                var query = _retailContract.Retails.Where(predict)
                                        .Include(r => r.Operator.Member)
                                        .Include(r => r.Store);

                //根据店铺权限筛选
                var enabledStores = _storeContract.FilterStoreId(AuthorityHelper.OperatorId, _administratorContract, storeId);
                query = query.Where(r => enabledStores.Contains(r.StoreId.Value));
                //获取零售订单
                var parentList = query.OrderByDescending(c => c.CreatedTime)
                     .Skip(gr.PageCondition.PageIndex)
                     .Take(gr.PageCondition.PageSize)
                     .Select(x => new
                     {
                         Id = x.Id,
                         Operator = x.Operator.Member.MemberName,
                         OutStorageTime = x.OutStorageDatetime,
                         ProductCount = x.RetailItems.Sum(t => t.RetailCount),
                         ConsumeCount = x.ConsumeCount,
                         RealName = x.Consumer.RealName,
                         ConsumerId = x.ConsumerId,
                         CouponNumber = x.CouponNumber,
                         IsDeleted = x.IsDeleted,
                         IsEnabled = x.IsEnabled,
                         RetailNumber = x.RetailNumber,
                         RetailState = x.RetailState,
                         StoreName = x.Store.StoreName,
                         HasStoreActivity = x.StoreActivityDiscount > 0,
                         HasCoupon = x.CouponConsume > 0
                     })
                     .ToList()
                     .Select(x => new
                     {
                         ParentId = "",
                         ProductNumber = "",
                         ProductPic = "",
                         RetailPrice = "",
                         SalesCampaignDiscuss = 10,
                         State = x.RetailState.ToString(),
                         x.ConsumeCount,
                         x.ConsumerId,
                         x.CouponNumber,
                         x.Id,
                         x.IsDeleted,
                         x.IsEnabled,
                         x.Operator,
                         x.OutStorageTime,
                         x.ProductCount,
                         x.RealName,
                         x.RetailNumber,
                         x.StoreName,
                         x.HasStoreActivity,
                         x.HasCoupon
                     }).ToList();

                List<object> dataList = new List<object>();

                //获取订单对应的零售明细list
                var parentIds = parentList.Select(p => p.Id).ToList();
                var retailItemList = _retailItemContract.RetailItems.Where(item => parentIds.Contains(item.RetailId))
                                                                    .Include(item => item.Product)
                                                                    .Include(item => item.Product.ProductOriginNumber)
                                                                    .Include(item => item.Product.Color)
                                                                    .Include(item => item.RetailInventorys)
                                                                    .ToList();
                var retailItemIds = retailItemList.Select(item => item.Id).ToList();

                //构造子项
                foreach (var parent in parentList)
                {
                    dataList.Add(parent);
                    var retailItem = retailItemList.Where(item => item.RetailId == parent.Id).ToList();

                    foreach (var item in retailItem)
                    {
                        var inventoryList = item.RetailInventorys.Select(r => r.Inventory).ToList();
                        var child = inventoryList.Select(t => new
                        {
                            ConsumeCount = string.Empty,
                            ConsumerId = "",
                            CouponNumber = "",
                            Id = "c" + item.Id,
                            Operator = "",
                            OutStorageTime = "",
                            ParentId = parent.Id,
                            ProductCount = 1,
                            ProductNumber = t.ProductBarcode,
                            ProductPic = t.Product.ThumbnailPath ?? t.Product.ProductOriginNumber.ThumbnailPath,
                            item.SalesCampaignDiscount,
                            RetailNumber = "",
                            RetailPrice = item.ProductRetailPrice,  //订单明细中的零售价
                            State = string.Empty,
                            t.IsDeleted,
                            t.IsEnabled,
                            t.Product.Color.ColorName,
                            t.Product.Color.IconPath,
                        });

                        dataList.AddRange(child);
                    }
                }
                GridData<object> gd = new GridData<object>(dataList, query.Count(), Request);
                return Json(gd);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, ex.Message));
            }
        }

        public ActionResult View(int Id)
        {
            var retailitem = _retailItemContract.RetailItems.Include(i => i.Retail)
                 .FirstOrDefault(c => c.Id == Id);
            return PartialView(retailitem);
        }

        public ActionResult Remove(int Id)
        {
            var res = _retailContract.Remove(Id);
            return Json(res);
        }

        public ActionResult Recovery(int Id)
        {
            var res = _retailContract.Recovery(Id);
            return Json(res);
        }

        public ActionResult Enable(int Id)
        {
            var res = _retailContract.Enable(Id);
            return Json(res);
        }

        public ActionResult Disable(int Id)
        {
            var res = _retailContract.Disable(Id);
            return Json(res);
        }

        public ActionResult Delete(int Id)
        {
            var res = _retailContract.Delete(Id);
            return Json(res);
        }

        public ActionResult PView(string retailNumber, bool showDetail = false)
        {
            ViewBag.ShowDetail = showDetail ? 1 : 0;
            var retail =
                _retailContract.Retails.FirstOrDefault(c => c.RetailNumber == retailNumber);
            return PartialView(retail);
        }

        /// <summary>
        /// 根据销售编号或者商品编号获取商品的一维码详情
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetRetailsByNum(string renum, string pnum)
        {
            ViewBag.renum = renum;
            ViewBag.pnum = pnum;
            return PartialView();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="renum"></param>
        /// <param name="pnum"></param>
        /// <returns></returns>
        public ActionResult GetDetailsByRetailNum()
        {
            GridRequest gr = new GridRequest(Request);
            Expression<Func<Retail, bool>> predict = FilterHelper.GetExpression<Retail>(gr.FilterGroup);
            List<object> li = new List<object>();
            var retail = _retailContract.Retails.Include(r => r.ReturnRecordHistory).FirstOrDefault(predict);
            Func<string, string> getRetailInventoryStatus = barcode =>
            {
                if (retail.ReturnRecordHistory.Any(i => i.ProductBarcode == barcode))
                {
                    return "已退货";
                }
                return "正常";
            };
            if (retail != null)
            {
                var da = retail.RetailItems.Select(c => new
                {
                    c.Id,
                    c.Product.ProductNumber,
                    c.RetailItemState,
                    childs = c.RetailInventorys.Select(g => new
                    {
                        Id = "cl" + g.Id,
                        g.Inventory.ProductBarcode,
                        g.Inventory.Storage.StorageName,
                        status = getRetailInventoryStatus(g.ProductBarcode)
                    })
                });
                foreach (var d in da)
                {
                    li.Add(new
                    {
                        Id = d.Id,
                        ParentId = "",
                        Pnum = d.ProductNumber,
                        status = string.Empty
                    });
                    li.AddRange(d.childs.Select(c => new
                    {
                        c.Id,
                        ParentId = d.Id,
                        num = c.ProductBarcode,
                        status = c.status
                    }));
                }
            }
            GridData<object> data = new GridData<object>(li, li.Count, Request);
            return Json(data);
        }

        public ActionResult GetDetailsByProductNum()
        {
            GridRequest gr = new GridRequest(Request);
            Expression<Func<RetailItem, bool>> predict = FilterHelper.GetExpression<RetailItem>(gr.FilterGroup);
            List<object> li = new List<object>();

            var retail = _retailItemContract.RetailItems.Where(predict)
            .Select(c => new
            {
                c.Id,
                c.Product.ProductNumber,
                c.RetailItemState,
                childs = c.RetailInventorys.Select(g => new
                {
                    Id = "cl" + g.Id,
                    g.Inventory.ProductBarcode,
                    g.Inventory.Status,
                    g.Inventory.Storage.StorageName
                })
            });

            foreach (var o in retail)
            {
                li.Add(new
                {
                    o.Id,
                    ParentId = "",
                    Pnum = o.ProductNumber,
                    status = o.RetailItemState.ToString()
                });
                li.AddRange(o.childs.Select(c => new
                {
                    c.Id,
                    ParentId = o.Id,
                    num = c.ProductBarcode,
                    status = InventoryStatusExten.GetDescriptionText((InventoryStatus)c.Status)
                }));
            }

            GridData<object> data = new GridData<object>(li, li.Count, Request);
            return Json(data);
        }

        /// <summary>
        /// 销售单状态
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        private string GetStatusText(int stat)
        {
            //{//状态 0:正常 1：整单退货 2：部分退货 3.删除 4.禁用 5.发货完成
            RetailStatus state;
            if (!Enum.TryParse(stat.ToString(), out state))
            {
                return "";
            }
            return state.ToDescription();
        }

        private string GetStorageNames(string storageids)
        {
            int[] ids = storageids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToArray();
            if (ids.Any())
            {
                var storagname = _storageContract.Storages.Where(c => ids.Contains(c.Id)).Select(c => c.StorageName);
                return string.Join(",", storagname);
            }
            return "";
        }


        /// <summary>
        /// 零售明细
        /// </summary>
        /// <param name="retailNumber"></param>
        /// <returns></returns>
        public ActionResult GetItemsByRetailId(string retailNumber)
        {

            var retail = _retailContract.Retails.Where(r => !r.IsDeleted && r.IsEnabled && r.RetailNumber == retailNumber).FirstOrDefault();

            GridData<object> data = new GridData<object>(new List<object>(), 0, Request);
            if (retail != null)
            {
                var li = retail.RetailItems.Select(c => new
                {
                    Id = c.Id,
                    ProductNumber = c.Product.ProductNumber,
                    ThumbnailPath = c.Product.ThumbnailPath ?? c.Product.ProductOriginNumber.ThumbnailPath,
                    StorageName = GetStorageNames(c.Id.ToString()),
                    ProductRetailPrice = c.ProductRetailPrice,
                    ProductTagPrice = c.ProductTagPrice,
                    RetailCount = c.RetailCount,
                    SalesCampaign = c.SalesCampaignId == null ? "否" : "是",
                    SalesCampaignDiscount = c.SalesCampaignDiscount,
                    ConsumeCount = c.ProductRetailItemMoney,
                    AdminName = c.Retail.Operator.Member.MemberName,
                    Child = c.RetailInventorys.Select(i => new {
                        ThumbnailPath = c.Product.ThumbnailPath??c.Product.ProductOriginNumber.ThumbnailPath??string.Empty,
                        i.ProductBarcode,
                        i.Inventory.Storage.StorageName,
                        ProductName = c.Product.ProductName??c.Product.ProductOriginNumber.ProductName??string.Empty,
                        c.Product.ProductOriginNumber.Brand.BrandName,
                        c.ProductTagPrice,
                        c.ProductRetailPrice
                    })
                });
                data = new GridData<object>(li, li.Count(), Request);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region 导出文件

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int? storeId)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

            //根据店铺权限筛选
            var enabledStores = _storeContract.FilterStoreId(AuthorityHelper.OperatorId, _administratorContract, storeId);
            var gr = new GridRequest(Request);
            Expression<Func<Retail, bool>> predict = FilterHelper.GetExpression<Retail>(gr.FilterGroup);
            var query = _retailContract.Retails.Where(w => w.StoreId.HasValue).Where(r => enabledStores.Contains(r.StoreId.Value)).Where(predict);

            var listdata = (from s in query
                            let item = s.RetailItems.Where(w => w.IsEnabled && !w.IsDeleted)
                            select new
                            {
                                s.RetailNumber,
                                StoreName = s.Store.StoreName,
                                ConsumerName = s.Consumer.RealName ?? string.Empty,
                                RetailCount = item.Sum(t => t.RetailCount),
                                s.ConsumeCount,
                                s.OutStorageDatetime,
                                IsStoreActivityDiscount = s.StoreActivityDiscount > 0 ? "是" : "否",
                                IsCouponConsume = s.CouponConsume > 0 ? "是" : "否",
                                RetailState = s.RetailState + "",
                                OperatorName = s.Operator.Member.MemberName,
                            }).ToList();

            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", listdata);
            return FileExcel(st, "零售记录查询");
            //return JsonLarge(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}