using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Attribute
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LicenseAttribute:AuthorizeAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        public IAdministratorContract _administratorContract { get; set; }
        public IPermissionContract _permissionContract { get; set; }
        public IModuleContract _moduleContract { get; set; }

        public CheckMode _checkMode;

        public LicenseAttribute(CheckMode mode)
        {
            _checkMode = mode;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var administrator = _administratorContract.Administrators.FirstOrDefault(m => m.Id == AuthorityHelper.OperatorId);
            if (administrator == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权管理员的身份可能已经注销！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    FormsAuthentication.SignOut();
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
                }
            }
            else
            {
                if (_checkMode == CheckMode.Verify)
                {
                    var area = (filterContext.RouteData.DataTokens.ContainsKey("area") ? filterContext.RouteData.DataTokens["area"].ToString() : string.Empty).ToLower();
                  
                    var controller = filterContext.RouteData.Values["controller"].ToString().ToLower();
                    var action = filterContext.RouteData.Values["action"].ToString().ToLower();
                    if (administrator.Member.MemberName.ToLower() != "admin".ToLower())
                    {
                        try
                        {
                          var module=  CacheAccess.GetModules(_moduleContract).Where(c=>c.PageController!=null&&c.PageAction!=null)
                                        .FirstOrDefault(c => c.PageController.ToLower() == controller && c.IsDeleted == false && c.IsEnabled == true);

                            if (module == null)
                            {
                                if (filterContext.HttpContext.Request.IsAjaxRequest())
                                {
                                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + controller + "不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                }
                                else
                                {
                                    FormsAuthentication.SignOut();
                                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
                                }
                                return;
                            }

                            var mod = GetCurrPermission();
                            if (mod.IsNullOrEmptyThis())
                            {
                                if (filterContext.HttpContext.Request.IsAjaxRequest())
                                {
                                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                }
                                else
                                {
                                    FormsAuthentication.SignOut();
                                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
                                }
                                return;
                            }
                            if (!mod.Exists(e => e.ModuleId == module.Id))
                            {
                                if (filterContext.HttpContext.Request.IsAjaxRequest())
                                {
                                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + module.Id + "中的" + action + "许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                }
                                else
                                {
                                    FormsAuthentication.SignOut();
                                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
                                }
                            }
                            else
                            {
                                var hasper = mod.Where(w => !string.IsNullOrWhiteSpace(w.ActionName)).FirstOrDefault(e => e.ModuleId == module.Id && e.ActionName.ToLower().Trim() == action);

                                if (hasper == null && this.CurrModuleAllActionName(module.Id).Exists(e => e.ToLower() == action))
                                {
                                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                                    {
                                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + module.Id + "中的" + action + "许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                    }
                                    else
                                    {
                                        FormsAuthentication.SignOut();
                                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
                                    }
                                }

                                //((ViewResult)filterContext.Result).ViewBag.AdminName = administrator.AdminName;
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            if (filterContext.HttpContext.Request.IsAjaxRequest())
                            {
                                filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权出现异常：" + ex.ToString()), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                            }
                            else
                            {
                                FormsAuthentication.SignOut();
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        public List<Permission> GetCurrPermission()
        {
            return CacheAccess.GetCurrentUserPermission(_administratorContract,_permissionContract);
        }
        /// <summary>
        /// 当前模块下所有的ActionName，权限所能控制到的方法
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public List<string> CurrModuleAllActionName(int moduleId)
        {
            return CacheAccess.GetPermissions(_permissionContract).Where(w => w.ModuleId == moduleId && !string.IsNullOrWhiteSpace(w.ActionName)).Select(s => s.ActionName).ToList();
        }
    }
}
