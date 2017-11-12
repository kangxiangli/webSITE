using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Properties
{
    /// <summary>
    /// 产品保养维护
    /// </summary>
    public class MaintainExtend : EntityBase<int>
    {
        [NonSerialized]
        private ICollection<ProductOriginNumber> _products;
        public int? ParentId { get; set; }
        /// <summary>
        /// 唯一识别的一个四位数编号
        /// </summary>
        public string ExtendNumber { get; set; }
        [StringLength(20)]
        [Required(AllowEmptyStrings = false,ErrorMessage = "名称不为空！")]
        [Display(Name = "名称")]
        public string ExtendName { get; set; }
        [Display(Name = "内容")]
        public string Descript { get; set; }
        /// <summary>
        /// 唯一标识
        /// </summary>
        public int OnlyNum { get; set; }
        [StringLength(200)]
        public string ImgPath { get; set; }
        public string Other { get; set; }

        public ICollection<ProductOriginNumber> Products
        {
            get { return _products; }
            set { _products = value; }
        }
    }
}
