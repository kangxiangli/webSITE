
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Province : EntityBase<int>
    {
        public Province() {
            Cities = new List<City>();
        }
			

		[Display(Name = "省份名称")]
        [Required, StringLength(50)]
        public virtual string ProvinceName { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<City> Cities { get; set; }

    }
}


