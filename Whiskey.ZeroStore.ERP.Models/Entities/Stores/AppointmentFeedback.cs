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
    /// 预约试穿单品的反馈信息
    /// </summary>
    public class AppointmentFeedback : EntityBase<int>
    {
        public int AppointmentId { get; set; }

        public int ProductId { get; set; }


        [Index(IsClustered = false, IsUnique = false)]
        [StringLength(11, ErrorMessage = "商品货号长度无效")]
        public string ProductNumber { get; set; }


        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


        /// <summary>
        /// 反馈   [{Key:1,Value:"0,1,2"},{Key:2,Value:"2,3"},...]
        /// </summary>
        public string Feedbacks { get; set; }

    }

    public class OptionEntry
    {
        public string Title { get; set; }
        public string Value { get; set; }

    }


    public class AppointmentFeedbackOptionDto
    {
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// 是否多选
        /// </summary>
        public bool Multiple { get; set; }


        //{ "0": "惊喜", "1": "失望", "2": "不明显", "3": "其他" }
        public List<OptionEntry> Options { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }


    }

    public class FeedbackOptionEntry
    {
        public int OptionId { get; set; }
        public string[] Checked { get; set; }

        public string Remarks { get; set; }
    }

    public class FeedbackEntry
    {
        public FeedbackEntry()
        {
            CheckOptions = new List<FeedbackOptionEntry>();
        }
        public List<FeedbackOptionEntry> CheckOptions;
        public string ProductNumber { get; set; }
    }




    public class AppointmentFeedbackConfig : EntityConfigurationBase<AppointmentFeedback, int>
    {
        public AppointmentFeedbackConfig()
        {
            ToTable("A_AppointmentFeedback");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


}
