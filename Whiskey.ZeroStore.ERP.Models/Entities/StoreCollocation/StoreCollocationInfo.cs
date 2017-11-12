using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.StoreCollocation
{
    [Serializable]
    public class StoreCollocationInfo : EntityBase<int>
    {
        public StoreCollocationInfo()
        {
            Children = new List<StoreCollocationInfo>();
        }

        [Display(Name = ("店铺搭配Id"))]
        public virtual int? StoreCollocationId { get; set; }

        [Display(Name = ("商品Id"))]
        public virtual int? ProductOrigNumberId { get; set; }

        [Display(Name = ("商品条码"))]
        public virtual string ProductBarcode { get; set; }

        [Display(Name = ("店铺搭配标识"))]
        public virtual string StoreCollocationUid { get; set; }

        [ForeignKey("StoreCollocationId")]
        public virtual StoreProductCollocation StoreCollocation { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<StoreCollocationInfo> Children { get; set; }
    }
}
