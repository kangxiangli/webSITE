using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AttendanceConfiguration : EntityConfigurationBase<Attendance, int>
    {
        public AttendanceConfiguration()
        {
            ToTable("O_Attendance");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Departments).WithMany(m => m.Attendances).Map(m =>
            {
                m.ToTable("O_Attendance_Department_Relation");
                m.MapLeftKey("AttendanceId");
                m.MapRightKey("DepartmentId");
            });
        }
    }
}
