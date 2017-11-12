using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using Whiskey.Utility.Filter;
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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{

    [License(CheckMode.Verify)]
    public class RoleController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(RoleController));

        protected readonly IRoleContract _roleContract;
        protected readonly IPermissionContract _permissionContract;
        //protected readonly IGroupContract _groupContract;
        protected readonly IModuleContract _moduleContract;

        public RoleController(IRoleContract roleContract, IPermissionContract permissionContract
            //, IGroupContract groupContract
            , IModuleContract moduleContract)
        {
            _roleContract = roleContract;
            _permissionContract = permissionContract;
            //_groupContract = groupContract;
            _moduleContract = moduleContract;
        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
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
        public ActionResult Create(Role ro)
        {
            var right = Request["right"];
            var rigshowstr = Request["rightShow"] ?? "";
            if (!string.IsNullOrEmpty(right))
            {
                List<int> ids = right.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
                List<int> pershowids = rigshowstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
                //ids = _permissionContract.Permissions.Where(c => ids.Contains(c.Id)).Select(s => s.Id).ToList();//最好做一次数据的校验，防止数据库中不存在传入的数据
                ro.ARolePermissionRelations = ids.Select(s => new ARolePermissionRelation()
                {
                    IsShow = pershowids.Exists(e => e == s),
                    PermissionsId = s,
                    RoleId = ro.Id,
                }).ToList();
            }

            #region 角色所属组已弃用

            //var group = Request["Group"];
            //List<Group> groups = new List<Group>();
            //if (!string.IsNullOrEmpty(group))
            //{
            //    List<int> groupids = right.Split(',').ToList().ConvertAll(c => Convert.ToInt32(c));
            //    groups.AddRanges(_groupContract.Groups.Where(c => groupids.Contains(c.Id)).ToList());
            //}
            //ro.Groups = groups;

            #endregion

            var result = _roleContract.Insert(ro);
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
        public ActionResult Update(RoleDto dto)
        {
            Role role = _roleContract.Roles.Where(c => c.Id == dto.Id).FirstOrDefault();
            role = AutoMapper.Mapper.Map(dto, role);

            #region 权限
            var rigstr = Request["right"];
            var rigshowstr = Request["rightShow"] ?? "";
            if (!string.IsNullOrEmpty(rigstr))
            {
                List<int> perids = rigstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
                List<int> pershowids = rigshowstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));

                var allperssionIds = new List<int>();
                if (role.ARolePermissionRelations.IsNotNullOrEmptyThis())
                {
                    allperssionIds = role.ARolePermissionRelations.Select(s => s.PermissionsId.Value).ToList();
                }
                var addperids = perids.Except(allperssionIds).ToList();
                var removeids = allperssionIds.Except(perids).ToList();
                if (role.ARolePermissionRelations.IsNullThis())
                    role.ARolePermissionRelations = new List<ARolePermissionRelation>();
                addperids.ForEach(s =>
                {
                    role.ARolePermissionRelations.Add(new ARolePermissionRelation
                    {
                        IsShow = pershowids.Exists(e => e == s),
                        PermissionsId = s,
                        RoleId = role.Id,
                    });
                });
                foreach (var item in removeids)
                {
                    var per = role.ARolePermissionRelations.FirstOrDefault(f => f.PermissionsId == item);
                    if (per.IsNotNull())
                        role.ARolePermissionRelations.Remove(per);
                }
                role.ARolePermissionRelations.Where(w => pershowids.Contains(w.PermissionsId.Value)).Each(e => e.IsShow = true);//需要修改为显示
                role.ARolePermissionRelations.Where(w => !pershowids.Contains(w.PermissionsId.Value)).Each(e => e.IsShow = false);//需要修改为不显示的
            }
            else
                role.ARolePermissionRelations.Clear();
            #endregion

            #region 组已弃用
            //string gr = Request["group"];
            //if (!string.IsNullOrEmpty(gr))
            //{
            //    List<int> groupIds = gr.Split(',').ToList().ConvertAll(c => Convert.ToInt32(c));
            //    var allgroupIds = new List<int>();
            //    if (role.Groups != null)
            //    {
            //        allgroupIds = role.Groups.Select(c => c.Id).ToList();
            //    }
            //    var addgroupIds = groupIds.Except(allgroupIds).ToList();
            //    var removegroupIds = allgroupIds.Except(groupIds).ToList();
            //    if (role.Groups.IsNullThis())
            //        role.Groups = new List<Group>();

            //    var gps = _groupContract.Groups.Where(w => addgroupIds.Contains(w.Id)).ToList();
            //    foreach (var item in gps)
            //    {
            //        role.Groups.Add(item);
            //    }

            //    foreach (var item in removegroupIds)
            //    {
            //        var per = role.Groups.FirstOrDefault(f => f.Id == item);
            //        if (per.IsNotNull())
            //            role.Groups.Remove(per);
            //    }
            //}
            //else {
            //    role.Groups.Clear();
            //}
            #endregion

            var result = _roleContract.Update(role);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _roleContract.Edit(Id);

            #region 角色所属组已弃用

            //var role = _roleContract.Roles.Where(c => c.Id == Id && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            //var allgroups = CacheAccess.GetGroups(_groupContract).Where(c => c.IsEnabled == true && c.IsDeleted == false);
            //var hasgroupIds = new List<int>();
            //if (role.Groups != null)
            //{
            //    hasgroupIds = role.Groups.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).ToList();
            //}
            //Dictionary<int, string[]> groupdic = new Dictionary<int, string[]>();
            //foreach (var item in allgroups)
            //{
            //    string selected = hasgroupIds.Contains(item.Id) ? "1" : "0";
            //    groupdic.Add(item.Id, new string[]{item.GroupName
            //  ,selected});
            //}
            //ViewBag.groups = groupdic;

            #endregion

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
            var result = _roleContract.View(Id);
            Role role = _roleContract.Roles.Where(c => c.Id == Id).FirstOrDefault();

            #region 获取角色的默认权限
            List<ModuPermission> li = new List<ModuPermission>();
            //foreach (var item in role.Permissions.ToList())
            foreach (var item in role.ARolePermissionRelations.Select(s => s.Permission).ToList())
            {
                int id = item.Module.Id;
                ModuPermission mod = li.Where(c => c.Id == id).FirstOrDefault();
                if (mod == null)
                {
                    li.Add(new ModuPermission()
                    {
                        Id = id,
                        Name = item.Module.ModuleName,
                        Description = item.Module.Description,
                        Child = new List<ModuPermission>(){
                          new ModuPermission(){
                           Id=item.Id,
                           Name=item.PermissionName,
                           Description=item.Description,
                           Child=null
                          }
                         }
                    });
                }
                else
                {
                    mod.Child.Add(new ModuPermission()
                    {
                        Id = item.Id,
                        Name = item.PermissionName,
                        Description = item.Description,
                        Child = null
                    });
                }
            }
            ViewBag.da = li;
            #endregion

            //ViewBag.groups = role.Groups.Select(c => c.GroupName).ToList();

            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Role, bool>> predicate = FilterHelper.GetExpression<Role>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _roleContract.Roles.Where<Role, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.RoleName,
                    m.Description,
                    m.Weight,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
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
            var result = _roleContract.Remove(Id);
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
            var result = _roleContract.Delete(Id);
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
            var result = _roleContract.Recovery(Id);
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
            var result = _roleContract.Enable(Id);
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
            var result = _roleContract.Disable(Id);
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
            var list = _roleContract.Roles.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _roleContract.Roles.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
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

                bool exis = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.RoleName == name).Count() > 0;
                if (exis)
                    resul = new OperationResult(OperationResultType.Success, "已存在同名的角色");

            }
            return Json(resul);
        }

        public ActionResult LoadPermissionTree()
        {
            return PartialView();
        }
        public ActionResult GetTree()
        {
            int depid = Convert.ToInt32(Request["rid"]);
            var model = new List<Module>();
            ResJson json = new ResJson() { };

            var dep = _roleContract.Roles.Where(c => c.Id == depid).FirstOrDefault();
            if (dep.IsNotNull() && dep.ARolePermissionRelations.IsNotNullThis())
            {
                var rolepers = dep.ARolePermissionRelations.ToList();
                json = new ResJson()
                {
                    msg = "测试",
                    obj = GetNode(rolepers),
                    success = true,
                    type = "json"
                };

            }
            return Json(json);
        }

        private RightTree GetNode(List<ARolePermissionRelation> rolepers)
        {
            RightTree retree = new RightTree()
            {
                id = "0",
                _checked = false,
                text = "选择权限",
                url = "",
                children = new List<RightTree>()
            };
            var listmodules = CacheAccess.GetModules(_moduleContract).Where(w => w.IsEnabled && !w.IsDeleted).ToList();
            var parmodules = listmodules.Where(c => c.ParentId == null).ToList();

            foreach (var item in parmodules)
            {
                var tre = new RightTree()
                {
                    id = item.Id + "",
                    text = item.ModuleName,
                    url = "",
                    children = GetChild(item.Id, rolepers, listmodules),
                    _checked = false,
                    msg = item.Description
                };
                tre._checked = tre.children.Any(c => c._checked);
                tre._isShow = tre.children.Any(x => x._isShow);
                retree.children.Add(tre);
            }
            retree._checked = retree.children.Any(x => x._checked);
            retree._isShow = retree.children.Any(x => x._isShow);
            return retree;
        }

        private List<RightTree> GetChild(int parid, List<ARolePermissionRelation> rolepers, List<Module> listmodules)
        {
            List<RightTree> li = new List<RightTree>();

            var mods = listmodules.Where(c => c.ParentId == parid);
            var listpers = CacheAccess.GetPermissions(_permissionContract).Where(c => c.IsEnabled == true && c.IsDeleted == false).ToList();
            foreach (var c in mods)
            {
                var tr = new RightTree()
                {
                    id = c.Id + "",
                    text = c.ModuleName,
                    url = "",
                    children = GetPermiss(c.Id, rolepers, listpers),
                    _checked = false,
                    msg = c.Description
                };

                tr._checked = tr.children.Any(x => x._checked);
                tr._isShow = tr.children.Any(x => x._isShow);
                li.Add(tr);
            }

            return li;
        }

        private List<RightTree> GetPermiss(int twoModId, List<ARolePermissionRelation> rolepers, List<Permission> listpers)
        {
            var pers = listpers.Where(c => c.ModuleId == twoModId).Select(c => new RightTree()
            {
                id = "c" + c.Id,
                text = c.PermissionName,
                url = "",
                msg = c.Description,
                _checked = rolepers.Any(cc => cc.PermissionsId == c.Id),
                _isShow = rolepers.Any(cc => cc.PermissionsId == c.Id && cc.IsShow != false),
                _gtype = (int?)c.Gtype
            }).ToList();
            return pers;
        }
    }
}
