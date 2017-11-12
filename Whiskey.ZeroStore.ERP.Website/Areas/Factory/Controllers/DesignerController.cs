using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using AutoMapper;
using Antlr3.ST;
using Antlr3.ST.Language;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Filter;
using System.Linq.Expressions;
//using Whiskey.ZeroStore.ERP.Website.Areas.Factory.Models;


namespace Whiskey.ZeroStore.ERP.Website.Areas.Factory.Controllers
{
    [License(CheckMode.Verify)]
    public class DesignerController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _log = LogManager.GetLogger(typeof(DesignerController));
        public static object objlock = new object();
        protected readonly IDesignerContract _designerContract;
        protected readonly IFactorysContract _factorysContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IJobPositionContract _jobPositionContract;
        //protected readonly IAdministratorTypeContract AdministratorTypeContract;
        protected readonly IDepartmentContract _departmentContract;
        protected readonly IRoleContract _roleContract;

        public DesignerController(
            IFactorysContract factoryContract,
            IDesignerContract designerContract,
            IBrandContract brandContract,
            IAdministratorContract administratorContract,
            IJobPositionContract jobPositionContract,
            IMemberContract memberContract,
            IDepartmentContract _departmentContract,
            //IAdministratorTypeContract AdministratorTypeContract,
            IRoleContract _roleContract
            )
        {
            _designerContract = designerContract;
            _factorysContract = factoryContract;
            _brandContract = brandContract;
            _memberContract = memberContract;
            _jobPositionContract = jobPositionContract;
            _administratorContract = administratorContract;
            //this.AdministratorTypeContract = AdministratorTypeContract;
            this._departmentContract = _departmentContract;
            this._roleContract = _roleContract;
        }

        #endregion
        [Layout]
   
        public ActionResult Index()
        {
            var li = CacheAccess.GetFactorys(_factorysContract, true);
            ViewBag.GetFactorys = li;

            ViewBag.GetBrand = CacheAccess.GetBrand(_brandContract, true, false);
            return View();
        }

        
        public ActionResult Update(int Id)
        {
            var modDes = _designerContract.View(Id);
            ViewBag.Designer = modDes;
            var modAdmin = modDes.Admin;
            Dictionary<int, string[]> roles = new Dictionary<int, string[]>();
            List<int> adRole = new List<int>();
            if (modAdmin != null)
            {
                adRole = modAdmin.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).ToList();
            }
            var roleLis = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            foreach (var item in roleLis)
            {
                string selected = adRole.Contains(item.Id) ? "1" : "0";
                roles.Add(item.Id, new string[] { item.RoleName, selected });
            }
            ViewBag.roles = roles;

            return PartialView(modAdmin);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Log]
        public ActionResult Update(Administrator dto)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            int countName = _administratorContract.Administrators.Where(x => x.Id != dto.Id && x.Member.MemberName == dto.Member.MemberName).Count();
            if (countName > 0)
            {
                result.Message = "员工昵称已经存在";
                return Json(result);
            }
            Administrator admin = _administratorContract.Administrators.Where(c => c.Id == dto.Id).FirstOrDefault();

            admin.Member.MemberName = dto.Member.MemberName;
            if (!dto.Member.MemberPass.IsNullOrEmpty())
            {
                admin.Member.MemberPass = dto.Member.MemberPass.MD5Hash();
            }
            admin.Member.Email = dto.Member.Email;
            admin.Member.MobilePhone = dto.Member.MobilePhone;
            admin.Member.RealName = dto.Member.RealName;
            admin.Member.Gender = dto.Member.Gender;
            admin.Member.UpdatedTime = DateTime.Now;
            admin.MacAddress = dto.MacAddress;
            //admin.AdministratorTypeId = 3;//设计师
            admin.Notes = dto.Notes;

            admin.IsEnabled = true;

            #region 角色
            var rolestr = Request["role"];
            if (!string.IsNullOrEmpty(rolestr))
            {
                var rol = Request["role"].Split(',');
                List<int> roleIds = new List<int>();
                foreach (var item in rol)
                {
                    roleIds.Add(Convert.ToInt32(item));
                }
                List<int> allRoleId = new List<int>();
                if (admin.Roles != null)
                    allRoleId = admin.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).ToList();

                List<int> newAddRoleIds = roleIds.Where(c => !allRoleId.Contains(c)).ToList();
                List<int> removRoleIds = allRoleId.Where(c => !roleIds.Contains(c)).ToList();

                foreach (var item in newAddRoleIds)
                {
                    var addRole = _roleContract.Roles.Where(c => c.Id == item).FirstOrDefault();
                    if (admin.Roles == null)
                        admin.Roles = new List<Role>() { addRole };
                    else
                        admin.Roles.Add(addRole);
                }
                foreach (var item in removRoleIds)
                {
                    var removRole = admin.Roles.Where(c => c.Id == item).FirstOrDefault();
                    if (admin.Roles != null)
                        admin.Roles.Remove(removRole);
                }
            }
            else
            {
                admin.Roles.Clear();
            }

            #endregion

            result = _administratorContract.Update(admin);
            return Json(result);
        }


        public ActionResult Create()
        {
            ViewBag.GetFactorys = CacheAccess.GetFactorys(_factorysContract, true);
            ViewBag.GetBrand = CacheAccess.GetBrand(_brandContract, false, false);
            return PartialView();
        }
        /// <summary>
        /// 获取工厂信息
        /// </summary>
        /// <param name="factoryId"></param>
        /// <returns></returns>
        public JsonResult GetFactoryInfo(int factoryId)
        {
            var modfac = _factorysContract.SelectFactorys.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == factoryId).Select(s => new
            {
                s.Brand.BrandName,
                s.Store.StoreName,
                s.Storage.StorageName,
                s.Department.DepartmentName,
                s.DepartmentId,
            }).FirstOrDefault();

            return Json(modfac, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(DesignerDto drod)
        {
            OperationResult oper = _designerContract.Insert(drod);
            return Json(oper);
        }


        [Log]
        public ActionResult View(int Id)
        {
            var result = _designerContract.View(Id);

            if (result?.Admin?.Roles != null)
            {
                ViewBag.roles = result.Admin.Roles.Where(c => !c.IsDeleted && c.IsEnabled).Select(c => c.RoleName).ToList();
            }
            else
                ViewBag.roles = null;

            return PartialView(result);
        }

        public ActionResult Admin()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var query = from a in _administratorContract.Administrators
                            join d in _designerContract.SelectDesigner on a.Id equals d.AdminId into de
                            from r in de.DefaultIfEmpty()
                            where r == null
                            select a;

                var count = 0;
                var list = query.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Member.RealName,
                    m.Department.DepartmentName,
                    m.Member.MobilePhone,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInfoById(int AdminId)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);

            //判断将要添加的员工是否已存在
            bool exis = _designerContract.SelectDesigner.Where(c => c.AdminId== AdminId && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;

            if (exis)
            {
                resul = new OperationResult(OperationResultType.Error, "员工已经存在");
            }
            else
            {
                resul = new OperationResult(OperationResultType.Success, "");
            }
            return Json(resul);
        }

     

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Designer, bool>> predicate = FilterHelper.GetExpression<Designer>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _designerContract.SelectDesigner.Where<Designer, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Factory.FactoryName,
                    m.Admin.Member.MemberName,
                    m.Admin.Member.RealName,
                    m.Factory.Brand.BrandName,
                    m.CreatedTime,
                    m.IsDeleted,
                    m.IsEnabled,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFactory()
        {
            string strDepartmentId = Request["FactoryId"];
            int departmentId = 0;
            if (!string.IsNullOrEmpty(strDepartmentId))
            {
                departmentId = int.Parse(strDepartmentId);
            }
            var listEntity = _factorysContract.SelectFactorys;
            var entity = listEntity.Select(x => new
            {
                x.Id,
                x.FactoryName,
            }).ToList();
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Remove(int[] Id)
        {
            var result = _designerContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Delete(int[] Id)
        {
            var result = _designerContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Recovery(int[] Id)
        {
            var result = _designerContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Log]
        public ActionResult Enable(int[] Id)
        {
            var result = _designerContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Disable(int[] Id)
        {
            var result = _designerContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _designerContract.SelectDesigner.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _designerContract.SelectDesigner.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
    }
}