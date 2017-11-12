
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class PartnerStatistics : EntityBase<int>
	{
		[Display(Name = "加盟店铺")]
        public virtual int PartnerCount { get; set; }

		[Display(Name = "订货件数")]
        public virtual int OrderCount { get; set; }

		[Display(Name = "订货金额")]
        public virtual float OrderMoney { get; set; }

		[Display(Name = "销售件数")]
        public virtual int SaleCount { get; set; }

		[Display(Name = "销售金额")]
        public virtual float SaleMoney { get; set; }

		[Display(Name = "会员数量")]
        public virtual int MemberCount { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
	}
}

