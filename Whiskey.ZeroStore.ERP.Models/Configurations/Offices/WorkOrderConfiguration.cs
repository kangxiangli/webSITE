
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class WorkOrderConfiguration : EntityConfigurationBase<WorkOrder, int>
    {
        public WorkOrderConfiguration()
        {
            ToTable("O_WorkOrder");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

