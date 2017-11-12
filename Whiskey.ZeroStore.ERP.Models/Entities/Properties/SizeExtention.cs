
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class SizeExtention : EntityBase<int>
	{
		[Display(Name = "名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Name { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Size> Sizes { get; set; }

	}
}

