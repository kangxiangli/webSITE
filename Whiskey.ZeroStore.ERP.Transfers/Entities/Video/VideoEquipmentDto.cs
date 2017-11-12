using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
   public class VideoEquipmentDto : EntityBase<int>, IAddDto, IEditDto<int>
    {
        public VideoEquipmentDto()
        {
            
        }

        [Display(Name = "名称")]
        public virtual string VideoName { get; set; }
        [Display(Name = "摄像机号")]
        public virtual string snNumber { get; set; }
        [Display(Name = "信息描述")]
        public virtual string Descript { get; set; }

        [Display(Name = "归属店铺")]
        public virtual int StoreId { get; set; }

    }
}
