using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
   public class ProductTrackDto : IAddDto, IEditDto<int>
    {
       
        [Display(Name = "实体标识")]
        public virtual Int32 Id { get; set; }

        /// <summary>
        /// 商品货号
        /// </summary>
        [Display(Name = "商品货号")]
        public virtual string ProductNumber { get; set; }

        [Display(Name = "商品流水号")]
        public virtual string ProductBarcode { get; set; }

        [Display(Name = "描述")]
        public virtual string Describe { get; set; }

        public virtual DateTime? CreatedTime { get; set; }
    }
}
