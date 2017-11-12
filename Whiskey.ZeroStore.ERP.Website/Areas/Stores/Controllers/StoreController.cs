




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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{

    [License(CheckMode.Verify)]
    public class StoreController : BaseController
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreController));

        protected readonly IStoreContract _storeContract;

        protected readonly IModuleContract _moduleContract;

        protected readonly IAdministratorContract _adminContract;

        protected readonly IDepartmentContract _departContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IJobPositionContract _jobPositionContract;
        protected readonly IStoreTypeContract _storeTypeContract;
        protected readonly IStoreDepositContract _storeDepositContract;
        protected readonly IStoreLevelContract _storeLevelContract;

        public StoreController(IStoreContract storeContract,
            IModuleContract moduleContract,
            IDepartmentContract departContract,
            IAdministratorContract adminContract, IMemberContract memberContract,
            IStoreTypeContract _storeTypeContract,
            IStoreDepositContract _storeDepositContract,
            IStoreLevelContract _storeLevelContract,
            IJobPositionContract jobPositionContract)
        {
            _storeContract = storeContract;
            _moduleContract = moduleContract;
            _departContract = departContract;
            _adminContract = adminContract;
            _memberContract = memberContract;
            _jobPositionContract = jobPositionContract;
            this._storeTypeContract = _storeTypeContract;
            this._storeDepositContract = _storeDepositContract;
            this._storeLevelContract = _storeLevelContract;
        }
        #endregion

        #region 初始化界面

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            ViewBag.StoreTypes = CacheAccess.GetStoreType(_storeTypeContract, true);
            return View();
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.StoreTypes = CacheAccess.GetStoreType(_storeTypeContract, false);
            return PartialView(new StoreDto() { StoreCredit = 100, Balance = 0 });
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(StoreDto dto)
        {
            var result = _storeContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(StoreDto dto)
        {
            var result = _storeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _storeContract.Edit(Id);
            ViewBag.StoreTypes = CacheAccess.GetStoreType(_storeTypeContract, false);
            return PartialView(result);
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _storeContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 获取数据列表

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Store, bool>> predicate = FilterHelper.GetExpression<Store>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Administrator> listAdmin = _adminContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var members =
                     _memberContract.Members
                        .Where(c => c.IsEnabled && !c.IsDeleted)
                        .Select(c => c.StoreId)
                        .ToList();

                var list = (from m in _storeContract.Stores.Where<Store, int>(predicate, request.PageCondition, out count).OrderByDescending(c => c.CreatedTime)
                            let depPrice = _storeDepositContract.Entities.Where(w => w.StoreId == m.Id && w.IsEnabled && !w.IsDeleted).Select(s => s.Price).Sum()
                            let modLevel = _storeLevelContract.Entities.Where(f => f.UpgradeCondition <= depPrice && f.IsEnabled && !f.IsDeleted)
                                            .OrderByDescending(o => o.UpgradeCondition).FirstOrDefault()
                            select new
                            {
                                m.StoreName,
                                StoreType = m.StoreType.TypeName,
                                m.IsMainStore,
                                m.Description,
                                m.StoreCredit,
                                m.Balance,
                                m.ContactPerson,
                                m.MobilePhone,
                                m.Telephone,
                                m.Province,
                                m.City,
                                m.ZipCode,
                                m.Address,
                                m.Notes,
                                m.Id,
                                m.IsDeleted,
                                m.IsEnabled,
                                m.Sequence,
                                m.UpdatedTime,
                                m.CreatedTime,
                                m.Operator.Member.MemberName,
                                m.IsAttached,
                                m.DepartmentId,
                                m.StorePhoto,
                                Level = modLevel != null ? modLevel.LevelName : "",
                                Discount = modLevel != null ? modLevel.Discount : 1,
                                m.StoreDiscount,
                                DepartmentName = m.DepartmentId == null ? string.Empty : m.Department.DepartmentName,
                                MemberCount = m.Members.Count(c => c.StoreId == m.Id && c.IsEnabled && !c.IsDeleted),
                                AdminCount = listAdmin.Where(x => x.DepartmentId == m.DepartmentId).Count(),
                                StorageCount = m.Storages.Count(c => c.IsEnabled && !c.IsDeleted),
                                Members = m.Members.Where(c => c.IsEnabled && !c.IsDeleted).Select(e => e.MemberName),
                                AdministratorCount = m.Administrators.Count(c => c.IsEnabled && !c.IsDeleted),
                                Administrators = m.Administrators.Where(c => c.IsEnabled && !c.IsDeleted).Select(a => a.Member.MemberName),
                                InventCou = m.Storages.Where(c => c.IsEnabled && !c.IsDeleted).Sum(c => (int?)(c.Inventories.Count(g => g.IsEnabled && !c.IsDeleted))) ?? 0
                            }).ToList();

                List<object> dataSource = new List<object>();
                foreach (var m in list)
                {
                    var obj = new
                    {
                        m.StoreName,
                        StoreType = m.StoreType,
                        m.IsMainStore,
                        m.Description,
                        m.StoreCredit,
                        m.Balance,
                        m.ContactPerson,
                        m.MobilePhone,
                        m.Telephone,
                        m.Province,
                        m.City,
                        m.ZipCode,
                        m.Address,
                        m.Notes,
                        m.Id,
                        StoreLeader = GetStoreLeader(m.DepartmentId),
                        m.IsDeleted,
                        m.IsEnabled,
                        m.Sequence,
                        m.UpdatedTime,
                        m.CreatedTime,
                        m.MemberName,
                        m.IsAttached,
                        m.DepartmentId,
                        m.DepartmentName,
                        m.MemberCount,
                        m.AdminCount,
                        m.StoreDiscount,
                        m.StorageCount,
                        m.Members,
                        m.StorePhoto,
                        m.AdministratorCount,
                        m.Administrators,
                        m.InventCou,
                        m.Level,
                        Discount = m.Discount
                    };
                    dataSource.Add(obj);
                }
                return new GridData<object>(dataSource, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult GetStorages(int Id)
        {
            ViewBag.AdminId = Id;
            return PartialView();
        }

        public ActionResult GetStoragesInfo(int Id)
        {
            var store = _storeContract.Stores.Where(x => x.Id == Id && x.IsEnabled && !x.IsDeleted).FirstOrDefault();
            List<object> list = new List<object>();
            if (store != null)
            {
                foreach (var item in store.Storages.Where(x => x.IsEnabled && !x.IsDeleted))
                {
                    var da = new
                    {
                        storageName = item.StorageName
                    };
                    list.Add(da);
                }
                list = list.Distinct().ToList();
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public string GetStoreLeader(int? departmentId)
        {
            var jobPositionId = _jobPositionContract.JobPositions.Where(x => !x.IsDeleted && x.IsEnabled
            && x.DepartmentId == departmentId && x.IsLeader).Select(x => x.Id).FirstOrDefault();
            return _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.JobPositionId == jobPositionId)
                .Select(x => x.Member.MemberName).FirstOrDefault();
        }


        #region 移除数据

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _storeContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _storeContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _storeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用数据

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _storeContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _storeContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印数据

        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _storeContract.Stores.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 导出数据

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _storeContract.Stores.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 部门
        public ActionResult Department()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取部门列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> DepartmentList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Department, bool>> predicate = FilterHelper.GetExpression<Department>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<int> listDepartmentId = new List<int>();
                List<Store> listStore = _storeContract.Stores.Where(x => x.DepartmentId != null).ToList();
                if (listStore.Count() > 0)
                {
                    listDepartmentId = listStore.Select(x => x.DepartmentId ?? 0).ToList();
                }
                List<Department> listDepart = _departContract.Departments.ToList();
                if (listDepartmentId.Count > 0)
                {
                    listDepart = listDepart.Where(x => !listDepartmentId.Contains(x.Id)).ToList();
                }
                var list = listDepart.AsQueryable().Where<Department, int>(predicate, request.PageCondition, out count).OrderByDescending(c => c.CreatedTime).Select(m => new
                {
                    m.Id,
                    m.DepartmentName,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult Thumbnails(int Id)
        {
            var result = new List<object>();
            var entity = _storeContract.Stores.FirstOrDefault(m => m.Id == Id);
            if (entity != null && !string.IsNullOrEmpty(entity.StorePhoto))
            {
                var counter = 1;
                var filePath = FileHelper.UrlToPath(entity.StorePhoto);
                if (System.IO.File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    result.Add(new
                    {
                        ID = counter.ToString(),
                        FileName = entity.StorePhoto,
                        FilePath = entity.StorePhoto,
                        FileSize = fileInfo.Length
                    });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 充值

        public ActionResult Recharge(int Id)
        {
            StoreDepositDto dto = new StoreDepositDto();
            dto.StoreId = Id;
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult Recharge(StoreDepositDto dto)
        {
            OperationResult oper = _storeDepositContract.Insert(dto);
            return Json(oper);
        }

        #endregion

        [HttpGet]
        public ActionResult EditConfig()
        {
            return PartialView();
        }


        [HttpGet]
        public ActionResult GetConfig()
        {
            var dict = RedisCacheHelper.GetSMSConfig();
            if (dict == null || dict.Keys.Count <= 0)
            {
                dict = new Dictionary<string, string>
                  {
                      {"retail","0" },
                      {"recharge","0" },
                  };
                RedisCacheHelper.SetSMSConfig(dict);
            }
            return Json(new OperationResult(OperationResultType.Success, string.Empty, dict), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditConfig(string config)
        {
            var cfg = JsonHelper.FromJson<Dictionary<string, string>>(config);
            RedisCacheHelper.SetSMSConfig(cfg);
            return Json(OperationResult.OK());
        }
    }
}
