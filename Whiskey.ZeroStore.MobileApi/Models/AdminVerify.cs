using System;

namespace Whiskey.ZeroStore.MobileApi.Models
{
    [Serializable]
    public class AdminVerify
    {
        public virtual int AdminId { get; set; }

        public virtual string Token { get; set; }
    }

    [Serializable]
    public class MemberVerify
    {
        public virtual string OpenId { get; set; }
    }
}