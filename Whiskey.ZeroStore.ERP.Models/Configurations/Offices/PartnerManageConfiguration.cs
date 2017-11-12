using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PartnerManageConfiguration : EntityConfigurationBase<PartnerManage, int>
    {
        public PartnerManageConfiguration()
        {
            ToTable("O_PartnerManage");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
