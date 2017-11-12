using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Offices
{
    public class WorkTimeDetaileConfiguration : EntityConfigurationBase<WorkTimeDetaile, int>
    {
        public WorkTimeDetaileConfiguration()
        {
            ToTable("O_WorkTimeDetaile");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
