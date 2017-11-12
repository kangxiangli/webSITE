
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Administrator : EntityBase<int>
    {
        public Administrator() {
            Stores=new List<Store>();
            Roles = new List<Role>();
            Messagers = new List<Messager>();
            Notifications = new List<Notification>();
            OrderFoods = new List<OrderFood>();
            //Groups = new List<Group>();
            //AdministratorPermissionRelations = new List<AAdministratorPermissionRelation>();
            //Storages = new List<Storage>();
        }

        [Display(Name = "入职时间")]
        [Required(ErrorMessage = "请选择入职时间")]
        public virtual DateTime EntryTime { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(1000)]
        public virtual string Notes { get; set; }

        [Display(Name = "登录次数")]      
        public virtual long LoginCount { get; set; }

        [Display(Name = "登录时间")]
        public virtual DateTime? LoginTime { get; set; }
        //2015-12-7
        /// <summary>
        /// 是否可用于系统登录
        /// </summary>
        [Display(Name="是否可以用于系统登录")]
        public virtual bool IsLogin { get; set; }

        public virtual bool IsPersonalTime { get; set; }

        [Display(Name = "Mac地址")]
        [StringLength(50,ErrorMessage="最大长度不能超过{1}个字符")]
        public virtual string MacAddress { get; set; }

        [Display(Name = "会员")]
        public virtual int? MemberId { get; set; }

        [Display(Name="所属部门")]
        public virtual int? DepartmentId { get; set; }

        [Display(Name = "职位")]
        public virtual int? JobPositionId { get; set; }

        [Display(Name = "工作时间")]
        public virtual int? WorkTimeId { get; set; }

        //[Display(Name = "员工类型")]
        //public virtual int AdministratorTypeId { get; set; }

        [Display(Name = "休假")]
        public virtual int? RestId { get; set; }
        [Display(Name = "工作时间类型是否改变")]
        public virtual bool? whetherChange { get; set; }

        [Display(Name = "改变时间")]
        public virtual DateTime? whetherDateTime { get; set; }

        [ForeignKey("RestId")]
        public virtual Rest Rest { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("JobPositionId")]
        public virtual JobPosition JobPosition { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("WorkTimeId")]
        public virtual WorkTime WorkTime { get; set; }

        //[ForeignKey("AdministratorTypeId")]
        //public virtual AdministratorType AdministratorType { get; set; }

        /// <summary>
        /// 应用程序在第一次成功注册到 JPush 服务器时，
        /// JPush 服务器会给客户端返回一个唯一的该设备的标识 - RegistrationID，
        /// 然后就可以根据 RegistrationID 来向设备推送消息或者通知。
        /// 由于用户可能登录多个设备,应该转换成数组,现在已启用别名和标签,准备弃用
        /// </summary>
        [Display(Name="JPush注册ID")]
        [StringLength(100, ErrorMessage = "最大长度不能超过{1}个字符")]
        [Obsolete("准备弃用")]
        public virtual string JPushRegistrationID { get; set; }

        public virtual ICollection<Store> Stores { get; set; }

        //public virtual ICollection<Storage> Storages { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        public virtual ICollection<Messager> Messagers { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }

        //public virtual ICollection<Group> Groups { get; set; }

        //public virtual ICollection<Permission> Permissions { get; set; }

        public virtual ICollection<Module> Modules { get; set; }

        public virtual ICollection<SellerMember> SellerMembers { get; set; }

        public virtual ICollection<OrderFood> OrderFoods { get; set; }

        //public virtual ICollection<AAdministratorPermissionRelation> AdministratorPermissionRelations { get; set; }
    }
}


