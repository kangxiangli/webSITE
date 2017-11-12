using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class TimeoutConfigConfiguration : EntityConfigurationBase<TimeoutConfig, int>
    {
        public TimeoutConfigConfiguration()
        {
            ToTable("A_TimeoutConfig");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
