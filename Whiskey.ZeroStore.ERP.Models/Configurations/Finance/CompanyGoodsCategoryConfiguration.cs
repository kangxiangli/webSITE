
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class CompanyGoodsCategoryConfiguration : EntityConfigurationBase<CompanyGoodsCategory, int>
    {
        public CompanyGoodsCategoryConfiguration()
        {
            ToTable("F_CompanyGoodsCategory");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

