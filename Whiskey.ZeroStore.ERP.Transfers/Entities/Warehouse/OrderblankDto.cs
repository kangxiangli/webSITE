using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OrderblankDto : IAddDto, IEditDto<int>
    {
        public OrderblankDto()
        {
            CreatedTime = DateTime.Now;
        }

        [Display(Name = "标识")]
        public int Id { get; set; }
        /// <summary>
        /// 关联的采购单,直接创建 的配货单是没有关联采购单的
        /// </summary>
        [Display(Name = "关联的采购单")]
        public int? PurchaseId { get; set; }

        /// <summary>
        /// 配货单号
        /// </summary>
        [Display(Name = "配货单号")]
        [StringLength(20)]
        public string OrderBlankNumber { get; set; }

        /// <summary>
        /// 配货单类型 参考OrderblankFlag
        /// </summary>
        public OrderblankType? OrderblankType { get; set; }

        /// <summary>
        /// 配货单状态
        /// </summary>
        [Display(Name = "配货单状态")]
        public OrderblankStatus? Status { get; set; }


        /// <summary>
        /// 发货店铺
        /// </summary>
        [Display(Name = "发货店铺")]
        [Required(ErrorMessage = "发货店铺")]
        public int OutStoreId { get; set; }
        /// <summary>
        /// 发货仓库
        /// </summary>
        [Display(Name = "发货仓库")]
        [Required(ErrorMessage ="请选择仓库")]
        public int OutStorageId { get; set; }


        /// <summary>
        /// 收货店铺
        /// </summary>
        [Display(Name = "收货店铺")]
        public int ReceiverStoreId { get; set; }
        /// <summary>
        /// 收货仓库
        /// </summary>
        [Display(Name = "收货仓库")]
        [Required(ErrorMessage = "请选择仓库")]
        public int ReceiverStorageId { get; set; }

        

        /// <summary>
        /// 发货操作员
        /// </summary>
        [Display(Name = "发货操作员")]
        public int? DeliverAdminId { get; set; }

        /// <summary>
        /// 收货操作员
        /// </summary>
        [Display(Name = "收货操作员")]
        public int? ReceiverAdminId { get; set; }



        /// <summary>
        /// 发货时,记录此时间
        /// </summary>
        [Display(Name = "发货时间")]
        public DateTime? DeliveryTime { get; set; }



        [Display(Name = "备注信息")]
        [StringLength(200,ErrorMessage="长度不能超过{1}")]
        public  string Notes { get; set; }


        
        public DateTime CreatedTime { get; set; }

        [StringLength(17)]
        public string AppointmentNumber { get; set; }



    }
}
