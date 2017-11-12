using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AttendanceRestItemConfiguration : EntityConfigurationBase<AttendanceRestItem, int>
    {
        public AttendanceRestItemConfiguration()
        {
            ToTable("O_Attendance_Rest_Item");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
