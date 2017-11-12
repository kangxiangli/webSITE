using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models 
{
    public class WorkLogAttributeConfiguration : EntityConfigurationBase<WorkLogAttribute, int>
    {
        public WorkLogAttributeConfiguration()
        {
            ToTable("O_Work_Log_Attribute");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
