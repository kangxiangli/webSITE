using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class DesignerDto : IAddDto, IEditDto<int>
    {
        public DesignerDto()
        {
            
        }
        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = "所属工厂")]
        public virtual int FactoryId { get; set; }

        [Display(Name = "员工编号")]
        public virtual int AdminId { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }
    }
}
