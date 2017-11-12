
using System;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Data;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models;
using System.Collections.Generic;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    [License(CheckMode.Verify)]
    public class AppointmentPackingController : BaseController
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(AppointmentPackingController));

        private readonly IAppointmentPackingContract _contract;
        private readonly IAdministratorContract _adminContract;
        private readonly IModuleContract _moduleContract;
        private readonly IAppointmentContract _appointmentContract;
        private readonly IOrderblankContract _orderblankContract;
        private readonly IInventoryContract _inventoryContract;
        private readonly IStorageContract _storageContract;
        private readonly IPermissionContract _permissionContract;

        public AppointmentPackingController(IAppointmentPackingContract contract,
            IAdministratorContract adminContract,
            IModuleContract moduleContract,
            IAppointmentContract appointmentContract,
            IStoreContract storeContract,
            IOrderblankContract orderblankContract,
            IInventoryContract inventoryContract,
            IStorageContract storageContract,
            IPermissionContract permissionContract)
        {
            _contract = contract;
            _adminContract = adminContract;
            _moduleContract = moduleContract;
            _appointmentContract = appointmentContract;
            _orderblankContract = orderblankContract;
            _inventoryContract = inventoryContract;
            _storageContract = storageContract;
            _permissionContract = permissionContract;

        }

        [Layout]
        public ActionResult Index(int? id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult Orderblank(int id)
        {
            var model = _contract.View(id);

            return PartialView(model);
        }

        public ActionResult Echo(string code)
        {
            return Json(new OperationResult(OperationResultType.Success, string.Empty, code));
        }

        public ActionResult GetPackingItem(int id)
        {
            var data = _contract.Entities.Where(p => !p.IsDeleted && p.IsEnabled && p.Id == id)
                .SelectMany(p => p.AppointmentPackingItem.Select(i => new
                {
                    i.ProductId,
                    i.ProductBarcode,
                    i.Product.ProductNumber,
                    i.Product.ProductOriginNumber.Brand.BrandName,
                    i.Product.ProductOriginNumber.Category.CategoryName,
                    i.Product.Color.ColorName,
                    i.Product.Size.SizeName,
                    ProductCollocationImg = i.Product.ProductCollocationImg ?? i.Product.ProductOriginNumber.ProductCollocationImg
                })).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }


        /// <summary>
        /// 查看/创建/修改数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                var model = _contract.View(id.Value);
                return PartialView("Edit", model);
            }
            else
            {
                return PartialView("Edit", new CollocationPlan());
            }
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? id, string name, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var query = _contract.Entities;
            query.Where(e => e.IsEnabled == isEnabled);
            if (id.HasValue)
            {

                query = query.Where(e => e.Id == id.Value);

            }
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                State = e.State.ToString(),
                                e.Orderblank.OrderBlankNumber,
                                FromStore = e.FromStore.StoreName,
                                FromStorage = e.FromStorage.StorageName,
                                ToStore = e.ToStore.StoreName,
                                ToStorage = e.ToStorage.StorageName,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.CreatedTime,
                                e.UpdatedTime
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ProductSelect(int id)
        {
            var numbers = _contract.Entities.Where(p => !p.IsDeleted && p.IsEnabled && p.Id == id).SelectMany(p => p.AppointmentPackingItem.Select(i => i.ProductNumber)).ToList();
            ViewBag.ProductNumbers = string.Join(",", numbers);
            return PartialView();
        }

        public ActionResult ProductList(string productNumbers, int pageIndex = 1, int pageSize = 10)
        {
            var numbers = productNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var storageEntry = _storageContract.Storages.Where(x => !x.IsDeleted && x.IsEnabled && x.IsOrderStorage)
                                                            .Select(s => new
                                                            {
                                                                s.Id,
                                                                s.StoreId
                                                            }).FirstOrDefault();
            if (storageEntry == null)
            {
                throw new Exception("采购仓库不存在");
            }



            var query = _inventoryContract.Inventorys.Where(p => !p.IsDeleted && p.IsEnabled && p.Status == InventoryStatus.Default)
                                                     .Where(p => p.StorageId == storageEntry.Id && p.StoreId == storageEntry.StoreId)
                                                     .Where(p => numbers.Contains(p.ProductNumber));

            var list = query.OrderByDescending(p => p.UpdatedTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.ProductBarcode,
                    p.ProductNumber,
                    ProductCollocationImg = p.Product.ProductCollocationImg ?? p.Product.ProductOriginNumber.ProductCollocationImg,
                    p.Product.Color.ColorName,
                    p.Product.Size.SizeName,
                    p.Product.ProductOriginNumber.Season.SeasonName,
                    p.Product.ProductOriginNumber.Brand.BrandName,
                    p.Product.ProductOriginNumber.TagPrice,
                    p.Product.ProductOriginNumber.Category.CategoryName,

                })
                .ToList();
            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AddBarcode(AddBarcodeReq dto)
        {
            var res = _contract.AddBarcode(AuthorityHelper.OperatorId.Value, dto);
            return Json(res);
        }

        [HttpPost]
        public ActionResult RemoveBarcode(RemoveBarcodeReq dto)
        {
            var res = _contract.RemoveBarcode(AuthorityHelper.OperatorId.Value, dto);
            return Json(res);
        }

        [HttpPost]
        public ActionResult FinishPacking(FinishPackingReq dto)
        {

            var res = _orderblankContract.SaveOrderblankFromAppointmentPacking(dto.Id);
            return Json(res);


        }

        #region garbage


        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _contract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _contract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _contract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _contract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [OutputCache(Duration = 300)]
        public ActionResult QueryPageFlag(bool isValid)
        {
            var area = RouteData.DataTokens.ContainsKey("area") ? RouteData.DataTokens["area"].ToString() : string.Empty;
            var controller = RouteData.Values["controller"].ToString();

            var pageUrl = string.Format("{0}/{1}/Index", area, controller);
            var permisstionList = new List<Permission>();
            try
            {
                if (isValid)  // 获取拥有的权限标识
                {
                    var res = PermissionHelper.GetCurrentUserPagePermission(pageUrl, _adminContract, _moduleContract, _permissionContract);
                    if (res != null && res.Any())
                    {
                        permisstionList.AddRange(res);
                    }

                }
                else  // 获取需要屏蔽的权限标识
                {
                    var res = PermissionHelper.GetCurrentUserPageNoPermission(pageUrl, _adminContract, _moduleContract, _permissionContract);
                    if (res != null && res.Any())
                    {
                        permisstionList.AddRange(res);
                    }
                }


                var flags = permisstionList.Where(p => !string.IsNullOrEmpty(p.OnlyFlag))
                                            .Select(p => p.OnlyFlag)
                                            .ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, flags), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("权限包含的页面元素加载出错，错误如下：" + ex.Message + "。");
                throw new Exception("error");
            }

        }


    }

}
