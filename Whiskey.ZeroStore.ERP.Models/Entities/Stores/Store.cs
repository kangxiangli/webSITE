using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Store : EntityBase<int>
    {
        public Store() {
            StoreName = "";
            Description = "";
            StoreCredit = 0;
            Balance = 0;
            StoreDiscount = 1;
            IsMainStore = false;
            MobilePhone = "";
            ContactPerson = "";
            Telephone = "";
            ZipCode = "";
            Address = "";
            Notes = "";
            Members = new List<Member>();
            Inventories = new List<Inventory>();
            Storages = new List<Storage>();
            Administrators = new List<Administrator>();
            Smss = new List<Sms>();
        }

        [Display(Name = "店铺名称")]
		[Required(ErrorMessage = "不能为空")]
		[StringLength(50, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
		public String StoreName { get; set; }

        [Display(Name = "店铺头像")]
        public String StorePhoto { get; set; }

        //[Display(Name = "商城类型")]
        //[DefaultValue(StoreMallType.店铺)]
        //public StoreMallType StoreMallType { get; set; }

        [Display(Name = "官方总店")]
		public Boolean IsMainStore { get; set; }

		[Display(Name = "店铺简介")]
        [StringLength(200, ErrorMessage = "字数不超过200字")]
		public String Description { get; set; }

		[Display(Name = "店铺信誉")]
		public Int32 StoreCredit { get; set; }

		[Display(Name = "店铺余额")]
		public float Balance { get; set; }

		[Display(Name = "联系人")]
        [StringLength(12,ErrorMessage = "12个字符以内")]
		public String ContactPerson { get; set; }

		[Display(Name = "手机号码")]
        [StringLength(11, ErrorMessage = "11位数以内")]
		public String MobilePhone { get; set; }

		[Display(Name = "店铺电话")]
        [StringLength(11, ErrorMessage = "11位数以内")]
		public String Telephone { get; set; }

		[Display(Name = "所在省份")]
        [StringLength(12, ErrorMessage = "12个字符以内")]
		public string Province { get; set; }

		[Display(Name = "所在城市")]
        [StringLength(12,ErrorMessage = "12个字符以内")]
		public string City { get; set; }

		[Display(Name = "邮政编码")]
        [StringLength(6,ErrorMessage = "6位数字")]
		public String ZipCode { get; set; }

		[Display(Name = "店铺地址")]
        [StringLength(50,ErrorMessage = "50个字符以内")]
		public String Address { get; set; }

        [Display(Name = "经度")]
        public float Longitude { get; set; }

        [Display(Name = "纬度")]
        public float Latitude { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(200, ErrorMessage = "200个字符以内")]
		public String Notes { get; set; }

        [Display(Name = "所属部门")]
        public int? DepartmentId { get; set; }

        [Display(Name = "店铺类型")]
        public int StoreTypeId { get; set; }

        [Display(Name = "归属店铺")]
        public virtual bool IsAttached { get; set; }

        [Display(Name = "店铺折扣")]
        [Range(0,1)]
        [DefaultValue(1)]
        public virtual float StoreDiscount { get; set; }

        [Display(Name = "POS机")]
        [StringLength(50)]
        public virtual string IMEI { get; set; }

        /// <summary>
        /// 是否
        /// </summary>
        public bool IsClosed { get; set; }

        [ForeignKey("OperatorId")]
        public Administrator Operator { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("StoreTypeId")]
        public virtual StoreType StoreType { get; set; }

        public virtual ICollection<Member> Members { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; }

        public virtual ICollection<Storage> Storages { get; set; }

        public virtual ICollection<Administrator> Administrators { get; set; }

        public virtual ICollection<Sms> Smss { get; set; }

    }
}


