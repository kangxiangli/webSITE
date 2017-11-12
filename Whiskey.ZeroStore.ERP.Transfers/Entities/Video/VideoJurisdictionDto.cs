using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
   public class VideoJurisdictionDto : EntityBase<int>, IAddDto, IEditDto<int>
    {
        public VideoJurisdictionDto()
        { }
        
        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        public virtual int VideoEquipmentId { get; set; }
    }
}
