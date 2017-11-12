using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Newtonsoft.Json;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.WebApi.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LayoutAttribute : ActionFilterAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        public  IAdministratorContract _administratorContract { get; set; }
        public  IPermissionContract _permissionContract { get; set; }
        public  IModuleContract _moduleContract { get; set; }
        public  IRoleContract _roleContract { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (AuthorityHelper.OperatorId == null)
                filterContext.HttpContext.Response.Redirect("/Authorities/Login/Index", true);
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {


            string key = AuthorityHelper.OperatorId + "Menu";
            var menu = CacheHelper.GetCache(key) as List<Module>;
            if (menu == null || menu.Count() == 0)
            {
                menu = this.Menu(filterContext);
               // CacheHelper.SetCache(key, menu, new TimeSpan(600));
            }
           var resul= filterContext.Result as ViewResult;
           if (resul != null)
           {
               ((ViewResult)filterContext.Result).ViewBag.Menu = menu as List<Module>;
               ((ViewResult)filterContext.Result).ViewBag.BreadCrumb = this.BreadCrumb(filterContext);
           }
        }
        /// <summary>
        /// 左侧菜单  yky 2015-11-13
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public List<Module> Menu(ResultExecutingContext filterContext)
        {

            var model = new List<Module>();
            try
            {
                //var administrator = _administratorContract.Administrators.FirstOrDefault(m => m.Id == AuthorityHelper.OperatorId);
                
                var administrator=_administratorContract.Administrators.FirstOrDefault(m => m.Id == AuthorityHelper.OperatorId);

                //根据当前用户具有的权限，查找到用户可以访问的模块  yxk 2015-11-27
                var permissions = GetCurrPermission();

                List<int> modules=permissions.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.ModuleId).Distinct().ToList();

                //foreach (var perm in permissions)
                //{
                //     listModuleId=perm.Modules.Select(x=>x.Id).ToList();
                //}
                //var modules = permissions.Select(m => m.ModuleId).Distinct();
               // var modules = listModuleId.Distinct();
                if (modules.Count() > 0 && EnvironmentHelper.IsStore)
                {
                    ((ViewResult)filterContext.Result).ViewBag.AdminName = administrator.RealName;
                    //var parentMenus = _moduleContract.Modules.Where(m => m.ParentId == null);
                    var parentMenus = _moduleContract.Modules.Where(c => modules.Contains(c.Id)).DistinctBy(C=>C.Id).Select(c => c.Parent).DistinctBy(c=>c.Id).ToList();
                   // var childMenus = _moduleContract.Modules.Where(m =>modules.Contains(m.Id));
                    //可以访问的所有二级模块
                    var childMenus = CacheAccess.GetModules(_moduleContract).Where(m => modules.Contains(m.Id));
                    foreach (var p in parentMenus)
                    {
                        var parent = new Module
                        {
                            Id = p.Id,
                            ParentId = p.ParentId,
                            Icon = p.Icon,
                            ModuleName = p.ModuleName,
                            PageUrl = p.PageUrl,
                            Sequence = p.Sequence
                        };
                        var childs=childMenus.Where(m => m.ParentId == p.Id);
                        foreach (var c in childs)
                        {
                            var child = new Module
                            {
                                Id = c.Id,
                                ParentId = c.ParentId,
                                Icon = c.Icon,
                                ModuleName = c.ModuleName,
                                PageUrl = c.PageUrl,
                                PageController = c.PageController,
                                PageAction = c.PageAction,
                                PageArea = c.PageArea,
                                Sequence = c.Sequence
                            };
                            parent.Children.Add(child);
                        }
                        parent.Children.OrderBy(c => c.Sequence);
                        model.Add(parent);
                    }
                }
                //找到用户可以直接访问的二级模块，如果能够进入二级模块，则默认具有一级模块的访问权限
                
                var TwoModu = administrator.Modules.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ParentId != null);
                foreach (var item in TwoModu)
                {
                    Module pModu = item.Parent;
                    pModu.Children.Clear();
                    
                    var exisMod = model.Where(c => c.Id == pModu.Id).FirstOrDefault();
                    if (exisMod!=null) {
                        exisMod.Children.Add(item);
                    }
                    else {
                        pModu.Children.Add(item);
                        model.Add(pModu);
                    }
                }
                //找打用户可以直接访问的一级模块
                var FModu = administrator.Modules.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ParentId == null);
                foreach (var item in FModu)
                {
                    item.Children.Clear();
                    
                    bool exisMod = model.Where(c => c.Id == item.Id).Count() > 0;
                    if (!exisMod)
                        model.Add(item);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("菜单加载出错，错误如下：" + ex.Message + "。");
            }
           
            //return model.Where(m => m.Children.Count > 0).OrderBy(m => m.Sequence).ToList();
            model = model.DistinctBy(c => c.Id).ToList();
            return model.OrderBy(m => m.Sequence).ToList();
        }


        private Module BreadCrumb(ResultExecutingContext filterContext)
        {
            var breadCrumb = new Module();
            try
            {
                var area = filterContext.RouteData.DataTokens.ContainsKey("area") ? filterContext.RouteData.DataTokens["area"].ToString().ToLower() : string.Empty;
                var controller = filterContext.RouteData.Values["controller"].ToString().ToLower();
                var action = filterContext.RouteData.Values["action"].ToString().ToLower();
                string pageUrl = string.Format("{0}/{1}/{2}", area, controller, action);
               
               // var module = _moduleContract.Modules.FirstOrDefault(m => m.PageUrl.ToLower().Contains(pageUrl.ToLower()));
                var module = CacheAccess.GetModules(_moduleContract).FirstOrDefault(m => m.PageUrl.ToLower().Contains(pageUrl.ToLower()));

                if (module != null)
                {

                    if (module.ParentId != null)
                    {
                       // var parentMenu = _moduleContract.Modules.FirstOrDefault(p => p.Id == module.ParentId);
                        var parentMenu = CacheAccess.GetModules(_moduleContract).FirstOrDefault(x => x.Id == module.ParentId);
                        var parentModel = new Module
                        {
                            ModuleName = parentMenu.ModuleName,
                            Icon = parentMenu.Icon
                        };
                        breadCrumb.Children.Add(parentModel);
                    }

                    var childrenModel = new Module
                    {
                        ModuleName = module.ModuleName,
                        Icon = module.Icon
                    };
                    breadCrumb.Children.Add(childrenModel);

                    breadCrumb.Icon = module.Icon;
                    breadCrumb.ModuleName = module.ModuleName;

                    ((ViewResult)filterContext.Result).ViewBag.ModuleName = module.ModuleName;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("面包屑加载出错，错误如下：" + ex.Message + "。");
            }
            return breadCrumb;
        }

        //yxk 2015-11-27
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        public List<Permission> GetCurrPermission()
        {

            List<Permission> li = new List<Permission>();
            //用户本身具有的权限
            Administrator admin = _administratorContract.Administrators
          .Where(c => c.Id == AuthorityHelper.OperatorId).FirstOrDefault();
            if (admin != null && admin.Permissions.ToList().Count() > 0)
                li.AddRange(admin.Permissions.ToList());
            //用户角色具有的权限
            var roles = admin.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Permissions).ToList();
            foreach (var item in roles)
            {
                li.AddRange(item);
            }

            //用户所属组具有的权限
            var groupli = admin.Groups.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Permissions).ToList();
            foreach (var item in groupli)
            {
                li.AddRange(item.ToList());
            }
            //用户所属部门具有的权限
            li.AddRange(admin.Department.Permissions.ToList());
            List<Permission> newli = new List<Permission>();
            foreach (var item in li)
            {
                if (newli.Where(c => c.Id == item.Id).Count() == 0)
                {
                    newli.Add(item);
                }
            }
            return newli;
        }

    }
}