using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Member : EntityBase<int>
    {
        public Member()
        {

            RegisterType = 0;
            MemberName = "";
            MemberPass = "";
            Balance = 0;
            Score = 0;
            CardNumber = "";
            Email = "";
            MobilePhone = "";
            RealName = "";
            Gender = 0;
            DateofBirth = DateTime.Now;
            IDCard = "";
            UserPhoto = "";
            LoginCount = 0;
            LoginTime = DateTime.Now;
            RecommendId = 0;
            Notes = "";
            IsLockedStore = false;
            MemberGifts = new List<MemberGift>();
            MemberDeposits = new List<MemberDeposit>();
            MemberConsumes = new List<MemberConsume>();
            MemberAddresses = new List<MemberAddress>();
            MemberFavourites = new List<MemberFavourite>();
            MemberSingleProduct = new List<MemberSingleProduct>();
            MobileInfos = new List<MobileInfo>();
            MemberFigures = new List<MemberFigure>();
            MemberBehaviors = new List<MemberBehavior>();
            MemberFaces = new List<MemberFace>();
            Smss = new List<Sms>();
            //MemberRoles = new List<MemberRole>();
        }

        [Display(Name = "归属店铺")]
        public virtual int? StoreId { get; set; }

        [Display(Name = "搭配师")]
        public virtual int? CollocationId { get; set; }

        [Display(Name = "会员等级")]
        public virtual int? LevelId { get; set; }

        [Display(Name = "注册类型")]
        public virtual int RegisterType { get; set; }//0：手工注册，1：网站注册，2：iOS注册，3：Android注册

        [Display(Name = "会员昵称")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(12, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        [Index(IsClustered = false)]
        public virtual string MemberName { get; set; }

        [Display(Name = "第三方登录标识")]
        [StringLength(50)]
        public virtual string ThirdLoginId { get; set; }

        [Display(Name = "第三方登录类型")]
        public virtual ThirdLoginFlag ThirdLoginType { get; set; }

        [Display(Name = "会员编号")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(6)]
        [Index(IsClustered = false, IsUnique = false)]
        public virtual string UniquelyIdentifies { get; set; }

        [Index(IsClustered = false)]
        [Display(Name = "会员密码")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string MemberPass { get; set; }

        [Display(Name = "会员类型")]
        public virtual int MemberTypeId { get; set; }

        [Display(Name = "账户余额")]
        public virtual decimal Balance { get; set; }

        [Display(Name = "账户积分")]
        public virtual decimal Score { get; set; }

        [Display(Name = "会员卡号")]
        [StringLength(50, ErrorMessage = "不能超过50个字符")]
        [Index(IsClustered = false)]
        public virtual string CardNumber { get; set; }

        [Display(Name = "电子邮箱")]
        [StringLength(50, ErrorMessage = "不能超过50个字符")]
        public virtual string Email { get; set; }

        [Display(Name = "手机号码")]
        [StringLength(15, ErrorMessage = "不能超过15个字符")]
        [Index(IsClustered = false)]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "真实姓名")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string RealName { get; set; }
        /// <summary>
        /// 0女,1男
        /// </summary>
        [Display(Name = "会员性别")]
        public virtual int Gender { get; set; }

        [Display(Name = "出生日期")]
        public virtual DateTime? DateofBirth { get; set; }

        [Display(Name = "身份证号")]
        [StringLength(20)]
        public virtual string IDCard { get; set; }

        [Display(Name = "会员照片")]
        [StringLength(200)]
        public virtual string UserPhoto { get; set; }

        //[Display(Name = "签到次数")]
        //public virtual long SignCount { get; set; }

        [Display(Name = "登录次数")]
        public virtual long LoginCount { get; set; }

        [Display(Name = "登录时间")]
        public virtual DateTime LoginTime { get; set; }

        [Display(Name = "推荐人")]
        public virtual int? RecommendId { get; set; }

        [Display(Name = "锁定店铺")]
        public virtual bool IsLockedStore { get; set; }//必须在注册店铺消费         


        [Display(Name = "备注信息")]
        [StringLength(200)]
        public virtual string Notes { get; set; }

        public virtual int? AdminiId { get; set; }

        [Obsolete("已废弃")]
        [ForeignKey("AdminiId")]
        public virtual Administrator Admin { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("LevelId")]
        public virtual MemberLevel MemberLevel { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("CollocationId")]
        public virtual Collocation Collocation { get; set; }

        [ForeignKey("MemberTypeId")]
        public virtual MemberType MemberType { get; set; }
        /// <summary>
        /// 保留的是最后一次登录的设备JpushId
        /// </summary>
        [Display(Name = "JPush注册Id")]
        [Obsolete("弃用")]
        public virtual string JPushRegistrationID { get; set; }

        public virtual ICollection<MemberGift> MemberGifts { get; set; }

        public virtual ICollection<MemberDeposit> MemberDeposits { get; set; }

        public virtual ICollection<MemberConsume> MemberConsumes { get; set; }

        public virtual ICollection<MemberAddress> MemberAddresses { get; set; }

        public virtual ICollection<MemberFavourite> MemberFavourites { get; set; }

        public virtual ICollection<MemberSingleProduct> MemberSingleProduct { get; set; }

        public virtual ICollection<MobileInfo> MobileInfos { get; set; }


        public virtual ICollection<MemberFigure> MemberFigures { get; set; }

        public virtual ICollection<AppArticle> AppArticles { get; set; }

        public virtual ICollection<Circle> Circles { get; set; }

        public virtual ICollection<SellerMember> SellerMembers { get; set; }

        public virtual ICollection<MemberBehavior> MemberBehaviors { get; set; }

        /// <summary>
        /// 导入记录id
        /// </summary>
        public int? MemberImportRecordId { get; set; }
        public virtual MemberImportRecord MemberImportRecord { get; set; }


        public virtual ICollection<MemberFace> MemberFaces { get; set; }

        public virtual ICollection<Sms> Smss { get; set; }

        public virtual ICollection<AppointmentGen> AppointmentGens { get; set; }

        /// <summary>
        /// 会员角色
        /// </summary>
        //public virtual ICollection<MemberRole> MemberRoles { get; set; }
    }


    public class MemberImportEntry
    {
        public int Gender { get; set; }
        public string RealName { get; set; }
        public string MobilePhone { get; set; }
        public int? StoreId { get; set; }

    }
}


