using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class PartnerExperienceDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "合作商")]
        public virtual int PartnerId { get; set; }

        [Display(Name = "经验类型")]
        public virtual int PartnerExperienceType { get; set; }

        [Display(Name = "经验值")]
        public virtual int Experience { get; set; }

        [Display(Name="标识Id")]
        public int Id { get; set; }
    }
}
