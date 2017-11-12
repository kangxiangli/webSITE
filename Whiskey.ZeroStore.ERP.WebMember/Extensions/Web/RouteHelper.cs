using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Web
{
    public static class RouteHelper
    {
        #region 获取路由信息并放入缓存中
        /// <summary>
        /// 获取路由信息并放入缓存中
        /// </summary>
        /// <returns></returns>     
        //public static List<MemberModule> GetModuleList()
        //{
        //    List<MemberModule> listModule = new List<MemberModule>();
        //    if (HttpRuntime.Cache["memberModuleUrl"] == null)
        //    {
        //        IMemberModuleContract _memberModuleContract = DependencyResolver.Current.GetService<IMemberModuleContract>();
        //        if (_memberModuleContract == null) return null;
        //        listModule = _memberModuleContract.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
        //        HttpRuntime.Cache["memberModuleUrl"] = listModule;
        //    }
        //    else
        //    {
        //        listModule = HttpRuntime.Cache["memberModuleUrl"] as List<MemberModule>;
        //    }
        //    return listModule;
        //}
        #endregion

        #region 计算路由是否重写
        /// <summary>
        /// 计算路由是否重写
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        //public static bool CheckRoute(string strUrl)
        //{
        //    List<MemberModule> listModule = GetModuleList();
        //    int length = strUrl.Split('/').Length;
        //    if (length==3)
        //    {
        //        strUrl = strUrl + "/index";
        //    }
        //    int count = listModule.Where(x => !string.IsNullOrEmpty(x.PageUrl) && x.PageUrl.Equals(strUrl, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(x.OverrideUrl)).Count();
        //    if (count == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        #endregion
    }
}