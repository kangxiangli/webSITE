using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 优惠卷实体
    /// </summary>
    
    public class Coupon : EntityBase<int>
    {
        public Coupon()
        {
            CouponItems = new List<CouponItem>();            
        }

        [Display(Name="优惠卷名称")]
        [StringLength(20)]
        [Required(ErrorMessage="0～20个字符")]
        public virtual string CouponName { get; set; }

        [Display(Name = "永久有效")]
        public virtual bool IsForever { get; set; }

        [Display(Name = "创建人标识")]
        [StringLength(10)]
        public virtual string UniqueNum { get; set; }           

        [Display(Name = "优惠卷编号")]
        [StringLength(10)]        
        public virtual string CouponNum { get; set; }

        [Display(Name = "合作商优惠券")]
        public virtual bool IsPartner { get; set; }

        [Display(Name = "供应商")]
        public virtual int? PartnerId { get; set; }  

        [Display(Name = "优惠卷图片")]
        [StringLength(100)]        
        public virtual string CouponImagePath { get; set; }

        //[Display(Name = "优惠卷二维码")]
        //[StringLength(100)]
        //public virtual string CouponQRCodePath { get; set; }  

        [Display(Name = "优惠卷价格")]
        [Required(ErrorMessage="请填写价格")]
        public virtual decimal CouponPrice { get; set; }

        [Display(Name = "优惠卷数量")]
        [Required(ErrorMessage = "请填写数量")]
        public virtual int Quantity { get; set; }

        [Display(Name = "有效开始时间")]
        [Required(ErrorMessage = "请填写有效时间")]
        public virtual DateTime StartDate { get; set; }

        [Display(Name = "有效结束时间")]
        [Required(ErrorMessage = "请填写有效时间")]
        public virtual DateTime EndDate { get; set; }

        [Display(Name = "备注")]
        [StringLength(200,ErrorMessage="最多不能超过{1}个字符")]
        public virtual string Notes { get; set; }
        /// <summary>
        /// 推荐会在手机活动页面列表中显示
        /// </summary>
        [Display(Name = "推荐")]
        public virtual bool IsRecommend { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("PartnerId")]
        public virtual Partner Partner { get; set; }

        public virtual ICollection<CouponItem> CouponItems { get; set; }

         
    }
}
