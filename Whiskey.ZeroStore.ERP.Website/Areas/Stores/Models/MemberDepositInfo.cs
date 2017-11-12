using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models
{
    public class MemberDepositInfo
    {
        public int MemberId { get; set; }
        public string MemberNumb { get; set; }
        public string MemberName { get; set; }
        public float BeforeBalance { get; set; }
        public float AfterBalance { get; set; }
        public float BeforeScore { get; set; }
        public float AfterScore { get; set; }
        public DateTime CreateTime { get; set; }
        public string Operator { get; set; }
        public string Other { get; set; }
    }
}