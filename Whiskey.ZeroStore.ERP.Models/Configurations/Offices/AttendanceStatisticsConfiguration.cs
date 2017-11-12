using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AttendanceStatisticsConfiguration : EntityConfigurationBase<AttendanceStatistics, int>
    {
        public AttendanceStatisticsConfiguration()
        {
            ToTable("O_Attendance_Statistics");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
