using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class OAuthRecordConfiguration : EntityConfigurationBase<OAuthRecord, int>
    {
        public OAuthRecordConfiguration()
        {
            ToTable("A_OAuthRecord");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
