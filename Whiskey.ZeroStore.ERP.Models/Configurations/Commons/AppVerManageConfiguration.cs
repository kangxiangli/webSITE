
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AppVerManageConfiguration : EntityConfigurationBase<AppVerManage, int>
    {
        public AppVerManageConfiguration()
        {
            ToTable("C_AppVerManage");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

