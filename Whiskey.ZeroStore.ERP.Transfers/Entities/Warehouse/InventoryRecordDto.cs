using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities.Warehouse
{
    public class InventoryRecordDto
    {

        [Display(Name = "编号")]
        public string IdentifyId { get; set; }

        [Display(Name = "所属店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "所属仓库")]
        public virtual int StorageId { get; set; }

        [Display(Name = "吊牌价格")]
        public virtual float TagPrice { get; set; }

        [Display(Name = "数量")]
        public int Count { get; set; }


        [Display(Name = "入库时间")]
        public DateTime InStorageTime { get; set; }

    }
}
