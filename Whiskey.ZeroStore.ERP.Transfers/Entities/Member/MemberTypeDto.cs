using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class MemberTypeDto: IAddDto, IEditDto<int>
    {
        [Display(Name = "名称")]
        [StringLength(20, ErrorMessage = ("名称在20个字符以内"))]
        [Required(ErrorMessage = "请填写会员类型名称")]
        public virtual string MemberTypeName { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(200)]
        public  string Notes { get; set; }

        [Display(Name="折扣")]
        public decimal MemberTypeDiscount { get; set; }

        [Display(Name="主键Id")]
        public  int Id { get; set; }

    }
}
