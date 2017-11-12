using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using System.Web.UI;
using Newtonsoft.Json;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.WebApi.Extensions.Attribute
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LicenseAttribute:AuthorizeAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        public IMemberContract _memberContract { get; set; }        

        public CheckMode _checkMode;

        public LicenseAttribute(CheckMode mode)
        {
            _checkMode = mode;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {                        
            if (HttpRuntime.Cache["List_Member_Id"] == null)
            {
                IQueryable<Member> listMember = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                if (listMember.Count()>0)
                {
                    string strMemberId = filterContext.HttpContext.Request["MemberId"];
                    int memberId = 0;
                    int.TryParse(strMemberId, out memberId);
                    List<int> listMemberId = listMember.Select(x => x.Id).ToList();
                    if (!listMemberId.Contains(memberId))
                    {
                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，你的身份可能已经注销！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    System.Web.Caching.Cache cache = HttpRuntime.Cache;
                    cache.Insert("List_Member_Id", listMemberId);
                }
                else
                {
                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，你的身份可能已经注销！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            else
            {
                string strMemberId = filterContext.HttpContext.Request["MemberId"];
                int memberId=0;
                int.TryParse(strMemberId,out memberId);
                List<int> listMemberId = HttpRuntime.Cache["List_Member_Id"] as List<int>;
                if (!listMemberId.Contains(memberId))
                {
                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，你的身份可能已经注销！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            #region 注释代码
            
            
            //var administrator = _administratorContract.Administrators.FirstOrDefault(m => m.Id == AuthorityHelper.OperatorId);
            //if (administrator == null)
            //{
            //    if (filterContext.HttpContext.Request.IsAjaxRequest())
            //    {
            //        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权管理员的身份可能已经注销！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //    }
            //    else
            //    {
            //        FormsAuthentication.SignOut();
            //        //filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权管理员的身份可能已经注销！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
            //    }
            //}
            //else
            //{
            //    if (_checkMode == CheckMode.Verify)
            //    {
            //        var area = (filterContext.RouteData.DataTokens.ContainsKey("area") ? filterContext.RouteData.DataTokens["area"].ToString() : string.Empty).ToLower();
            //        var controller = filterContext.RouteData.Values["controller"].ToString().ToLower();
            //        var action = filterContext.RouteData.Values["action"].ToString().ToLower();
            //        if (administrator.AdminName.ToLower() != "admin".ToLower())
            //        {
            //            try
            //            {
            //                var module = _moduleContract.Modules.FirstOrDefault(m => m.PageArea.ToLower() == area && m.PageController.ToLower() == controller);
            //                if (module == null)
            //                {
            //                    //filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + controller + "不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    if (filterContext.HttpContext.Request.IsAjaxRequest())
            //                    {
            //                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + controller + "不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    }
            //                    else
            //                    {
            //                        FormsAuthentication.SignOut();
            //                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
            //                    }
            //                }

            //                var roles = administrator.Roles;
            //                if (roles==null || !roles.Any())
            //                {
            //                    //filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权角色不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    if (filterContext.HttpContext.Request.IsAjaxRequest())
            //                    {
            //                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权角色不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    }
            //                    else
            //                    {
            //                        FormsAuthentication.SignOut();
            //                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
            //                    }
            //                }

            //                var permissions = roles.SelectMany(m => m.Permissions);
            //                if (permissions== null || !permissions.Any())
            //                {
            //                    //filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    if (filterContext.HttpContext.Request.IsAjaxRequest())
            //                    {
            //                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    }
            //                    else
            //                    {
            //                        FormsAuthentication.SignOut();
            //                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
            //                    }
            //                }
            //                //var result= permissions.Where(p => p.ModuleId== module.Id  && p.Identifier.ToLower() == action.ToLower());
            //                var result= permissions.Where(p =>p.ModuleId== module.Id&& p.Identifier.ToLower() == action.ToLower());
            //                if (result == null || !result.Any())
            //                {
            //                    //filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + module.Id + "中的" + action + "许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    if (filterContext.HttpContext.Request.IsAjaxRequest())
            //                    {
            //                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权模块" + module.Id + "中的" + action + "许可不存在！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                    }
            //                    else
            //                    {
            //                        FormsAuthentication.SignOut();
            //                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
            //                    }
            //                }
            //                else {
            //                    //((ViewResult)filterContext.Result).ViewBag.AdminName = administrator.AdminName;
            //                }
           


            //            }
            //            catch (Exception ex)
            //            {
            //                //filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权出现异常：" + ex.ToString()), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                if (filterContext.HttpContext.Request.IsAjaxRequest())
            //                {
            //                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.Error, "你没有权限进行此操作，授权出现异常：" + ex.ToString()), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //                }
            //                else
            //                {
            //                    FormsAuthentication.SignOut();
            //                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", area = "Authorities" }));
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
        }
            





    }
}
