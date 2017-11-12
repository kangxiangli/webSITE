using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class Designer : EntityBase<int>
    {
        public Designer()
        {
            
        }

        [Display(Name = "所属工厂")]
        public virtual int FactoryId { get; set; }

        [ForeignKey("FactoryId")]
        public virtual Factorys Factory { get; set; }

        [Display(Name = "员工编号")]
        public virtual int AdminId { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }
    }
}


