using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员IM信息
    /// </summary>
    public class MemberIMProfile : EntityBase<int>
    {
        /// <summary>
        /// 会员id
        /// </summary>
        public int MemberId { get; set; }


        public string Token { get; set; }


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }


    }

    /// <summary>
    /// 会员好友
    /// </summary>
    public class MemberIMFriend : EntityBase<int>
    {

        /// <summary>
        /// 好友1id
        /// </summary>
        public int MemberId1 { get; set; }

        /// <summary>
        /// 好友2id
        /// </summary>
        public int MemberId2 { get; set; }



        [ForeignKey("MemberId1")]
        public virtual Member Member1 { get; set; }

        [ForeignKey("MemberId2")]
        public virtual Member Member2 { get; set; }

        public int Status { get; set; }
    }



}
