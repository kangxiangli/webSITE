using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class JobPositionConfiguration: EntityConfigurationBase<JobPosition, int>
    {
        public JobPositionConfiguration()
        {
            ToTable("O_JobPosition");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Departments).WithMany(m => m.JobPositions).Map(m =>
            {
                m.ToTable("O_Department_JobPosition_Relation");
                m.MapLeftKey("DepartmentId");
                m.MapRightKey("JobPositionId");
            });
            HasMany(m => m.Administrators).WithOptional(o => o.JobPosition).Map(m =>
            {
                m.MapKey("JobPositionId");
            });
            HasMany(m => m.AppVerManages).WithMany(o => o.JobPositions).Map(m =>
            {
                m.ToTable("O_AppVerManage_JobPosition_Relation");
                m.MapLeftKey("AppVerManageId");
                m.MapRightKey("JobPositionId");
            });
            HasMany(m => m.Factorys).WithMany(o => o.JobPositions).Map(m =>
            {
                m.ToTable("O_Factory_JobPosition_Relation");
                m.MapLeftKey("FactoryId");
                m.MapRightKey("JobPositionId");
            });
        }
    }
}
