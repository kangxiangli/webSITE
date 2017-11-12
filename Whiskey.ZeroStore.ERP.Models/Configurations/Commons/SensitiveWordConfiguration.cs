using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models 
{
    public class SensitiveWordConfiguration : EntityConfigurationBase<SensitiveWord, int>
    {
        public SensitiveWordConfiguration()
        {
            ToTable("C_SensitiveWord");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
