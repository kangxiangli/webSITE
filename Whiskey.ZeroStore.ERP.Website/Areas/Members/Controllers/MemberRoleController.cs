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
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberRoleController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberRoleController));

        protected readonly IMemberRoleContract _MemberRoleContract;
        protected readonly IMemberModuleContract _MemberModuleContract;

        public MemberRoleController(
            IMemberRoleContract _MemberRoleContract,
            IMemberModuleContract _MemberModuleContract
            )
        {
            this._MemberRoleContract = _MemberRoleContract;
            this._MemberModuleContract = _MemberModuleContract;
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
        public ActionResult Create(MemberRole ro)
        {
            string right = Request["ModuleIds"];
            if (right.IsNotNullAndEmpty())
            {
                List<int> ids = right.Split(",", true).ToList().ConvertAll(c => c.CastTo<int>());

                ro.MemberModules = _MemberModuleContract.Entities.Where(w => ids.Contains(w.Id)).ToList();
            }

            var result = _MemberRoleContract.Insert(ro);
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
        public ActionResult Update(MemberRoleDto dto)
        {
            MemberRole role = _MemberRoleContract.Entities.Where(c => c.Id == dto.Id).FirstOrDefault();
            role = AutoMapper.Mapper.Map(dto, role);

            string right = Request["ModuleIds"];
            if (right.IsNotNullAndEmpty())
            {
                List<int> ids = right.Split(",", true).ToList().ConvertAll(c => c.CastTo<int>());

                var orgids = role.MemberModules.Select(s => s.Id).ToList();

                var addperids = ids.Except(orgids).ToList();
                var removeids = orgids.Except(ids).ToList();

                foreach (var item in removeids)
                {
                    var per = role.MemberModules.FirstOrDefault(f => f.Id == item);
                    role.MemberModules.Remove(per);
                }

                var addeds = _MemberModuleContract.Entities.Where(w => addperids.Contains(w.Id)).ToList();

                foreach (var item in addeds)
                {
                    role.MemberModules.Add(item);
                }
            }
            else
            {
                role.MemberModules.Clear();
            }

            var result = _MemberRoleContract.Update(role);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _MemberRoleContract.View(Id);
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
            var result = _MemberRoleContract.View(Id);

            #region 获取角色模块

            var li = new List<ModuPermission>();

            if (result.IsNotNull() && result.MemberModules.Any())
            {
                foreach (var item in result.MemberModules.Where(w => w.IsEnabled && !w.IsDeleted).OrderBy(o => o.ParentId).ToList())
                {
                    var mod = new ModuPermission()
                    {
                        Id = item.Id,
                        Name = item.ModuleName,
                        Description = item.Description,
                    };

                    if (!item.ParentId.HasValue)
                    {
                        li.Add(mod);
                    }
                    else
                    {
                        var par = li.FirstOrDefault(f => f.Id == item.ParentId);
                        if (par.IsNotNull())
                        {
                            par.Child.Add(mod);
                        }
                    }
                }
            }

            ViewBag.da = li;

            #endregion

            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberRole, bool>> predicate = FilterHelper.GetExpression<MemberRole>(request.FilterGroup);
            var count = 0;

            var list = (from s in _MemberRoleContract.Entities.Where<MemberRole, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.UpdatedTime,
                            s.Description,
                            s.Weight,
							s.Name,
                            s.Operator.Member.RealName,

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
            var result = _MemberRoleContract.DeleteOrRecovery(true, Id);
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
            var result = _MemberRoleContract.DeleteOrRecovery(false, Id);
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
            var result = _MemberRoleContract.EnableOrDisable(true, Id);
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
            var result = _MemberRoleContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 是否存在同名的角色
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExisRoleName(string name)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(name))
            {
                bool exis = _MemberRoleContract.Entities.Where(c => !c.IsDeleted && c.IsEnabled && c.Name == name).Count() > 0;
                if (exis)
                    resul = new OperationResult(OperationResultType.Success, "已存在同名的角色");
            }
            return Json(resul);
        }
    }
}

