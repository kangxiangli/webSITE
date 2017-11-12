using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Models;
using System.IO;
using Whiskey.Web.Helper;
using Antlr3.ST;
using Antlr3.ST.Language;
using Whiskey.Utility.Data;
using System.Text;
using Newtonsoft.Json;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    [License(CheckMode.Verify)]
    public class InventoryRecordController : BaseController
    {
        private IInventoryRecordContract _inventoryRecordContract;
        private readonly IStorageContract _storageContract;
        private readonly IBrandContract _brandContract;
        private readonly IInventoryContract _inventoryContract;
        private readonly IStoreContract _storeContract;
        private readonly IAdministratorContract _adminContract;
        public InventoryRecordController(IInventoryRecordContract invetoryRecordContract, IStorageContract storageContract,
            IBrandContract brandContract,
            IStoreContract _storeContract
            , IAdministratorContract _adminContract
            , IInventoryContract inventoryContract)
        {
            _inventoryRecordContract = invetoryRecordContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            _inventoryContract = inventoryContract;
            this._adminContract = _adminContract;
            this._storeContract = _storeContract;
        }

        [Layout]
        public ActionResult Index()
        {
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true);
            var data = _adminContract.GetDesignerStoreStorageList(AuthorityHelper.OperatorId.Value);
            ViewBag.DesignerInfo = data;

            return View();
        }



        /// <summary>
        /// 入库记录数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List(string txtBarcode, string txtBrandName)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<InventoryRecord, bool>> predicate;
            try
            {
                //权限检查
                var inventoryQuery = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract);
                var inventoryRecordQuery = CacheAccess.GetInventoryRecords(_inventoryRecordContract, _storeContract);

                predicate = FilterHelper.GetExpression<InventoryRecord>(request.FilterGroup);
                var data = await Task.Run(() =>
                {
                    // 根据identifyId或者商品productbarcode 查询
                    List<int> inventoryRecordIds = new List<int>();
                    string identifyId = string.Empty;
                    if (!string.IsNullOrEmpty(txtBarcode))
                    {
                        if (txtBarcode.Length == 14)  //productbarcode
                        {
                            // 根据商品条码查找所有的库存信息,然后找到与之关联的库存记录id
                            inventoryRecordIds = inventoryQuery.Where(i => i.ProductBarcode == txtBarcode && i.InventoryRecordId.HasValue)
                            .Select(i => i.InventoryRecordId.Value)
                            .Distinct()
                            .ToList();
                        }
                        else if (txtBarcode.Length == 17) //identifyId
                        {
                            identifyId = txtBarcode;
                        }
                    }

                    // 多条件查询
                    var count = 0;
                    var source = inventoryRecordQuery;
                    if (inventoryRecordIds.Count > 0)
                    {
                        source = source.Where(i => inventoryRecordIds.Contains(i.Id));
                    }
                    if (!string.IsNullOrEmpty(identifyId))
                    {
                        source = source.Where(i => i.IdentifyId == identifyId);
                    }
                    if (!string.IsNullOrEmpty(txtBrandName))
                    {
                        source = source.Where(i => i.Inventories.Count(j => j.Product.ProductOriginNumber.Brand.BrandName == txtBrandName) > 0);
                    }
                    source = source.Where<InventoryRecord>(predicate);
                    count = source.Count();
                    // 组装分页数据
                    var list = source.OrderByDescending(i => i.CreatedTime)
                    .Skip(request.PageCondition.PageIndex)
                    .Take(request.PageCondition.PageSize)
                    .Select(i => new
                    {
                        i.Id,
                        i.RecordOrderNumber,
                        i.IdentifyId,
                        i.Store.StoreName,
                        i.Storage.StorageName,
                        i.TagPrice,
                        i.Quantity,
                        i.Operator.Member.MemberName,
                        i.CreatedTime,
                        i.IsDeleted,
                        i.IsEnabled,
                        BrandCount = i.Inventories.Count(j => j.Product.ProductOriginNumber.Brand.BrandName == txtBrandName),
                        TotalBrandCount = i.Inventories.Select(j => j.Product.ProductOriginNumber.Brand.BrandName).Distinct().Count()

                    }).ToList();
                    return new GridData<object>(list, count, request.RequestInfo);

                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new GridData<object>(new List<string>(), 0, request.RequestInfo));

            }

        }


        public ActionResult GetInventory(string RecordId)
        {
            ViewBag.RecordId = RecordId;
            return PartialView();
        }
        /// <summary>
        /// 获取入库记录关联的库存信息
        /// </summary>
        /// <param name="RecordId">入库记录id</param>
        /// <returns></returns>
        public ActionResult GetInventoryList(int RecordId)
        {
            GridRequest rq = new GridRequest(Request);
            Expression<Func<Inventory, bool>> predicate;
            try
            {
                predicate = FilterHelper.GetExpression<Inventory>(rq.FilterGroup);
                var source = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                .Where(i => i.InventoryRecordId.Value == RecordId)
                .Where(predicate);
                int count = source.Count();
                var inventoryList = source.OrderByDescending(c => c.CreatedTime)
                    .Skip(rq.PageCondition.PageIndex)
                    .Take(rq.PageCondition.PageSize).Select(c => new
                    {
                        c.Id,
                        c.Store.StoreName,
                        c.Product.ProductOriginNumber.Brand.BrandName,
                        c.Product.ProductOriginNumber.Category.CategoryName,
                        c.Product.ProductOriginNumber.Season.SeasonName,
                        c.Product.Size.SizeName,
                        c.Product.Color.ColorName,
                        c.Product.Color.IconPath,
                        c.Storage.StorageName,
                        ThumbnailPath = c.Product.ThumbnailPath ?? c.Product.ProductOriginNumber.ThumbnailPath,
                        c.ProductBarcode,
                        c.CreatedTime,
                        c.Operator.Member.MemberName
                    }).ToList<object>();
                GridData<object> ojbdat = new GridData<object>(inventoryList, count, Request);
                return Json(ojbdat);
            }
            catch (Exception ex)
            {
                return Json(new GridData<object>(new List<string>(), 0, rq.RequestInfo));
            }

        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _inventoryRecordContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _inventoryRecordContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 导出数据
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpGet]
        public ActionResult Export(string Ids)
        {

            try
            {
                var arr = Ids.Split(',');

                List<int> data = new List<int>();
                for (int i = 0; i < arr.Length; i++)
                {
                    data.Add(int.Parse(arr[i]));
                }
                var Id = data.ToArray();
                var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
                var list = _inventoryRecordContract.InventoryRecords
                    .Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.RecordOrderNumber,
                        i.IdentifyId,
                        i.Store.StoreName,
                        i.Storage.StorageName,
                        i.TagPrice,
                        i.Quantity,
                        i.Operator.Member.MemberName,
                        i.CreatedTime,
                        i.IsDeleted,
                        i.IsEnabled,
                        TotalBrandCount = i.Inventories.Count() > 0 ? i.Inventories.Select(j => j.Product.ProductOriginNumber.Brand.BrandName).Distinct().Count() : 0
                    })
                    .ToList();
                var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
                var st = group.GetInstanceOf("Exporter");
                st.SetAttribute("list", list);
                var str = st.ToString();
                var buffer = Encoding.UTF8.GetBytes(str);
                var stream = new MemoryStream(buffer);
                return File(stream, "application/ms-excel", "入库记录.xls");


            }
            catch (Exception e)
            {

                return Json(new OperationResult(OperationResultType.Error, e.Message), JsonRequestBehavior.AllowGet);
            }

        }


        /// <summary>
        /// 导出库存数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpGet]
        public ActionResult ExportInventory(int? recordId, string Ids)
        {

            try
            {

                var inventoryIds = new List<int>();
                if (recordId.HasValue && recordId != -1)
                {
                    //导出记录下所有库存数据
                    inventoryIds.AddRange(CacheAccess.GetInventoryRecords(_inventoryRecordContract, _storeContract)
                                             .Where(r => r.Id == recordId.Value)
                                             .SelectMany(r => r.Inventories)
                                             .Where(i => !i.IsDeleted && i.IsEnabled)
                                             .Select(i => i.Id).ToList());
                }
                else
                {
                    //导出选中的数据
                    inventoryIds.AddRange(Ids.Split(',').Select(id => int.Parse(id)));
                }

                var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));


                var list = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                    .Where(i => inventoryIds.Contains(i.Id))
                    .OrderByDescending(c => c.CreatedTime)
                    .Select(c => new
                    {
                        c.Id,
                        c.Store.StoreName,
                        c.Product.ProductOriginNumber.Brand.BrandName,
                        c.Product.ProductOriginNumber.Category.CategoryName,
                        c.Product.ProductOriginNumber.Season.SeasonName,
                        c.Product.Size.SizeName,
                        c.Product.Color.ColorName,
                        c.Product.Color.IconPath,
                        c.Storage.StorageName,
                        c.Product.ThumbnailPath,
                        c.ProductBarcode,
                        c.CreatedTime,
                        c.Operator.Member.MemberName
                    }).ToList();
                var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
                var st = group.GetInstanceOf("ExporterInventory");
                st.SetAttribute("list", list);
                var str = st.ToString();
                var buffer = Encoding.UTF8.GetBytes(str);
                var stream = new MemoryStream(buffer);
                return File(stream, "application/ms-excel", "入库记录明细.xls");
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, e.Message), JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

    }
}