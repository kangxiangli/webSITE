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
    /// 试穿预约
    /// </summary>
    public class Appointment : EntityBase<int>
    {
        public Appointment()
        {
            CollocationPlans = new List<CollocationPlan>();
            AppointmentFeedbacks = new List<AppointmentFeedback>();
        }
        public int MemberId { get; set; }

        public int StoreId { get; set; }


        /// <summary>
        /// 订单号
        /// </summary>
        [Index(IsClustered = false, IsUnique = false), StringLength(17)]
        public string Number { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }


        /// <summary>
        /// 预约的商品货号,允许多个,用逗号分隔
        /// </summary>
        public string ProductNumber { get; set; }


        /// <summary>
        /// 不喜欢的货号
        /// </summary>
        public string DislikeProductNumbers { get; set; }



        /// <summary>
        /// 状态
        /// </summary>
        [Required]
        public AppointmentState? State { get; set; }

        [MaxLength(500, ErrorMessage = "{0}最多不可超过500个字符")]
        public string Notes { get; set; }

        /// <summary>
        /// 装箱id
        /// </summary>
        public int? AppointmentPackingId { get; set; }


        /// <summary>
        /// 用户最终确认的搭配方案,多个用逗号分隔
        /// </summary>
        public int? SelectedPlanId { get; set; }


        /// <summary>
        /// 预约开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 预约截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 预约类型
        /// </summary>
        public AppointmentType AppointmentType { get; set; }


        [ForeignKey("SelectedPlanId")]
        public virtual CollocationPlan SelectedPlan { get; set; }


        [ForeignKey("AppointmentPackingId")]
        public virtual AppointmentPacking AppointmentPacking { get; set; }


        public virtual ICollection<CollocationPlan> CollocationPlans { get; set; }

        public virtual ICollection<AppointmentFeedback> AppointmentFeedbacks { get; set; }


        public string Quantity { get; set; }
        public string Top { get; set; }
        public string Bottom { get; set; }
        public string Jumpsuit { get; set; }
        public string Budget { get; set; }
        public string Situation { get; set; }
        public string Style { get; set; }
        public string Color { get; set; }
        public string Fabric { get; set; }
        public string Season { get; set; }
    }





    /// <summary>
    /// 预约状态
    /// </summary>

    public enum AppointmentState
    {
        /// <summary>
        /// 新增预约信息触发
        /// </summary>
        预约中 = 0,

        /// <summary>
        /// 推送搭配方案触发
        /// </summary>
        已处理 = 1,

        /// <summary>
        /// 确认搭配方案触发
        /// </summary>
        已预约 = 2,

        /// <summary>
        /// 取消预约触发
        /// </summary>
        已撤销 = 3,

        /// <summary>
        /// 配货单[发货中]触发
        /// </summary>
        已装箱 = 4,

        /// <summary>
        /// 配货单[已完成]触发
        /// </summary>
        已接收 = 5,

        /// <summary>
        /// 提交反馈后触发
        /// </summary>
        已试穿 = 6,


        /// <summary>
        /// 回收衣服触发
        /// </summary>
        已回收 = 7,

        /// <summary>
        /// 过了预约时间没有试穿触发
        /// </summary>
        已超时 = 8



    }

    /// <summary>
    /// 预约类型
    /// </summary>
    public enum AppointmentType
    {
        自助,
        快速
    }

    public class AppointmentProductEntry
    {
        public string ProductNumber { get; set; }
    }

    public class AppointmentOptionEntry
    {
        public string Desc { get; set; }
        public bool Multiple { get; set; }
        public Dictionary<string, string> Options { get; set; }
    }
    public class AppointmentOption : Dictionary<string, AppointmentOptionEntry>
    {
        public AppointmentOptionEntry Quantity
        {
            get
            {
                return this["Quantity"];
            }
        }
        public AppointmentOptionEntry Top
        {
            get
            {
                return this["Top"];
            }
            set
            {
                this["Top"] = value;
            }
        }
        public AppointmentOptionEntry Bottom
        {
            get
            {
                return this["Bottom"];
            }
            set
            {
                this["Bottom"] = value;
            }
        }
        public AppointmentOptionEntry Jumpsuit
        {
            get
            {
                return this["Jumpsuit"];
            }
            set
            {
                this["Jumpsuit"] = value;
            }
        }
        public AppointmentOptionEntry Budget
        {
            get
            {
                return this["Budget"];
            }
            set
            {
                this["Budget"] = value;
            }
        }
        public AppointmentOptionEntry Situation
        {
            get
            {
                return this["Situation"];
            }
            set
            {
                this["Situation"] = value;
            }
        }
        public AppointmentOptionEntry Style
        {
            get
            {
                return this["Style"];
            }
            set
            {
                this["Style"] = value;
            }
        }
        public AppointmentOptionEntry Color
        {
            get
            {
                return this["Color"];
            }
            set
            {
                this["Color"] = value;
            }
        }
        public AppointmentOptionEntry Fabric
        {
            get
            {
                return this["Fabric"];
            }
            set
            {
                this["Fabric"] = value;
            }
        }
        public AppointmentOptionEntry Season
        {
            get
            {
                return this["Season"];
            }
            set
            {
                this["Season"] = value;
            }
        }
    }

    public class AppointmentOptionCheckEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }



    public class AppointmentConfiguration : EntityConfigurationBase<Appointment, int>
    {
        public AppointmentConfiguration()
        {
            ToTable("M_Appointment");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(a => a.CollocationPlans).WithMany(p => p.Appointments).Map(config =>
            config.MapLeftKey("AppointmentId")
                   .MapRightKey("CollocationPlanId")
                   .ToTable("Appointment_CollocationPlan_Relation"));
        }
    }
}
