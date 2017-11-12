using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Shares.Models
{
    public class M_ColloDetail
    {
        public int ProductId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberImage { get; set; }
        public string ColloName { get; set; }
        public string ColloNotes { get; set; }
        public string ColloImagePath { get; set; }
        public int CommentCount { get; set; }
        public int ApproveCount { get; set; }
        public int IsApproval { get; set; }
    }
}