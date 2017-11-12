
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class WorkOrderDealtWithConfiguration : EntityConfigurationBase<WorkOrderDealtWith, int>
    {
        public WorkOrderDealtWithConfiguration()
        {
            ToTable("O_WorkOrderDealtWith");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

