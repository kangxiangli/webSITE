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
    /// 预约装箱
    /// </summary>
    public class AppointmentPacking : EntityBase<int>
    {
        public AppointmentPacking()
        {
            AppointmentPackingItem = new List<AppointmentPackingItem>();
        }

        public AppointmentPackingState State { get; set; }

        [Index(IsClustered = false, IsUnique = false), StringLength(17)]
        public string AppointmentNumber { get; set; }

        /// <summary>
        /// 发货店铺
        /// </summary>
        [Display(Name = "发货店铺")]
        [Required]
        public virtual int FromStoreId { get; set; }

        /// <summary>
        /// 收货店铺
        /// </summary>
        [Display(Name = "收货店铺")]
        [Required]

        public virtual int ToStoreId { get; set; }

        /// <summary>
        /// 发货仓库
        /// </summary>
        [Display(Name = "发货仓库")]
        [Required]
        public virtual int FromStorageId { get; set; }

        /// <summary>
        /// 收货仓库
        /// </summary>
        [Display(Name = "收货仓库")]
        public virtual int? ToStorageId { get; set; }


        /// <summary>
        /// 配货单
        /// </summary>
        public virtual int? OrderblankId { get; set; }


        [ForeignKey("FromStoreId")]
        public virtual Store FromStore { get; set; }

        [ForeignKey("ToStoreId")]
        public virtual Store ToStore { get; set; }

        [ForeignKey("FromStorageId")]
        public virtual Storage FromStorage { get; set; }

        [ForeignKey("ToStorageId")]
        public virtual Storage ToStorage { get; set; }


        [ForeignKey("OrderblankId")]
        public virtual Orderblank Orderblank { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<AppointmentPackingItem> AppointmentPackingItem { get; set; }

    }

    public enum AppointmentPackingState
    {
        装箱中 = 0,
        已装箱 = 1,
        已接收 = 2
    }

    public class AppointmentPackingConfig : EntityConfigurationBase<AppointmentPacking, int>
    {
        public AppointmentPackingConfig()
        {
            ToTable("A_AppointmentPacking");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


    public class AddBarcodeReq
    {
        public int Id { get; set; }
        public int ToStorageId { get; set; }
        public string[] ProductBarcodes { get; set; }

    }

    public class RemoveBarcodeReq
    {
        public int Id { get; set; }
        public string ProductBarcode { get; set; }

    }

    public class FinishPackingReq
    {
        public int Id { get; set; }

    }


}
