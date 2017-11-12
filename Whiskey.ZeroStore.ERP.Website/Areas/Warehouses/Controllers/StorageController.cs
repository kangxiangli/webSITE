
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
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using System.Web.Caching;
using Whiskey.ZeroStore.ERP.Website.App_Start;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{

    [License(CheckMode.Verify)]
    public class StorageController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StorageController));

        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IDepartmentContract _departmentContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IInventoryContract _InventoryContract;

        public StorageController(IStorageContract storageContract, IStoreContract storeContract, IDepartmentContract departmentContract,
            IInventoryContract _InventoryContract,
            IAdministratorContract administratorContract)
        {
            _storageContract = storageContract;
            _storeContract = storeContract;
            _departmentContract = departmentContract;
            _administratorContract = administratorContract;
            this._InventoryContract = _InventoryContract;
        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        [HttpGet]
        public ActionResult Index()
        {
            return View();
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
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(StorageDto dto)
        {
            OperationResult resl = new OperationResult(OperationResultType.Error, "异常");
            int err = 0;
            if (dto.IsDefaultStorage)
            { //将已经存在的默认仓库覆盖为非默认
                _storageContract.UpdateWord(dto.StoreId, c => c.IsDefaultStorage, new string[] { "IsDefaultStorage" }, new object[] { false });
            }
            if (dto.IsOrderStorage)
            {
                var orderStorages = _storageContract.Storages.Where(w => w.IsOrderStorage).ToList();
                foreach (var item in orderStorages)
                {
                    item.IsOrderStorage = false;
                }
                var sresult = _storageContract.Update(orderStorages.ToArray());
            }
            if (dto.IsTempStorage)
            {
                var tempStorages = _storageContract.Storages.Where(w => w.IsTempStorage).ToList();
                foreach (var item in tempStorages)
                {
                    item.IsOrderStorage = false;
                }
                var sresult = _storageContract.Update(tempStorages.ToArray());
            }

            if (dto.StorageType == 0)//同一个店铺只能有一个线上仓库
            {
                var storag = _storageContract.Storages.FirstOrDefault(
                     c => c.StoreId == dto.StoreId && c.StorageType == 0 && !c.IsDeleted && c.IsEnabled);
                if (storag != null)
                {
                    resl.Message = "同一个店铺只能有一个线上仓库，已存在可用的线上仓库：" + storag.StorageName;
                    err += 1;
                }
            }
            #region MyRegion

            #endregion
            if (err == 0)
                resl = _storageContract.Insert(dto);
            return Json(resl, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(StorageDto dto)
        {
            var t = _storageContract.Storages.FirstOrDefault(c => c.Id == dto.Id && !c.IsDeleted && c.IsEnabled);
            if (t != null && t.CheckLock)
            {
                return Json(OperationResultType.Error, "当前仓库正在盘点已被锁定，请等待盘点完成");
            }
            if (dto.IsDefaultStorage)
            {
                _storageContract.UpdateWord(dto.StoreId, c => c.IsDefaultStorage, new string[] { "IsDefaultStorage" }, new object[] { false });
            }
            if (dto.IsOrderStorage)
            {
                var orderStorages = _storageContract.Storages.Where(w => w.IsOrderStorage).ToList();
                foreach (var item in orderStorages)
                {
                    item.IsOrderStorage = false;
                }
                var sresult = _storageContract.Update(orderStorages.ToArray());
            }
            if (dto.IsTempStorage)
            {
                var tempStorages = _storageContract.Storages.Where(w => w.IsTempStorage).ToList();
                foreach (var item in tempStorages)
                {
                    item.IsOrderStorage = false;
                }
                var sresult = _storageContract.Update(tempStorages.ToArray());
            }
            var result = _storageContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取仓库的管理员信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAdminOfStorages(int[] ids)
        {
            var li = _administratorContract.Administrators.Where(c => ids.Contains(c.Id) && c.IsEnabled && !c.IsDeleted).Select(c => new
            {
                c.Id,
                c.Member.RealName,
                DepartmentId = c.Department.Id,
                DepartmentName = c.Department.DepartmentName,
            }).ToList().GroupBy(c => c.DepartmentId).Select(x => new
            {
                DeparId = x.Key,
                DeparName = x.Select(c => c.DepartmentName).FirstOrDefault(),
                Member = x.Select(c => new
                {
                    c.Id,
                    AdminName = c.RealName
                }).ToList()
            });
            return Json(li);
        }

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _storageContract.Edit(Id);

            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _storageContract.View(Id);
            ViewBag.storeName = _storeContract.Stores.Single(c => c.Id == result.StoreId).StoreName;
            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> List()
        {
            GridRequest request = new GridRequest(Request);

            Expression<Func<Storage, bool>> predicate = FilterHelper.GetExpression<Storage>(request.FilterGroup);

            var data = await Task.Run(() =>
             {
                 var count = 0;

                 var list = _storageContract.Storages.Where<Storage, int>(predicate, request.PageCondition, out count).Select(m => new
                 {
                     m.Store.StoreName,
                     m.StorageType,
                     m.StorageName,
                     m.CheckLock,
                     m.Id,
                     m.IsDeleted,
                     m.IsEnabled,
                     m.Sequence,
                     m.UpdatedTime,
                     m.CreatedTime,
                     m.Operator.Member.MemberName,
                     m.IsDefaultStorage,
                     m.IsOrderStorage,
                     m.IsTempStorage,
                     m.IsForAddInventory,
                     Invens = m.Inventories.Count(g => g.IsEnabled && !g.IsDeleted && g.Status == InventoryStatus.Default && !g.IsLock),
                     Check = m.checkers.OrderByDescending(c => c.UpdatedTime).FirstOrDefault(),
                     //StorageAdminCou = m.StorageAdmin.Count
                 }).ToList().Select(m => new
                 {
                     m.StoreName,
                     m.StorageType,
                     m.StorageName,
                     m.CheckLock,
                     m.Id,
                     m.IsDeleted,
                     m.IsEnabled,
                     m.Sequence,
                     m.UpdatedTime,
                     m.CreatedTime,
                     m.MemberName,
                     m.IsDefaultStorage,
                     m.IsOrderStorage,
                     m.IsTempStorage,
                     m.Invens,
                     m.IsForAddInventory,
                     Check = GetCheckState(m.Check),
                     //m.StorageAdminCou

                 });


                 return new GridData<object>(list, count, request.RequestInfo);
             });

            return Json(data, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 获取盘点状态
        /// </summary>
        /// <param name="checker"></param>
        /// <returns></returns>
        private int GetCheckState(Checker checker)
        {  //0: 未开始  1:  盘点中 2：中断 3：完成 4:完成校对

            int stat = 0;
            if (checker != null && checker.UpdatedTime.Month == DateTime.Now.Month)
            {
                stat = (int)checker.CheckerState;
            }
            return stat;
        }



        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var bo = _storageContract.Storages.Where(c => Id.Contains(c.Id) && c.CheckLock).Count();
            if (bo > 0)
            {
                var resu = new OperationResult(OperationResultType.Error, "当前正在盘点，仓库已锁定，请等待盘点完成");
                return Json(resu, JsonRequestBehavior.AllowGet);
            }

            var result = _storageContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var bo = _storageContract.Storages.Where(c => Id.Contains(c.Id) && c.CheckLock).Count();
            if (bo > 0)
            {
                var resu = new OperationResult(OperationResultType.Error, "当前正在盘点，仓库已锁定，请等待盘点完成");
                return Json(resu, JsonRequestBehavior.AllowGet);
            }
            var result = _storageContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var bo = _storageContract.Storages.Where(c => Id.Contains(c.Id) && c.CheckLock).Count();
            if (bo > 0)
            {
                var resu = new OperationResult(OperationResultType.Error, "当前正在盘点，仓库已锁定，请等待盘点完成");
                return Json(resu, JsonRequestBehavior.AllowGet);
            }
            var result = _storageContract.Recovery(Id);
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
            var bo = _storageContract.Storages.Where(c => Id.Contains(c.Id) && c.CheckLock).Count();
            if (bo > 0)
            {
                var resu = new OperationResult(OperationResultType.Error, "当前正在盘点，仓库已锁定，请等待盘点完成");
                return Json(resu, JsonRequestBehavior.AllowGet);
            }
            var result = _storageContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
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
            var bo = _storageContract.Storages.Where(c => Id.Contains(c.Id) && c.CheckLock).Count();
            if (bo > 0)
            {
                var resu = new OperationResult(OperationResultType.Error, "当前正在盘点，仓库已锁定，请等待盘点完成");
                return Json(resu, JsonRequestBehavior.AllowGet);
            }
            var result = _storageContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _storageContract.Storages.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _storageContract.Storages.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        //yxk 2015-9-
        /// <summary>
        /// 根据客户端传递过来的storeid获得与之关联的仓库信息
        /// </summary>
        /// <param name="storeId">店铺id,如果传入的为-1，查询所有没有被禁用的店铺</param>
        /// <returns></returns>
        [Log]
        public JsonResult GetStorage(int? storeId, string title)
        {
            if (!storeId.HasValue)
            {
                return new JsonResult() { Data = new List<SelectListItem>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }
            var list = CacheAccess.GetManagedStorageByStoreId(_storageContract, _administratorContract, storeId.Value, true);
            return new JsonResult() { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetStorages(int? storeId)
        {
            if (!storeId.HasValue)
            {
                return new JsonResult() { Data = new List<SelectListItem>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }
            var list = CacheAccess.GetManagedStorageByStoreId(_storageContract, _administratorContract, storeId.Value, true);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        #region 获取仓库
        public JsonResult GetStoragesByStoreId(int storeId)
        {
            var da = _storageContract.Storages.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.StoreId == storeId).Select(c => new { Id = c.Id, Name = c.StorageName, IsDefault = c.IsDefaultStorage });
            return Json(da);
        }

        #endregion
        //yxk 2015-9-23
        /// <summary>
        /// 以json格式返回店铺和仓库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(Duration = 3600)]
        public JsonResult GetStoreWithStorage()
        {
            Response.Cache.SetOmitVaryStar(true);
            var sto = CacheHelper.GetCache("StoreWithStorage") as List<StoreWithStorage<int>>;
            if (sto == null || sto.Count() == 0)
            {
                sto = new List<StoreWithStorage<int>>();
                var storages = _storageContract.Storages.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new { c.Id, c.StoreId, c.StorageName, c.IsDefaultStorage });

                foreach (var stora in storages)
                {
                    StoreWithStorage<int> stoTe = new StoreWithStorage<int>();
                    if (sto.Where(c => c.Id == stora.StoreId).Count() > 0) continue;

                    string storeName = _storeContract.Stores.Where(c => c.Id == stora.StoreId).Select(c => c.StoreName).FirstOrDefault();

                    stoTe.Id = stora.StoreId;
                    stoTe.Name = storeName;

                    var storaTe = storages.Where(c => c.StoreId == stora.StoreId).Select(m => new StoreWithStorage<int>
                    {
                        Id = m.Id,
                        Name = m.StorageName,
                        Other = m.IsDefaultStorage ? "1" : "0"
                    }).ToList(); ;
                    stoTe.Children.AddRange(storaTe);
                    sto.Add(stoTe);

                }
                CacheHelper.SetCache("StoreWithStorage", sto, new SqlCacheDependency("ZeroStore", "W_Storage"));
            }

            return Json(sto);
        }
        //yxk 2015-10
        /// <summary>
        /// 在添加仓库时判断是否存在同名仓库
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public int NameExist(string name)
        {
            var st = _storageContract.Storages.Where(c => c.StorageName == name && c.IsEnabled == true && c.IsDeleted == false);
            if (st.Count() > 0)
            {
                return 1;
            }
            return 0;
        }


        #region 转移库存

        public ActionResult ShiftStorage(int Id)
        {
            ViewBag.Id = Id;
            return PartialView();
        }

        [HttpPost]
        public JsonResult ShiftStorage(int Id, int StoreId, int StorageId)
        {
            var data = _storageContract.ShiftStorage(Id, StoreId, StorageId);
            return Json(data);
        }

        #endregion


    }
}
