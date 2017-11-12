
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
    public class City : EntityBase<int>
    {
        public City() {

        }

			
		[Display(Name = "城市名称")]
        public virtual string CityName { get; set; }


        [Display(Name = "所属省份")]
        public virtual int ProvinceId { get; set; }


        [ForeignKey("ProvinceId")]
        public virtual Province Province { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}


