
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class WorkOrderCategoryConfiguration : EntityConfigurationBase<WorkOrderCategory, int>
    {
        public WorkOrderCategoryConfiguration()
        {
            ToTable("O_WorkOrder_Category");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

