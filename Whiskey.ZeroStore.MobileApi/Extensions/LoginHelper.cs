using System.Collections.Generic;
using System.Web.Caching;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.MobileApi.Models;

namespace Whiskey.ZeroStore.MobileApi
{
    public class LoginHelper
    {
        private static readonly string _Key = "_AdminVerify_201705081707";
        private static readonly string _KeyMember = "_MemberVerify_201709141010";
        public static List<AdminVerify> GetAdmin()
        {
            var adminverify = CacheHelper.GetCache(_Key) as List<AdminVerify>;
            if (adminverify == null) adminverify = new List<AdminVerify>();
            return adminverify;
        }

        public static void SetAdmin(List<AdminVerify> adminVerifies)
        {
            if (adminVerifies != null)
            {
                CacheHelper.SetCache(_Key, adminVerifies);
            }
        }
        public static List<MemberVerify> GetMemberOpenId()
        {
            var adminverify = CacheHelper.GetCache(_Key) as List<MemberVerify>;
            if (adminverify == null) adminverify = new List<MemberVerify>();
            return adminverify;
        }

        public static void SetMemberOpenId(List<MemberVerify> memberVerifies)
        {
            if (memberVerifies != null)
            {
                CacheHelper.SetCache(_Key, memberVerifies);
            }
        }
    }
}