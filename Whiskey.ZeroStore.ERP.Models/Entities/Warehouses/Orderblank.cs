using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;


namespace Whiskey.ZeroStore.ERP.Models
{

    public class Orderblank : EntityBase<int>
    {

        public Orderblank()
        {
            OrderblankItems = new List<OrderblankItem>();
            OrderblankAudit = new List<OrderblankAudit>();
        }
        /// <summary>
        /// 关联的采购单,直接创建 的配货单是没有关联采购单的
        /// </summary>
        [Display(Name = "关联的采购单")]
        public int? PurchaseId { get; set; }

        [ForeignKey("PurchaseId")]
        public Purchase Purchase { get; set; }

        /// <summary>
        /// 配货单号
        /// </summary>
        [Display(Name = "配货单号")]
        [StringLength(20)]
        public string OrderBlankNumber { get; set; }


        /// <summary>
        /// 配货单类型
        /// </summary>
        public virtual OrderblankType OrderblankType { get; set; }


        /// <summary>
        /// 配货单状态
        /// </summary>
        [Display(Name = "配货单状态")]
        public virtual OrderblankStatus Status { get; set; }

        #region 店铺 仓库
        /// <summary>
        /// 发货店铺
        /// </summary>
        [Display(Name = "发货店铺")]
        [Required(ErrorMessage = "发货店铺")]
        public int OutStoreId { get; set; }
        [ForeignKey("OutStoreId")]
        public virtual Store OutStore { get; set; }
        /// <summary>
        /// 发货仓库
        /// </summary>
        [Display(Name = "发货仓库")]
        public int OutStorageId { get; set; }
        [ForeignKey("OutStorageId")]
        public virtual Storage OutStorage { get; set; }

        /// <summary>
        /// 收货店铺
        /// </summary>
        [Display(Name = "收货店铺")]
        public virtual int ReceiverStoreId { get; set; }

        [ForeignKey("ReceiverStoreId")]
        public virtual Store ReceiverStore { get; set; }
        /// <summary>
        /// 收货仓库
        /// </summary>
        [Display(Name = "收货仓库")]
        public int ReceiverStorageId { get; set; }
        [ForeignKey("ReceiverStorageId")]
        public virtual Storage ReceiverStorage { get; set; }
        #endregion


        #region 操作人

        /// <summary>
        /// 配货单创建人
        /// </summary>
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
        /// <summary>
        /// 发货操作员
        /// </summary>
        [Display(Name = "发货操作员")]
        public virtual int? DeliverAdminId { get; set; }
        [ForeignKey("DeliverAdminId")]
        public virtual Administrator DeliverAdmin { get; set; }


        /// <summary>
        /// 收货操作员
        /// </summary>
        [Display(Name = "收货操作员")]
        public virtual int? ReceiverAdminId { get; set; }
        [ForeignKey("ReceiverAdminId")]
        public virtual Administrator ReceiverAdmin { get; set; }



        #endregion


        /// <summary>
        /// 权重，直接添加的配货单比由采购单产生的配货单优先
        /// </summary>
        public virtual int Weight { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Display(Name = "备注信息")]
        [StringLength(200, ErrorMessage = "长度不能超过{1}")]
        public virtual string Notes { get; set; }


        #region 时间点
        /// <summary>
        /// 发货时,记录此时间
        /// </summary>
        [Display(Name = "发货时间")]
        public DateTime? DeliveryTime { get; set; }

        /// <summary>
        /// 确认收货时,记录此时间
        /// </summary>
        [Display(Name = "收货时间")]
        public DateTime? ReceiveTime { get; set; }
        #endregion

        [Index(IsClustered = false, IsUnique = false), StringLength(17)]
        public string AppointmentNumber { get; set; }

        /// <summary>
        /// 配货单对应的配货明细
        /// </summary>
        public virtual ICollection<OrderblankItem> OrderblankItems { get; set; }
        /// <summary>
        /// 配货单审核记录
        /// </summary>
        public virtual ICollection<OrderblankAudit> OrderblankAudit { get; set; }


    }

    /// <summary>
    /// 配货单类型
    /// </summary>
    public enum OrderblankType
    {

        /// <summary>
        /// 直接创建配货单
        /// </summary>
        直接创建 = 0,

        /// <summary>
        /// 采购单到配货单
        /// </summary>
        采购单创建 = 1,
    }

    /// <summary>
    /// 配货单状态
    /// </summary>
    public enum OrderblankStatus
    {
        /// <summary>
        /// 配货中
        /// </summary>
        [Description("配货中")]
        配货中 = 0,


        /// <summary>
        /// 发货中
        /// </summary>
        [Description("发货中")]
        发货中 = 1,


        /// <summary>
        /// 已撤销
        /// </summary>
        [Description("已撤销")]
        已撤销 = 2,


        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        已完成 = 3

    }

    /// <summary>
    /// 配货单操作
    /// </summary>
    public enum OrderblankAction
    {
        /// <summary>
        /// 创建
        /// </summary>
        Create = 0,

        /// <summary>
        /// 删除
        /// </summary>
        Delete = 1,

        /// <summary>
        /// 发货
        /// </summary>
        Delivery = 2,

        /// <summary>
        /// 拒绝收货
        /// </summary>
        Reject = 3,

        /// <summary>
        /// 收货
        /// </summary>
        Accept = 4

    }

    public class OrderblankNoticeModel
    {
        public int ReceiverStorageId { get; set; }
        public int OutStorageId { get; set; }
        public string OrderblankNumer { get; set; }
        public bool IsReject { get; set; }
        public string RejectReason { get; set; }

    }


}
