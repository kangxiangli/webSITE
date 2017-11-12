using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 预约装箱商品
    /// </summary>
    public class AppointmentPackingItem : EntityBase<int>
    {        
        public int? ProductId { get; set; }

        [StringLength(20)]
        [Index(IsClustered =false,IsUnique = false)]
        public string ProductNumber { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public string ProductBarcode { get; set; }

        public int AppointmentPackingId { get; set; }
        
        public virtual AppointmentPacking AppointmentPacking { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }

    public class AppointmentPackingItemConfig : EntityConfigurationBase<AppointmentPackingItem, int>
    {
        public AppointmentPackingItemConfig()
        {
            ToTable("A_AppointmentPackingItem");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


}
