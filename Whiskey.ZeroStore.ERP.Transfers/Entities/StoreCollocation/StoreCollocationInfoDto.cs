using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities.StoreCollocation
{
   public class StoreCollocationInfoDto : IAddDto, IEditDto<int>
    {
        public StoreCollocationInfoDto()
        {

        }
        [Display(Name = ("店铺搭配Id"))]
        public virtual int? StoreCollocationId { get; set; }

        [Display(Name = ("商品Id"))]
        public virtual int? ProductOrigNumberId { get; set; }
        [Display(Name = ("店铺搭配标识"))]
        public virtual string StoreCollocationUid { get; set; }
        [Display(Name = "实体标识")]
        public virtual Int32 Id { get; set; }
    }
}
