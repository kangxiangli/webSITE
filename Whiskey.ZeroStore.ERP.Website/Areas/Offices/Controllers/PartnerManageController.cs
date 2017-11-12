using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Collections.Generic;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class PartnerManageController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PartnerManageController));

        protected readonly IPartnerManageContract _PartnerManageContract;
        protected readonly IStoreTypeContract _StoreTypeContract;
        protected readonly IRoleContract _roleContract;
        protected readonly IPartnerManageCheckContract _PartnerManageCheckContract;

        protected readonly IAdministratorContract _AdministratorContract;
        protected readonly IStoreContract _StoreContract;
        protected readonly IDepartmentContract _DepartmentContract;
        protected readonly IJobPositionContract _JobPositionContract;
        protected readonly IStorageContract _StorageContract;

        public PartnerManageController(
            IPartnerManageContract _PartnerManageContract,
            IStoreTypeContract _StoreTypeContract,
            IRoleContract _roleContract,
            IPartnerManageCheckContract _PartnerManageCheckContract
            

            , IAdministratorContract _AdministratorContract
            , IStoreContract _StoreContract
            , IDepartmentContract _DepartmentContract
            , IJobPositionContract _JobPositionContract
            , IStorageContract _StorageContract
            )
        {
            this._PartnerManageContract = _PartnerManageContract;
            this._StoreTypeContract = _StoreTypeContract;
            this._roleContract = _roleContract;
            this._PartnerManageCheckContract = _PartnerManageCheckContract;

            this._AdministratorContract = _AdministratorContract;
            this._StoreContract = _StoreContract;
            this._DepartmentContract = _DepartmentContract;
            this._JobPositionContract = _JobPositionContract;
            this._StorageContract = _StorageContract;
        }

        [Layout]
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
        public ActionResult Create(PartnerManageDto dto)
        {
            var result = _PartnerManageContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(PartnerManageDto dto)
        {
            var result = _PartnerManageContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _PartnerManageContract.Edit(Id);
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
            var result = _PartnerManageContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<PartnerManage, bool>> predicate = FilterHelper.GetExpression<PartnerManage>(request.FilterGroup);
            var count = 0;

            var list = (from s in _PartnerManageContract.Entities.Where<PartnerManage, int>(predicate, request.PageCondition, out count)
                        let p = _PartnerManageCheckContract.Entities.FirstOrDefault(f => f.PartnerManageId == s.Id)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            /*--加盟注册信息---*/
                            s.MemberName,
                            s.MobilePhone,
                            Address = s.Province + s.City + s.Address,
                            s.LicencePhoto,
                            s.StorePhoto,
                            s.Email,
                            Gender = s.Gender.ToString(),
                            s.ZipCode,
                            CheckStatus = s.CheckStatus,
                            CheckStatus_CN = s.CheckStatus.ToString(),
                            OperatorName = p.Operator.Member.MemberName,
                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
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
            var result = _PartnerManageContract.DeleteOrRecovery(true, Id);
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
            var result = _PartnerManageContract.DeleteOrRecovery(false, Id);
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
            var result = _PartnerManageContract.EnableOrDisable(true, Id);
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
            var result = _PartnerManageContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 资料审核

        public ActionResult Check(int Id)
        {
            var dto = new PartnerManageCheck() { PartnerManageId = Id };

            var mod = _PartnerManageCheckContract.Entities.FirstOrDefault(a => a.PartnerManageId == Id);
            if (mod.IsNotNull()) dto = mod;

            ViewBag.StoreTypes = CacheAccess.GetStoreType(_StoreTypeContract, false);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            var rols = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            foreach (var item in rols)
            {
                dic.Add(item.Id, item.RoleName);
            }
            ViewBag.roles = dic;

            return PartialView(dto);
        }

        [Log]
        [HttpPost]
        public ActionResult Check(PartnerManageCheckDto dto, int[] role)
        {
            var data = OperationHelper.Try((opera) =>
             {
                 var mod = _PartnerManageCheckContract.Entities.FirstOrDefault(a => a.PartnerManageId == dto.PartnerManageId);
                 if (mod.IsNotNull())
                 {
                     mod.DepartmentName = dto.DepartmentName;
                     mod.PartnerManageId = dto.PartnerManageId;
                     mod.StorageName = dto.StorageName;
                     mod.StoreName = dto.StoreName;
                     mod.StoreTypeId = dto.StoreTypeId;

                     if (role.IsNotNull())
                     {
                         var has = mod.Roles.Select(s => s.Id);
                         var del = has.Except(role);//需要删除的
                         var add = role.Except(has);//需要添加的

                         var listrol = mod.Roles.ToList();
                         listrol.RemoveAll(a => del.Contains(a.Id));

                         var listadd = _roleContract.Roles.Where(w => add.Contains(w.Id)).ToList();
                         listrol.AddRange(listadd);

                         mod.Roles = listrol;
                     }
                     else
                     {
                         mod.Roles.Clear();
                     }

                     return _PartnerManageCheckContract.Update(mod);
                 }
                 else
                 {
                     mod = new PartnerManageCheck();
                     mod.DepartmentName = dto.DepartmentName;
                     mod.PartnerManageId = dto.PartnerManageId;
                     mod.StorageName = dto.StorageName;
                     mod.StoreName = dto.StoreName;
                     mod.StoreTypeId = dto.StoreTypeId;
                     mod.PartnerManageId = dto.PartnerManageId;
                     mod.PartnerManage = _PartnerManageContract.View(dto.PartnerManageId);

                     if (role.IsNotNull())
                     {
                         mod.Roles = _roleContract.Roles.Where(w => role.Contains(w.Id)).ToList();
                     }

                     return _PartnerManageCheckContract.Insert(mod);
                 }

             }, "审核");

            return Json(data);
        }

        public ActionResult CheckRefuse(int Id)
        {
            var dto = _PartnerManageContract.Edit(Id);
            return PartialView(dto);
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult CheckRefuse(PartnerManageDto dto)
        {
            var dtoOrg = _PartnerManageContract.Edit(dto.Id);
            dtoOrg.CheckNotes = dto.CheckNotes;
            dtoOrg.CheckStatus = CheckStatusFlag.未通过;
            var result = _PartnerManageContract.Update(dtoOrg);
            return Json(result);
        }

        #endregion
    }
}