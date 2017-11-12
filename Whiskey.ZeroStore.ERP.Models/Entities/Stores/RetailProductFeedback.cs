using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{

    /// <summary>
    /// 零售商品反馈信息
    /// </summary>
    public class RetailProductFeedback : EntityBase<int>
    {
        /// <summary>
        /// 反馈商品所属零售单id
        /// </summary>
        public int RetailId { get; set; }

        /// <summary>
        /// 反馈商品id
        /// </summary>
        public int ProductId { get; set; }

        [Index(IsClustered = false, IsUnique = false)]
        [StringLength(11, ErrorMessage = "商品货号长度无效")]
        public string ProductNumber { get; set; }

        [Index(IsClustered = false, IsUnique = false)]
        [StringLength(50, ErrorMessage = "零售单号长度无效")]
        public string RetailNumber { get; set; }

        /// <summary>
        /// 反馈会员
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// 反馈数据
        /// </summary>
        public string Feedbacks { get; set; }


        /// <summary>
        /// 评分，[0-5]
        /// </summary>
        [Range(0, 5, ErrorMessage = "评分范围在0-5之间")]
        public decimal? RatePoints { get; set; }


        [ForeignKey("RetailId")]
        public Retail Retail { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }


    public class RetailProductFeedbackOptionDto : AppointmentFeedbackOptionDto
    {



    }

    public class RetailProductFeedbackEntry : FeedbackEntry
    {
        /// <summary>
        /// 顾客评分
        /// </summary>
        [Range(0, 5, ErrorMessage = "评分范围在0-5之间")]
        public decimal? RatePoints { get; set; }
    }




    public class RetailProductFeedbackConfig : EntityConfigurationBase<RetailProductFeedback, int>
    {
        public RetailProductFeedbackConfig()
        {
            ToTable("R_RetailProductFeedback");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }


}
