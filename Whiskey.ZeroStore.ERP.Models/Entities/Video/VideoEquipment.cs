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
   public class VideoEquipment : EntityBase<int>
    {
        [Display(Name = "名称")]
        public virtual string VideoName { get; set; }
        [Display(Name = "摄像机号")]
        public virtual string snNumber { get; set; }

        [Display(Name = "店铺")]
        public virtual int StoreId { get; set; }
        [Display(Name = "信息描述")]
        public virtual string Descript { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
