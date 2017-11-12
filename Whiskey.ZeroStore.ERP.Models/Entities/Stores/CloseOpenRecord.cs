



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Enums;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 闭店/开店记录
    /// </summary>
	[Serializable]
    public class OpenCloseRecord : EntityBase<int>
    {
        public int StoreId { get; set; }

        public OpenCloseFlag OpenOrClose{ get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

    }

    public enum OpenCloseFlag
    {
        /// <summary>
        /// 闭店
        /// </summary>
        Close = 0,
        /// <summary>
        /// 开店
        /// </summary>
        Open = 1
    }
}


