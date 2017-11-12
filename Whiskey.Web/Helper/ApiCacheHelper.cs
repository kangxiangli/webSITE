using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Whiskey.Web.Helper
{
    public static class ApiCacheHelper
    {
        /// <summary>
        /// 清除缓存会员Id
        /// </summary>
        public static void ClearMemberCache()
        {
            HttpRuntime.Cache.Remove("List_Member_Id");
        }

        /// <summary>
        /// 添加缓存会员Id
        /// </summary>
        /// <param name="listMemberId"></param>
        public static void AddMemberIdCache(List<int> listMemberId)
        {
            HttpRuntime.Cache["List_Member_Id"] = listMemberId;            
        }
    }
}
