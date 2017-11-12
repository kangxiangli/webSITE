using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 推荐搭配
    /// </summary>
    public class RecommendMemberCollocation : EntityBase<int>
    {
        public int MemberCollocationId { get; set; }

        [ForeignKey("MemberCollocationId")]
        public MemberCollocation MemberCollocationEntity { get; set; }
        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        public virtual Administrator Operator { get; set; }

    }





    public class RecommendMemberSingleProduct : EntityBase<int>
    {

        [StringLength(20)]
        [Index(IsClustered = false, IsUnique = false)]
        public string BigProdNumber { get; set; }

        public int ProductOriginNumberId { get; set; }

        [ForeignKey("ProductOriginNumberId ")]
        public virtual ProductOriginNumber ProductOriginNumber { get; set; }

        public int ColorId { get; set; }

        [ForeignKey("ColorId")]

        public virtual Color Color { get; set; }

        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        public virtual Administrator Operator { get; set; }

    }


    public class SaveMemberRecommendEntry
    {
        public int MemberId { get; set; }
        public int ColorId { get; set; }

    }

    public class RecommendMemberSingleProductConfig : EntityConfigurationBase<RecommendMemberSingleProduct, int>
    {
        public RecommendMemberSingleProductConfig()
        {
            ToTable("R_RecommendMemberSingleProduct");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


    public class RecommendMemberCollocationEntityConfig : EntityConfigurationBase<RecommendMemberCollocation, int>
    {
        public RecommendMemberCollocationEntityConfig()
        {
            ToTable("R_RecommendMemberCollocation");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }












}
