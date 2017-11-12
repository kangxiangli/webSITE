using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Warehouses
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class InventoryRecord : EntityBase<int>
    {
        public InventoryRecord()
        {
            this.IdentifyId = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            Inventories = new List<Inventory>();
        }

        [Display(Name = "编号")]
        [StringLength(20)]
        [Description("和盘点的CheckerUid向对应")]
        public string IdentifyId { get; set; }

        [Display(Name = "所属店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "所属仓库")]
        public virtual int StorageId { get; set; }

        [Display(Name = "吊牌价格")]
        public virtual float TagPrice { get; set; }

        [Display(Name = "数量")]
        public int Quantity { get; set; }

        [Display(Name = "入库单号")]
        public string RecordOrderNumber { get; set; }


        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("StorageId")]
        public virtual Storage Storage { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


        public virtual ICollection<Inventory> Inventories { get; set; }


    }
}
