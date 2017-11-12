using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    public partial class Retail
    {
        /// <summary>
        ///是否只能整单退的条件： 使用优惠券||参加店铺活动优惠
        /// </summary>
        [NotMapped]
        public bool UseCouponOrStoreActivity
        {
            
            get
            {
                if(this.StoreActivityId > 0 || !string.IsNullOrWhiteSpace(this.CouponNumber))
                {
                    return true;
                }
                return false;
                 
            }
        }

    }
}
