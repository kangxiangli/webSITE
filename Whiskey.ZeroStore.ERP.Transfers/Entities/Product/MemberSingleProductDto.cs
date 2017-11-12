using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class MemberSingleProductDto : IAddDto, IEditDto<int>,ICloneable
    {
        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "单品名称")]
        [Required(ErrorMessage = "不能为空！")]
        [MaxLength(100)]
        public virtual string ProductName { get; set; }

        [Display(Name = "封面图片")]
        [StringLength(150)]
        public virtual string CoverImage { get; set; }

        [Display(Name = "图片")]
        [StringLength(150)]
        public virtual string Image { get; set; }

        [Display(Name = "颜色名称")]
        public virtual int? ColorId { get; set; }

        [Display(Name = "尺码")]
        public virtual int? SizeId { get; set; }

        [Display(Name = "季节")]
        public virtual int? SeasonId { get; set; }

        [Display(Name = "风格")]
        public virtual string ProductAttrId { get; set; }

        [Display(Name = "分类")]
        public virtual int? CategoryId { get; set; }

        [Display(Name = "价格")]
        public virtual decimal? Price { get; set; }

        [Display(Name = "品牌")]
        [StringLength(15)]
        public virtual string Brand { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <returns></returns>
        public MemberSingleProductDto DeepClone()
        {            
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, this);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as MemberSingleProductDto;
            }
        }

        /// <summary>
        /// 浅复制
        /// </summary>
        /// <returns></returns>
        public MemberSingleProductDto ShallowClone()
        {
            return Clone() as MemberSingleProductDto;
        }
    }

   
}
