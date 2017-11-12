using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Utility.Extensions;
using AutoMapper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    public class GroupController : BaseController
    {

        // GET: /Authorities/Group/
        protected readonly IGroupContract _groupContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IRoleContract _roleContract;
        protected readonly IAdministratorContract _administratContract;
        public GroupController(IGroupContract groupContract, IPermissionContract permissionContract, IModuleContract moduleContract, IRoleContract roleContract, IAdministratorContract administratContract)
        {
            _groupContract = groupContract;
            _permissionContract = permissionContract;
            _moduleContract = moduleContract;
            _roleContract = roleContract;
            _administratContract = administratContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {

            GridRequest request = new GridRequest(Request);
            Expression<Func<Group, bool>> predicate = FilterHelper.GetExpression<Group>(request.FilterGroup);

            var li = _groupContract.Groups.Where(predicate);

            var groups = li.Select(c => new
            {
                c.Id,
                c.IsDeleted,
                c.IsEnabled,
                c.GroupName,
                c.Description,
                c.CreatedTime,
                c.UpdatedTime
            }).OrderByDescending(c => c.CreatedTime).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).ToList();
            GridData<object> obj = new GridData<object>(groups, li.Count(), request.RequestInfo);
            return Json(obj);

        }
        public ActionResult View(int Id)
        {
            Group group = _groupContract.Groups.Where(c => c.Id == Id).FirstOrDefault();

            #region 加载组权限已弃用

            //var per = _permissionContract.Permissions.Where(c => c.Id == 10).FirstOrDefault();
            //List<ModuPermission> li = new List<ModuPermission>();
            //if (group.AGroupPermissionRelations.IsNotNullOrEmptyThis())
            //{
            //    //List<int> permissIds = group.Permissions.Select(c => c.Id).ToList();
            //    //var parMod = group.Permissions.Select(c => c.Module);
            //    List<int> permissIds = group.AGroupPermissionRelations.Select(s => s.PermissionsId.Value).ToList();
            //    var parMod = group.AGroupPermissionRelations.Select(c => c.Permission.Module);
            //    List<Module> modli = new List<Module>();
            //    foreach (var item in parMod)
            //    {
            //       var exis= modli.Where(c => c.Id == item.Id).Count() > 0;
            //       if (!exis) {
            //           modli.Add(item);
            //       }
            //    }
            //    foreach (var item in modli)
            //    {
            //        li.Add(new ModuPermission()
            //        {
            //            Id = item.Id,
            //            Name = item.ModuleName,
            //            Description = item.Description,
            //            Child = GetPermissionByModId(item.Id, permissIds)
            //        });
            //    }

            //    ViewBag.da = li;
            //}
            //else
            //{
            //    ViewBag.da = new List<ModuPermission>();
            //}

            #endregion

            if (group.Admins != null)
                ViewBag.Membs = group.Admins.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Member.MemberName).ToList();
            else
                ViewBag.Membs = new List<string>();
            if (group.Roles != null)
                ViewBag.Groups = group.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.RoleName).ToList();
            else ViewBag.Groups = new List<string>();
            return PartialView(group);
        }
        //yxk 2015-12-1
        /// <summary>
        ///  根据模块id获取模块下的权限
        /// </summary>
        /// <param name="mid">模块id</param>
        /// <param name="perIds">组所有的权限id</param>
        /// <returns></returns>
        private List<ModuPermission> GetPermissionByModId(int mid, List<int> perIds)
        {
            var pers = _moduleContract.Modules.Where(c => c.Id == mid).FirstOrDefault().Permissions;
            if (pers != null)
            {
                return pers.Where(c => perIds.Contains(c.Id)).Select(c => new ModuPermission()
                {
                    Id = c.Id,
                    Name = c.PermissionName,
                    Description = c.Description,
                    Child = null
                }).ToList();
            }
            return null;
        }
        [HttpGet]
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(string GroupName)
        {
            bool exis = _groupContract.Groups.Where(c => c.GroupName == GroupName && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;
            if (exis)
            {
                return Json(new OperationResult(OperationResultType.Error, "已存在同名的组"));
            }
            
            string Description = Request["Description"];
            bool isEnabled = Request["IsEnabled"] == "1" ? true : false;
            Group grou = new Group()
            {
                GroupName = GroupName,
                IsDeleted = false,
                IsEnabled = true,
                Description = Description,
                CreatedTime = DateTime.Now,
                OperatorId = AuthorityHelper.OperatorId
            };

            #region 组权限已弃用

            //var right = Request["right"];
            //var rigshowstr = Request["rightShow"] ?? "";
            //if (!string.IsNullOrEmpty(right))
            //{
            //    List<int> perids = right.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    List<int> pershowids = rigshowstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    //perids = _permissionContract.Permissions.Where(c => perids.Contains(c.Id)).Select(s => s.Id).ToList();//最好做一次数据的校验，防止数据库中不存在传入的数据
            //    if (grou.AGroupPermissionRelations.IsNullThis())
            //        grou.AGroupPermissionRelations = new List<AGroupPermissionRelation>();
            //    perids.ForEach(s =>
            //    {
            //        grou.AGroupPermissionRelations.Add(new AGroupPermissionRelation()
            //        {
            //            PermissionsId = s,
            //            GroupId = grou.Id,
            //            IsShow = pershowids.Exists(e => e == s)
            //        });
            //    });
            //}

            #endregion

            OperationResult resul = _groupContract.Insert(grou);
            return Json(resul);
        }
        /// <summary>
        /// 判断组名是否存在
        /// </summary>
        /// <param name="gName"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Update()
        {
            int id = Convert.ToInt32(Request["Id"]);
            Group group = _groupContract.Groups.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == id).FirstOrDefault(); ;

            #region 组中包含的用户
            if (group.Admins != null)
            {
                Dictionary<int, string> membdic = new Dictionary<int, string>();
                var membs = group.Admins.Where(c => c.IsDeleted == false && c.IsEnabled == true);
                foreach (var item in membs)
                {
                    membdic.Add(item.Id, item.Member.MemberName);
                }
                ViewBag.Membs = membdic;
            }
            else
                ViewBag.Membs = new Dictionary<int, string>();
            #endregion
            #region 组中包含的角色
            if (group.Roles != null)
            {
                Dictionary<int, string> roledic = new Dictionary<int, string>();
                foreach (var item in group.Roles)
                {
                    roledic.Add(item.Id, item.RoleName);
                }
                ViewBag.Roles = roledic;
            }
            else
            {
                ViewBag.Roles = new Dictionary<int, string>();
            }
            #endregion
            return PartialView(group);
        }
        [HttpPost]
        public ActionResult Update(Group group)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            Group grp = _groupContract.Groups.Where(c => c.Id == group.Id).FirstOrDefault();
            #region 角色
            string st = Request["role"];
            if (!string.IsNullOrEmpty(st))
            {
                List<int> _roleid = st.Split(',').ToList().ConvertAll(c => Convert.ToInt32(c));
                var roles = _roleContract.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true && _roleid.Contains(c.Id)).ToList();
                if (grp.Roles == null)
                    grp.Roles = new List<Role>();

                var removRoles = grp.Roles.Where(c => !_roleid.Contains(c.Id));
                foreach (var item in roles)
                {
                    var exis = grp.Roles.Where(c => c.Id == item.Id).Count() > 0;
                    if (!exis)
                        grp.Roles.Add(item);
                }
                //移除被标记为删除的
                if (removRoles != null && grp.Roles != null)
                {
                    var tem = removRoles.ToList();
                    for (int i = 0; i < tem.Count(); i++)
                    {
                        grp.Roles.Remove(tem[i]);
                    }
                  
                }

            } 
            #endregion
            #region 用户
            string memb = Request["memb"];
            if (!string.IsNullOrEmpty(memb))
            {
                string[] membarr = memb.Split(',');
                List<int> membids = new List<int>();
                foreach (var item in membarr)
                {
                    membids.Add(Convert.ToInt32(item));
                }
                var membs = _administratContract.Administrators.Where(c => c.IsDeleted == false && c.IsEnabled == true && membids.Contains(c.Id)).ToList();
                if (grp.Admins == null)
                    grp.Admins = new List<Administrator>();

                var removeAdmi = grp.Admins.Where(c => !membids.Contains(c.Id));
                foreach (var item in membs)
                {
                    var exis = grp.Admins.Where(c => c.Id == item.Id).Count() > 0;
                    if (!exis)
                        grp.Admins.Add(item);
                }
                //删除被标记为移除的用户
                if (removeAdmi != null && grp.Admins != null)
                {
                    var tem = removeAdmi.ToList();
                    for (int i = 0; i < tem.Count(); i++)
                    {
                        grp.Admins.Remove(tem[i]);
                    }
                }
            } 
            #endregion
            #region 组权限已弃用

            //var rigstr = Request["right"];
            //var rigshowstr = Request["rightShow"] ?? "";
            //if (!string.IsNullOrEmpty(rigstr))
            //{
            //    List<int> perids = rigstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    List<int> pershowids = rigshowstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    List<int> allgroupIds = new List<int>();
            //    if (grp.AGroupPermissionRelations.IsNotNullOrEmptyThis())
            //    {
            //        allgroupIds = grp.AGroupPermissionRelations.Select(c => c.PermissionsId.Value).ToList();
            //    }
            //    var addgroupIds = perids.Except(allgroupIds).ToList();
            //    var removegroupIds = allgroupIds.Except(perids).ToList();
            //    if (grp.AGroupPermissionRelations.IsNullThis())
            //        grp.AGroupPermissionRelations = new List<AGroupPermissionRelation>();

            //    addgroupIds.ForEach(s =>
            //    {
            //        grp.AGroupPermissionRelations.Add(new AGroupPermissionRelation()
            //        {
            //            GroupId = grp.Id,
            //            PermissionsId = s,
            //            IsShow = pershowids.Exists(e => e == s)
            //        });
            //    });
            //    foreach (var item in removegroupIds)
            //    {
            //        var per = grp.AGroupPermissionRelations.FirstOrDefault(f => f.PermissionsId == item);
            //        if (per.IsNotNull())
            //            grp.AGroupPermissionRelations.Remove(per);
            //    }
            //    grp.AGroupPermissionRelations.Where(w => pershowids.Contains(w.PermissionsId.Value)).Each(e => e.IsShow = true);//需要修改为显示
            //    grp.AGroupPermissionRelations.Where(w => !pershowids.Contains(w.PermissionsId.Value)).Each(e => e.IsShow = false);//需要修改为不显示的
            //}
            //else {
            //    grp.AGroupPermissionRelations.Clear();
            //}
            
            #endregion
            resul = _groupContract.Update(grp);
            return Json(resul);
        }
        public ActionResult Exist(string gName)
        {
            bool exis = _groupContract.Groups.Where(c => c.GroupName == gName && c.IsEnabled == true && c.IsDeleted == false).Count() > 0;
            if (exis)
            {
                return Json(new OperationResult(OperationResultType.Success, "存在同名的组") { Data = "1" });
            }
            else
                return Json(new OperationResult(OperationResultType.Error));

        }
        /// <summary>
        /// 获取组列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGroups()
        {
            var li = _groupContract.Groups.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new
              {
                  c.Id,
                  c.GroupName
              }).ToList();
            return Json(li);
        }
        public ActionResult GetGroupInfo(int gid)
        {
            var info = _groupContract.Groups.Where(c => c.Id == gid).Select(c => c.Description).FirstOrDefault();
            return Json(info);

        }
        
        
        public ActionResult AddMemb()
        {
            return PartialView();
        }

        #region 组权限已弃用

        ///// <summary>
        ///// 将组的权限已树结构加载
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult LoadPermissionTree()
        //{
        //    return PartialView();
        //}

        //public ActionResult GetTree()
        //{
        //    int depid = Convert.ToInt32(Request["gid"]);
        //    var model = new List<Module>();
        //    ResJson json = new ResJson() { };

        //    var dep = _groupContract.Groups.Where(c => c.Id == depid).FirstOrDefault();
        //    if (dep.IsNotNull() && dep.AGroupPermissionRelations.IsNotNullThis())
        //    {
        //        var rolepers = dep.AGroupPermissionRelations.ToList();
        //        List<Permission> li = new List<Permission>();
        //        if (dep.AGroupPermissionRelations.IsNotNullOrEmptyThis())
        //            li = dep.AGroupPermissionRelations.Select(s => s.Permission).ToList();
        //        json = new ResJson()
        //        {
        //            msg = "测试",
        //            obj = GetNode(li, rolepers),
        //            success = true,
        //            type = "json"
        //        };

        //    }
        //    return Json(json);
        //}

        //private RightTree GetNode(List<Permission> perli, List<AGroupPermissionRelation> rolepers)
        //{
        //    RightTree retree = new RightTree()
        //    {
        //        id = "0",
        //        _checked = false,
        //        text = "选择权限",
        //        url = "",
        //        children = new List<RightTree>()
        //    };
        //    var parmodules = CacheAccess.GetModules(_moduleContract).Where(c => c.ParentId == null && c.IsEnabled == true && c.IsDeleted == false).ToList();
        //    foreach (var item in parmodules)
        //    {
        //        var tre = new RightTree()
        //        {
        //            id = item.Id + "",
        //            text = item.ModuleName,
        //            url = "",
        //            children = GetChild(item.Id, perli, rolepers),
        //            _checked = false,
        //            msg = item.Description
        //        };
        //        tre._checked = tre.children.Count(c => c._checked) > 0;
        //        tre._isShow = tre.children.Count(x => x._isShow) > 0;
        //        retree.children.Add(tre);
        //    }
        //    retree._checked = retree.children.Count(x => x._checked) > 0;
        //    retree._isShow = retree.children.Count(x => x._isShow) > 0;
        //    return retree;
        //}
        //private List<RightTree> GetChild(int parid, List<Permission> perli, List<AGroupPermissionRelation> rolepers)
        //{
        //    bool sel = false;
        //    List<RightTree> li = new List<RightTree>();

        //    var mods = CacheAccess.GetModules(_moduleContract).Where(c => c.ParentId == parid && c.IsDeleted == false && c.IsEnabled == true);
        //    List<bool> ches = new List<bool>();
        //    foreach (var c in mods)
        //    {

        //        var tr = new RightTree()
        //        {
        //            id = c.Id + "",
        //            text = c.ModuleName,
        //            //text=GetCheckPermiss(c.Id,perli,out ch),
        //            url = "",
        //            children = GetPermiss(c.Id, perli, rolepers),
        //            _checked = false,
        //            msg = c.Description
        //        };
        //        tr._checked = tr.children.Count(x => x._checked) > 0;
        //        tr._isShow = tr.children.Count(x => x._isShow) > 0;
        //        li.Add(tr);
        //    }

        //    return li;
        //}
        ///// <summary>
        ///// 根据二级菜单id获取每个页面下的权限
        ///// </summary>
        ///// <param name="twoModId"></param>
        ///// <param name="perli"></param>
        ///// <returns></returns>
        //private List<RightTree> GetPermiss(int twoModId, List<Permission> perli, List<AGroupPermissionRelation> rolepers)
        //{
        //    if (perli == null)
        //        perli = new List<Permission>();
        //    var pers = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == twoModId && c.IsEnabled == true && c.IsDeleted == false).ToList();

        //    return pers.Select(c => new RightTree()
        //    {
        //        id = "c" + c.Id,
        //        text = c.PermissionName,
        //        url = "",
        //        msg = c.Description,
        //        _checked = perli.Select(x => x.Id).Contains(c.Id),
        //        _isShow = rolepers.Count(cc => cc.PermissionsId == c.Id && cc.IsShow != false) > 0,
        //        _gtype = (int?)c.Gtype
        //    }).ToList();
        //}

        #endregion
    }
}